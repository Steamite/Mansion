using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

[UxmlElement]
public partial class KeybindOverview : VisualElement
{
    const string MOUSE_PATH = "MouseIcon";

    public KeybindOverview()
    {

    }
    public KeybindOverview(MonoBehaviour loadingScreen)
    {

    }
    public void LoadKeybinds()
    {
        style.flexDirection = FlexDirection.RowReverse;
        VisualElement element = new() { style = { flexDirection = FlexDirection.Column } };
        AddItemText(element, new() { { "W", "Dopředu" }, { "S", "Dozadu" }, { "A", "Doprava" }, { "D", "Doleva" }, { "C", "Skrčit" } }, "Pohyb");
        //yield return AddItemImage(element, new() { { MOUSE_PATH, "Otáčeni" } }, "Kamera");
        Add(element);

        element = new() { style = { flexDirection = FlexDirection.Column, marginRight = 25 } };
        AddItemText(element, new() { { "E", "Prohlédnout" } }, "Interakce");
        Add(element);
    }

    void AddItemText(VisualElement _parent, Dictionary<string, string> controls, string title)
    {
        _parent.Add(new Label(title) { style = { unityTextAlign = TextAnchor.MiddleCenter, marginTop = 50 } });
        foreach (var key in controls.Keys)
        {
            VisualElement element = MakeItem(true);
            BindItem(element, key, controls[key]);
            _parent.Add(element);
        }
    }

    IEnumerator AddItemImage(VisualElement _parent, Dictionary<string, string> controls, string title)
    {
        _parent.Add(new Label(title) { style = { unityTextAlign = TextAnchor.MiddleCenter, marginTop = 50 } });
        foreach (var key in controls.Keys)
        {
            VisualElement element = MakeItem(false);
            AsyncOperationHandle<Texture2D> operation = Addressables.LoadAssetAsync<Texture2D>(key);
            yield return operation;
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                BindItem(element, operation.Result, controls[key]);
                _parent.Add(element);
            }
        }
    }

    VisualElement MakeItem(bool textIcon)
    {
        VisualElement element = new() { name = "control-view" };
        VisualElement icon = new() { name = "icon" };
        if (textIcon)
            icon.Add(new Label() { name = "icon-label" });
        else
            icon.Add(new VisualElement { name = "icon-label" });
        element.Add(icon);
        element.Add(new Label() { name = "action-label" });
        return element;
    }

    void BindItem(VisualElement el, string control, string name)
    {
        (el[0][0] as Label).text = control;
        (el[1] as Label).text = name;
    }
    void BindItem(VisualElement el, Texture2D control, string name)
    {
        el[0][0].style.backgroundImage = control;
        (el[1] as Label).text = name;
    }
}