using Items;
using System;
using UnityEditor;
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
        EnumField itemType;
        TextField itemName;
        ObjectField itemData;
        Vector3Field position;
        Vector3Field centerOffset;

        Vector2Field radiusRange;
        Vector2Field startRotation;

        Toggle staticObject;

        Button zoomTo;
        InteractableItem _item;

        public ItemEditor()
        {
            Add(itemName = new TextField());
            Add(itemType = new EnumField());
            Add(itemData = new ObjectField());
            Add(radiusRange = new Vector2Field());
            Add(startRotation = new Vector2Field());
            //Add(field = new ObjectField());
            Add(position = new Vector3Field());
            Add(centerOffset = new Vector3Field());

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
            text = item.ItemName;
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
            position.label = "position";
            
            centerOffset.Unbind();
            centerOffset.BindProperty(sItem.FindProperty(nameof(InteractableItem.offset)));
            centerOffset.label = "center";

            //maxDrag.BindProperty(sItem.FindProperty(InteractableItem.MaxDrag));
            radiusRange.Unbind();
            radiusRange.BindProperty(sItem.FindProperty(nameof(InteractableItem.RadiusRange)));
            radiusRange.label = "radius";


            startRotation.Unbind();
            startRotation.BindProperty(sItem.FindProperty(nameof(InteractableItem.startRotation)));
            startRotation.label = "start rotation";


            BindStaticToggle();
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
