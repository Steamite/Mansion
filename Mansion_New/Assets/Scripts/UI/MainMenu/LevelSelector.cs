using Assets.Scripts.UI.MainMenu;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] UIDocument document;
    [SerializeField] LoadingScreen loadingScreen;
    
    [SerializeField] List<string> loadableScenes;

    private void Awake()
    {
        ShowUI();
    }

    void ShowUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
