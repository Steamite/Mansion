using Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Assets.Scripts.UI.VRMenu
{
    [RequireComponent(typeof(VRInspectionManager))]
    public class VRInteractionInit : MonoBehaviour, IVRInteractionInit
    {
        VRInspectionManager manager;
        private void Awake()
        {
            manager = GetComponent<VRInspectionManager>();
        }

        public void SetupItem(InteractableItem item)
        {
            XRBaseInteractable xRBase;
            if(item.Rotatable)
                xRBase = item.gameObject
                .AddComponent<XRGrabInteractable>();
            else
                xRBase = item.gameObject
                .AddComponent<XRSimpleInteractable>();

            FillListeners(item, xRBase);
        }

        void FillListeners(InteractableItem item, XRBaseInteractable xRBase)
        {
            if(item is PDFItem)
            {
                xRBase.selectEntered.AddListener(manager.Open);
                //xRBase.selectExited.AddListener(manager.Close);
            }
            else
            {
                xRBase.hoverEntered.AddListener(manager.Hover);
                xRBase.hoverExited.AddListener(manager.EndHover);
            }
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
