using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.Player;
using Assets.Scripts.Player.RoomEntrance;
using Assets.Scripts.UI.VRMenu;
using Items;
using Player;
using Rooms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.MainMenu.SceneLoader
{
    public abstract class BaseLoadingScreen : MonoBehaviour
    {
        [SerializeField] protected UIDocument loadingScreen;
        protected bool canExit = false;
        /*[SerializeField] protected bool useVR = false;
        public bool UseVR => useVR;
*/
        [SerializeField] protected VRInteractionInit vrInteractabeInitPrefab;

        [SerializeField] protected InputAction useAction;

        protected Vector3 spawnPosition;

        protected void ShowControls()
        {
            VisualElement controls = loadingScreen.rootVisualElement.Q<VisualElement>("ContolsOverview");
            KeybindOverview a;
            controls.Add(a = new KeybindOverview());
            a.LoadKeybinds();
        }
        protected async Awaitable LoadPlayer(SceneInstance instance)
        {
            EntranceDetector.mainRoom = instance.Scene.GetRootGameObjects()[0].GetComponent<Room>();

            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
#if WEB
            var scene = await AddressableSceneManager.LoadScene(
                "Player",
                SceneType.Player,
                (percent) => progressBar.value = 0.5f + percent / 2);
            FinishLoadVisual(scene);

#else
                FinishLoadVisual(default);
#endif
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
            useAction.performed += (_) => UnloadMainMenu();
        }

        protected void TextFlashingTransitionToggle(VisualElement l)
        {
            l.ToggleInClassList("disabledText");
        }

        async protected Awaitable UnloadMainMenu()
        {
            if (canExit == false)
                return;

            canExit = false;
            useAction.Disable();

            Transform playerTransform = await EntranceDetector.Activate(); // GameObject.FindGameObjectWithTag("Player").transform;
            playerTransform.position = spawnPosition;

#if WEB
            PlayerMovement.Activate();
#else

            Camera.main.cullingMask = -1;
            playerTransform.GetComponentInChildren<GravityProvider>().enabled = true;
#endif
            await AddressableSceneManager.UnloadScene("Main Menu");
        }

        public abstract Awaitable StartRoomLoad(object sceneToload);
    }
}
