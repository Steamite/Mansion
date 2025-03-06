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
        [SerializeField] public List<string> AdjacentRooms;


        public Room EnterRoom(Room lastRoom)
        {
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

        public void ExitRoom(IEnumerable<string> enumerable)
        {
            foreach (string room in enumerable)
            {
                SceneManager.UnloadSceneAsync(room);
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
