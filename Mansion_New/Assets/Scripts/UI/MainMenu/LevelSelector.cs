using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.MainMenu;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] UIDocument document;
    [SerializeField] LoadingScreen loadingScreen;
    
    [SerializeField] List<string> loadableScenes;

    [SerializeField] PanelSettings screenSettings;
    [SerializeField] PanelSettings worldSettings;

    [SerializeField] GameObject mainCamera;

    [SerializeField] List<LevelData> levels;
    private void Awake()
    {
        levels = new();
        Addressables.LoadAssetsAsync<LevelData>(
            "Levels", 
            (data) => levels.Add(data)).Completed += (_) =>
            {
                ShowUI();
            };
    }

    void ShowUI()
    {
        if (loadingScreen.UseVR)
        {
            mainCamera.SetActive(false);

            document.enabled = false;
            document.panelSettings = worldSettings;
            //document.worldSpaceSize = new(1920, 1080); TEST
            document.enabled = true;

            UIDocument doc = loadingScreen.GetComponent<UIDocument>();
            doc.enabled = true;
            doc.panelSettings = worldSettings;
            doc.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
            doc.enabled = false;

            AddressableSceneManager.LoadScene(
                "Player VR",
                SceneType.Player,
                null,
                (scene) =>
                {
                    GameObject[] objs = scene.Scene.GetRootGameObjects();
                    Camera.main.cullingMask = LayerMask.GetMask("UI", "Ignore Raycast");// LayerMask.NameToLayer("UI");
                    objs[1].transform.GetChild(0)
                        .GetComponent<SpriteRenderer>().enabled = false;
                });
        }
        else
        {
            mainCamera.SetActive(true);

            document.panelSettings = screenSettings;
            loadingScreen.GetComponent<UIDocument>().panelSettings = screenSettings;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Debug.Log("Opened Main Menu");
        ListView menuList = document.rootVisualElement.Q<ListView>("List");
        menuList.bindItem = (el, i) =>
        {
            el.Q<Button>().text = loadableScenes[i];
            el.Q<Button>().RegisterCallbackOnce<ClickEvent>((_) => LoadGame(i));
        };
        menuList.itemsSource = loadableScenes;
    }

    public void LoadGame(int i = 0)
    {
        document.enabled = false;
        loadingScreen.StartRoomLoad(loadableScenes[i]);
    }
}
