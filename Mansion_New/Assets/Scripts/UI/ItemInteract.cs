using Items;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Inspect
{
    public class ItemInteract : MonoBehaviour
    {
        [SerializeField] CinemachineCamera cam;
        [SerializeField] Image backgroundImage;
        [SerializeField] Canvas canvas;

        #region Init
        public void Init(Transform _item, InputActionAsset _asset)
        {
            transform.position = _item.position;
            _item.parent = transform;
            _item.localPosition = new(0, 0, 0);
            _item.localRotation = Quaternion.identity;
            foreach (Transform trans in _item.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 6;

            Camera.main.cullingMask = 183;

            _asset.actionMaps[2].Disable();
            StartCoroutine(WaitOnPostRender(_asset, _item.GetComponent<InteractableItem>()));
        }

        IEnumerator WaitOnPostRender(InputActionAsset _asset, InteractableItem _item)
        {
            // Create the picture
            yield return new WaitForEndOfFrame();
            canvas.transform.GetChild(0).GetComponent<Image>().sprite = GaussianBlur.Blur();
            canvas.gameObject.SetActive(true);

            // Setup camera
            _item.gameObject.SetActive(true);
            Camera.main.cullingMask = 96;
            Camera.main.transform.parent = transform;
            Camera.main.transform.SetParent(null);
            canvas.worldCamera = Camera.main;

            // rotate camera to match current player rotation
            CinemachineOrbitalFollow orbit = cam.GetComponent<CinemachineOrbitalFollow>();
            orbit.HorizontalAxis.Value = Camera.main.transform.rotation.eulerAngles.y;
            orbit.RadialAxis.Range = _item.radiusRange;


            cam.Priority = 3;

            // Enable camera movement
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            while (brain.ActiveBlend == null)
                yield return null;
            yield return new WaitUntil(() => brain.ActiveBlend == null);
            canvas.GetComponent<InspectMenu>().Init(_asset, _item);
        }
        #endregion
    }
}