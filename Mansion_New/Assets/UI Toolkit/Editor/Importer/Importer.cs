#if UNITY_EDITOR
using Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

/// <summary>
/// Custom Editor window for managing streaming assets. 
/// </summary>
public class Importer : EditorWindow
{
	#region Variables
	/// <summary>UI document containing the window definition</summary>
	[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

	/// <summary>Text field in <see cref="win"/></summary>
	TextField nameField;
	Button clearButton;

	public static List<InteractableItem> items = new();
	public List<int> choiceIds = new();
	public static Action<string> stringAction;

	AddressableAssetSettings settings;

	TextTab textTab;
	PDFTab pdfTab;
	#endregion

	#region Init
	[MenuItem("Window/UI Toolkit/Importer _1")]
	public static void ShowExample()
	{

		Importer wnd = GetWindow<Importer>();
		wnd.titleContent = new GUIContent("Importer");
	}

	/// <summary>Inits the document and all of its parts.</summary>
	public void CreateGUI()
	{
		settings = AddressableAssetSettingsDefaultObject.Settings;
		#region Base
		minSize = new(800, 800);

		VisualElement doc = m_VisualTreeAsset.Instantiate();
		VisualElement win = InitRenameWindow(doc);
		doc.style.flexGrow = 1;
		rootVisualElement.Add(doc);

		textTab = new(doc, win, UpdateChoices);
		pdfTab = new(doc, UpdateChoices);


		doc.Q<TabView>().activeTabChanged += (_, newT) => {
			int newTabIndex = newT.parent.IndexOf(newT);
			if (newTabIndex == 0)
			{
				textTab.SelectTab();
			}
			else
			{
				pdfTab.SelectTab();
			}
			Debug.Log(newTabIndex);
			UpdateChoices(newTabIndex);
		};
		#endregion

		ReloadData();
		InitUniButtons(doc, win);
		textTab.SelectTab();
		EditorSceneManager.activeSceneChangedInEditMode += (_, _) => { if (hasFocus) OnFocus(); };
		EditorSceneManager.activeSceneChanged += (_, _) => { if (hasFocus) OnFocus(); };
	}
	VisualElement InitRenameWindow(VisualElement doc)
	{
		VisualElement win = doc.Q<VisualElement>("TextDialog");
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
		return win;
	}
	void InitUniButtons(VisualElement doc, VisualElement win)
	{
		#region Rename
		doc.Q<VisualElement>("Rename").RegisterCallback<ClickEvent>((_) =>
		{
			win.style.display = DisplayStyle.Flex;
			if (doc.Q<TabView>().selectedTabIndex == 0)
			{
				pdfTab.Rename();
			}
			else
			{
				textTab.Rename();
			}
		}
		);
		#endregion

		#region ClearBinding
		clearButton = doc.Q<Button>("Clear");
		clearButton.clicked += () =>
		{
			AddressableAssetGroup g;
			int activeTab = doc.Q<TabView>().selectedTabIndex, i;
			string GUId;

			if (activeTab == 0)
			{
				textTab.Clear(out i, out g);
			}
			else
			{
				pdfTab.Clear(out i, out g);
			}

			GUId = items[i].sourceObject.AssetGUID;
			items[i].sourceObject = new("");

			settings.CreateOrMoveEntry(GUId, g);
			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
			AssetDatabase.SaveAssets();
			UpdateChoices();
		};
		#endregion

		#region Dropdown
		DropdownField field = doc.Q<DropdownField>("Binding");
		field.RegisterValueChangedCallback<string>((s) =>
		{
			int i = items.FindIndex(q => q.gameObject.GetInstanceID() == choiceIds[field.index]);
			AddressableAssetGroup g = settings.FindGroup(EditorSceneManager.GetActiveScene().name);
			if (i > -1)
			{
				string GUId;
				if (rootVisualElement.Q<TabView>().selectedTabIndex == 0)
					GUId = textTab.OpenEntry();
				else
					GUId = pdfTab.OpenEntry();

				settings.CreateOrMoveEntry(GUId, g);
				items[i].sourceObject = new AssetReference(GUId);

				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
				AssetDatabase.SaveAssets();
			}

			i = items.FindIndex(q => q.gameObject.GetInstanceID() == choiceIds[field.choices.IndexOf(s.previousValue)]);
			if (i > -1)
				items[i].sourceObject = new("");

			UpdateChoices();
		});
		#endregion
	}

	void ReloadData()
	{
		items = FindObjectsByType<InteractableItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
		textTab.ReloadData();
		pdfTab.ReloadData();
	}

	#endregion

	#region Updates
	private void OnFocus()
	{
		if (textTab != null)
		{
			ReloadData();
			UpdateChoices();
		}
	}

	/// <summary>
	/// Updates the selection data for <see cref="textField"/>.
	/// </summary>
	private void OnGUI()
	{
		if (textTab != null)
			textTab.UpdateIndexes();
	}

	void UpdateChoices(int i = -1)
	{
		if (i == -1)
			i = rootVisualElement.Q<TabView>().selectedTabIndex;
		DropdownField field = rootVisualElement.Q<DropdownField>();

		field.choices = items.Where(q => q.sourceObject == null || q.sourceObject.AssetGUID == "").Select(q => $"{q.name} ({q.GetType().ToString().Replace("Items.", "")})").ToList();
		choiceIds = items.Where(q => q.sourceObject == null || q.sourceObject.AssetGUID == "").Select(q => q.gameObject.GetInstanceID()).ToList();

		InteractableItem item;
		string selection;
		if(i == 0)
		{
			selection = UpdateText(out item, textTab.textList);
		}
		else
		{
			selection = UpdateText(out item, pdfTab.pdfList);
		}

		field.choices.Insert(0, selection);
		if(item)
			choiceIds.Insert(0, item.gameObject.GetInstanceID());
		else
			choiceIds.Insert(0, -1);

		field.SetValueWithoutNotify(selection);
	}

	public string UpdateText(out InteractableItem item, ListView listView)
	{
		if (item = Importer.items.Where(q => q.sourceObject != null && q.sourceObject.AssetGUID != "").ToList()
			.FirstOrDefault(q => q.sourceObject.AssetGUID == AssetDatabase.GUIDFromAssetPath((string)listView.selectedItem).ToString()))
		{
			clearButton.style.display = DisplayStyle.Flex;
			return $"{item.name} ({item.GetType().ToString().Replace("Items.", "")})";
		}
		else
		{
			clearButton.style.display = DisplayStyle.None;
			if (!EditorSceneManager.GetActiveScene().name.Contains("Room"))
				return "Select a room Scene!";
			return "Select binding";
		}
	}
	#endregion

}
#endif