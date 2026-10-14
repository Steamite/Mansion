using Assets.Scripts.UI;
using Items;
using Player;
using System.Collections;
using System.Threading.Tasks;
using Unity.Cinemachine;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;

namespace UI.Inspect
{
    /// <summary>Handles camera events for inspecting the item.</summary>
    [RequireComponent(typeof(BlurTexture))]
    public class InpectionInit : MonoBehaviour, IInspectionInit
    {
        [Header("Canvas")]
        /// <summary>Orbital camera.</summary>
        [SerializeField] CinemachineCamera cam;
        /// <summary>Image component that needs the image.</summary>
        [SerializeField] Image backgroundImage;
        /// <summary>Canvas with the background Image.</summary>
        [SerializeField] Canvas canvas;

        


        InteractableItem item;
        InputActionAsset asset;

        #region Init
        /// <summary>
        /// Disables movement actions and hides objects that are not needed for the inspection.
        /// </summary>
        /// <param name="_item">Item for inspection</param>
        /// <param name="_asset">Asset containging actions.</param>
        public async Awaitable Init(Transform _item, InputActionAsset _asset)
        {
            while (_item != null)
            {
                if (_item.TryGetComponent(out item))
                    break;
                _item = _item.parent;
            }
            if (_item.TryGetComponent(out Rigidbody rigid))
                rigid.isKinematic = true;

            asset = _asset;

            _asset.actionMaps[2].Disable();

            transform.position = _item.position;
            _item.parent = transform;
            _item.localPosition = new(0, 0, 0);
            foreach (Transform trans in _item.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 6;


            

            Debug.Log("got the screenshot");
            Sprite s = await GetComponent<BlurTexture>().Blur();
            SetupCameras(s);
        }

        
        #endregion

        void SetupCameras(Sprite sprite)
        {
            Debug.Log("created the sprite");
            canvas.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
            canvas.gameObject.SetActive(true);
            Camera.main.cullingMask = 96;
            canvas.worldCamera = Camera.main;

            PlayerCamera.instance.ToggleLight(true);

            CinemachinePositionComposer composer = cam.GetComponent<CinemachinePositionComposer>();
            composer.TargetOffset = item.Offset;
            composer.CameraDistance = (item.RadiusRange.x + item.RadiusRange.y) / 2;

            CinemachinePanTilt panTilt = cam.GetComponent<CinemachinePanTilt>();
            panTilt.PanAxis.Value = item.StartRotation.x;
            panTilt.TiltAxis.Value = item.StartRotation.y;


            StartCoroutine(WaitForBlend());
        }

        IEnumerator WaitForBlend()
        {
            cam.Priority = 3;
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();

            //wait for the blend to activate
            yield return new WaitUntil(() => brain.ActiveBlend != null);

            yield return new WaitForSeconds(brain.ActiveBlend.Duration);/*
            //wait for the blend to end
            yield return new WaitUntil(() => brain.ActiveBlend == null);*/

            CinemachinePanTilt panTilt = cam.GetComponent<CinemachinePanTilt>();
            panTilt.enabled = !item.IsStatic;
            
            canvas.GetComponent<InspectMenu>().Init(asset, item);
        }
    }
}