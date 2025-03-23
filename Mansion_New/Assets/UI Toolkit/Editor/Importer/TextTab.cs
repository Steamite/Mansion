using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Linq;
using System.IO;
using Items;
using System;
using UnityEngine;
using static UnityEditor.Rendering.InspectorCurveEditor;

public class TextTab : ITab
{
	const string TEXT_PATH = "Assets/ItemData/Text/";
	const string BCK_PATH = "BCK/";

	TextField textField;
	public ListView textList;


	public int selectedIndex, cursorIndex;

	AddressableAssetSettings settings;
	AddressableAssetGroup textGroup;

	public TextTab(VisualElement _doc, VisualElement _win, Action<int> choicesUpdate)
	{
		settings = AddressableAssetSettingsDefaultObject.Settings;
		textGroup = settings.FindGroup("Texts");
		InitTextList(_doc, _win, choicesUpdate);
		InitTextButtons(_doc, _win);
	}

	/// <summary>
	/// Adds items source and callbacks to <see cref="textList"/>.
	/// </summary>
	/// <param name="doc">Root of the document.</param>
	void InitTextList(VisualElement _doc, VisualElement _win, Action<int> choicesUpdate)
	{
		textField = _doc.Q<TextField>("TextContent");
		textList = _doc.Q<Tab>("Text").Q<ListView>("List");
		ReloadData();

		textList.onAdd = (view) =>
		{
			_doc.Q<VisualElement>("TextDialog").style.display = DisplayStyle.Flex;
			_win.Q<TextField>("EntryName").value = "";

			Importer.stringAction = (s) =>
			{
				s = TEXT_PATH + s + ".asset";
				if (File.Exists(s))
				{
					EditorUtility.DisplayDialog("Cannot add", "FILE ALREADY EXISTS", "ok");
					return;
				}

				TextData data = ScriptableObject.CreateInstance<TextData>();
				data.content = "";
				AssetDatabase.CreateAsset(data, s);

				string GUId = AssetDatabase.GUIDFromAssetPath(s).ToString();
				settings.CreateOrMoveEntry(GUId, textGroup);
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
				AssetDatabase.SaveAssets();

				view.itemsSource.Add(s);
				view.Rebuild();
				view.SetSelection(0);
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
				// BCK
				File.WriteAllText(
					TEXT_PATH + BCK_PATH + fileName.Replace(".asset", ".txt"),
					AssetDatabase.LoadAssetAtPath<TextData>((string)textList.selectedItem).content);

				// Removing reference from item
				string GUId = AssetDatabase.GUIDFromAssetPath((string)textList.selectedItem).ToString();
				InteractableItem itemAssigned = Importer.items.FirstOrDefault(q => q.sourceObject?.AssetGUID == GUId);
				if (itemAssigned != null)
					itemAssigned.sourceObject = new("");

				// Removing from addressables
				settings.RemoveAssetEntry(GUId);
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, GUId, true);

				AssetDatabase.DeleteAsset((string)textList.selectedItem);

				textList.itemsSource.RemoveAt(textList.selectedIndex);
				textList.Rebuild();
			}
		};

		textList.bindItem = (element, id) =>
		{
			element.Q<Label>().text = Path.GetFileNameWithoutExtension(textList.itemsSource[id].ToString());
		};

		textList.selectionChanged += newSelection =>
		{
			if (textList.selectedIndex > -1)
			{
				textField.value = AssetDatabase.LoadAssetAtPath<TextData>((string)textList.selectedItem).content;
				choicesUpdate(0);
			}
		};
	}


	/// <summary>
	/// Implements functionality for the bottom buttons.
	/// </summary>
	/// <param name="_doc">Root of the document.</param>
	void InitTextButtons(VisualElement _doc, VisualElement _win)
	{
		_doc.Q<Button>("Link").RegisterCallback<ClickEvent>((_) =>
		{
			if (selectedIndex - cursorIndex != 0)
			{
				Importer.stringAction = (linkAddr) =>
				{
					int start = Math.Min(selectedIndex, cursorIndex);
					int end = Math.Max(selectedIndex, cursorIndex);

					string s = textField.text;

					s = s.Insert(end, @"</a></b></color>");
					s = s.Insert(start, $"<color=\"blue\"><b><a href=\"{linkAddr}\">");
					textField.value = s;
				};
				_win.style.display = DisplayStyle.Flex;
			}
		});

		_doc.Q<Button>("Save").RegisterCallback<ClickEvent>((_) =>
		{
			TextData data = AssetDatabase.LoadAssetAtPath<TextData>((string)textList.selectedItem);
			data.content = textField.text;
			EditorUtility.SetDirty(data);
		});

		_doc.Q<Button>("ClearLinks").RegisterCallback<ClickEvent>((_) =>
		{
			string s = textField.text;
			s = s.Replace("<color=\"blue\"><b>", "");
			s = s.Replace("</a></b></color>", "");
			int i;
			while ((i = s.IndexOf("<a href")) > -1)
			{
				int j = s.IndexOf("\">");
				s = s.Remove(i, j - i + 2);
			}
			textField.value = s;
		});
		textField.RegisterValueChangedCallback<string>((s) => _doc.Q<Label>("PreviewText").text = s.newValue);
	}

	public void Clear(out int i, out AddressableAssetGroup g)
	{
		i = Importer.items.FindIndex(q => q.sourceObject.AssetGUID == AssetDatabase.GUIDFromAssetPath((string)textList.selectedItem).ToString());
		g = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Texts");
	}

	public void ReloadData()
	{
		List<string> files = new();
		List<string> availableAssets = settings.FindGroup("Texts").entries.Select(q => q.guid).ToList();
		if (settings.FindGroup(EditorSceneManager.GetActiveScene().name))
			availableAssets.AddRange(settings.FindGroup(EditorSceneManager.GetActiveScene().name).entries.Select(q => q.guid));

		foreach (var GUId in AssetDatabase.FindAssets("t:TextData"))
		{
			if (availableAssets.Contains(GUId))
				files.Add(AssetDatabase.GUIDToAssetPath(GUId));
		}

		textList.itemsSource = files;
	}
	public void SelectTab()
	{
		if(textList.itemsSource.Count > 0)
			textList.SetSelection(0);
	}

	public string LinkEntry()
	{
		return AssetDatabase.GUIDFromAssetPath((string)textList.selectedItem).ToString();
	}

	public void Rename()
	{
		if ((string)textList.selectedItem != "")
		{
			Importer.stringAction = (s) =>
			{
				AssetDatabase.MoveAsset((string)textList.selectedItem, TEXT_PATH + s + ".txt");
				textList.itemsSource[textList.selectedIndex] = TEXT_PATH + s + ".txt";
				textList.RefreshItems();
			};
		}

	}

	/// <summary>
	/// Updates selection indexes.
	/// </summary>
	public void UpdateIndexes()
	{
		if (textField.textSelection.HasSelection())
			selectedIndex = textField.selectIndex;
		cursorIndex = textField.cursorIndex;
	}
}
