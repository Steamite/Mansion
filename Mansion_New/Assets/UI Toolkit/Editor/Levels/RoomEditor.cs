using Assets.Scripts.Interactable_Items.Rooms;
using Assets.UI_Toolkit.Editor.Levels.Items;
using Items;
using LevelEditor;
using Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static FBXMeshMaterialMapper;

namespace Assets.UI_Toolkit.Editor.Levels
{
    [UxmlElement]
    public partial class RoomEditor : VisualElement
    {
        SceneList _sceneExplorer;
        
        Room selectedRoom;
        public Room SelectedRoom => selectedRoom;
        

        string openedScene;
        public string RoomScene => $"{LevelData.LEVEL_SCENE_PATH}{_sceneExplorer.SelectedLevel}/{openedScene}.unity";

        ItemList itemList;

        public RoomEditor(){}
        public RoomEditor(SceneList sceneExplorer)
        {
            _sceneExplorer = sceneExplorer;
            
            style.display = DisplayStyle.None;
            style.borderTopColor = Color.gray;
            style.borderTopWidth = 5;
            style.paddingTop = 10;

            RoomInfo();
            Add(itemList = new(this));
        }


        TextField roomName;
        ObjectField meshField;
        Button saveButton;

        void RoomInfo()
        {
            VisualElement row = new() { style = { flexDirection = FlexDirection.Row } };
            Add(row);
            row.Add(roomName = new("Room Name") { style = {flexGrow = 1}});
            row.Add(saveButton = new() { text = "Save"});
            saveButton.clicked += ChangeRoomName;
            Add(meshField = new() { label = "Mesh", objectType = typeof(Mesh)});
        }

        public void LoadSceneItems(string sceneName, Room room)
        {
            openedScene = sceneName;
            style.display = DisplayStyle.Flex;
            selectedRoom = room;
            Reload();
        }

        void Reload()
        {
            itemList.Load(selectedRoom.Interactables
                .GetComponentsInChildren<InteractableItem>()
                .ToList());

            roomName.UnregisterValueChangedCallback(TextChange);
            roomName.SetValueWithoutNotify(selectedRoom.name);
            roomName.RegisterValueChangedCallback(TextChange);

            
            meshField.UnregisterValueChangedCallback(MeshChanged);
            meshField.SetValueWithoutNotify(selectedRoom.GetComponent<MeshFilter>().sharedMesh);
            meshField.RegisterValueChangedCallback(MeshChanged);
        }

        private void TextChange(ChangeEvent<string> evt)
        {
            saveButton.enabledSelf = evt.newValue != selectedRoom.name;
        }


        void ChangeRoomName()
        {
            string val = roomName.value;
            int x = _sceneExplorer.SelectedLevel.scenes.IndexOf(openedScene);
            if (x == -1)
            {
                throw new ArgumentException("Scene not found");
            }

            _sceneExplorer.SelectedLevel.scenes[x] = val;
            selectedRoom.name = val;
            EditorUtility.SetDirty(_sceneExplorer.SelectedLevel);
            EditorUtility.SetDirty(selectedRoom);
            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath(RoomScene));

            string path = $"{LevelData.LEVEL_SCENE_PATH}{_sceneExplorer.SelectedLevel.WorldName}/{openedScene}.unity";

            AssetDatabase.RenameAsset(path, $"{val}.unity");

            openedScene = val;
            AssetDatabase.SaveAssets();
            saveButton.enabledSelf = false;
        }

        void MeshChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Mesh mesh = evt.newValue as Mesh;
            MeshObjectData meshObject = FBXMeshMaterialMapper.GetMeshMaterials(mesh);
            if (meshObject.meshObject == null)
                return;
            selectedRoom.GetComponent<MeshFilter>().sharedMesh = mesh;
            selectedRoom.GetComponent<MeshRenderer>().sharedMaterials = meshObject.materials;
            selectedRoom.transform.position = meshObject.meshObject.transform.localPosition;

            EditorUtility.SetDirty(selectedRoom);
            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath(RoomScene));
        }

        public void ClearItem()
        {
            style.display = DisplayStyle.None;
        }
    }
}
