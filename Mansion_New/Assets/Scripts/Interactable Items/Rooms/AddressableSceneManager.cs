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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Clear()
        {
            instance = null;
            UseVR = false;
        }

        private void Awake()
        {
            instance = this;
            loadedScenes = new();
            DontDestroyOnLoad(gameObject);
        }

        public static void LoadScene(string sceneToLoad, SceneType sceneType, Action<float> proggressAction = null, Action<SceneInstance> onFinish = null) 
            => instance.StartCoroutine(
                instance.WaitForSceneLoad(
                    sceneToLoad, 
                    sceneType, 
                    proggressAction, 
                    onFinish
                    )
                );

        IEnumerator WaitForSceneLoad(string sceneToLoad, SceneType sceneType, Action<float> proggressAction = null, Action<SceneInstance> onFinish = null)
        {
            Debug.Log(loadedScenes.Count);

            AsyncOperationHandle<SceneInstance> loadHandle =
                    Addressables.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive, false);

            while (!loadHandle.IsDone)
            {
                proggressAction?.Invoke(loadHandle.PercentComplete);
                yield return null;
            }


            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedScenes.Add(sceneToLoad, new (sceneType, loadHandle));
                
                yield return loadHandle.Result.ActivateAsync();
                SceneInstance instance = loadHandle.Result;

                switch (sceneType)
                {
                    case SceneType.Menu:
                        break;
                    case SceneType.Player:

                        break;
                    case SceneType.Room:
                        Room loadedRoom = instance.Scene.GetRootGameObjects()[0].GetComponent<Room>();
                        loadedRoom.FinishLoad(loadedScenes.Count(q=> q.Value.type == SceneType.Room) == 1);
                        break;
                }

                Debug.Log($"Loaded Scene: {sceneToLoad}");

                // Invoke custom action (allows async chaining)
                onFinish?.Invoke(instance);
            }
            else
            {
                Debug.LogError($"Failed to load Scene: {sceneToLoad}");
            }
        }

        public static void UnloadScene(string sceneName, Action onUnload = null)
            => instance.StartCoroutine(instance.WaitForSceneUnLoad(sceneName, onUnload));

        IEnumerator WaitForSceneUnLoad(string roomName, Action onUnload = null)
        {
            AsyncOperationHandle<SceneInstance> unloadHandle = Addressables.UnloadSceneAsync(loadedScenes[roomName].handle);
            yield return unloadHandle;

            loadedScenes.Remove(roomName);
            onUnload?.Invoke();
        }

        public static void UnloadAll(string newScene, SceneType type)
        {
            instance.StartCoroutine(instance.UnloadScenes(newScene,type));
        }

        IEnumerator UnloadScenes(string newScene, SceneType type)
        {
            Dictionary<string, HandleDetails> toUnload = loadedScenes;
            loadedScenes = new();

            yield return WaitForSceneLoad(newScene, type);
            foreach (var roomName in toUnload.Values)
            {
                var handle = Addressables.UnloadSceneAsync(roomName.handle);

                yield return handle;
            }
            Debug.Log(loadedScenes.Count);
        }
    }
}
