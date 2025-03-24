using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
	[SerializeField] UIDocument loadingScreen;
	[SerializeField] UIDocument document;
	[SerializeField] List<SceneAsset> loadableScenes;
	[SerializeField] InputAction useAction;

	[HideInInspector]public AsyncOperationHandle<SceneInstance> unloadMainMenu;
	private void Awake()
	{
		ListView menuList = document.rootVisualElement.Q<ListView>("Menu");
		menuList.bindItem = (el, i) =>
		{
			el.Q<Button>().text = loadableScenes[i].name;
			el.Q<Button>().RegisterCallbackOnce<ClickEvent>((_) => StartCoroutine(LoadRoom(loadableScenes[i].name)));
		};
		menuList.itemsSource = loadableScenes;
	}

	IEnumerator LoadRoom(string roomToLoad)
	{
		loadingScreen.enabled = true;
		document.enabled = false;
		ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();

		AsyncOperationHandle<SceneInstance> playerLoad = Addressables.LoadSceneAsync("Player", UnityEngine.SceneManagement.LoadSceneMode.Additive, false);
		while (!playerLoad.IsDone)
		{
			progressBar.value = playerLoad.PercentComplete / 2;
			yield return new();
		}
		if (playerLoad.Status == AsyncOperationStatus.Succeeded)
		{
			progressBar.value = 0.5f;
			AsyncOperationHandle<SceneInstance> roomLoad = Addressables.LoadSceneAsync(roomToLoad, UnityEngine.SceneManagement.LoadSceneMode.Additive, false);
			while (!roomLoad.IsDone)
			{
				progressBar.value = roomLoad.PercentComplete / 2 + 0.5f;
				yield return new();
			}
			if (roomLoad.Status == AsyncOperationStatus.Succeeded)
			{
				progressBar.value = 1;
				yield return roomLoad.Result.ActivateAsync();
				yield return playerLoad.Result.ActivateAsync();
				progressBar.title = "Loaded";
				useAction.Enable();
				useAction.performed += (_) => UnloadMainMenu();
			}
		}
	}

	private void UnloadMainMenu()
	{
		useAction.Disable();
		Addressables.UnloadSceneAsync(unloadMainMenu, UnityEngine.SceneManagement.UnloadSceneOptions.None);
	}
}
