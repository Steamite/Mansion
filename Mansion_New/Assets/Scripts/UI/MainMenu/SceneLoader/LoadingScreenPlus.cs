using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.VRMenu;
using ImageMagick;
using Items;
using Player;
using Rooms;
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
            LevelData level = (LevelData)lData;
            AddressableSceneManager.Init(level, useVR);

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
            PlayerMovement.mainRoom = default;
            loadingScreen.enabled = true;
            ShowControls();
            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            int sceneNumber = level.scenes.Count + 1;
            AddressableSceneManager.LoadScene(
                level.LightPath,
                SceneType.Lighting,
                (percent) => progressBar.value = percent / sceneNumber,
                (_) => LoadPath(level, progressBar));//LoadScenePart(levelData, 0, sceneNumber, progressBar));

            /*AddressableSceneManager.LoadScene();
            AddressableSceneManager.LoadScene(
                sceneName,
                SceneType.Room,
                (percent) => progressBar.value = percent / 2,
                LoadPlayer);*/
        }
        SceneInstance mainScene;

        void LoadPath(LevelData levelData, ProgressBar progressBar)
        {
            spawnPosition = levelData.spawn;
            int i = levelData.initScene;

            AddressableSceneManager.LoadScene(
                levelData.GetRoomPath(i),
                SceneType.MainRoom,
                (percent) => progressBar.value = 0.5f + percent / 2,
                (scene) =>
                {
                    mainScene = scene;

                    /*List<string> rooms = scene.Scene
                        .GetRootGameObjects()[0]
                        .GetComponent<Room>().AdjacentRooms;

                    if (rooms.Count > 0)
                        LoadScenePart(levelData, 0, rooms, progressBar);
                    else*/
                        LoadPlayer(mainScene);
                });
        }

        /*void LoadScenePart(LevelData levelData, int i, List<string> scenesToLoad, ProgressBar progressBar)
        {
            float percentBase = i * (1 / scenesToLoad.Count);

            string path = levelData.GetRoomPath(scenesToLoad[i]);
            AddressableSceneManager.LoadScene(
                path,
                SceneType.Room, 
                (percent) => progressBar.value = percentBase + percent / scenesToLoad.Count,
                (scene) =>
                {
                    i++;
                    if (i < scenesToLoad.Count)
                        LoadScenePart(levelData, i, scenesToLoad, progressBar);
                    else
                        LoadPlayer(mainScene);
                });
        }*/
    }
}
