using System.Collections;
using System.Threading;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Items
{
    /// <summary>
    /// Elemet that stores text, which can contain some tags to make it more interesting.
    /// </summary>
    public class TextItem : InteractableItem
    {
        AsyncOperationHandle<TextData> handle;
        TextData data;

        public override async Awaitable LoadContent()
        {
            if (SourceObject == null || SourceObject.AssetGUID == "")
                return;
            tokenSource?.Cancel();
            tokenSource = new();
            
            await GetContent(tokenSource.Token);
        }

        protected override async Awaitable GetContent(CancellationToken token)
        {
            for (int i = 0; i < 3; i++)
            {
                handle = Addressables.LoadAssetAsync<TextData>(SourceObject);
                if (!handle.IsDone)
                    await handle.Task;
                if (token.IsCancellationRequested)
                    return;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    data = handle.Result;
                    return;
                }
            }
        }

        public override void FillUI(VisualElement element)
        {
            Label _text = element.Q<Label>("Label");
            _text.text = "Downloading text...";
            _text.style.display = DisplayStyle.Flex;

            _text.text = data.content;
        }

        public override void Unload(VisualElement displayElem)
        {
            displayElem.Q<Label>("Label").text = "";
            tokenSource.Cancel();
            handle.Release();
        }

    }
}
