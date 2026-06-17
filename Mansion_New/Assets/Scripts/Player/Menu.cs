using Assets.Scripts.Interactable_Items.Rooms;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

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
            AddressableSceneManager.UnloadAll("Main Menu", SceneType.Menu);
        }
    }
}
