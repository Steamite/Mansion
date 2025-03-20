#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Custom Editor window for managing streaming assets. 
/// </summary>
public class Importer : EditorWindow
{
	#region Variables
	/// <summary>Assets/StreamingAssets/Text/</summary>
	string TEXT_PATH = "Assets/StreamingAssets/Text/";
	/// <summary>Assets/StreamingAssets/PDF/</summary>
	string PDF_PATH = "Assets/StreamingAssets/PDF/";
	/// <summary>/BCK/</summary>
	const string BCK_PATH = "/BCK/";

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

	/// <summary>Action callback for <see cref="win"/>.</summary>
	Action<string> stringAction;

	/// <summary>Stores last cursor position in <see cref="textField"/>>.</summary>
	int cursorIndex;
	/// <summary>Stores last selection position in <see cref="textField"/>>.</summary>
	int selectedIndex;
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

		InitTextList(doc);
		InitTextControls(doc);

		InitPDFList(doc);

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
	}

	#region Text
	/// <summary>
	/// Adds items source and callbacks to <see cref="textList"/>.
	/// </summary>
	/// <param name="doc">Root of the document.</param>
	void InitTextList(VisualElement doc)
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
				if (File.Exists(s))
				{
					EditorUtility.DisplayDialog("Cannot add", "FILE ALREADY EXISTS", "ok");
					return;
				}
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
				AssetDatabase.MoveAsset((string)textList.selectedItem, ((string)textList.selectedItem).Replace(fileName, "") + BCK_PATH + fileName);
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
	
	/// <summary>
	/// Implements functionality for the bottom buttons.
	/// </summary>
	/// <param name="doc">Root of the document.</param>
	void InitTextControls(VisualElement doc)
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
		List<string> folders = new();
		foreach (var a in Directory.GetDirectories(PDF_PATH).Where(q => !q.Contains("BCK")))
			folders.Add(a);

		pdfList = doc.Q<Tab>("PDF").Q<ListView>("List");
		pdfList.itemsSource = folders;


		pdfList.onAdd = (view) =>
		{
			string folder = EditorUtility.OpenFolderPanel("Choose folder to use", "C:\\Users\\%username%", "");
			if (folder != null && folder != "")
			{
				string uFolder = PDF_PATH + Path.GetFileName(folder);
				if (Directory.Exists(uFolder))
				{
					EditorUtility.DisplayDialog("Cannot add", "FOLDER ALREADY EXISTS", "ok");
					return;
				}

				Directory.Move(folder, uFolder);
				List<string> files = Directory.GetFiles(uFolder).ToList();
				int i;
				if ((i = files.FindIndex(q => q.EndsWith(".pdf"))) != -1)
				{
					try
					{
						File.Move(files[i], uFolder + "\\pdf.pdf");
					}
					catch { }
				}
				i = 0;
				foreach (string s in files.Where(q => !q.EndsWith(".pdf")))
				{
					try
					{
						File.Move(s, uFolder + $"\\img{i}.png");
					}
					catch { }
				}


				view.itemsSource.Add(uFolder);
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
				string fileName = Path.GetFileName((string)pdfList.selectedItem);

				AssetDatabase.MoveAsset((string)pdfList.selectedItem, ((string)pdfList.selectedItem).Replace(fileName, "").TrimEnd('\\') + BCK_PATH + fileName);
				pdfList.itemsSource.RemoveAt(pdfList.selectedIndex);
				pdfList.Rebuild();
			}
		};

		pdfList.bindItem = (element, id) =>
		{
			element.Q<Label>().text = Path.GetFileName(pdfList.itemsSource[id].ToString());
		};

		pdfList.selectionChanged += newSelection =>
		{
			ScrollView view = pdfList.parent.Q<ScrollView>("Preview");
			int i;
			for (i = view.childCount - 1; i > -1; i--)
				view.RemoveAt(i);

			i = 0;
			foreach (string s in Directory.GetFiles((string)pdfList.selectedItem).Where(q => !q.EndsWith(".pdf")))
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
		};

		pdfList.parent.Q<Button>("ShowPDF").RegisterCallback<ClickEvent>((_) => Application.OpenURL((string)pdfList.selectedItem + "\\pdf.pdf"));
	}
	#endregion

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
	#endregion
}
#endif