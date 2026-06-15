#if UNITY_EDITOR
using System.CodeDom;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


/// <summary>Editor shorcuts for quickly switching levels(only in edit mode).</summary>
[InitializeOnLoad]
public class SceneLoadingShortucts : MonoBehaviour
{
    const string SCENE_PATH = "Assets/Scenes/";
    const string MASTER_PATH = SCENE_PATH + "Master/";
    const string ROOM_PATH = SCENE_PATH + "Room Scenes/";

    const string TOGGLE_PATH = "Custom Editors/Load/Load ^l";
    const string LOAD_PREF = "Load";
    
    /// <summary>
    /// Needs to be initialized to register the event.
    /// </summary>
    static SceneLoadingShortucts()
    {
        //GetSettings();
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem(TOGGLE_PATH, priority = -50)]
    static void Toggle()
    {
        bool loadState = !EditorPrefs.GetBool(LOAD_PREF, false);
        EditorPrefs.SetBool("Load", loadState);//.Load = !activeLoadConfig.Load;
        Debug.Log($"Loading: {loadState}");
    }

    [MenuItem(TOGGLE_PATH, true)]
    static bool ToggleCheck()
    {
        Menu.SetChecked(TOGGLE_PATH, EditorPrefs.GetBool(LOAD_PREF, false));
        return true;
    }

    /// <summary>
    /// If the scene is played from "Level" scene, then initializes all previus parts.
    /// </summary>
    /// <param name="state">New state.</param>
    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        bool load = !EditorPrefs.GetBool(LOAD_PREF, false);
        if (load)
            return;
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            string activeSceneName = EditorSceneManager.GetActiveScene().name;
            if (activeSceneName != "Init")
            {
                activeSceneName = EditorSceneManager.GetActiveScene().path;
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene($"{SCENE_PATH}Init/Init.unity");
                EditorApplication.EnterPlaymode();
                File.WriteAllText($"{Application.persistentDataPath}/openScene.txt", activeSceneName);
            }
            else
            {
                File.Delete($"{Application.persistentDataPath}/openScene.txt");
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (File.Exists($"{Application.persistentDataPath}/openScene.txt"))
                EditorSceneManager.OpenScene($"{File.ReadAllText($"{Application.persistentDataPath}/openScene.txt")}");
        }
    }

    [MenuItem("Custom Editors/Load/Init _F1", priority = 0)]
    static void LoadOpenScene()
    {
        if (EditorSceneManager.GetActiveScene().name != "Init")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene($"{SCENE_PATH}Init/Init.unity");
        }
    }
    [MenuItem("Custom Editors/Load/Main Menu _F2", priority = 1)]
    static void LoadMainMenu()
    {
        if (EditorSceneManager.GetActiveScene().name != "Main Menu")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene($"{SCENE_PATH}Init/Main Menu.unity");
        }
    }
    [MenuItem("Custom Editors/Load/Player _F3", priority = 1)]
    static void LoadPlayer()
    {
        if (EditorSceneManager.GetActiveScene().name != "Player")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene($"{MASTER_PATH}Player.unity");
        }
    }
    [MenuItem("Custom Editors/Load/Interact _F4", priority = 2)]
    static void LoadInteract()
    {
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene($"{MASTER_PATH}Interact.unity");
        EditorSceneManager.OpenScene($"{MASTER_PATH}Player.unity", OpenSceneMode.Additive);
    }

    [MenuItem("Custom Editors/Load/Room A _F5", priority = 3)]
    static void LoadRoom()
    {
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene($"{ROOM_PATH}Prototype.unity");
        EditorSceneManager.OpenScene($"{MASTER_PATH}Player.unity", OpenSceneMode.Additive);
    }
    [MenuItem("Custom Editors/Load/VR Player _F6", priority = 3)]
    static void LoadVRPlayer()
    {
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene($"{ROOM_PATH}VR Test.unity");
        EditorSceneManager.OpenScene($"{MASTER_PATH}Player VR.unity", OpenSceneMode.Additive);
    }
}
#endif