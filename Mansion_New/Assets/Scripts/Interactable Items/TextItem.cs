using UI.Inspect;
using UnityEngine.UIElements;

namespace Items
{
	/// <summary>
	/// Elemet that stores text, which can contain some tags to make it more interesting.
	/// </summary>
    class TextItem : InteractableItem
    {
		/// <summary>Cached string.</summary>
		string content = null;

		public override void LoadContent(VisualElement displayElem)
		{
			if (SourcePath == "")
				return;

			Label _text = (Label)displayElem;
			if (content == null || content == "ERROR")
			{
				_text.text = "Downloading text...";
				WebUtil.GetTextFromServer(
					SourcePath,
					(s) =>
					{
						content = s;
						_text.text = s;
					});
			}
			else
				_text.text = content;
		}
	}
}
