using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.Player.RoomEntrance;
using Assets.Scripts.UI.VRMenu;
using ImageMagick;
using Items;
using Player;
using Rooms;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.MainMenu.SceneLoader
{
    public class LoadingScreenPlus : BaseLoadingScreen
    {
        public override async Awaitable StartRoomLoad(object lData)
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
            EntranceDetector.mainRoom = null;

            loadingScreen.enabled = true;
            ShowControls();
            ProgressBar progressBar = loadingScreen.rootVisualElement.Q<ProgressBar>();
            try
            {
                await LoadLevel(level, progressBar);
                await LoadPlayer(mainScene);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

        }
        SceneInstance mainScene;

        async Awaitable LoadLevel(LevelData levelData, ProgressBar progressBar)
        {
            int sceneNumber = levelData.scenes.Count + 1;

            await AddressableSceneManager.LoadScene(
                levelData.LightPath,
                SceneType.Lighting,
                (percent) => progressBar.value = percent / sceneNumber);


            spawnPosition = levelData.spawn;
            int i = levelData.initScene;

            mainScene = await AddressableSceneManager.LoadScene(
                levelData.GetRoomPath(i),
                SceneType.MainRoom,
                (percent) => progressBar.value = 0.5f + percent / 2);
        }
    }
}
