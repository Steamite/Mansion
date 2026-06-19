using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.MainMenu.SceneLoader;
using Assets.Scripts.UI.VRMenu;
using Items;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

namespace Assets.Scripts.UI.MainMenu
{
    public class LoadingScreen : BaseLoadingScreen
    {
        public override void StartRoomLoad(object sceneToLoad)
        {
            string sceneName = (string)sceneToLoad;
            AddressableSceneManager.UseVR = useVR;

            if (useVR)
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
    }
}
