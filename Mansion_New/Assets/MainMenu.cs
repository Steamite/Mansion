using System.Collections;
using System.Collections.Generic;
using Player;
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
	[SerializeField] List<string> loadableScenes;
	[SerializeField] InputAction useAction;

	[HideInInspector]public SceneInstance unloadMainMenu;
	private async void Awake()
	{
		document.visualTreeAsset = await Addressables.LoadAssetAsync<VisualTreeAsset>("MainMenuDoc").Task;
		ListView menuList = document.rootVisualElement.Q<ListView>("List");
		menuList.bindItem = (el, i) =>
		{
			el.Q<Button>().text = loadableScenes[i];
			el.Q<Button>().RegisterCallbackOnce<ClickEvent>((_) => StartCoroutine(LoadRoom(loadableScenes[i])));
		};
		menuList.itemsSource = loadableScenes;
	}

	IEnumerator LoadRoom(string roomToLoad)
	{
		loadingScreen.enabled = true;
		document.enabled = false;
		ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();

        AsyncOperationHandle<SceneInstance> roomLoad = Addressables.LoadSceneAsync(roomToLoad, UnityEngine.SceneManagement.LoadSceneMode.Additive, false);
        while (!roomLoad.IsDone)
        {
            progressBar.value = roomLoad.PercentComplete / 2;
            yield return null;
        }
        if (roomLoad.Status == AsyncOperationStatus.Succeeded)
        {
            yield return roomLoad.Result.ActivateAsync();
            progressBar.value = 0.5f;

            AsyncOperationHandle<SceneInstance> playerLoad = Addressables.LoadSceneAsync("Player", UnityEngine.SceneManagement.LoadSceneMode.Additive, false);
            while (!playerLoad.IsDone)
            {
                progressBar.value = playerLoad.PercentComplete / 2;
                yield return null;
            }
            if (playerLoad.Status == AsyncOperationStatus.Succeeded)
            {
                progressBar.value = 1;
                progressBar.title = "Loaded";
                yield return playerLoad.Result.ActivateAsync();
				VisualElement l = loadingScreen.rootVisualElement.Q<Label>("Label");
				l.RegisterCallback<TransitionEndEvent>((_) => ToggleTransition((VisualElement)_.target));
				ToggleTransition(l);
                useAction.Enable();
                useAction.performed += (_) => UnloadMainMenu();
            }
        }
	}

	private void UnloadMainMenu()
	{
		useAction.Disable();
		Addressables.UnloadSceneAsync(unloadMainMenu, UnityEngine.SceneManagement.UnloadSceneOptions.None);
        GameObject.FindFirstObjectByType<PlayerMovement>().Activate();
	}

	void ToggleTransition(VisualElement l)
	{
		l.ToggleInClassList("disabledText");
	}
}
