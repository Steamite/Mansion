using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Assets.UI_Toolkit.Editor.Importer.PDF
{
    class PDFConversion
    {
        public const string PDF_PATH = "Assets/ItemData/PDF/";
        public const string PDF_FILE_PATH = "Assets/StreamingAssets/PDF/";
        public const string IMAGE_FILE_PATH = "ItemData/Images/";

        AddressableAssetGroup spriteGroup;
        AddressableAssetSettings settings;

        public PDFConversion(AddressableAssetSettings settings, AddressableAssetGroup spriteGroup)
        {
            this.settings = settings;
            this.spriteGroup = spriteGroup;
        }

        PDFData CreatePDFAsset(string originaPDF, out string pdfName, out string pdfPath)
        {
            pdfName = "";
            pdfPath = "";
            

            if (originaPDF == null)
                return null;

            pdfName = Path.GetFileNameWithoutExtension(originaPDF);
            pdfPath = PDF_FILE_PATH + Path.GetFileName(originaPDF);

            if (File.Exists(pdfPath))
            {
                EditorUtility.DisplayDialog("Cannot add", "PDF ALREADY EXISTS", "ok");
                return null;
            }
            File.Move(originaPDF, pdfPath);

            PDFData pdfData = ScriptableObject.CreateInstance<PDFData>();
            pdfData.pdfName = pdfName;
            pdfData.images = new();

            AssetDatabase.CreateAsset(pdfData, $"{PDF_PATH}{pdfName}.asset");
            
            settings.CreateOrMoveEntry(AssetDatabase.GUIDFromAssetPath($"{PDF_PATH}{pdfName}.asset").ToString(), settings.FindGroup("PDFs"));
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, AssetDatabase.GUIDFromAssetPath($"{PDF_PATH}{pdfName}.asset").ToString(), true);
            return pdfData;
        }

        async Task<string> SaveImage(string pdfName, int page, IMagickImage<ushort> image)
        {
            // Write page to file that contains the page number
            string path = $"{Application.dataPath}/{IMAGE_FILE_PATH}{pdfName}/img{page}.jpg";
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

            return path;
        }

        public async Task<string> CreatePDF()
        {
            string originalPdf = EditorUtility.OpenFilePanel("Choose pdf to use", "C:\\Users\\%username%", "");
            PDFData pdfData = CreatePDFAsset(originalPdf, out string pdfName, out string pdfPath);
            if (pdfData == null)
                return null;

            using var magickCollection = new MagickImageCollection();

            var depth = new MagickReadSettings
            {
                Density = new Density(300, 300)
            };

            // Add all the pages of the pdf file to the collection
            magickCollection.Read(File.ReadAllBytes(pdfPath), depth);
            Directory.CreateDirectory($"{Application.dataPath}/{IMAGE_FILE_PATH}{pdfName}");

            int page = 0;
            foreach (var image in magickCollection)
            {
                string imagePath = await SaveImage(pdfName, page, image);

                pdfData.images.Add(new(imagePath));
                
                settings.CreateOrMoveEntry(imagePath, spriteGroup);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, imagePath, true);
                page++;
            }
            EditorUtility.SetDirty(pdfData);
            AssetDatabase.SaveAssets();
            return pdfPath;
        }
    }
}
