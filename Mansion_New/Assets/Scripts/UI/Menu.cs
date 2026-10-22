using Assets.Scripts.Interactable_Items.Rooms;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player
{
    public class Menu : MonoBehaviour, IMenu
    {
        [SerializeField] InputAction showMainMenu;
        bool canMove = false;

        public bool CanMove 
        { 
            get => canMove; 
            set => canMove = value; 
        }

        private void Awake()
        {
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
#if !WEB
            // Find all interactors in the scene (or keep a reference to your player's hands)
            var interactors = GetComponentsInChildren<XRBaseInteractor>();

            foreach (var interactor in interactors)
            {
                // Disabling the component forces it to drop anything it is holding
                // and stops its Update loop from looking for the destroyed object.
                interactor.enabled = false;
            }
#endif      
            AddressableSceneManager.LoadScene("Main Menu", SceneType.Menu);
        }
    }
}
