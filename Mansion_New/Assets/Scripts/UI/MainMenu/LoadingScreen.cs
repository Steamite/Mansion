using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.VRMenu;
using Items;
using Player;
using Unity.VectorGraphics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.MainMenu
{
    internal class LoadingScreen : MonoBehaviour
    {

        [SerializeField] UIDocument loadingScreen;
        bool canExit = false;

        [SerializeField] bool useVR = false;
        [SerializeField] VRInteractionInit vrInteractabeInitPrefab;

        [SerializeField] InputAction useAction;

        void ShowControls()
        {
            VisualElement controls = loadingScreen.rootVisualElement.Q<VisualElement>("ContolsOverview");
            KeybindOverview a;
            controls.Add(a = new KeybindOverview());
            a.LoadKeybinds();
        }

        public void StartRoomLoad(string sceneName)
        {
            if(useVR)
            {
                if (VRManagerLink.VRManager == null)
                {
                    VRInteractionInit vrManager = Instantiate(vrInteractabeInitPrefab);
                    VRManagerLink.VRManager = vrManager;
                    DontDestroyOnLoad(vrManager);
                }
            }
            else
            {
                VRManagerLink.DestroyManager();
            }

            loadingScreen.enabled = true;
            ShowControls();

            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            AddressableSceneManager.LoadScene(
                sceneName,
                SceneType.Room,
                (percent) => progressBar.value = percent / 2,
                LoadPlayer);
        }

        void LoadPlayer(SceneInstance scene)
        {
            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            AddressableSceneManager.LoadScene(
                useVR ? "Player VR" : "Player",
                SceneType.Player,
                (percent) => progressBar.value = 0.5f + percent / 2,
                FinishLoadVisual
                );
        }

        void FinishLoadVisual(SceneInstance scene)
        {
            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            progressBar.value = 1;
            progressBar.title = "Načteno";//"Loaded";


            VisualElement l = loadingScreen.rootVisualElement.Q<Label>("Label");
            l.RegisterCallback<TransitionEndEvent>(
                (_) => TextFlashingTransitionToggle((VisualElement)_.target));
            TextFlashingTransitionToggle(l);

            canExit = true;
            useAction.Enable();
            useAction.performed += (_) => UnloadMainMenu();;
        }

        void TextFlashingTransitionToggle(VisualElement l)
        {
            l.ToggleInClassList("disabledText");
        }

        private void UnloadMainMenu()
        {
            if (canExit == false)
                return;

            canExit = false;
            useAction.Disable();

            AddressableSceneManager.UnloadScene("Main Menu");
            PlayerMovement.Activate();
        }
    }
}
