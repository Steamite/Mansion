using Assets.UI_Toolkit.Editor.Importer.PDF;
using ImageMagick;
using Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Importer.Tabs
{
    public class PDFTab : ITab
    {

        AddressableAssetGroup spriteGroup;
        AddressableAssetSettings settings;

        readonly PDFConversion conversion;
        /// <summary>ListView for pdf elements.</summary>
        public ListView pdfList;

        public PDFTab(VisualElement doc, Action<int> choicesUpdate)
        {
            settings = AddressableAssetSettingsDefaultObject.Settings;
            spriteGroup = settings.FindGroup("Sprites");
            conversion = new (settings, spriteGroup);

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
                string pdfPath = await conversion.CreatePDF();
                if (pdfPath == null)
                    return;
                view.itemsSource.Add(pdfPath);
                view.Rebuild();
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
                    AssetDatabase.DeleteAsset("Assets/" + PDFConversion.IMAGE_FILE_PATH + fileName);
                    AssetDatabase.MoveAsset(PDFConversion.PDF_FILE_PATH + fileName + ".pdf", PDFConversion.PDF_FILE_PATH + "BCK/" + fileName + ".pdf");

                    // Removing reference from item
                    string GUId = AssetDatabase.GUIDFromAssetPath((string)pdfList.selectedItem).ToString();
                    InteractableItem itemAssigned = Importer.items.FirstOrDefault(q => q.SourceObject?.AssetGUID == GUId);
                    if (itemAssigned != null)
                        Importer.instance.UnlinkData(itemAssigned);

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
                if ((string)pdfList.selectedItem != null)
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
                    Application.OpenURL($"{Application.dataPath}/{PDFConversion.PDF_FILE_PATH.Remove(0, 6)}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}.pdf");
                });
        }

        public void Clear(out int i, out AddressableAssetGroup g)
        {
            int z = Directory.GetFiles($"{Application.dataPath}/{PDFConversion.IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}", "*.jpg").Length;
            string GUId;
            for (int x = 0; x < z; x++)
            {
                GUId = AssetDatabase.GUIDFromAssetPath($"Assets/{PDFConversion.IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}/img{x}.jpg").ToString();
                settings.CreateOrMoveEntry(GUId, spriteGroup);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
            }
            AssetDatabase.SaveAssets();
            i = Importer.items.FindIndex(q => q.SourceObject.AssetGUID == AssetDatabase.GUIDFromAssetPath((string)pdfList.selectedItem).ToString());
            g = settings.FindGroup("PDFs");
        }

        public void ReloadData()
        {
            List<string> files = new();
            List<string> availableAssets = settings.FindGroup("PDFs").entries.Select(q => q.guid).ToList();
            if (settings.FindGroup(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
                availableAssets.AddRange(settings.FindGroup(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name).entries.Select(q => q.guid));

            foreach (var GUId in AssetDatabase.FindAssets($"t:{nameof(PDFData)}", new string[] { PDFConversion.PDF_PATH }))
            {
                if (availableAssets.Contains(GUId))
                    files.Add(AssetDatabase.GUIDToAssetPath(GUId));
            }
            pdfList.itemsSource = files;
        }

        public string LinkEntry(InteractableItem item, out InteractableItem newItem, out string contentName)
        {
            if (item is not PDFItem)
            {
                newItem = item.gameObject.AddComponent<PDFItem>();
                newItem.Clone(item);
                UnityEngine.Object.DestroyImmediate(item);
            }
            else newItem = item;

            contentName = Path.GetFileNameWithoutExtension((string)pdfList.selectedItem);
            int z = Directory.GetFiles($"{Application.dataPath}/{PDFConversion.IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}", "*.jpg").Length;
            AddressableAssetGroup group = settings.FindGroup(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            string GUId;
            for (int x = 0; x < z; x++)
            {
                GUId = AssetDatabase.GUIDFromAssetPath($"Assets/{PDFConversion.IMAGE_FILE_PATH}{Path.GetFileNameWithoutExtension((string)pdfList.selectedItem)}/img{x}.jpg").ToString();
                settings.CreateOrMoveEntry(GUId, group);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, GUId, true);
            }
            AssetDatabase.SaveAssets();
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
                    if (Directory.Exists(PDFConversion.PDF_PATH + s))
                    {
                        EditorUtility.DisplayDialog("ALREADY IN USE", "FOLDER EXISTS", "ok");
                        return;
                    }
                    AssetDatabase.MoveAsset((string)pdfList.selectedItem, PDFConversion.PDF_PATH + s);
                    pdfList.itemsSource[pdfList.selectedIndex] = PDFConversion.PDF_PATH + s;
                    pdfList.RefreshItems();
                };
            }
        }
    }
}