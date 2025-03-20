using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace Items
{
    /// <summary>
    /// Base class for all interactable items, has data for inspection.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableItem : MonoBehaviour
	{
		/// <summary>Display name of the item.</summary>
		[SerializeField] public string ItemName;
		/// <summary>Inner and Outer limits of zoom.</summary>
        [SerializeField][MinMaxRangeSlider(0.5f, 5)] public Vector2 radiusRange;
		/// <summary>Path for downloading the content.</summary>
        [SerializeField] public string SourcePath = "";

        /// <summary>
        /// Each type of Item must has it's own implementation for dowloading and viewing content.
        /// </summary>
        /// <param name="element">Content element.</param>
        public abstract void LoadContent(VisualElement element);

        public abstract void Unload(VisualElement visualElement);
	}
}
