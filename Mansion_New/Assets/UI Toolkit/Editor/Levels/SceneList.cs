using Assets.Scripts.Interactable_Items.Rooms;
using LevelEditor;
using NUnit.Framework;
using Rooms;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Splines.ExtrusionShapes;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.Editor.Levels
{
    [UxmlElement]
    public partial class SceneList : VisualElement
    {

        public LevelData SelectedLevel => selectedLevel;
        LevelData selectedLevel;
        LevelList levelList;

        ListView sceneList;

        ObjectField assetData;

        Button saveButton;
        Button loadButton;

        Vector3Field spawnField;

        TextField levelName;

        public RoomEditor roomEditor;

        public SceneList() { }
        public SceneList(LevelList list)
        {
            levelList = list;
            InitLevelInspect();
            InitScenesView();
            Add(roomEditor = new RoomEditor(this));

            style.display = DisplayStyle.None;
            style.borderTopColor = Color.gray;
            style.borderTopWidth = 5;
            style.paddingTop = 10;
        }

        public void LoadLevelData(LevelData newLevelData)
        {
            selectedLevel = newLevelData;
            Reload();
        }

        void Reload()
        {
            style.display = DisplayStyle.Flex;
            levelName.value = selectedLevel.WorldName;
            assetData.value = selectedLevel;

            LevelData data = selectedLevel;
            sceneList.itemsSource = data.scenes;
            sceneList.RefreshItems();

            sceneList.selectedIndex = -1;

            spawnField.Unbind();
            SerializedObject lData = new SerializedObject(selectedLevel);
            spawnField.BindProperty(lData.FindProperty(nameof(LevelData.spawn)));
        }

        private void InitScenesView()
        {
            Add(sceneList = new());
            sceneList.InitStyles("Rooms");

            sceneList.makeItem = () => 
            {
                VisualElement el = new() 
                { 
                    style = 
                    { 
                        flexDirection = FlexDirection.Row, 
                        paddingLeft = 15, 
                        paddingRight = 5, 
                        justifyContent = Justify.SpaceBetween
                    }
                };
                ObjectField field = new ObjectField() 
                { 
                    objectType = typeof(SceneAsset), 
                    style = 
                    { 
                        flexGrow = 1, 
                        marginRight = 35
                    } 
                };
                Toggle toggle = new("");
                el.Add(field);
                el.Add(toggle);
                return el;
            };
            sceneList.bindItem = (e, i) =>
            {
                ObjectField obj = (e[0] as ObjectField);
                string path = $"{LevelEditor.LevelEditor.LEVEL_SCENE_PATH}{selectedLevel.WorldName}/{selectedLevel.scenes[i]}.unity";
                obj.userData = i;
                obj.UnregisterValueChangedCallback(SceneChanged);
                obj.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<SceneAsset>(path));
                obj.RegisterValueChangedCallback(SceneChanged);/*
                obj.UnregisterCallback<FocusInEvent>(RoomFocus);
                obj.RegisterCallback<FocusInEvent>(RoomFocus);*/

                Toggle toggle = (e[1] as Toggle);
                toggle.UnregisterValueChangedCallback(ToggleMainRoom);
                toggle.SetValueWithoutNotify(selectedLevel.initScene == i);
                toggle.userData = i;

                toggle.RegisterValueChangedCallback(ToggleMainRoom);
            };
            sceneList.unbindItem = (e, i) =>
            {
                (e as ObjectField).UnregisterValueChangedCallback(SceneChanged);
            };

            sceneList.onAdd = (_) =>
            {
                LevelData editedLevel = selectedLevel;

                string assetName = $"scene {Guid.NewGuid()}";
                string scenePath = System.IO.Path.Combine(LevelEditor.LevelEditor.LEVEL_SCENE_PATH, editedLevel.WorldName, $"{assetName}.unity");
                SceneTemplateService.Instantiate(
                    LevelEditor.LevelEditor.SceneTemplate,
                    true,
                    scenePath);

                editedLevel.scenes.Add(assetName);

                EditorUtility.SetDirty(editedLevel);
                AssetDatabase.SaveAssets();
                sceneList.RefreshItems();
            };

            sceneList.onRemove = (_) =>
            {
                int i = sceneList.selectedIndex;
                if (i == -1 || i > selectedLevel.scenes.Count)
                    return;

                selectedLevel.scenes.RemoveAt(i);
                EditorUtility.SetDirty(selectedLevel);
                sceneList.RefreshItems();
            };

            sceneList.selectionChanged += (_) => 
            {
                if (sceneList.selectedIndex == -1)
                {
                    roomEditor.ClearItem();
                    return;
                }
                string name = selectedLevel.scenes[sceneList.selectedIndex];
                string path = $"{LevelEditor.LevelEditor.LEVEL_SCENE_PATH}{selectedLevel.WorldName}/{name}.unity";
                Scene s = EditorSceneManager.GetSceneByPath(path);
                if (s == null || s.isLoaded == false)
                    Load();

                Room selectedRoom = EditorSceneManager.GetSceneByPath(path)
                    .GetRootGameObjects()[0]
                    .GetComponent<Room>();
                roomEditor.LoadSceneItems(name, selectedRoom);
                Selection.activeGameObject = selectedRoom.gameObject;
            };
        }
/*
        private void RoomFocus(FocusInEvent evt)
        {
            VisualElement elem = evt.currentTarget as VisualElement;
            int x = (int)elem.userData;
            sceneList.selectedIndex = x;
        }*/

        void ToggleMainRoom(ChangeEvent<bool> evt)
        {
            Toggle toggle = evt.target as Toggle;
            int x = (int)toggle.userData;
            int prev = selectedLevel.initScene;
            selectedLevel.initScene = x;
            EditorUtility.SetDirty(selectedLevel);
            sceneList.RefreshItem(x);
            sceneList.RefreshItem(prev);
        }

        void SceneChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            int i = (int)(evt.target as ObjectField).userData;
            LevelData data = selectedLevel;
            data.scenes[i] = evt.newValue.name;

            string path = AssetDatabase.GetAssetPath(evt.newValue);
            string newPath = $"Assets/Scenes/Levels/{data.WorldName}/{data.scenes[i]}.unity";
            if (path != newPath)
            {
                AssetDatabase.MoveAsset(path, newPath);
            }
        }

        void InitLevelInspect()
        {
            Add(assetData = new("LevelAsset")
            {
                allowSceneObjects = false,
                enabledSelf = false
            });
            VisualElement row = new();
            row.style.flexDirection = FlexDirection.Row;
            Add(row);
            row.Add(levelName = new("Name") { style = {flexGrow = 1}});
            levelName.RegisterValueChangedCallback(q
                => saveButton.enabledSelf = q.newValue != selectedLevel.WorldName);

            row.Add(saveButton = new() { text = "Save" });
            saveButton.clicked += () =>
            {
                string val = levelName.value;
                AssetDatabase.RenameAsset(
                    $"{LevelEditor.LevelEditor.LEVEL_SCENE_PATH}{selectedLevel.WorldName}", 
                    val);

                selectedLevel.WorldName = val;
                EditorUtility.SetDirty(selectedLevel);
                levelList.RefreshItems();
            };
            
            Add(loadButton = new() { text = "Load" });
            loadButton.clicked += Load;


            Add(spawnField = new Vector3Field("SpawnPosition"));
        }
        void Load()
        {
            EditorSceneManager.SaveOpenScenes();
            string FolderPath = $"{LevelEditor.LevelEditor.LEVEL_SCENE_PATH}{selectedLevel.WorldName}/";
            EditorSceneManager.OpenScene($"{FolderPath}Lightning.unity");
            foreach (string sceneToLoad in selectedLevel.scenes)
            {
                EditorSceneManager.OpenScene(
                    $"{FolderPath}{sceneToLoad}.unity",
                    OpenSceneMode.Additive);
            }
        }
    }
}
