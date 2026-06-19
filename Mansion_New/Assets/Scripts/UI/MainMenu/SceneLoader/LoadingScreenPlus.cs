using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.VRMenu;
using Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.MainMenu.SceneLoader
{
    public class LoadingScreenPlus : BaseLoadingScreen
    {
        public override void StartRoomLoad(object lData)
        {
            LevelData levelData = (LevelData)lData;
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
            mainRoom = default;
            loadingScreen.enabled = true;
            ShowControls();
            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            int sceneNumber = levelData.scenes.Count + 1;
            AddressableSceneManager.LoadScene(
                levelData.LightPath, 
                SceneType.Lighting, 
                (percent) => progressBar.value = percent / sceneNumber,
                (_) => LoadScenePart(levelData, 0, sceneNumber, progressBar));
            /*AddressableSceneManager.LoadScene();
            AddressableSceneManager.LoadScene(
                sceneName,
                SceneType.Room,
                (percent) => progressBar.value = percent / 2,
                LoadPlayer);*/
        }
        SceneInstance mainRoom;
        void LoadScenePart(LevelData levelData, int i, int sceneNumber, ProgressBar progressBar)
        {
            float percentBase = (i+1) * (1 / sceneNumber);
            AddressableSceneManager.LoadScene(
                levelData.GetRoomPath(i), 
                SceneType.Room, 
                (percent) => progressBar.value = percentBase + percent / sceneNumber,
                (scene) =>
                {
                    if (i == levelData.initScene)
                        mainRoom = scene;

                    i++;
                    if (i < levelData.scenes.Count)
                        LoadScenePart(levelData, i, sceneNumber, progressBar);
                    else
                        LoadPlayer(mainRoom);
                });
        }
    }
}
