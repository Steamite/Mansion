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
		/// <summary>
		/// Downloaded textures.
		/// </summary>
        Texture2D[] sprites;

		public override void LoadContent(VisualElement displayElem)
		{
			if (SourcePath == null)
				return;

			//ListView list = (ListView)displayElem;

			Application.OpenURL(Application.streamingAssetsPath +"\\"+ SourcePath);
			/*if (sprites == null)
			{
				_text.text = "Downloading text...";
				WebUtil.GetImagesFromServer(
					SourcePath,
					(s) =>
					{
						sprites = s;
						// CREATE
					});
			}
			else
			{

			}*/
		}
	}
}
