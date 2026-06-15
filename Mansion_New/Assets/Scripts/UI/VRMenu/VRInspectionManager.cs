using Items;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Scripts.UI.VRMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class VRInspectionManager : MonoBehaviour
    {
        const string TITLE_LABEL = "Title";
        UIDocument inspectionDocument;

        private void Awake()
        {
            inspectionDocument = GetComponent<UIDocument>();
        }

        public void Hover(HoverEnterEventArgs hoverEnter)
        {
            InteractableItem item = hoverEnter.interactableObject.transform
                .GetComponent<InteractableItem>();

            transform.position = item.rotationVR;
            transform.rotation = item.transform.rotation;
            transform.Translate(new(0.5f, 0, 0));

            inspectionDocument.enabled = true;
            inspectionDocument.rootVisualElement.Q<Label>(TITLE_LABEL).text = item.ItemName;
            item.LoadContent(inspectionDocument.rootVisualElement);
        }
        public void EndHover(HoverExitEventArgs hoverExit)
        {
            inspectionDocument.enabled = false;

            inspectionDocument.rootVisualElement.Q<Label>(TITLE_LABEL).text = "";

        }

    }
}
