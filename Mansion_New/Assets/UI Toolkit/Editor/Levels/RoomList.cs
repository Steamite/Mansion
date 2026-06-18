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

namespace Assets.UI_Toolkit.Editor.Levels
{
    [UxmlElement]
    public partial class RoomList : VisualElement
    {
        SceneList _sceneExplorer;
        
        Room selectedRoom;
        string openedScene;
        string roomScene => $"{LevelEditor.LevelEditor.LEVEL_SCENE_PATH}{_sceneExplorer.SelectedLevel}/{openedScene}.unity";

        List<InteractableItem> itemsOnScene;
        ListView view;

        public RoomList(){}
        public RoomList(SceneList sceneExplorer)
        {
            _sceneExplorer = sceneExplorer;
            
            style.display = DisplayStyle.None;
            style.borderTopColor = Color.gray;
            style.borderTopWidth = 5;
            style.paddingTop = 10;

            RoomInfo();
            InteractableList();
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


        void InteractableList()
        {
            Add(view = new());
            view.InitStyles("Interactables");
            view.fixedItemHeight = 250;
            view.makeItem = () => new ItemEditor();
            view.bindItem = (e, i) => (e as ItemEditor).Bind(itemsOnScene[i]);
            view.onAdd = (_) =>
            {
                GameObject item = new(
                    "", 
                    typeof(MeshFilter), typeof(MeshRenderer), 
                    typeof(TextItem), typeof(BoxCollider));
                item.transform.parent = selectedRoom.Interactables;
                item.transform.SetLocalPositionAndRotation(
                    new(0, 0, 0), 
                    Quaternion.Euler(0, 0, 0));

                EditorSceneManager.SaveScene(
                    EditorSceneManager.GetSceneByPath(roomScene));

                Reload();
            };

            view.onRemove = (_) =>
            {
                int i = view.selectedIndex;
                if (i < 0 || i > itemsOnScene.Count)
                    return;

                selectedRoom.Interactables.GetChild(i).gameObject.SetActive(false);
                EditorSceneManager.SaveScene(
                    EditorSceneManager.GetSceneByPath(roomScene));
                Reload();
            };
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
            itemsOnScene = selectedRoom.Interactables
                .GetComponentsInChildren<InteractableItem>()
                .ToList();

            view.itemsSource = itemsOnScene;
            view.RefreshItems();

            roomName.UnregisterValueChangedCallback(TextChange);
            roomName.value = selectedRoom.name;
            roomName.RegisterValueChangedCallback(TextChange);

            
            meshField.UnregisterValueChangedCallback(MeshChanged);
            meshField.value = selectedRoom.GetComponent<MeshFilter>().sharedMesh;
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
            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath(roomScene));

            string path = $"{LevelEditor.LevelEditor.LEVEL_SCENE_PATH}{_sceneExplorer.SelectedLevel.WorldName}/{openedScene}.unity";

            AssetDatabase.RenameAsset(path, $"{val}.unity");

            openedScene = val;
            AssetDatabase.SaveAssets();
            saveButton.enabledSelf = false;
        }

        void MeshChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Mesh mesh = evt.newValue as Mesh;
            Material[] materials = FBXMeshMaterialMapper.GetMeshMaterials(mesh);
            selectedRoom.GetComponent<MeshFilter>().sharedMesh = mesh;
            selectedRoom.GetComponent<MeshRenderer>().sharedMaterials = materials;
            
            EditorUtility.SetDirty(selectedRoom);
            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath(roomScene));
        }

        public void ClearItem()
        {
            style.display = DisplayStyle.None;
        }
    }
}
