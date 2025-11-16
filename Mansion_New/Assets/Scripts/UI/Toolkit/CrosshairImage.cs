using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Handles transitions off the croshair.
    /// </summary>
    [UxmlElement]
    public partial class CrosshairImage : VisualElement, ICrosshairImage
    {
#pragma warning restore UDR0001 // Domain Reload Analyzer

        public CrosshairImage()
        {
            Add(new());
        }

        /// <summary>Transitions to active state</summary>
        public void Enter()
            => AddToClassList("Active");

        /// <summary>Transitions from active state</summary>
        public void Exit()
        {
            RemoveFromClassList("Active");
            EndHold();
        }

        /// <summary>Starts filling the crosshair.</summary>
        public void StartHold()
            => AddToClassList("Holding");
        /// <summary>Ends the filling effect.</summary>
        public void EndHold()
            => RemoveFromClassList("Holding");

        /// <summary>Toggles by removing treeAsset from UIDoc component.</summary>
        public void Toggle(bool show)
        {
            if (!show)
            {
                this.parent.parent.style.display = DisplayStyle.None;
                EndHold();
            }
            else
            {
                this.parent.parent.style.display = DisplayStyle.Flex;
                Enter();
            }
        }
    }
}