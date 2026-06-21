using Assets.Scripts.Interactable_Items.Rooms;
using Assets.UI_Toolkit.Editor.Levels.Items;
using Items;
using LevelExplorer;
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
        EntrancesList entrancesList;

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
            Add(entrancesList = new());
        }


        TextField roomName;
        ObjectField meshField;
        Button meshFixButton;
        Button saveButton;
        Vector3Field centerOffset;

        void RoomInfo()
        {
            VisualElement row = new() { style = { flexDirection = FlexDirection.Row } };
            Add(row);
            row.Add(roomName = new("Room Name") { style = {flexGrow = 1}});
            row.Add(saveButton = new() { text = "Save"});
            saveButton.clicked += ChangeRoomName;
            
            Add(meshField = new() { label = "Mesh", objectType = typeof(Mesh)});
            Add(centerOffset = new() { label = "Center"});

            Add(meshFixButton = new() { text = "Fix Colliders" });
            meshFixButton.clicked += () => FixColliders();

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

            entrancesList.Bind(
                SelectedRoom,
                LevelExplorer.LevelEditor.LevelData);


            roomName.UnregisterValueChangedCallback(TextChange);
            roomName.SetValueWithoutNotify(selectedRoom.name);
            roomName.RegisterValueChangedCallback(TextChange);

            if(selectedRoom.TryGetComponent<MeshFilter>(out MeshFilter filter))
            {
                meshField.UnregisterValueChangedCallback(MeshChanged);
                FixColliders(false);
                meshField.SetValueWithoutNotify(filter.sharedMesh);
                meshField.RegisterValueChangedCallback(MeshChanged);
            }

            SerializedObject room = new(selectedRoom);
            centerOffset.Unbind();
            centerOffset.UnregisterValueChangedCallback(MoveCenter);
            centerOffset.BindProperty(room.FindProperty(nameof(Room.centerOffset)));
            centerOffset.RegisterValueChangedCallback(MoveCenter);
        }

        private void MoveCenter(ChangeEvent<Vector3> evt) => Center(evt.newValue);
        void Center(Vector3 center) => selectedRoom.Colliders.transform.localPosition = center;

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
            FixColliders();
        }

        void FixColliders(bool move = true)
        {
            if(selectedRoom.TryGetComponent<MeshFilter>(out MeshFilter filter))
            {
                Mesh mesh = filter.sharedMesh;
                MoveCollider(selectedRoom.Walls, "Walls");
                MoveCollider(selectedRoom.Floors, "Floor");
                CreateFloor(mesh);
                CreateWalls(mesh);
                Center(selectedRoom.centerOffset);

                EditorUtility.SetDirty(selectedRoom);
                //Scene scene = EditorSceneManager.GetSceneByPath(RoomScene);
                LevelEditor.SaveScenes();// EditorSceneManager.SaveOpenScenes();
            }
        }

        void MoveCollider(Transform querryTransform, string name)
        {
            if (!querryTransform)
            {
                GameObject wallColliders = new();
                wallColliders.transform.parent = selectedRoom.Colliders;
                querryTransform = wallColliders.transform;
            }
            if (querryTransform.gameObject.name != name)
                querryTransform.gameObject.name = name;
            querryTransform.localPosition = new();
        }

        void CreateFloor(Mesh mesh)
        {
            Transform floor = selectedRoom.Floors;
            if(floor != null)
            {
                Vector3 size = mesh.bounds.size;
                BoxCollider floorCollider;
                if (!floor.TryGetComponent<BoxCollider>(out floorCollider))
                {
                    floorCollider = floor.gameObject.AddComponent<BoxCollider>();
                }
                floorCollider.size = new(size.x, 0.2f, size.z);
                floor.localPosition = new(
                    0, 
                    (mesh.bounds.center.y / 2) - (size.y / 2), 
                    0);
            }
        }

        void CreateWalls(Mesh mesh)
        {
            Transform walls = selectedRoom.Walls;
            walls.position = new(walls.position.x, selectedRoom.Floors.position.y + 0.5f, walls.position.z);
            Vector3 size = mesh.bounds.size;
            for (int i = 0; i < 4; i++)
            {
                if (walls.childCount <= i)
                {
                    GameObject obj = new("Wall " + i);
                    obj.AddComponent<BoxCollider>();
                    obj.transform.parent = walls;

                    Transform wall = walls.GetChild(i);
                    BoxCollider collider = wall.GetComponent<BoxCollider>();

                    switch (i)
                    {
                        case 0:
                            MoveWall(wall, collider, new(size.x, 1, 0.2f), new(0, -1, size.z / 2));
                            break;
                        case 1:
                            MoveWall(wall, collider, new(0.2f, 1, size.z), new(size.x / 2, -1, 0));
                            break;
                        case 2:
                            MoveWall(wall, collider, new(size.x, 1, 0.2f), new(0, -1, -size.z / 2));
                            break;
                        case 3:
                            MoveWall(wall, collider, new(0.2f, 1, size.z), new(-size.x / 2, -1, 0));
                            break;
                    }
                }
            }
        }

        void MoveWall(Transform wall, BoxCollider collider, Vector3 size, Vector3 position)
        {
            collider.size = size;
            wall.localPosition = position;
        }


        public void ClearItem()
        {
            style.display = DisplayStyle.None;
        }
    }
}
