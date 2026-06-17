using Items;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Scripts.UI.VRMenu
{
    public class VRInspectionManager : MonoBehaviour
    {
        const string TITLE_LABEL = "Title";

        UIDocument hoverDocument;
        VisualElement hoverRoot;

        UIDocument openDocument;
        VisualElement openRoot;


        int hoverNumber = 0;

        InteractableItem item;

        private void Awake()
        {
            hoverDocument = transform.GetChild(0).GetComponent<UIDocument>();
            hoverDocument.enabled = true;
            hoverRoot = hoverDocument.rootVisualElement;
            hoverRoot.style.display = DisplayStyle.None;

            openDocument = transform.GetChild(1).GetComponent<UIDocument>();
            openDocument.enabled = true;
            openRoot = openDocument.rootVisualElement;
            openRoot.style.display = DisplayStyle.None;
        }

        public void Hover(HoverEnterEventArgs hoverEnter)
        {
            hoverNumber++;
            if (hoverNumber > 1)
                return;
            item = hoverEnter.interactableObject.transform
                .GetComponent<InteractableItem>();

            transform.SetPositionAndRotation(
                item.transform.position + item.OffsetVR, 
                Quaternion.Euler(
                    item.transform.rotation.eulerAngles + new Vector3(0,180,0)
                    )
                );

            hoverRoot.style.display = DisplayStyle.Flex;
            hoverRoot.Q<Label>(TITLE_LABEL).text = item.ItemName;
            item.LoadContent(hoverDocument.rootVisualElement);
        }
        public void EndHover(HoverExitEventArgs hoverExit)
        {
            hoverNumber--;
            if (hoverNumber > 0)
                return;

            item.Clear();

            if (hoverDocument.rootVisualElement == null)
                return;

            hoverRoot.style.display = DisplayStyle.None;
            hoverRoot.Q<Label>(TITLE_LABEL).text = "";
        }


        public void Open(SelectEnterEventArgs arg0)
        {
            hoverNumber = 0;
            hoverRoot.style.display = DisplayStyle.None;
            
            openRoot.style.display = DisplayStyle.Flex;
            item = arg0.interactableObject.transform
                .GetComponent<InteractableItem>();

            openRoot.Q<Label>("Title").text = item.ItemName;
            item.LoadContent(openRoot);
        }
    }
}
