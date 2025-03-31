using Items;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;

namespace UI.Inspect
{
    /// <summary>Handles camera events for inspecting the item.</summary>
    public class InpectionInit : MonoBehaviour
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

		#region Init
		/// <summary>
		/// Disables movement actions and hides objects that are not needed for the inspection.
		/// </summary>
		/// <param name="_item">Item for inspection</param>
		/// <param name="_asset">Asset containging actions.</param>
		public void Init(Transform _item, InputActionAsset _asset, AsyncOperationHandle<SceneInstance> scene)
		{
			_asset.actionMaps[2].Disable();

			transform.position = _item.position;
            _item.parent = transform;
            _item.localPosition = new(0, 0, 0);
            foreach (Transform trans in _item.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 6;

            Camera c = Camera.main.transform.GetChild(0).GetComponent<Camera>();
            c.enabled = true;
            screenShot = new RenderTexture(Screen.width, Screen.height, 24);
            c.targetTexture = screenShot;
            

			StartCoroutine(WaitOnPostRender(_item.GetComponent<InteractableItem>(), _asset, scene));
        }

		/// <summary>
		/// Assigns the background and sets up the orbital camera.
		/// </summary>
		/// <param name="_item">Item for inspection</param>
		/// <param name="_asset">Asset containging actions.</param>
		/// <returns></returns>
		IEnumerator WaitOnPostRender(InteractableItem _item, InputActionAsset _asset, AsyncOperationHandle<SceneInstance> scene)
        {
            // Create the picture
            yield return new WaitForEndOfFrame();
            canvas.transform.GetChild(0).GetComponent<Image>().sprite = Blur();
			canvas.gameObject.SetActive(true);
            Camera.main.cullingMask = 96;
            canvas.worldCamera = Camera.main;

			// rotate camera to match current player rotation
			CinemachineOrbitalFollow orbit = cam.GetComponent<CinemachineOrbitalFollow>();

            CapsuleCollider capsuleCollider;
            if (capsuleCollider = _item.GetComponent<CapsuleCollider>())
            {
                if(_item.transform.rotation.x != 0)
                {
                    orbit.TargetOffset.z = _item.transform.eulerAngles.x < 0 ? capsuleCollider.center.y : -capsuleCollider.center.y;
                    orbit.GetComponent<CinemachineRotationComposer>()
                        .TargetOffset.z = orbit.TargetOffset.z;
                    orbit.Orbits.Top.Height = capsuleCollider.radius*2;
                    orbit.Orbits.Center.Height = capsuleCollider.radius;
                    orbit.Orbits.Bottom.Height = -capsuleCollider.radius;
                }
                else
                {
                    orbit.GetComponent<CinemachineRotationComposer>()
                        .TargetOffset.y = capsuleCollider.center.y;
                    orbit.Orbits.Top.Height = capsuleCollider.center.y + capsuleCollider.height / 2;
                    orbit.Orbits.Center.Height = capsuleCollider.center.y;
                    orbit.Orbits.Bottom.Height = capsuleCollider.center.y - capsuleCollider.height / 2;
                }
            }

            orbit.HorizontalAxis.Value = Camera.main.transform.rotation.eulerAngles.y;
            orbit.RadialAxis.Range = _item.RadiusRange;

            cam.Priority = 3;

            // Enable camera movement
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            while (brain.ActiveBlend == null)
                yield return null;
            yield return new WaitUntil(() => brain.ActiveBlend == null);
            canvas.GetComponent<InspectMenu>().Init(_asset, _item, scene);
        }
		#endregion

        /// <summary>
        /// Creates a blured screenshot.
        /// </summary>
        /// <returns></returns>
		public Sprite Blur()
		{
			int width = Screen.width;
			int height = Screen.height;

            RenderTexture.active = screenShot;
			Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
			tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			tex.Apply(); // Apply texture changes before reading pixels
            RenderTexture.active = null;
			Camera c = Camera.main.transform.GetChild(0).GetComponent<Camera>();
            c.enabled = false;

			RenderTexture renH = RenderTexture.GetTemporary(width, height);
			RenderTexture renV = RenderTexture.GetTemporary(width, height);

			horizontalMaterial.SetFloat("_BlurSize", Radial);
			verticalMaterial.SetFloat("_BlurSize", Radial);

			Graphics.Blit(tex, renH, horizontalMaterial);
			Graphics.Blit(renH, renV, verticalMaterial);

			RenderTexture.active = renV;
			tex.ReadPixels(new Rect(0, 0, renV.width, renV.height), 0, 0);
			tex.Apply();
			RenderTexture.active = null;

			RenderTexture.ReleaseTemporary(renH);
			RenderTexture.ReleaseTemporary(renV);


			return Sprite.Create(tex, new(0, 0, width, height), new(0, 0));
		}
	}
}