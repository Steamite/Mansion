using Assets.Scripts.Interactable_Items.Rooms;
using Items;
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
        public List<string> AdjacentRooms = new();
        public string roomName;
        public Transform entrances;

        public void FinishLoad(bool startingRoom)
        {
            ToggleEntrances(!startingRoom);
            VRManagerLink.OnRoomLoad(transform);
        }

        /// <summary>
        /// Loads new rooms and unloads the old ones that are not needed.
        /// </summary>
        /// <param name="previousRoom">Previous active room, null if leaving the starting location.</param>
        /// <returns>itself</returns>
        public void EnterRoom(Room previousRoom)
        {
            Debug.Log("enter room");
            if (previousRoom)
            {
                List<string> roomsToUnload = previousRoom.AdjacentRooms.Where(q => !AdjacentRooms.Contains(q) && q != roomName).ToList();
                previousRoom.ExitRoom(roomsToUnload);
                List<string> roomsToLoad = AdjacentRooms.Where(loadRoom => !previousRoom.AdjacentRooms.Contains(loadRoom) && loadRoom != previousRoom.gameObject.scene.name).ToList();
                foreach (string asset in roomsToLoad)
                {
                    AddressableSceneManager.LoadScene(asset, SceneType.Room);
                }
            }
            else
            {
                foreach (string asset in AdjacentRooms)
                {
                    AddressableSceneManager.LoadScene(asset, SceneType.Room);
                }
            }

            //Disable all entrances and wall colliders
            ToggleEntrances(false);
        }

        /// <summary>
        /// Unloads unnedded scenes and enables it's own entrances.
        /// </summary>
        /// <param name="RoomsToUnload">Scenes to unload</param>
        void ExitRoom(IEnumerable<string> RoomsToUnload)
        {
            foreach (string roomName in RoomsToUnload)
            {
                AddressableSceneManager.UnloadScene(roomName);
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
