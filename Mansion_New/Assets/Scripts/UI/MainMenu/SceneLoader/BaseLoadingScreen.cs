using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.VRMenu;
using Items;
using Player;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

namespace Assets.Scripts.UI.MainMenu.SceneLoader
{
    public abstract class BaseLoadingScreen : MonoBehaviour
    {
        [SerializeField] protected UIDocument loadingScreen;
        protected bool canExit = false;

        [SerializeField] protected bool useVR = false;
        public bool UseVR => useVR;
        [SerializeField] protected VRInteractionInit vrInteractabeInitPrefab;

        [SerializeField] protected InputAction useAction;

        protected void ShowControls()
        {
            VisualElement controls = loadingScreen.rootVisualElement.Q<VisualElement>("ContolsOverview");
            KeybindOverview a;
            controls.Add(a = new KeybindOverview());
            a.LoadKeybinds();
        }
        protected void LoadPlayer(SceneInstance scene)
        {
            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            if (!useVR)
                AddressableSceneManager.LoadScene(
                    "Player",
                    SceneType.Player,
                    (percent) => progressBar.value = 0.5f + percent / 2,
                    FinishLoadVisual
                    );
            else
            {

                FinishLoadVisual(new());
            }
        }

        protected void FinishLoadVisual(SceneInstance _)
        {
            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            progressBar.value = 1;
            progressBar.title = "Načteno";


            VisualElement l = loadingScreen.rootVisualElement.Q<Label>("Label");
            l.RegisterCallback<TransitionEndEvent>(
                (_) => TextFlashingTransitionToggle((VisualElement)_.target));
            TextFlashingTransitionToggle(l);

            canExit = true;
            useAction.Enable();
            useAction.performed += (_) => UnloadMainMenu(); ;
        }

        protected void TextFlashingTransitionToggle(VisualElement l)
        {
            l.ToggleInClassList("disabledText");
        }

        protected void UnloadMainMenu()
        {
            if (canExit == false)
                return;

            canExit = false;
            useAction.Disable();

            if (useVR)
            {
                Camera.main.cullingMask = -1;
                GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<GravityProvider>().enabled = true;
            }
            GameObject.FindGameObjectWithTag("Player").transform.position
                = Rooms.Room.StartingRoom.SpawnPoint.transform.position;

            AddressableSceneManager.UnloadScene("Main Menu");
            PlayerMovement.Activate();
        }
        public abstract void StartRoomLoad(object sceneToload);
    }
}
