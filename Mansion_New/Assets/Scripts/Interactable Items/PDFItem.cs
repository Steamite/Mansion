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
		const string PDF_LOCATION = "PDF/";

		/// <summary>Downloaded textures.</summary>
        Texture2D[] sprites;
		[SerializeField] int amountOfPages;
		public override void LoadContent(VisualElement displayElem)
		{
			if (SourcePath == null)
				return;

			VisualElement el = displayElem.Q<VisualElement>("Images");
			Label label = displayElem.Q<Label>("Label");
			label.text = "Downloading text...";

			for (int i = 0; i < amountOfPages; i++)
			{
				WebUtil.GetImageFromServer(
					PDF_LOCATION + SourcePath + $"/img{i}.png", 
					(source_img) => 
					{
						if (source_img == null)
							return;
						VisualElement element_img = new();
						element_img.style.width = source_img.width;
						element_img.style.height = source_img.height;
						element_img.style.backgroundImage = source_img;
						el.Add(element_img);
					});
			}
		}

		public override void Unload(VisualElement visualElement)
		{
			VisualElement el = visualElement.Q<VisualElement>("Images");
			for (int i = el.childCount - 1; i > -1; i--)
				el.RemoveAt(i);
		}
	}
}
