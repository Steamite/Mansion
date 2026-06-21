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

    /// Hiearchy
    /// root
    ///     - Interactables 0
    ///         - ...
    ///     - Colliders 1
    ///         - Walls
    ///             - ...
    ///         - Floors (Teleportation Area)
    ///             - ...
    ///     - Entrances 2
    ///         - ...
    ///     - Lighting 3
    ///         - Adaptive Probe Volume
    ///         - Lights
    ///             - ...
    ///     - SpawnPoint 4 
        
    /// <summary>
    /// Handles transitions between rooms.
    /// </summary>
    public class Room : MonoBehaviour
    {
        public List<string> AdjacentRooms = new();
        
        public Transform Interactables => transform.GetChild(0);

        public Transform Colliders => transform.GetChild(1);
        public Transform Walls => Colliders.childCount > 0 
            ? Colliders.GetChild(0) 
            : null;
        public Transform Floors => Colliders.childCount > 1 
            ? Colliders.GetChild(1) 
            : null;

        public Transform Entrances => transform.GetChild(2);
        public Transform SpawnPoint => transform.GetChild(4);
        public Vector3 centerOffset;

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
                List<string> roomsToUnload = previousRoom.AdjacentRooms.Where(q => !AdjacentRooms.Contains(q) && q != name).ToList();
                previousRoom.ToggleEntrances(true);
                AddressableSceneManager.UnloadRooms(roomsToUnload, () =>
                {
                    List<string> roomsToLoad = AdjacentRooms.Where(
                        loadRoom =>
                        !previousRoom.AdjacentRooms.Contains(loadRoom) &&
                        loadRoom != previousRoom.name
                        ).ToList();

                    AddressableSceneManager.LoadRooms(roomsToLoad,
                        () => ToggleEntrances(false));
                });
            }
            else
            {
                AddressableSceneManager.LoadRooms(AdjacentRooms, () => ToggleEntrances(false));
            }
        }


        /// <summary>
        /// Toggles states of entrances.
        /// </summary>
        /// <param name="state">New state to toggle to.</param>
        void ToggleEntrances(bool state)
        {
            foreach (BoxCollider collider in Entrances.GetComponentsInChildren<BoxCollider>())
            {
                collider.enabled = state;
            }
        }
    }

}
