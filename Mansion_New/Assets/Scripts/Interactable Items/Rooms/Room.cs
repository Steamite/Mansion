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

        /// <summary>
        /// Oneshot audio
        /// </summary>
        public AudioClip enterAudio;
        /// <summary>
        /// Loop - edit 
        /// </summary>
        public AudioClip ambientAudio;

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
            InteractableItem[] interactableItems = Interactables.GetComponentsInChildren<InteractableItem>();
            foreach (var item in interactableItems)
            {
                item.LoadContent();
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Debug.Log("Start sound Load" + name);
            PreLoadAudio(ambientAudio);
            PreLoadAudio(enterAudio);
            Debug.Log("Sound Load finished" + name);

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#if !WEB
            VRManagerLink.OnRoomLoad(transform);
#endif 
        }
        async Awaitable PreLoadAudio(AudioClip clip)
        {
            if (clip.loadState != AudioDataLoadState.Loaded)
            {
                clip.LoadAudioData();
            }
        }

        private void OnDestroy()
        {
            if (ambientAudio != null)
                ambientAudio.UnloadAudioData();
            if (enterAudio != null)
                enterAudio.UnloadAudioData();
        }


        public bool entered = false;
        /// <summary>
        /// Loads new rooms and unloads the old ones that are not needed.
        /// </summary>
        /// <param name="previousRoom">Previous active room, null if leaving the starting location.</param>
        /// <returns>itself</returns>
        public async Awaitable EnterRoom(Room previousRoom)
        {
            Debug.Log("enter room");


            SoundManager.instance.EnterRoom(enterAudio, ambientAudio, entered);
            entered = true;


            if (previousRoom)
            {
                List<string> roomsToUnload = previousRoom.AdjacentRooms.Where(q => !AdjacentRooms.Contains(q) && q != name).ToList();
                previousRoom.ToggleEntrances(true);
                await AddressableSceneManager.UnloadRooms(roomsToUnload);

                List<string> roomsToLoad = AdjacentRooms.Where(
                        loadRoom =>
                        !previousRoom.AdjacentRooms.Contains(loadRoom) &&
                        loadRoom != previousRoom.name
                        ).ToList();

                await AddressableSceneManager.LoadRooms(roomsToLoad);
                ToggleEntrances(false);
            }
            else
            {
                await AddressableSceneManager.LoadRooms(AdjacentRooms);
                ToggleEntrances(false);
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
