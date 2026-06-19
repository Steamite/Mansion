using Assets.Scripts.Interactable_Items.Rooms;
using LevelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.Editor.Levels
{
    [UxmlElement]
    public partial class LevelList : VisualElement
    {
        List<LevelData> levelData;
        public LevelData SelectedLevel => 
            (levelView.selectedIndex > -1 && levelData.Count > levelView.selectedIndex )
                ? levelData[levelView.selectedIndex] 
                : null;
        ListView levelView;

        public SceneList scenelist;

        public LevelList()
        {
            InitList();
            Add(scenelist = new SceneList(this));
        }
        void InitList()
        {
            levelView = new();
            Add(levelView);
            levelView.InitStyles("Levels");

            levelView.makeItem = () => new Label();//new ObjectField() { objectType = typeof(SceneAsset)};
            levelView.bindItem = (e, i) =>
            {
                (e as Label).text = levelData[i].WorldName;
            };

            levelView.onAdd = (_) =>
            {
                LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
                string name = $"Level {Guid.NewGuid()}";
                newLevel.WorldName = name;
                newLevel.name = name;
                AssetDatabase.CreateAsset(newLevel, Path.Combine(LevelData.LEVEL_DATA_PATH, $"{name}.asset"));
                AssetDatabase.CreateFolder(LevelData.LEVEL_SCENE_PATH, name);
                SceneTemplateService.Instantiate(
                    LevelEditor.LevelEditor.LightTemplate, 
                    true, 
                    Path.Combine(
                        LevelData.LEVEL_SCENE_PATH,
                        name, 
                        "Lightning.unity")
                    );
                LoadData();
            };

            levelView.onRemove = (_) =>
            {
                int i = levelView.selectedIndex;
                if (i == -1 || i > levelData.Count)
                    return;

                levelData[i].active = false;
                LoadData();
            };
            levelView.selectionChanged += 
                (_) => scenelist.LoadLevelData(levelData[levelView.selectedIndex]);
        }


        public void LoadData()
        {
            string[] _levels = 
                Directory.GetFiles(LevelData.LEVEL_DATA_PATH, "*.asset");
            levelData = new();
            foreach (var item in _levels)
            {
                levelData.Add(AssetDatabase.LoadAssetAtPath<LevelData>(item));
            }
            levelData = levelData.Where(q => q.active).ToList();
            levelView.itemsSource = levelData;
            levelView.RefreshItems();
            if (levelData.Count > 0)
                levelView.selectedIndex = 0;
        }

        public void RefreshItems() => levelView.RefreshItems();
    }
}
