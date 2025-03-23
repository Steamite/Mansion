using System.Collections.Generic;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.AddressableAssets;
using ImageMagick;
using Items;
using System.Linq;
using System;
using UnityEngine.AddressableAssets;

public class PDFTab : ITab
{
	const string PDF_PATH = "Assets/ItemData/PDF/";
	const string PDF_FILE_PATH = "Assets/StreamingAssets/PDF/";
	const string IMAGE_FILE_PATH = "ItemData/Images/";

	AddressableAssetGroup spriteGroup;
	AddressableAssetSettings settings;

	/// <summary>ListView for pdf elements.</summary>
	public ListView pdfList;

	public PDFTab(VisualElement doc, Action<int> choicesUpdate)
	{
		settings = AddressableAssetSettingsDefaultObject.Settings;
		spriteGroup = settings.FindGroup("Sprites");
		InitPDFList(doc, choicesUpdate);
	}

	

	/// <summary>
	/// Fills <see cref="pdfList"/> and assigns callbacks to it.
	/// </summary>
	/// <param name="doc">Root of the document.</param>
	void InitPDFList(VisualElement doc, Action<int> choicesUpdate)
	{
		pdfList = doc.Q<Tab>("PDF").Q<ListView>("List");

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
					foreach (var image in images)
					{
						// Write page to file that contains the page number
						path = $"{Application.dataPath}/{IMAGE_FILE_PATH}{pdfName}/img{page}.jpg";
						await image.WriteAsync(path, MagickFormat.Jpg);
						AssetDatabase.Refresh();

						Debug.Log($"Assets/{IMAGE_FILE_PATH}{pdfName}/img{page}.jpg");
						path = AssetDatabase.GUIDFromAssetPath($"Assets/{IMAGE_FILE_PATH}{pdfName}/img{page}.jpg").ToString();

						TextureImporter importer = AssetImporter.GetAtPath($"Assets/{IMAGE_FILE_PATH}{pdfName}/img{page}.jpg") as TextureImporter;
						TextureImporterSettings spriteSettings = new TextureImporterSettings();
						importer.textureType = TextureImporterType.Sprite;
						importer.ReadTextureSettings(spriteSettings);
						spriteSettings.spriteMode = (int)SpriteImportMode.Single;
						importer.SetTextureSettings(spriteSettings);
						importer.SaveAndReimport();


						settings.CreateOrMoveEntry(path, spriteGroup);
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
				InteractableItem itemAssigned = Importer.items.FirstOrDefault(q => q.sourceObject?.AssetGUID == GUId);
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
			view.Clear();
			if((string)pdfList.selectedItem != null)
			{
				foreach (AssetReference imgRef in AssetDatabase.LoadAssetAtPath<PDFData>((string)pdfList.selectedItem).images)
				{
					VisualElement element = new();
					element.name = "IMG";

					Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(imgRef.AssetGUID));
					element.style.backgroundImage = tex;
					element.style.height = tex.height;
					element.style.width = tex.width;
					view.Add(element);
				}
				choicesUpdate(1);
			}
		};
		pdfList.parent.Q<Button>("ShowPDF").RegisterCallback<ClickEvent>(
			(_) =>
			{
				//Debug.Log($"{Application.dataPath}/{PDF_FILE_PATH.Remove(0, 6)}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}.pdf");
				Application.OpenURL($"{Application.dataPath}/{PDF_FILE_PATH.Remove(0, 6)}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}.pdf");
			});
	}

	public void Clear(out int i, out AddressableAssetGroup g)
	{
		int z = Directory.GetFiles($"{Application.dataPath}/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}", "*.jpg").Length;
		string GUId;
		for (int x = 0; x < z; x++)
		{
			GUId = AssetDatabase.GUIDFromAssetPath($"Assets/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}/img{x}.jpg").ToString();
			settings.CreateOrMoveEntry(GUId, spriteGroup);
			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
		}
		AssetDatabase.SaveAssets();
		i = Importer.items.FindIndex(q => q.sourceObject.AssetGUID == AssetDatabase.GUIDFromAssetPath((string)pdfList.selectedItem).ToString());
		g = settings.FindGroup("PDFs");
	}

	public void ReloadData()
	{
		List<string> files = new();
		List<string> availableAssets = settings.FindGroup("PDFs").entries.Select(q => q.guid).ToList();
		if (settings.FindGroup(EditorSceneManager.GetActiveScene().name))
			availableAssets.AddRange(settings.FindGroup(EditorSceneManager.GetActiveScene().name).entries.Select(q => q.guid));

		foreach (var GUId in AssetDatabase.FindAssets($"t:{nameof(PDFData)}", new string[] { PDF_PATH }))
		{
			if (availableAssets.Contains(GUId))
				files.Add(AssetDatabase.GUIDToAssetPath(GUId));
		}
		pdfList.itemsSource = files;
	}

	public string LinkEntry()
	{
		int z = Directory.GetFiles($"{Application.dataPath}/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}", "*.jpg").Length;
		AddressableAssetGroup group = settings.FindGroup(EditorSceneManager.GetActiveScene().name);
		string GUId;
		for (int x = 0; x < z; x++)
		{
			GUId = AssetDatabase.GUIDFromAssetPath($"Assets/{IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}/img{x}.jpg").ToString();
			settings.CreateOrMoveEntry(GUId, group);
			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
		}
		return AssetDatabase.GUIDFromAssetPath((string)pdfList.selectedItem).ToString();
	}

	public void SelectTab()
	{
		pdfList.SetSelection(-1);
		pdfList.SetSelection(0);
	}

	public void Rename()
	{
		if ((string)pdfList.selectedItem != "")
		{
			Importer.stringAction = (s) =>
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
}
