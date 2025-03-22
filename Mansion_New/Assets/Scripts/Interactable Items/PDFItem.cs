using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Items
{
	/// <summary>
	/// Item with *.pdf and .png description.
	/// Each page of the pdf equals one png.
	/// </summary>
    class PDFItem : InteractableItem
    {
		public const string PDF_LOCATION = "PDF/";

		/// <summary>Downloaded textures.</summary>
        Texture2D[] sprites;
		[SerializeField] int amountOfPages;
		public override void LoadContent(VisualElement displayElem)
		{
			if (SourcePath == null)
				return;

			if(sprites == null)
				sprites = new Texture2D[amountOfPages];

			GetImages(displayElem);
			// Find option for button T
			VisualElement t = displayElem.panel.visualTree.Q<VisualElement>("T");
			t.style.display = DisplayStyle.Flex;
			((Label)t.ElementAt(2)).text = "Otevřít pdf";
		}

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
		}

		public override void Unload(VisualElement displayElem)
		{
			VisualElement el = displayElem.Q<VisualElement>("Images");
			for (int i = el.childCount - 1; i > -1; i--)
				el.RemoveAt(i);
			displayElem.panel.visualTree.Q<VisualElement>("T").style.display = DisplayStyle.None;
		}

		protected override IEnumerator GetContent(object element)
		{
			throw new System.NotImplementedException();
		}
	}
}
