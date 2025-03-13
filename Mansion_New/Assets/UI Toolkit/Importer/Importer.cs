using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Importer : EditorWindow
{
    // Paths
    string TEXT_PATH = Application.streamingAssetsPath + "\\TEXT\\";
    string PDF_PATH = Application.streamingAssetsPath + "\\PDF\\";
    const string BCK_PATH = "\\BCK\\";

    // UI Toolkit element
	[SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
	VisualElement win;

	ListView textList;
	TextField textField;
    TextField nameField;

    Action<string> stringAction;

	int cursorIndex;
	int selectedIndex;

    [MenuItem("Window/UI Toolkit/Importer _1")]
    public static void ShowExample()
    {
        Importer wnd = GetWindow<Importer>();
        wnd.titleContent = new GUIContent("Importer");
    }

    public void CreateGUI()
    {
		#region Base
		minSize = new(800, 800);

		VisualElement root = rootVisualElement;

        VisualElement doc = m_VisualTreeAsset.Instantiate();
        doc.style.flexGrow = 1;
        root.Add(doc);
		#endregion

		#region Window
		win = doc.Q<VisualElement>("TextDialog");
        nameField = win.Q<TextField>("EntryName");
		win.Q<Button>("Save").RegisterCallback<ClickEvent>((_) =>
        {
            if (nameField.text.Length > 0)
            {
                stringAction(nameField.text);
                win.style.display = DisplayStyle.None;
            }
        });
		win.Q<Button>("Cancel").RegisterCallback<ClickEvent>((_) => win.style.display = DisplayStyle.None);
		#endregion

		InitTextList(doc, win);
		InitTextControls(doc, win);

		#region PDF

		#endregion
	}

	private void OnGUI()
	{
		if (textField.textSelection.HasSelection())
			selectedIndex = textField.selectIndex;
		cursorIndex = textField.cursorIndex;
	}
	void InitTextList(VisualElement doc, VisualElement win)
    {
		List<string> files = new();
		foreach (var a in Directory.GetFiles(TEXT_PATH, "*.txt"))
		{
			files.Add(a);
		}

		textField = doc.Q<TextField>("TextContent");
		textList = doc.Q<Tab>("Text").Q<ListView>("List");
		textList.itemsSource = files;

		textList.onAdd = (view) =>
		{
			doc.Q<VisualElement>("TextDialog").style.display = DisplayStyle.Flex;
			win.Q<TextField>("EntryName").value = "";

			stringAction = (s) =>
			{
				s = TEXT_PATH + s + ".txt";
				File.WriteAllText(s, "Dummy text");
				view.itemsSource.Add(s);
				view.Rebuild();
			};
		};

		textList.onRemove = (_) =>
		{
			if (textList.selectedIndex != -1 &&
				EditorUtility.DisplayDialog("Delete string entry",
				$"Are you sure you want to delete entry: {textList.selectedItem}",
				"confirm", "cancel"))
			{
				string fileName = Path.GetFileName((string)textList.selectedItem);
				File.Move((string)textList.selectedItem, ((string)textList.selectedItem).Replace(fileName, "") + BCK_PATH + fileName);
				File.Move((string)textList.selectedItem + ".meta", ((string)textList.selectedItem).Replace(fileName, "") + BCK_PATH + fileName + ".meta");
				textList.itemsSource.RemoveAt(textList.selectedIndex);
				textList.Rebuild();
			}
		};

		textList.bindItem = (element, id) =>
		{
			element.Q<Label>().text = Path.GetFileName(textList.itemsSource[id].ToString());
		};

		textList.selectionChanged += newSelection =>
		{
			textField.value = File.ReadAllText((string)textList.selectedItem);
		};

	}

	void InitTextControls(VisualElement doc, VisualElement win)
	{
		doc.Q<Button>("Link").RegisterCallback<ClickEvent>((_) =>
		{
			if(selectedIndex - cursorIndex != 0)
			{
				stringAction = (linkAddr) =>
				{
					int start = Math.Min(selectedIndex, cursorIndex);
					int end = Math.Max(selectedIndex, cursorIndex);

					string s = textField.text;

					s = s.Insert(end, @"</a></b></color>");
					s = s.Insert(start, $"<color=\"blue\"><b><a href=\"{linkAddr}\">");
					textField.value = s;
				};
				win.style.display = DisplayStyle.Flex;
			}
		});

		doc.Q<Button>("Save").RegisterCallback<ClickEvent>((_) =>
		{
			File.WriteAllText((string)textList.selectedItem, textField.text);
		});

		doc.Q<Button>("ClearLinks").RegisterCallback<ClickEvent>((_) =>
		{
			string s = textField.text;
			s = s.Replace("<color=\"blue\"><b>", "");
			s = s.Replace("</a></b></color>", "");
			int i;
			while ((i = s.IndexOf("<a href")) > -1)
			{
				int j = s.IndexOf("\">");
				s = s.Remove(i, j - i+2);
			}
			textField.value = s;
		});
		textField.RegisterValueChangedCallback<string>((s) => doc.Q<Label>("PreviewText").text = s.newValue);
	}


}
