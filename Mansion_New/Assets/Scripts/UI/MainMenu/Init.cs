using Assets.Scripts.Interactable_Items.Rooms;
using Rooms;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
    [SerializeField] string mainMenu = "Main Menu";
    [SerializeField] bool gameInit = true;

    void Awake()
    {
        AddressableSceneManager.LoadScene(
            mainMenu, 
            SceneType.Menu, 
            null, 
            async (scene) =>
            {
                LevelSelector menu = scene.Scene.GetRootGameObjects()[1].GetComponent<LevelSelector>();
                if(!gameInit)
                {
                    menu.LoadGame();
                }
                await SceneManager.UnloadSceneAsync(0);
            });
    }
}
