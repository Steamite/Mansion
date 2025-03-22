#if UNITY_EDITOR
using ImageMagick;
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
using Object = UnityEngine.Object;

/// <summary>
/// Custom Editor window for managing streaming assets. 
/// </summary>
public class Importer : EditorWindow
{
	#region Variables
	const string TEXT_PATH = "Assets/ItemData/Text/";
	const string PDF_PATH = "Assets/ItemData/PDF/";
	const string PDF_FILE_PATH = "Assets/StreamingAssets/PDF/";
	const string IMAGE_FILE_PATH = "ItemData/Images/";
	const string BCK_PATH = "BCK/";

	const string SpriteGroup = "Sprites";

	/// <summary>UI document containing the window definition</summary>
	[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
	/// <summary>Dialog window</summary>
	VisualElement win;

	/// <summary>ListView for text elements.</summary>
	ListView textList;
	/// <summary>ListView for pdf elements.</summary>
	ListView pdfList;
	/// <summary>Text field for text content</summary>
	TextField textField;
	/// <summary>Text field in <see cref="win"/></summary>
	TextField nameField;

	Button clearButton;

	/// <summary>Action callback for <see cref="win"/>.</summary>
	Action<string> stringAction;

	/// <summary>Stores last cursor position in <see cref="textField"/>>.</summary>
	int cursorIndex;
	/// <summary>Stores last selection position in <see cref="textField"/>>.</summary>
	int selectedIndex;

	List<InteractableItem> items;

	int lastTab;
	int lastTextSelected;
	int lastPdfSelected;

	AddressableAssetSettings settings;
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

		VisualElement root = rootVisualElement;

		VisualElement doc = m_VisualTreeAsset.Instantiate();
		doc.style.flexGrow = 1;
		root.Clear();
		root.Add(doc);

		root.Q<TabView>().activeTabChanged += (_, newT) => {
			int newTabIndex = newT.parent.IndexOf(newT);
			if (newTabIndex == 0)
			{
				if (textList.itemsSource.Count > 0)
					textList.SetSelection(lastTextSelected);
			}
			else
			{
				if (pdfList.itemsSource.Count > 0)
					pdfList.SetSelection(lastPdfSelected);
			}
			Debug.Log(newTabIndex);
			UpdateChoices(newTabIndex);
		};
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

		InitTextList(doc);
		InitTextButtons(doc);

		InitPDFList(doc);

		InitUniButtons(doc);
	}

	private void OnFocus()
	{
		CreateGUI();
		if(lastTab == 0)
		{
			if (textList.itemsSource.Count > 0 && textList.selectedIds == null || textList.selectedIds.Count() == 0)
				textList.SetSelection(lastTextSelected);
		}
		else
		{
			if (pdfList.itemsSource.Count > 0 && (pdfList.selectedIds == null || pdfList.selectedIds.Count() == 0))
				pdfList.SetSelection(lastPdfSelected);
		}
		UpdateChoices();

		Debug.Log("focus");
		rootVisualElement.Q<TabView>().selectedTabIndex = lastTab;
	}

