using Assets.Scripts.Interactable_Items.Rooms;
using Assets.UI_Toolkit.Editor.Levels;
using Importer.Tabs;
using Items;
using Rooms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace LevelEditor
{
    /// <summary>
    /// Custom Editor window for managing streaming assets. 
    /// </summary>
    public class LevelEditor : EditorWindow
    {
        static LevelEditor instance;
        public const string LEVEL_DATA_PATH = "Assets/Levels/";
        public const string LEVEL_SCENE_PATH = "Assets/Scenes/Levels/";

        public static SceneTemplateAsset LightTemplate 
            => AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>("Assets/Scenes/Template/Lighting.scenetemplate");
        public static SceneTemplateAsset SceneTemplate 
            => AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>("Assets/Scenes/Template/Room.scenetemplate");
        #region Variables
        AddressableAssetSettings settings;
        ListView scenes;

        #endregion
        #region Init
        [MenuItem("Custom Editors/Levels _2")]
        public static void ShowImporter()
        {
            LevelEditor wnd = GetWindow<LevelEditor>();
            wnd.titleContent = new GUIContent("Level Explorer");
        }

        /// <summary>Inits the document and all of its parts.</summary>
        public void CreateGUI()
        {
            instance = this;
            settings = AddressableAssetSettingsDefaultObject.Settings;
            #region Base
            minSize = new(800, 800);
            LevelList list = new LevelList();
            rootVisualElement.Add(list);
            list.LoadData();
            return;

            #endregion
        }
        #endregion

    }
}