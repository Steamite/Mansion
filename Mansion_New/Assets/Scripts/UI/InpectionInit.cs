using Assets.Scripts.UI;
using Items;
using System.Collections;
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
        AsyncOperationHandle<SceneInstance> scene;

        #region Init
        /// <summary>
        /// Disables movement actions and hides objects that are not needed for the inspection.
        /// </summary>
        /// <param name="_item">Item for inspection</param>
        /// <param name="_asset">Asset containging actions.</param>
        public void Init(Transform _item, InputActionAsset _asset, AsyncOperationHandle<SceneInstance> _scene)
        {
            while (_item != null)
            {
                if (_item.TryGetComponent<InteractableItem>(out item))
                    break;
                _item = _item.parent;
            }

            asset = _asset;
            scene = _scene;

            _asset.actionMaps[2].Disable();

            transform.position = _item.position;
            _item.parent = transform;
            _item.localPosition = new(0, 0, 0);
            foreach (Transform trans in _item.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 6;


            

            Debug.Log("got the screenshot");
            GetComponent<BlurTexture>().Blur(SetupCameras);
        }

        
        #endregion

        

        /*public void GetImg(bool firstTry = true)
        {
            AsyncGPUReadback.Request(
                screenShot,
                0,
                TextureFormat.RGB24,
                (req) =>
                {
                    if (!req.hasError)
                    {
                        Debug.Log("request is done");
                        var data = req.GetData<byte>();
                        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                        tex.LoadRawTextureData(data);
                        tex.Apply();

                    }
                    else if (firstTry)
                    {
                        Debug.LogError("fuck you");
                        GetImg(false);
                    }
                });
        }*/


        void SetupCameras(Sprite sprite)
        {
            Debug.Log("created the sprite");
            canvas.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
            canvas.gameObject.SetActive(true);
            Camera.main.cullingMask = 96;
            canvas.worldCamera = Camera.main;

            CinemachinePositionComposer composer = cam.GetComponent<CinemachinePositionComposer>();
            composer.TargetOffset = item.Offset;
            composer.CameraDistance = (item.RadiusRange.x + item.RadiusRange.y) / 2;

            CinemachinePanTilt panTilt = cam.GetComponent<CinemachinePanTilt>();
            panTilt.PanAxis.Value = item.StartRotation.x;
            panTilt.TiltAxis.Value = item.StartRotation.y;


            cam.Priority = 3;
            StartCoroutine(WaitForBlend());
        }
        IEnumerator WaitForBlend()
        {
            /* CinemachineOrbitalFollow orbit = cam.GetComponent<CinemachineOrbitalFollow>();

             orbit.Orbits.Top.Height = item.top;
             orbit.Orbits.Center.Height = item.center;
             orbit.Orbits.Bottom.Height = item.bottom;
             orbit.TargetOffset = item.offset;

             orbit.HorizontalAxis.Value = Camera.main.transform.rotation.eulerAngles.y;
             orbit.RadialAxis.Range = item.RadiusRange;*/


            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            /*while (brain.ActiveBlend == null)
                yield return null;*/

            //wait for the blend to activate
            yield return new WaitUntil(() => brain.ActiveBlend != null);

            //wait for the blend to end
            yield return new WaitUntil(() => brain.ActiveBlend == null);

            CinemachinePanTilt panTilt = cam.GetComponent<CinemachinePanTilt>();
            panTilt.enabled = item.Rotatable;
            
            canvas.GetComponent<InspectMenu>().Init(asset, item, scene);
        }
    }
}