	private void OnLostFocus()
	{
		lastTab = rootVisualElement.Q<TabView>().selectedTabIndex;
		lastTextSelected = textList.selectedIndex;
		lastPdfSelected = pdfList.selectedIndex;
	}
	private void InitUniButtons(VisualElement doc)
	{

		#region Rename
		doc.Q<VisualElement>("Rename").RegisterCallback<ClickEvent>((_) =>
		{
			if (doc.Q<TabView>().activeTab.name == "PDF")
			{
				if ((string)pdfList.selectedItem != "")
				{
					win.style.display = DisplayStyle.Flex;
					stringAction = (s) =>
					{
						if (Directory.Exists(PDF_PATH + s))
						{
							EditorUtility.DisplayDialog("ALREADY IN USE", "FOLDER EXISTS", "ok");
							return;
						}
						AssetDatabase.MoveAsset((string)pdfList.selectedItem, PDF_PATH + s);
						pdfList.itemsSource[pdfList.selectedIndex] = PDF_PATH + s;
						pdfList.RefreshItems();
					};
				}
			}
			else
			{
				if ((string)textList.selectedItem != "")
				{
					win.style.display = DisplayStyle.Flex;
					stringAction = (s) =>
					{
						AssetDatabase.MoveAsset((string)textList.selectedItem, TEXT_PATH + s + ".txt");
						textList.itemsSource[textList.selectedIndex] = TEXT_PATH + s + ".txt";
						textList.RefreshItems();
					};
				}
			}
		}
		);
		#endregion

		#region ClearBinding
		clearButton = doc.Q<Button>("Clear");
		clearButton.clicked += () =>
		{
			AddressableAssetGroup g;
			int activeTab = doc.Q<TabView>().selectedTabIndex;
			int i;
			string GUId;
			if (activeTab == 0)
			{
				i = items.FindIndex(q => q.sourceObject.AssetGUID == AssetDatabase.GUIDFromAssetPath((string)textList.selectedItem).ToString());
				g = settings.FindGroup("Texts");
			}
			else
			{
				int z = Directory.GetFiles($"{Application.dataPath}/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}", "*.jpg").Length;
				g = settings.FindGroup(SpriteGroup);
				for (int x = 0; x < z; x++)
				{
					GUId = AssetDatabase.GUIDFromAssetPath($"Assets/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}/img{x}.jpg").ToString();
					settings.CreateOrMoveEntry(GUId, g);
					settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
				}
				AssetDatabase.SaveAssets();
				i = items.FindIndex(q => q.sourceObject.AssetGUID == AssetDatabase.GUIDFromAssetPath((string)pdfList.selectedItem).ToString());
				g = settings.FindGroup("PDFs");
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

		if (!EditorSceneManager.GetActiveScene().name.Contains("Room"))
		{
			field.style.display = DisplayStyle.None;
			return;
		}
		field.style.display = DisplayStyle.Flex;
		items = FindObjectsByType<InteractableItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

		field.RegisterValueChangedCallback<string>((s) =>
		{
			int i = items.FindIndex(q => q.name == s.newValue);
			string GUId = null;
			AddressableAssetGroup g = settings.FindGroup(EditorSceneManager.GetActiveScene().name);

			if (i > -1)
			{
				if (rootVisualElement.Q<TabView>().selectedTabIndex == 0)
					GUId = AssetDatabase.GUIDFromAssetPath((string)textList.selectedItem).ToString();
				else
				{
					int z = Directory.GetFiles($"{Application.dataPath}/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}", "*.jpg").Length;
					for (int x = 0; x < z; x++)
					{
						GUId = AssetDatabase.GUIDFromAssetPath($"Assets/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}/img{x}.jpg").ToString();
						settings.CreateOrMoveEntry(GUId, g);
						settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
					}
					GUId = AssetDatabase.GUIDFromAssetPath((string)pdfList.selectedItem).ToString();
				}

				settings.CreateOrMoveEntry(GUId, g);

				items[i].sourceObject = new AssetReference(GUId);
			}

			i = items.FindIndex(q => q.name == s.previousValue);
			if (i > -1)
			{
				items[i].sourceObject = new("");
			}

			if(GUId != null)
			{
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
				AssetDatabase.SaveAssets();
			}

			UpdateChoices();
		});
		#endregion
	}

	#endregion

	#region Text
	/// <summary>
	/// Adds items source and callbacks to <see cref="textList"/>.
	/// </summary>
	/// <param name="doc">Root of the document.</param>
	void InitTextList(VisualElement doc)
	{
		List<string> files = new();
		foreach (var a in AssetDatabase.FindAssets("t:TextData"))
		{
			files.Add(AssetDatabase.GUIDToAssetPath(a));
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
				s = TEXT_PATH + s + ".asset";
				if (File.Exists(s))
				{
					EditorUtility.DisplayDialog("Cannot add", "FILE ALREADY EXISTS", "ok");
					return;
				}

				TextData data = CreateInstance<TextData>();
				data.content = "";
				AssetDatabase.CreateAsset(data, s);

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
				InteractableItem itemAssigned = items.FirstOrDefault(q => q.sourceObject?.AssetGUID == GUId);
				if(itemAssigned != null)
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
			if(textList.selectedIndex > -1)
			{
				textField.value = AssetDatabase.LoadAssetAtPath<TextData>((string)textList.selectedItem).content;
				UpdateChoices(0);
			}
		};
	}
	
	/// <summary>
	/// Implements functionality for the bottom buttons.
	/// </summary>
	/// <param name="doc">Root of the document.</param>
	void InitTextButtons(VisualElement doc)
	{
		doc.Q<Button>("Link").RegisterCallback<ClickEvent>((_) =>
		{
			if (selectedIndex - cursorIndex != 0)
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
			TextData data = AssetDatabase.LoadAssetAtPath<TextData>((string)textList.selectedItem);
			data.content = textField.text;
			EditorUtility.SetDirty(data);
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
				s = s.Remove(i, j - i + 2);
			}
			textField.value = s;
		});
		textField.RegisterValueChangedCallback<string>((s) => doc.Q<Label>("PreviewText").text = s.newValue);
	}
	#endregion

	#region PDF
	/// <summary>
	/// Fills <see cref="pdfList"/> and assigns callbacks to it.
	/// </summary>
	/// <param name="doc">Root of the document.</param>
	void InitPDFList(VisualElement doc)
	{
		List<string> files = new();

		AddressableAssetGroup _g = settings.FindGroup(EditorSceneManager.GetActiveScene().name);
		foreach (var a in AssetDatabase.FindAssets($"t:{nameof(PDFData)}", new string[] { PDF_PATH }))
		{
			files.Add(AssetDatabase.GUIDToAssetPath(a));
		}

		pdfList = doc.Q<Tab>("PDF").Q<ListView>("List");
		pdfList.itemsSource = files;


		pdfList.onAdd = async (view) =>
		{
			string originalPdf = EditorUtility.OpenFilePanel("Choose pdf to use", "C:\\Users\\%username%", "");
			if (originalPdf != null && originalPdf != "")
			{
				string pdfName = Path.GetFileNameWithoutExtension(originalPdf);
				string newPdf = PDF_FILE_PATH + Path.GetFileName(originalPdf);
				if (File.Exists(newPdf))
				{
					EditorUtility.DisplayDialog("Cannot add", "PDF ALREADY EXISTS", "ok");
					return;
				}
				File.Move(originalPdf, newPdf);

				PDFData pdfData = ScriptableObject.CreateInstance<PDFData>();
				pdfData.pdf = pdfName;
				pdfData.images = new();
				AssetDatabase.CreateAsset(pdfData, $"{PDF_PATH}{pdfName}.asset");
				settings.CreateOrMoveEntry(AssetDatabase.GUIDFromAssetPath($"{PDF_PATH}{pdfName}.asset").ToString(), settings.FindGroup("PDFs"));
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, AssetDatabase.GUIDFromAssetPath($"{PDF_PATH}{pdfName}.asset").ToString(), true);

				using (var images = new MagickImageCollection())
				{
					var depth = new MagickReadSettings
					{
						Density = new Density(300, 300)
					};

					// Add all the pages of the pdf file to the collection
					images.Read(File.ReadAllBytes(newPdf), depth);
					Directory.CreateDirectory($"{Application.dataPath}/{IMAGE_FILE_PATH}{pdfName}");

					var page = 0;
					string path;
					AddressableAssetGroup g = settings.FindGroup(SpriteGroup);
					foreach (var image in images)
					{
						// Write page to file that contains the page number
						path = $"{Application.dataPath}/{IMAGE_FILE_PATH}{pdfName}/img{page}.jpg";
						await image.WriteAsync(path, MagickFormat.Jpg);
						AssetDatabase.Refresh();

						Debug.Log($"Assets/{IMAGE_FILE_PATH}{pdfName}/img{page}.jpg");
						path = AssetDatabase.GUIDFromAssetPath($"Assets/{IMAGE_FILE_PATH}{pdfName}/img{page}.jpg").ToString();
						settings.CreateOrMoveEntry(path, g);
						settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, path, true);
						pdfData.images.Add(new(path));

						page++;
					}
					AssetDatabase.SaveAssets();
				}
				

				view.itemsSource.Add(newPdf);
				view.Rebuild();
			}
		};

