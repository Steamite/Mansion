using Rooms;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rooms
{
    public class Room : MonoBehaviour
    {
        //[SerializeField] RoomData data;
        [SerializeField] List<SceneAsset> AdjacentRooms;

        public Room EnterRoom(Room lastRoom)
        {
            if (lastRoom)
            {
                lastRoom.ExitRoom(lastRoom.AdjacentRooms.Where(unloadRoom => !AdjacentRooms.Contains(unloadRoom) && unloadRoom.name != gameObject.scene.name));
                foreach (SceneAsset asset in AdjacentRooms.Where(loadRoom => !lastRoom.AdjacentRooms.Contains(loadRoom) && loadRoom.name != lastRoom.gameObject.scene.name).ToList())
                {
                    SceneManager.LoadSceneAsync(asset.name, LoadSceneMode.Additive);
                }
            }
            else
            {
                foreach (SceneAsset asset in AdjacentRooms)
                {
                    SceneManager.LoadSceneAsync(asset.name, LoadSceneMode.Additive);
                }
            }


            ToggleEntrances(false);
            return this;
            //Disable all entrances and wall enable colliders
        }

        public void ExitRoom(IEnumerable<SceneAsset> enumerable)
        {
            foreach (SceneAsset room in enumerable)
            {
                SceneManager.UnloadSceneAsync(room.name);
            }
            ToggleEntrances(true);
        }

        void ToggleEntrances(bool state)
        {
            foreach (BoxCollider collider in transform.GetChild(1).GetComponentsInChildren<BoxCollider>())
            {
                collider.enabled = state;
            }
        }
    }

}
