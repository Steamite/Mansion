using Rooms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Interactable_Items.Rooms
{
    public enum SceneType
    {
        Menu,
        Player,
        Room,
        MainRoom,
        Lighting,
    }

    public class AddressableSceneManager : MonoBehaviour
    {
        struct HandleDetails
        {
            public SceneType type;
            public AsyncOperationHandle<SceneInstance> handle;
            public HandleDetails(SceneType type, AsyncOperationHandle<SceneInstance> handle)
            {
                this.type = type;
                this.handle = handle;
            }
        }


        Dictionary<string, HandleDetails> loadedScenes;

        public static bool UseVR { get; set; }
        static AddressableSceneManager instance;

        public static LevelData ActiveLevel
        {
            get;
            private set;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Clear()
        {
            instance = null;
            UseVR = false;
            ActiveLevel = null;
        }

        private void OnApplicationQuit()
        {
            if (loadedScenes == null) return;

            SceneManager.LoadScene(0, LoadSceneMode.Additive);

            // Safely drop the reference counts instantly without awaiting frames
            foreach (var item in loadedScenes.Values)
            {
                if (item.handle.IsValid())
                {
                    Addressables.Release(item.handle);
                }
            }
            loadedScenes.Clear();
        }

        async Awaitable ClearRooms()
        {
            if (loadedScenes == null)
                return;
            List<AsyncOperationHandle> awaitables = new();
            foreach (var item in loadedScenes)
            {
                awaitables.Add(Addressables.UnloadSceneAsync(
                    item.Value.handle,
                    UnloadSceneOptions.UnloadAllEmbeddedSceneObjects,
                    false));
            }

            foreach (var item in awaitables)
            {
                while (!item.IsDone)
                {
                    await Awaitable.EndOfFrameAsync();
                }
                item.Release();
            }
            loadedScenes.Clear();
            try 
            { 
                await Resources.UnloadUnusedAssets();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Awake()
        {
            instance = this;
            loadedScenes = new();
            ActiveLevel = null;
            DontDestroyOnLoad(gameObject);
        }

        public static async Awaitable<SceneInstance> LoadScene(
            string sceneToLoad,
            SceneType sceneType,
            Action<float> proggressAction = null)
            => await instance.WaitForSceneLoad(
                    sceneToLoad,
                    sceneType,
                    proggressAction
                    );

        public static async Awaitable LoadRooms(List<string> scenes)
        {
            foreach (var item in scenes)
            {
                await LoadScene(item, SceneType.Room);
            }
        }

        public static async Awaitable UnloadRooms(List<string> scenes)
        {
            try
            {
                foreach (var item in scenes)
                {
                    await UnloadScene(item);
                }

            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }



        async Awaitable<SceneInstance> WaitForSceneLoad(
            string sceneToLoad, 
            SceneType sceneType, 
            Action<float> proggressAction = null)
        {
            Debug.Log(loadedScenes.Count);

            if (sceneType == SceneType.Room)
                sceneToLoad = ActiveLevel.GetRoomPath(sceneToLoad);
            else if (sceneType == SceneType.Menu)
                await ClearRooms();


            AsyncOperationHandle<SceneInstance> loadHandle =
                    Addressables.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive, false);
            
            while (!loadHandle.IsDone)
            {
                proggressAction?.Invoke(loadHandle.PercentComplete);
                await Awaitable.NextFrameAsync();
            }


            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedScenes.Add(sceneToLoad, new (sceneType, loadHandle));
                
                await loadHandle.Result.ActivateAsync();
                SceneInstance sceneInstance = loadHandle.Result;


                Room loadedRoom;
                switch (sceneType)
                {
                    case SceneType.Menu:
                        break;
                    case SceneType.Player:
                        break;
                    case SceneType.Room:
                        loadedRoom = sceneInstance.Scene.GetRootGameObjects()[0].GetComponent<Room>();
                        loadedRoom.FinishLoad(false);
                        break;
                    case SceneType.MainRoom:
                        loadedRoom = sceneInstance.Scene.GetRootGameObjects()[0].GetComponent<Room>();
                        loadedRoom.FinishLoad(true);
                        break;
                    case SceneType.Lighting:
                        SceneManager.SetActiveScene(sceneInstance.Scene);
                        break;
                }

                Debug.Log($"Loaded Scene: {sceneToLoad}");
                return sceneInstance;
            }
            else
            {
                Debug.LogError($"Failed to load Scene: {sceneToLoad}");
                return default;
            }
        }

        public static async Awaitable UnloadScene(string sceneName)
            => await instance.WaitForSceneUnLoad(sceneName);
        

        async Awaitable WaitForSceneUnLoad(string sceneName)
        {
            if (loadedScenes[sceneName].type == SceneType.Room)
                sceneName = ActiveLevel.GetRoomPath(sceneName);

            AsyncOperationHandle<SceneInstance> unloadHandle = Addressables.UnloadSceneAsync(loadedScenes[sceneName].handle, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects, false);
            loadedScenes.Remove(sceneName);
            await unloadHandle.Task;
            unloadHandle.Release();
        }

        public static void Init(LevelData lData, bool useVR)
        {
            ActiveLevel = lData;
            UseVR = useVR;
        }
    }
}
