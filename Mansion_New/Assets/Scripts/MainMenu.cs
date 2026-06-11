using Player;
using Rooms;
using System.Collections;
using System.Collections.Generic;
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

    [HideInInspector] public SceneInstance unloadMainMenu;
    bool canExit = false;
    private void Awake()
    {
        Debug.Log("Opened Main Menu");
        ListView menuList = document.rootVisualElement.Q<ListView>("List");
        menuList.bindItem = (el, i) =>
        {
            el.Q<Button>().text = loadableScenes[i];
            el.Q<Button>().RegisterCallbackOnce<ClickEvent>((_) => LoadRoomByIndex(i));
        };
        menuList.itemsSource = loadableScenes;
    }

    public void LoadRoomByIndex(int loadableIndex = 0)
    {
        LoadRoom(loadableScenes[loadableIndex]);
    }

    void ShowControls()
    {
        VisualElement controls = loadingScreen.rootVisualElement.Q<VisualElement>("ContolsOverview");
        KeybindOverview a;
        controls.Add(a = new KeybindOverview());
        a.LoadKeybinds();
    }

    void LoadRoom(string roomName)
    {
        loadingScreen.enabled = true;
        document.enabled = false;
        ShowControls();

        AsyncOperationHandle<SceneInstance> roomLoad = Addressables.LoadSceneAsync(roomName, UnityEngine.SceneManagement.LoadSceneMode.Additive, false);
        StartCoroutine(WaitForRoomLoad(roomLoad, roomName));   
    }


    IEnumerator WaitForRoomLoad(AsyncOperationHandle<SceneInstance> roomLoad, string roomName)
    {
        ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();

        while (!roomLoad.IsDone)
        {
            progressBar.value = roomLoad.PercentComplete / 2;
            yield return null;
        }

        if (roomLoad.Status == AsyncOperationStatus.Succeeded)
        {
            Room.loadedScenes = new() { { roomName, roomLoad } };
            yield return roomLoad.Result.ActivateAsync();
            progressBar.value = 0.5f;

            yield return WaitForPlayerLoad(roomLoad, roomName, progressBar);
        }
    }

    IEnumerator WaitForPlayerLoad(AsyncOperationHandle<SceneInstance> roomLoad, string roomName, ProgressBar progressBar)
    {
        AsyncOperationHandle<SceneInstance> playerLoad = 
            Addressables.LoadSceneAsync(
                "Player", 
                UnityEngine.SceneManagement.LoadSceneMode.Additive, false);

        while (!playerLoad.IsDone)
        {
            progressBar.value = 0.5f + playerLoad.PercentComplete / 2;
            yield return null;
        }

        if (playerLoad.Status == AsyncOperationStatus.Succeeded)
        {
            progressBar.value = 1;
            progressBar.title = "Načteno";//"Loaded";

            yield return playerLoad.Result.ActivateAsync();

            VisualElement l = loadingScreen.rootVisualElement.Q<Label>("Label");
            l.RegisterCallback<TransitionEndEvent>(
                (_) => ToggleTransition((VisualElement)_.target));
            ToggleTransition(l);
            
            canExit = true;
            useAction.Enable();
            useAction.performed += (_) => UnloadMainMenu();
        }
    }


    private void UnloadMainMenu()
    {
        if (canExit == false)
            return;

        canExit = false;
        useAction.Disable();
        Addressables.UnloadSceneAsync(unloadMainMenu, UnityEngine.SceneManagement.UnloadSceneOptions.None);
        GameObject.FindAnyObjectByType<PlayerMovement>().Activate();
    }

    void ToggleTransition(VisualElement l)
    {
        l.ToggleInClassList("disabledText");
    }
}
