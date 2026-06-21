using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.MainMenu;
using Assets.Scripts.UI.MainMenu.New;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class LevelSelectorPlus : BaseSelector
{
    [SerializeField] List<LevelData> levels;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levels = new();
        Addressables.LoadAssetsAsync<LevelData>(
            "Levels",
            (data) => {if (data.active) { levels.Add(data); } }).Completed += (_) =>
            {
                ShowUI();
            };
    }

    protected override void ShowUI()
    {
        base.ShowUI();
        Debug.Log("Opened Main Menu");
        ListView menuList = document.rootVisualElement.Q<ListView>("List");

        menuList.bindItem = (el, i) =>
        {
            el.Q<Button>().text = levels[i].WorldName;
            el.Q<Button>().RegisterCallbackOnce<ClickEvent>((_) => LoadGame(i));
        };
        menuList.itemsSource = levels;
    }

    public void LoadGame(int i = 0)
    {
        document.enabled = false;
        loadingScreen.StartRoomLoad(levels[i]);
    }
}
