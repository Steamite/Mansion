using Rooms;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class Init : MonoBehaviour
{
#pragma warning disable UDR0001 // Domain Reload Analyzer
    public static string toLoad;
#pragma warning restore UDR0001 // Domain Reload Analyzer
    [SerializeField] string player = "Player";
    [SerializeField] string mainMenu = "Main Menu";
    [SerializeField] bool gameInit = true;
    IEnumerator Start()
    {
        Room.loadedScenes = new();
        //WebGLInput.stickyCursorLock = false;
        //Application.targetFrameRate = 60;
        //QualitySettings.vSyncCount = 1;
        toLoad = gameInit ? mainMenu : player;
        AsyncOperationHandle<SceneInstance> initialLoad =
            Addressables.LoadSceneAsync(toLoad, UnityEngine.SceneManagement.LoadSceneMode.Single, false);

        Debug.Log("adsadasd 1");
        yield return initialLoad;
        ActivateScene(initialLoad);
        Debug.Log("adsadasd 2:" + initialLoad.Status);
    }

    /// <summary>
    /// Loads the main menu or the level itself.
    /// </summary>
    /// <param name="initialLoad"></param>
    async void ActivateScene(AsyncOperationHandle<SceneInstance> initialLoad)
    {
        if (initialLoad.Status == AsyncOperationStatus.Succeeded)
        {
            SceneInstance instance = initialLoad.Result;
            await initialLoad.Result.ActivateAsync();
            MainMenu menu = instance.Scene.GetRootGameObjects()[1].GetComponent<MainMenu>(); // GameObject.Find("Main Menu").GetComponent<MainMenu>();
            menu.unloadMainMenu = instance;
            if (toLoad == "Player")
            {
                menu.InitLoad();
            }
        }
    }
}
