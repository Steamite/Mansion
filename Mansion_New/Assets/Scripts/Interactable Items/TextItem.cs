using UI.Inspect;
using UnityEngine.UIElements;

namespace Items
{
	/// <summary>
	/// Elemet that stores text, which can contain some tags to make it more interesting.
	/// </summary>
    class TextItem : InteractableItem
    {
		const string TEXT_FOLDER = "TEXT/";
		/// <summary>Cached string.</summary>
		string content = null;

		public override void LoadContent(VisualElement displayElem)
		{
			if (SourcePath == "")
				return;

			Label _text = displayElem.Q<Label>("Label");
			if (content == null || content == "ERROR")
			{
				_text.text = "Downloading text...";
				WebUtil.GetTextFromServer(
					TEXT_FOLDER + SourcePath,
					(s) =>
					{
						content = s;
						_text.text = s;
					});
			}
			else
				_text.text = content;
		}

		public override void Unload(VisualElement visualElement)
		{
			visualElement.Q<Label>("Label").text = "";
		}
	}
}
