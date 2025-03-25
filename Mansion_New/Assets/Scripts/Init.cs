using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class Init : MonoBehaviour
{
    public static string toLoad;
    [SerializeField] string player = "Player";
    [SerializeField] string mainMenu = "Main Menu";
    [SerializeField] bool gameInit = true;
    IEnumerator Start()
    {
        toLoad = gameInit ? mainMenu : player;
        AsyncOperationHandle<SceneInstance> initialLoad = 
			Addressables.LoadSceneAsync(toLoad, UnityEngine.SceneManagement.LoadSceneMode.Single, false);
		
		yield return initialLoad;
        ActivateScene(initialLoad);
    }
    async void ActivateScene(AsyncOperationHandle<SceneInstance> initialLoad)
	{
        if (initialLoad.Status == AsyncOperationStatus.Succeeded)
        {
            SceneInstance instance = initialLoad.Result;
            await initialLoad.Result.ActivateAsync();
            if (toLoad == "Main Menu")
                GameObject.Find("Main Menu").GetComponent<MainMenu>().unloadMainMenu = instance;
            else
                GameObject.FindFirstObjectByType<PlayerMovement>().Activate();
        }
    }
}
