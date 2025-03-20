using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Handles transitions off the croshair.
    /// </summary>
    [UxmlElement]
    public partial class CrosshairImage : VisualElement
    {
        /// <summary>Instance so it can be used from everywhere.</summary>
        static CrosshairImage instance;
        /// <summary>Copy of self for toggling during interaction.</summary>
        static VisualTreeAsset document = null;

		public CrosshairImage()
        {
            Add(new());
            instance = this;
        }

        /// <summary>Transitions to active state</summary>
        public static void Enter()
            => instance.AddToClassList("Active");

        /// <summary>Transitions from active state</summary>
        public static void Exit()
        {
            instance.RemoveFromClassList("Active");
            EndHold();
        }

        /// <summary>Starts filling the crosshair.</summary>
        public static void StartHold()
            => instance.AddToClassList("Holding");
        /// <summary>Ends the filling effect.</summary>
        public static void EndHold()
			=> instance.RemoveFromClassList("Holding");

        /// <summary>Toggles by removing treeAsset from UIDoc component.</summary>
        public static void Toggle()
		{
            if(document == null)
            {
                EndHold();
                document = GameObject.Find("UI").GetComponent<UIDocument>().visualTreeAsset;
                GameObject.Find("UI").GetComponent<UIDocument>().visualTreeAsset = null;
            }
            else
            {
                GameObject.Find("UI").GetComponent<UIDocument>().visualTreeAsset = document;
                document = null;
            }
        }
    }
}