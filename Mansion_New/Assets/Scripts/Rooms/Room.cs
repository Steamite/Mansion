using Rooms;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        /// <summary>
        /// Loads new rooms and unloads the old ones that are not needed.
        /// </summary>
        /// <param name="lastRoom">Previus active room.</param>
        /// <returns>itself</returns>
        public Room EnterRoom(Room lastRoom)
        {

			Debug.Log("enter room");
			return this;
            if (lastRoom)
            {
                lastRoom.ExitRoom(lastRoom.AdjacentRooms.Where(unloadRoom => !AdjacentRooms.Contains(unloadRoom) && unloadRoom != gameObject.scene.name));
                foreach (string asset in AdjacentRooms.Where(loadRoom => !lastRoom.AdjacentRooms.Contains(loadRoom) && loadRoom != lastRoom.gameObject.scene.name).ToList())
                {
                    SceneManager.LoadSceneAsync(asset, LoadSceneMode.Additive);
                }
            }
            else
            {
                foreach (string asset in AdjacentRooms)
                {
                    SceneManager.LoadSceneAsync(asset, LoadSceneMode.Additive);
                }
            }


            ToggleEntrances(false);
            return this;
            //Disable all entrances and wall enable colliders
        }

        /// <summary>
        /// Unloads unnedded scenes and enables it's own entrances.
        /// </summary>
        /// <param name="enumerable">Scenes to unload</param>
        public void ExitRoom(IEnumerable<string> enumerable)
        {
            foreach (string room in enumerable)
            {
                SceneManager.UnloadSceneAsync(room);
            }
            ToggleEntrances(true);
        }

        /// <summary>
        /// Toggles states of entrances.
        /// </summary>
        /// <param name="state">New state to toggle to.</param>
        void ToggleEntrances(bool state)
        {
            foreach (BoxCollider collider in transform.GetChild(1).GetComponentsInChildren<BoxCollider>())
            {
                collider.enabled = state;
            }
        }
    }

}
