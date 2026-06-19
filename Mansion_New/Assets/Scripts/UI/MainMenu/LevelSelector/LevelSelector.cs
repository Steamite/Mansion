using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.MainMenu;
using Assets.Scripts.UI.MainMenu.New;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class LevelSelector : BaseSelector
{
    [SerializeField] List<string> loadableScenes;
    private void Awake()
    {
        ShowUI();    
    }

    protected override void ShowUI()
    {
        base.ShowUI();
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
