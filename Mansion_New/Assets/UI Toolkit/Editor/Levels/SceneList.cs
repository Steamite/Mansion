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

        TextField levelName;

        RoomList roomList;
        public SceneList() { }
        public SceneList(LevelList list)
        {
            levelList = list;
            InitScenesView();
            InitLevelInspect();
            Add(roomList = new RoomList(this));

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
        }

        private void InitScenesView()
        {
            Add(sceneList = new());
            sceneList.InitStyles("Rooms");

            sceneList.makeItem = () => new ObjectField("scene asset:") { objectType = typeof(SceneAsset) };
            sceneList.bindItem = (e, i) =>
            {
                string path = $"{LevelEditor.LevelEditor.LEVEL_SCENE_PATH}{selectedLevel.WorldName}/{selectedLevel.scenes[i]}.unity";
                (e as ObjectField).value = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                (e as ObjectField).userData = i;
                (e as ObjectField).RegisterValueChangedCallback(SceneChanged);
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
                    false,
                    scenePath);

                editedLevel.scenes.Add(assetName);

                EditorUtility.SetDirty(editedLevel);
                AssetDatabase.SaveAssets();
                levelList.RefreshItems();
            };

            sceneList.onRemove = (_) =>
            {
                int i = sceneList.selectedIndex;
                if (i == -1 || i > selectedLevel.scenes.Count)
                    return;

                selectedLevel.scenes.RemoveAt(i);
                EditorUtility.SetDirty(selectedLevel);
                levelList.RefreshItems();
            };

            sceneList.selectionChanged += (_) => 
            {
                if (sceneList.selectedIndex == -1)
                {
                    roomList.ClearItem();
                    return;
                }
                string name = selectedLevel.scenes[sceneList.selectedIndex];
                string path = $"{LevelEditor.LevelEditor.LEVEL_SCENE_PATH}{selectedLevel.WorldName}/{name}.unity";
                Scene s = EditorSceneManager.GetSceneByPath(path);
                if (s == null || s.isLoaded == false)
                    Load();
                roomList.LoadSceneItems(name,
                    EditorSceneManager.GetSceneByPath(path)
                    .GetRootGameObjects()[0]
                    .GetComponent<Room>());
            };
        }

        private void SceneChanged(ChangeEvent<UnityEngine.Object> evt)
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