		pdfList.onRemove = (_) =>
		{
			if (pdfList.selectedIndex != -1 &&
				EditorUtility.DisplayDialog("Delete string entry",
				$"Are you sure you want to delete entry: {pdfList.selectedItem}",
				"confirm", "cancel"))
			{
				string fileName = Path.GetFileNameWithoutExtension((string)pdfList.selectedItem);

				AssetDatabase.DeleteAsset((string)pdfList.selectedItem);
				AssetDatabase.DeleteAsset("Assets/" + IMAGE_FILE_PATH + fileName);
				AssetDatabase.MoveAsset(PDF_FILE_PATH + fileName + ".pdf", PDF_FILE_PATH + "BCK/" + fileName + ".pdf");

				// Removing reference from item
				string GUId = AssetDatabase.GUIDFromAssetPath((string)pdfList.selectedItem).ToString();
				InteractableItem itemAssigned = items.FirstOrDefault(q => q.sourceObject?.AssetGUID == GUId);
				if (itemAssigned != null)
					itemAssigned.sourceObject = new("");

				// Removing from addressables
				settings.RemoveAssetEntry(GUId);
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, GUId, true);

				pdfList.itemsSource.RemoveAt(pdfList.selectedIndex);
				pdfList.Rebuild();
				pdfList.SetSelection(-1);
				pdfList.SetSelection(0);
			}
		};

		pdfList.bindItem = (element, id) =>
		{
			element.Q<Label>().text = Path.GetFileNameWithoutExtension(pdfList.itemsSource[id].ToString());
		};

		pdfList.selectionChanged += newSelection =>
		{
			ScrollView view = pdfList.parent.Q<ScrollView>("Preview");
			int i;
			for (i = view.childCount - 1; i > -1; i--)
				view.RemoveAt(i);

			i = 0;
			foreach (string s in Directory.GetFiles(
				$"{Application.dataPath}/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}"))
			{
				VisualElement element = new();
				element.name = "IMG";
				Texture2D tex = new Texture2D(2, 2);
				tex.LoadImage(File.ReadAllBytes(s));
				element.style.backgroundImage = tex;
				element.style.height = tex.height / 2;
				element.style.width = tex.width / 2;
				view.Add(element);
			}
			UpdateChoices(1);
		};

		pdfList.parent.Q<Button>("ShowPDF").RegisterCallback<ClickEvent>(
			(_) =>
			{
				//Debug.Log($"{Application.dataPath}/{PDF_FILE_PATH.Remove(0, 6)}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}.pdf");
				Application.OpenURL($"{Application.dataPath}/{PDF_FILE_PATH.Remove(0, 6)}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}.pdf");
			});
	}
	#endregion

	#region Updates
	/// <summary>
	/// Updates the selection data for <see cref="textField"/>.
	/// </summary>
	private void OnGUI()
	{
		if (textField.textSelection.HasSelection())
			selectedIndex = textField.selectIndex;
		cursorIndex = textField.cursorIndex;
	}

	void UpdateChoices(int i = -1)
	{
		if(i == -1)
			i = rootVisualElement.Q<TabView>().selectedTabIndex;
		DropdownField field = rootVisualElement.Q<DropdownField>();
		field.choices = items.Where(q => q.sourceObject == null || q.sourceObject.AssetGUID == "").Select(q => $"{q.name} ({q.GetType().ToString().Replace("Items.", "")})").ToList();
		
		InteractableItem item = null;
		string selection;
		if (i == 0 &&
			(item = items.Where(q => q.sourceObject != null && q.sourceObject.AssetGUID != "").ToList()
			.FirstOrDefault(q => q.sourceObject.AssetGUID == AssetDatabase.GUIDFromAssetPath((string)textList.selectedItem).ToString())))
			selection = $"{item.name} ({item.GetType().ToString().Replace("Items.", "")})";
		else if (i == 1 &&
			(item = items.Where(q => q.sourceObject != null && q.sourceObject.AssetGUID != "").ToList()
			.FirstOrDefault(q => q.sourceObject.AssetGUID == AssetDatabase.GUIDFromAssetPath((string)pdfList.selectedItem).ToString())))
			selection = $"{item.name} ({item.GetType().ToString().Replace("Items.", "")})";
		else
			selection = "Select binding";


		if (selection != "Select binding")
			clearButton.style.display = DisplayStyle.Flex;
		else
			clearButton.style.display = DisplayStyle.None;

		field.choices.Insert(0, selection);
		field.SetValueWithoutNotify(selection);
	}
	#endregion
}
#endif