using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class Init : MonoBehaviour
{
    IEnumerator Start()
    {
		AsyncOperationHandle<SceneInstance> initialLoad = 
			Addressables.LoadSceneAsync("Player", UnityEngine.SceneManagement.LoadSceneMode.Single, false);
		
		
		yield return initialLoad;
		if (initialLoad.Status == AsyncOperationStatus.Succeeded)
			yield return initialLoad.Result.ActivateAsync();
	}
}
