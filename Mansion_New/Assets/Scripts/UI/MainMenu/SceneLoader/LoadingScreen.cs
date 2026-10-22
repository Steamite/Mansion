using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.MainMenu.SceneLoader;
using Assets.Scripts.UI.VRMenu;
using Items;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.MainMenu
{
    public class LoadingScreen : BaseLoadingScreen
    {
        public override async Awaitable StartRoomLoad(object sceneToLoad)
        {
            string sceneName = (string)sceneToLoad;

#if WEB
            //VRManagerLink.DestroyManager();
#else
            if (VRManagerLink.VRManager == null)
            {
                VRInteractionInit vrManager = Instantiate(vrInteractabeInitPrefab);
                VRManagerLink.VRManager = vrManager;
                DontDestroyOnLoad(vrManager);
            }
#endif

            loadingScreen.enabled = true;
            ShowControls();

            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            var a = await AddressableSceneManager.LoadScene(
                sceneName,
                SceneType.MainRoom,
                (percent) => progressBar.value = percent / 2);
            await LoadPlayer(a);
        }
    }
}
