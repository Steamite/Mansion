using Unity.Properties;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Items
{
    public interface IVRInteractionInit
    {
        void SetupItem(InteractableItem item);
        void DestroySelf();
    }

    public static class VRManagerLink
    {
        static IVRInteractionInit vrManager;
        public static IVRInteractionInit VRManager { set => vrManager = value; get => vrManager; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Clear() => vrManager = null;
        public static void DestroyManager()
        {
            if (vrManager == null)
                return;
            vrManager.DestroySelf();
            Clear();
        }


        public static void OnRoomLoad(Transform room)
        {
            if (vrManager == null)
                return;


            var interactables = room.GetChild(2).GetComponentsInChildren<InteractableItem>();
            foreach (var item in interactables)
            {
                vrManager.SetupItem(item);
            }
        }
    }
}
