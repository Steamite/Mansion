using Items;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.Editor.Levels.Items
{
    enum ItemTypeEdit
    {
        Text,
        Pdf
    }
    public partial class ItemEditor : Foldout
    {
        RoomList _roomList;
        ObjectField itemField;

        EnumField itemType;
        TextField itemName;
        ObjectField itemData;
        Vector3Field position;
        Vector3Field centerOffset;

        CustomMinMaxSlider radiusRange;
        Vector2Field startRotation;

        Toggle staticObject;

        ObjectField modelField;

        Button zoomTo;
        InteractableItem _item;
        public ItemEditor()
        { }
        public ItemEditor(RoomList roomList)
        {
            _roomList = roomList;
            Add(itemField = new ObjectField() 
            { 
                enabledSelf = false 
            });

            Add(itemName = new TextField() { label = "Item Name"});
            Add(itemType = new EnumField() { label = "Type"});
            Add(itemData = new ObjectField() { label = "Data"});
            Add(modelField = new ObjectField() { label = "Item Name", objectType = typeof(Mesh)});

            Add(radiusRange = new CustomMinMaxSlider("Radius"));

            Add(startRotation = new Vector2Field() { label = "Start Rotation"});
            //Add(field = new ObjectField());
            Add(position = new Vector3Field() { label = "Position"});
            Add(centerOffset = new Vector3Field() { label = "Center Offset"});

            Add(zoomTo = new Button() { text = "Show" });
            zoomTo.clicked += () =>
            {
                Selection.activeTransform = _item.transform;
                SceneView.lastActiveSceneView.FrameSelected();
            };
            Add(staticObject = new Toggle() { text = "Static" });
        }

        public void Bind(InteractableItem item)
        {
            _item = item;
            itemField.value = _item;

            text = _item.ItemName;
            SerializedObject sItem = new(_item);
            itemName.Unbind();
            itemName.BindProperty(sItem.FindProperty(nameof(InteractableItem.ItemName)));
            itemName.UnregisterValueChangedCallback(UpdateGOName);
            itemName.RegisterValueChangedCallback(UpdateGOName);

            itemData.value = _item.SourceObject.Asset;

            itemType.value = (_item is PDFItem) ? ItemTypeEdit.Pdf : ItemTypeEdit.Text;
            itemType.enabledSelf = false;

            SerializedObject sTran = new SerializedObject(_item.transform);
            position.Unbind();
            position.BindProperty(sTran.FindProperty("m_LocalPosition"));
            
            centerOffset.Unbind();
            centerOffset.BindProperty(sItem.FindProperty(nameof(InteractableItem.offset)));

            //maxDrag.BindProperty(sItem.FindProperty(InteractableItem.MaxDrag));
            radiusRange.Bind(sItem.FindProperty(nameof(InteractableItem.RadiusRange)));

            modelField.UnregisterValueChangedCallback(MeshChanged);
            modelField.value = _item.GetComponent<MeshFilter>().sharedMesh;
            modelField.RegisterValueChangedCallback(MeshChanged);


            startRotation.Unbind();
            startRotation.BindProperty(sItem.FindProperty(nameof(InteractableItem.startRotation)));

            BindStaticToggle();
        }

        void MeshChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Mesh mesh = evt.newValue as Mesh;
            Material[] materials = FBXMeshMaterialMapper.GetMeshMaterials(mesh);
            _item.GetComponent<MeshFilter>().sharedMesh = mesh;
            _item.GetComponent<MeshRenderer>().sharedMaterials = materials;

            EditorUtility.SetDirty(_item);
            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath(_roomList.RoomScene));
        }

        private void UpdateGOName(ChangeEvent<string> evt)
        {
            _item.gameObject.name = evt.newValue;
            text = evt.newValue;
            EditorUtility.SetDirty(_item.gameObject);
        }

        void BindStaticToggle()
        {
            staticObject.Unbind();
            SerializedObject sObj = new SerializedObject(_item.gameObject);
            SerializedProperty staticFlagsProp = sObj.FindProperty("m_StaticEditorFlags");

            staticObject.value = _item.gameObject.isStatic;

            staticObject.UnregisterValueChangedCallback(ToggleStatic);
            staticObject.RegisterValueChangedCallback(ToggleStatic);

            staticObject.TrackPropertyValue(staticFlagsProp, prop =>
            {
                staticObject.SetValueWithoutNotify(_item.gameObject.isStatic);
            });
        }
        private void ToggleStatic(ChangeEvent<bool> evt)
        {
            Undo.RecordObject(_item.gameObject, "Change Static State");
            _item.gameObject.isStatic = evt.newValue;
        }
    }
}
