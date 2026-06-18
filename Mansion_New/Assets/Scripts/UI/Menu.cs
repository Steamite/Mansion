using Assets.Scripts.Interactable_Items.Rooms;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Assets.Scripts.Player
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] InputAction showMainMenu;
        bool canMove = false;
        private void Awake()
        {
            showMainMenu.Enable();
            canMove = true;
/*
            showMainMenu.performed -= Transition;
            showMainMenu.performed += Transition;*/
        }
        private void OnEnable()
        {
            showMainMenu.Enable();
        }
        private void OnDisable()
        {
            showMainMenu.Disable();
        }

        private void Update()
        {
            if (showMainMenu.triggered && canMove)
            {
                canMove = false;
                Transition();
            }
        }

        public void Transition(/*InputAction.CallbackContext callbackContext*/)
        {
            if (AddressableSceneManager.UseVR)
            {
                // Find all interactors in the scene (or keep a reference to your player's hands)
                var interactors = GetComponentsInChildren<XRBaseInteractor>();

                foreach (var interactor in interactors)
                {
                    // Disabling the component forces it to drop anything it is holding
                    // and stops its Update loop from looking for the destroyed object.
                    interactor.enabled = false;
                }
            }
            AddressableSceneManager.UnloadAll("Main Menu", SceneType.Menu);
        }
    }
}
