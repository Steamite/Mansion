using Assets.Scripts.Interactable_Items.Rooms;
using Assets.UI_Toolkit.Editor.Levels;
using Importer.Tabs;
using Items;
using NUnit.Framework;
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
        
        public static SceneTemplateAsset LightTemplate 
            => AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>("Assets/Scenes/Template/Lighting.scenetemplate");
        public static SceneTemplateAsset SceneTemplate 
            => AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>("Assets/Scenes/Template/Room.scenetemplate");
        
        
        
        #region Variables
        AddressableAssetSettings settings;
        LevelList levelList;

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
            levelList = new LevelList();
            rootVisualElement.Add(levelList);
            levelList.LoadData();
            return;

            #endregion
        }
        #endregion
        private void OnEnable()
        {
            // Hook into the Scene View when the window is opened
            SceneView.duringSceneGui -= FakeOnSceneGUI;
            SceneView.duringSceneGui += FakeOnSceneGUI;
        }

        private void OnDisable()
        {
            // ALWAYS unhook when the window closes to prevent memory leaks 
            // and ghost handles left behind in the Scene View
            SceneView.duringSceneGui -= FakeOnSceneGUI;
        }

        private void FakeOnSceneGUI(SceneView sceneView)
        {
            if (levelList?.SelectedLevel != null)
            {
                SerializedObject obj = new SerializedObject(levelList.SelectedLevel);

                Handles.color = Color.red;
                Handles.Button(
                    obj.FindProperty(nameof(LevelData.spawn)).vector3Value, 
                    Quaternion.identity, 
                    0.5f, 
                    0.5f, 
                    Handles.SphereHandleCap);

                Vector3 vec = Handles.PositionHandle(
                    obj.FindProperty(nameof(LevelData.spawn)).vector3Value, 
                    Quaternion.identity);
                if (vec != obj.FindProperty(nameof(LevelData.spawn)).vector3Value)
                {
                    obj.FindProperty(nameof(LevelData.spawn)).vector3Value 
                        = vec;
                    obj.ApplyModifiedProperties();
                }
            }
            
        }
    }
}