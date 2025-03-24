using System.Collections;
using System.Linq;
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
		public override void LoadContent(VisualElement displayElem)
		{
			if (SourceObject == null || SourceObject.AssetGUID == "")
				return;

			StartCoroutine(GetContent(displayElem));

		}

		protected override IEnumerator GetContent(VisualElement displayElem)
		{
			Label _text = displayElem.Q<Label>("Label");
			_text.text = "Downloading images...";

			for (int i = 0; i < 3; i++)
			{
				AsyncOperationHandle<PDFData> pdfHandle = Addressables.LoadAssetAsync<PDFData>(SourceObject);
				yield return pdfHandle;
				if (pdfHandle.Status == AsyncOperationStatus.Succeeded)
				{
					_text.text = "";
					pdfPath = Application.streamingAssetsPath + PDF_LOCATION + pdfHandle.Result.pdf + ".pdf";
					VisualElement t = displayElem.panel.visualTree.Q<VisualElement>("T");
					t.style.display = DisplayStyle.Flex;
					((Label)t.ElementAt(2)).text = "Otevřít pdf";

					AsyncOperationHandle<Texture2D> spriteHandle;
					Label label = new Label("Loading...");
					VisualElement imagesElement = displayElem.Q<VisualElement>("Images");
					foreach (AssetReference image in pdfHandle.Result.images)
					{
						imagesElement.Add(label);
						spriteHandle = Addressables.LoadAssetAsync<Texture2D>(image);
						yield return spriteHandle;
						if(spriteHandle.Status == AsyncOperationStatus.Succeeded)
						{
							imagesElement.Remove(label);
							VisualElement imgElem = new();
							imgElem.style.backgroundImage = spriteHandle.Result;
							imagesElement.Add(imgElem);
							imgElem.style.width = spriteHandle.Result.width;
							imgElem.style.height = spriteHandle.Result.height;

							//spriteHandle.Release();
						}
						else
						{
							_text.text = "ERROR";
							break;
						}

					}
					pdfHandle.Release();
					
					yield break;
				}
			}
			_text.text = "ERROR";
		}
		/*
		void GetImages(VisualElement displayElem)
		{
			VisualElement imgGroup = displayElem.Q<VisualElement>("Images");

			for (int i = 0; i < sprites.Length; i++)
			{
				VisualElement elementImg = new();
				imgGroup.Add(elementImg);
				if (sprites[i] == null)
				{
					elementImg.Add(new Label($"Downloading image {i}"));

					var x = i;
					WebUtil.GetImageFromServer(
						PDF_LOCATION + SourcePath + $"/img{i}.png",
						(source_img) =>
						{
							if (source_img == null)
								return;
							elementImg.RemoveAt(0);
							sprites[x] = source_img;
							elementImg.style.width = source_img.width;
							elementImg.style.height = source_img.height;
							elementImg.style.backgroundImage = source_img;
						});
				}
				else
				{
					elementImg.style.width = sprites[i].width;
					elementImg.style.height = sprites[i].height;
					elementImg.style.backgroundImage = sprites[i];
				}
			}
		}*/

		public override void Unload(VisualElement displayElem)
		{
			VisualElement el = displayElem.Q<VisualElement>("Images");
			for (int i = el.childCount - 1; i > -1; i--)
				el.RemoveAt(i);
			displayElem.panel.visualTree.Q<VisualElement>("T").style.display = DisplayStyle.None;
		}
	}
}
