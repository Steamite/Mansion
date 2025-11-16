using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Rooms
{
    /// <summary>
    /// Handles transitions between rooms.
    /// </summary>
    public class Room : MonoBehaviour
    {
        [SerializeField] public List<string> AdjacentRooms = new();
        [SerializeField] public string roomName;
        [SerializeField] public Transform entrances;
        public static Dictionary<string, AsyncOperationHandle<SceneInstance>> loadedScenes;

        IEnumerator LoadRoom(string asset)
        {
            AsyncOperationHandle<SceneInstance> initialLoad =
                    Addressables.LoadSceneAsync($"{asset}", LoadSceneMode.Additive, false);
            yield return initialLoad;
            if (initialLoad.Status == AsyncOperationStatus.Succeeded)
            {
                yield return initialLoad.Result.ActivateAsync();
                loadedScenes.Add(asset, initialLoad);
                initialLoad.Result.Scene.GetRootGameObjects()[0].GetComponent<Room>().ToggleEntrances(true);
            }
        }

        /// <summary>
        /// Loads new rooms and unloads the old ones that are not needed.
        /// </summary>
        /// <param name="lastRoom">Previus active room.</param>
        /// <returns>itself</returns>
        public IEnumerator EnterRoom(Room lastRoom)
        {
            Debug.Log("enter room");
            if (lastRoom)
            {
                List<string> roomsToUnload = lastRoom.AdjacentRooms.Where(q => !AdjacentRooms.Contains(q) && q != gameObject.scene.name).ToList();
                yield return lastRoom.ExitRoom(roomsToUnload);
                List<string> roomsToLoad = AdjacentRooms.Where(loadRoom => !lastRoom.AdjacentRooms.Contains(loadRoom) && loadRoom != lastRoom.gameObject.scene.name).ToList();
                foreach (string asset in roomsToLoad)
                {
                    yield return LoadRoom(asset);
                }
            }
            else
            {
                foreach (string asset in AdjacentRooms)
                {
                    yield return LoadRoom(asset);
                }
            }


            ToggleEntrances(false);
            yield return this;
            //Disable all entrances and wall enable colliders
        }

        /// <summary>
        /// Unloads unnedded scenes and enables it's own entrances.
        /// </summary>
        /// <param name="enumerable">Scenes to unload</param>
        public IEnumerator ExitRoom(IEnumerable<string> enumerable)
        {
            foreach (string asset in enumerable)
            {
                AsyncOperationHandle<SceneInstance> instance = Addressables.UnloadSceneAsync(loadedScenes[asset]);
                yield return instance.Result;
                loadedScenes.Remove(asset);
            }
            ToggleEntrances(true);
        }

        /// <summary>
        /// Toggles states of entrances.
        /// </summary>
        /// <param name="state">New state to toggle to.</param>
        void ToggleEntrances(bool state)
        {
            foreach (BoxCollider collider in entrances.GetComponentsInChildren<BoxCollider>())
            {
                collider.enabled = state;
            }
        }
    }

}
