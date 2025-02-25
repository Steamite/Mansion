using Items;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI.Inspect
{
    public class InspectMenu : MonoBehaviour
    {
        [SerializeField] CinemachineCamera cam;
        InputActionAsset asset;

        InputAction endAction;
        InputAction infoAction;
        InputAction takeAction;

        InteractableItem item;

        #region
        public void Init(InputActionAsset _asset, InteractableItem _item)
        {
            asset = _asset;
            item = _item;

            endAction = asset.actionMaps[2].actions[0];
            infoAction = asset.actionMaps[2].actions[1];
            takeAction = asset.actionMaps[2].actions[2];
            asset.actionMaps[2].Enable();
            enabled = true;
        }
        #endregion

        void Update()
        {
            if (endAction.triggered)
                EndInteract();
            if (infoAction.triggered)
                ItemInfo();
            if (takeAction.triggered)
                PickupItem();
        }

        void EndInteract()
        {
            asset.actionMaps[2].Disable();
            if (item)
            {
                item.gameObject.layer = 7;
                item.transform.SetParent(GameObject.Find("World").transform);
            }
            Camera.main.transform.SetParent(GameObject.Find("EventSystem").transform);
            Camera.main.transform.SetParent(null);

            cam.Priority = -1;
            StartCoroutine(WaitForBlend());
        }

        IEnumerator WaitForBlend()
        {
            yield return new();
            Camera.main.cullingMask = -1;
            gameObject.SetActive(false);
            SceneManager.UnloadSceneAsync(1);
            asset.actionMaps[0].Enable();
        }

        void ItemInfo()
        {

        }

        void PickupItem()
        {
            Destroy(item.gameObject);
            item = null;
            EndInteract();
        }
    }
}