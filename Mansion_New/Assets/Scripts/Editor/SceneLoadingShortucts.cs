#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

/// <summary>Editor shorcuts for quickly switching levels(only in edit mode).</summary>
[InitializeOnLoad]
public class SceneLoadingShortucts : MonoBehaviour
{
    /// <summary>Base path to the scene folde</summary>
    static readonly string scenePath = "Assets/Scenes/Master/";

    /// <summary>
    /// Needs to be initialized to register the event.
    /// </summary>
    static SceneLoadingShortucts()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    /// <summary>
    /// If the scene is played from "Level" scene, then initializes all previus parts.
    /// </summary>
    /// <param name="state">New state.</param>
    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            string activeSceneName;
			if ((activeSceneName = EditorSceneManager.GetActiveScene().name) != "Init")
            {
                activeSceneName = EditorSceneManager.GetActiveScene().path;
				EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene("Assets/Scenes/Init/Init.unity");
                EditorApplication.EnterPlaymode();
                File.WriteAllText($"{Application.persistentDataPath}/openScene.txt", activeSceneName);
            }
            else
            {
                File.Delete($"{Application.persistentDataPath}/openScene.txt");
            }
        }
        else if(state == PlayModeStateChange.EnteredEditMode)
        {
            if(File.Exists($"{Application.persistentDataPath}/openScene.txt"))
                EditorSceneManager.OpenScene($"{File.ReadAllText($"{Application.persistentDataPath}/openScene.txt")}");
        }
    }

    [MenuItem("Custom Editors/Load/Init _F1", priority = 0)]
    static void LoadOpenScene()
    {
        if (EditorSceneManager.GetActiveScene().name != "Init")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene("Assets/Scenes/Init/Init.unity");
        }
    }
	[MenuItem("Custom Editors/Load/Main Menu _F2", priority = 1)]
	static void LoadMainMenu()
	{
		if (EditorSceneManager.GetActiveScene().name != "Main Menu")
		{
			EditorSceneManager.SaveOpenScenes();
			EditorSceneManager.OpenScene("Assets/Scenes/Init/Main Menu.unity");
		}
	}
	[MenuItem("Custom Editors/Load/Player _F3", priority = 1)]
    static void LoadPlayer()
    {
        if (EditorSceneManager.GetActiveScene().name != "Player")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene($"{scenePath}Player.unity");
        }
    }
    [MenuItem("Custom Editors/Load/Interact _F4", priority = 2)]
    static void LoadInteract()
    {
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene($"{scenePath}Interact.unity");
		EditorSceneManager.OpenScene($"{scenePath}Player.unity", OpenSceneMode.Additive);
    }

	[MenuItem("Custom Editors/Load/Room A _F5", priority = 3)]
	static void LoaRoom()
	{
		EditorSceneManager.SaveOpenScenes();
		EditorSceneManager.OpenScene("Assets/Scenes/Rooms/Room A.unity");
		EditorSceneManager.OpenScene($"{scenePath}Player.unity", OpenSceneMode.Additive);
	}
}
#endif