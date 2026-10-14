using Rooms;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

namespace Assets.Scripts.Player.RoomEntrance
{
    public class EntranceDetector : MonoBehaviour, INotifyBindablePropertyChanged
    {
        public static Room mainRoom = null;
        
        /// <summary>Active room for displayText under the minimap.</summary>
        [CreateProperty] public Room ActiveRoom;
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        static EntranceDetector instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Clear()
        {
            mainRoom = null;
            instance = null;
        }
        private void Awake()
        {
            instance = this;
        }

        public static async Awaitable<Transform> Activate()
            => await instance.Init();

        async Awaitable<Transform> Init()
        {
            ActiveRoom = mainRoom;
            propertyChanged?.Invoke(this, new(nameof(ActiveRoom)));

            await ActiveRoom.EnterRoom(null);
            return transform;
        }
        /// <summary>
        /// Checks for entering different rooms.
        /// </summary>
        /// <param name="hit">The object that was hit.</param>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.CompareTag("Entrance"))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                EntranceEnter(hit.transform);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
        private void OnTriggerEnter(Collider hit)
        {
            if (hit.gameObject.CompareTag("Entrance"))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                EntranceEnter(hit.transform);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        async Awaitable EntranceEnter(Transform entranceTransform)
        {
            Room newRoom = entranceTransform.parent.parent.GetComponent<Room>();
            await newRoom.EnterRoom(ActiveRoom);

            ActiveRoom = newRoom;
            propertyChanged?.Invoke(this, new(nameof(ActiveRoom)));
        }
    }
}
