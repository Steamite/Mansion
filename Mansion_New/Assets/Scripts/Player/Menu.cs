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
        
        private void Awake()
        {
            showMainMenu.Enable();

            showMainMenu.performed -= Transition;
            showMainMenu.performed += Transition;
        }
        public void Transition(InputAction.CallbackContext callbackContext)
        {
            AddressableSceneManager.UnloadAll("Main Menu", SceneType.Menu);
        }
    }
}
