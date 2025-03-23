using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Items
{
	/// <summary>
	/// Elemet that stores text, which can contain some tags to make it more interesting.
	/// </summary>
    class TextItem : InteractableItem
    {
		public override void LoadContent(VisualElement displayElem)
		{
			if (sourceObject == null || sourceObject.AssetGUID == "")
				return;

			StartCoroutine(GetContent(displayElem));
		}

		protected override IEnumerator GetContent(VisualElement displayElem)
		{
			Label _text = displayElem.Q<Label>("Label");
			_text.text = "Downloading text...";

			for (int i = 0; i < 3; i++)
			{
				AsyncOperationHandle<TextData> handle = Addressables.LoadAssetAsync<TextData>(sourceObject);
				yield return handle;
				if (handle.Status == AsyncOperationStatus.Succeeded)
				{
					_text.text = handle.Result.content;
					if (sourceObject.IsValid())
						sourceObject.ReleaseAsset();
					yield break;
				}
			}
			_text.text = "ERROR";
		}

		public override void Unload(VisualElement displayElem)
		{
			displayElem.Q<Label>("Label").text = "";
		}

	}
}
