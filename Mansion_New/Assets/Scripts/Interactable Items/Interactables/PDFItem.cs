using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Items
{
    /// <summary>
    /// Item with *.pdf and .png description.
    /// Each page of the pdf equals one png.
    /// </summary>
    public class PDFItem : InteractableItem
    {
        const string PDF_LOCATION = "/PDF/";
        public string pdfPath;
        public override async Awaitable LoadContent()
        {
            if (SourceObject == null || SourceObject.AssetGUID == "")
                return;
            sprites = new();

            tokenSource?.Cancel();
            tokenSource = new();

            await GetContent(tokenSource.Token);
        }
        List<AsyncOperationHandle<Texture2D>> sprites = null;
        AsyncOperationHandle<PDFData> pdfHandle;

        protected override async Awaitable GetContent(CancellationToken token)
        {
            sprites = new();

            for (int i = 0; i < 3; i++)
            {
                pdfHandle = Addressables.LoadAssetAsync<PDFData>(SourceObject);
                if (!pdfHandle.IsDone)
                    await pdfHandle.Task;

                if (pdfHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    pdfPath = Application.streamingAssetsPath + PDF_LOCATION + pdfHandle.Result.pdfName + ".pdf";

                    // Menu button
                    

                    AsyncOperationHandle<Texture2D> spriteHandle;

                    foreach (AssetReference image in pdfHandle.Result.images)
                    {
                        if (token.IsCancellationRequested)
                            return;
                        spriteHandle = Addressables.LoadAssetAsync<Texture2D>(image);
                        if (!spriteHandle.IsDone)
                            await spriteHandle.Task;

                        if (spriteHandle.Status == AsyncOperationStatus.Succeeded)
                        {;
                            sprites.Add(spriteHandle);
                        }
                        else
                        {
                            break;
                        }

                    }
                    return;
                }
            }
        }

        public override void FillUI(VisualElement element)
        {
            /*Label _text = element.Q<Label>("Label");

            _text.text = "loading images...";
            _text.style.display = DisplayStyle.Flex;

            _text.style.display = DisplayStyle.None;*/

            VisualElement t = element.panel.visualTree.Q<VisualElement>("T");
            if (t != null)
            {
                t.style.display = DisplayStyle.Flex;
                ((Label)t.ElementAt(2)).text = "Otevřít pdf";
            }

            VisualElement imagesElement = element.Q<VisualElement>("Images");
            foreach (var item in sprites)
            {
                Texture2D tex = item.Result;
                VisualElement imgElem = new();
                imgElem.style.backgroundImage = tex;
                imagesElement.Add(imgElem);
                imgElem.style.width  = tex.width;
                imgElem.style.height = tex.height;
            }
/*
            _text.text = "ERROR";
            _text.style.display = DisplayStyle.Flex;*/
        }

        private void OnDestroy()
        {
            if (sprites == null)
                return;

            foreach (var item in sprites)
            {
                item.Release();
            }
            tokenSource.Cancel();
        }

        public override void Unload(VisualElement displayElem)
        {
            tokenSource.Cancel();

            VisualElement el = displayElem.Q<VisualElement>("Images");

            for (int i = el.childCount - 1; i > -1; i--)
                el.RemoveAt(i);

            //// Menu button
            VisualElement TButton = displayElem.panel.visualTree.Q<VisualElement>("T");
            if(TButton != null)
                TButton.style.display = DisplayStyle.None;
        }
    }
}
