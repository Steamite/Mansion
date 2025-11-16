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
    public class InpectionInit : MonoBehaviour, IInspectionInit
    {
        [Header("Canvas")]
        /// <summary>Orbital camera.</summary>
        [SerializeField] CinemachineCamera cam;
        /// <summary>Image component that needs the image.</summary>
        [SerializeField] Image backgroundImage;
        /// <summary>Canvas with the background Image.</summary>
        [SerializeField] Canvas canvas;

        [Header("Blur")]
        [SerializeField] Material horizontalMaterial;
        [SerializeField] Material verticalMaterial;
        [SerializeField] int Radial = 3;

        RenderTexture screenShot;

        int width;
        int height;

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
            width = Screen.width;
            height = Screen.height;

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


            Debug.Log("starting to render");
            Camera c = Camera.main.transform.GetChild(0).GetComponent<Camera>();
            c.enabled = true;
            screenShot = new RenderTexture(width, height, 24);
            c.targetTexture = screenShot;

            c.Render();
            /*
                        c.targetTexture = null;
                        c.enabled = false;*/

            Debug.Log("got the screenshot");
            StartCoroutine(WaitOnPostRender());
        }

        /// <summary>
        /// Assigns the background and sets up the orbital camera.
        /// </summary>
        /// <param name="_item">Item for inspection</param>
        /// <param name="_asset">Asset containging actions.</param>
        /// <returns></returns>
        IEnumerator WaitOnPostRender()
        {
            // Create the picture
            yield return new WaitForEndOfFrame();

            RenderTexture renH = RenderTexture.GetTemporary(width, height);

            horizontalMaterial.SetFloat("_BlurSize", Radial);
            verticalMaterial.SetFloat("_BlurSize", Radial);

            Graphics.Blit(screenShot, renH, horizontalMaterial);
            Graphics.Blit(renH, screenShot, verticalMaterial);
            RenderTexture.ReleaseTemporary(renH);

            Debug.Log("Shaded");
            Test();
            yield break;
            NativeArray<byte> tempvar = new NativeArray<byte>(screenShot.width * screenShot.height * 16, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var req = AsyncGPUReadback.RequestIntoNativeArrayAsync(ref tempvar, screenShot);
            yield return req;
            Texture2D tempimg = new Texture2D(screenShot.width, screenShot.height);
            tempimg.LoadRawTextureData(tempvar);
            tempimg.Apply();
            StartCoroutine(FinishStuff(Sprite.Create(tempimg, new(0, 0, tempimg.width, tempimg.height), new(0, 0))));

        }
        #endregion

        public async void Test()
        {
            NativeArray<byte> tempvar = new NativeArray<byte>(screenShot.width * screenShot.height * 16, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var req = AsyncGPUReadback.RequestIntoNativeArrayAsync(ref tempvar, screenShot);
            await req;
            Texture2D tempimg = new Texture2D(screenShot.width, screenShot.height);
            tempimg.LoadRawTextureData(tempvar);
            tempimg.Apply();
            StartCoroutine(FinishStuff(Sprite.Create(tempimg, new(0, 0, tempimg.width, tempimg.height), new(0, 0))));
        }

        public void GetImg(bool firstTry = true)
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
        }


        IEnumerator FinishStuff(Sprite sprite)
        {
            Debug.Log("created the sprite");
            canvas.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
            canvas.gameObject.SetActive(true);
            Camera.main.cullingMask = 96;
            canvas.worldCamera = Camera.main;

            CinemachinePositionComposer composer = cam.GetComponent<CinemachinePositionComposer>();
            composer.TargetOffset = item.offset;
            composer.CameraDistance = (item.RadiusRange.x + item.RadiusRange.y) / 2;

            CinemachinePanTilt panTilt = cam.GetComponent<CinemachinePanTilt>();
            panTilt.PanAxis.Value = item.startRotation.x;
            panTilt.TiltAxis.Value = item.startRotation.y;

            // rotate camera to match current player rotation
            /* CinemachineOrbitalFollow orbit = cam.GetComponent<CinemachineOrbitalFollow>();

             orbit.Orbits.Top.Height = item.top;
             orbit.Orbits.Center.Height = item.center;
             orbit.Orbits.Bottom.Height = item.bottom;
             orbit.TargetOffset = item.offset;

             orbit.HorizontalAxis.Value = Camera.main.transform.rotation.eulerAngles.y;
             orbit.RadialAxis.Range = item.RadiusRange;*/

            cam.Priority = 3;

            // Enable camera movement
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            while (brain.ActiveBlend == null)
                yield return null;
            yield return new WaitUntil(() => brain.ActiveBlend == null);
            canvas.GetComponent<InspectMenu>().Init(asset, item, scene);
        }
    }
}