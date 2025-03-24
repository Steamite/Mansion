using System;
using System.Collections;
using UI;
using UI.Inspect;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Player
{
    /// <summary>
    /// Handles camera rotation, triggers interaction raycasts and crouch transitions.
    /// </summary>
    public class PlayerCamera : MonoBehaviour, INotifyBindablePropertyChanged
    {
        #region Variables
        [Header("Assign")]
        [SerializeField] AssetReference inspectScene;

		/// <summary>Reference to the input asset.</summary>
		[SerializeField] InputActionAsset asset;
        

        /// <summary>Intraction raycast range.</summary>
        [SerializeField][Range(0.5f, 1.5f)] float range = 1;
        /// <summary>Speed of camera rotation.</summary>
        [SerializeField][Range(0, 10)] float lookSpeed;
        /// <summary>Max camara angle.</summary>
        [SerializeField][Range(300, 360)] float lookLockMax;
        /// <summary>Min camara angle.</summary>
        [SerializeField][Range(0, 60)] float lookLockMin;

		/// <summary>Item that the player is currently looking at.</summary>
		[SerializeField] GameObject item;
		/// <summary>Ensures correct croshair transitions(Backup for when the <see cref="item"/> is destoyed).</summary>
        bool hasItem;

		/// <summary>Standing camera.</summary>
		CinemachineCamera topCam;
		/// <summary>Crouch camera.</summary>
        CinemachineCamera bottomCam;

		/// <summary>Action for interacting with <see cref="item"/>.</summary>
		InputAction interactAction;
		/// <summary>Vector composite for capturing mouse movement.</summary>
        InputAction lookAction;
		/// <summary>Input for toggling crouch and standing state.</summary>
		[HideInInspector] public InputAction crouchAction;

		/// <summary>Current rotation on the xAxis(up/down)</summary>
		float xRotation;
		/// <summary>
        /// Number of the last frame that caused a raycast. 
        /// <br/>(prevents multiple raycasts in one frame)
        /// </summary>
		int lastFrame;
        /// <summary>If the caracter is standing or not.</summary>
		bool standing = true;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        /// <summary>Property for minimap rotation binding.</summary>
		[CreateProperty] public float yRotation;
		#endregion

		/// <summary>Ray constructor.</summary>
		Ray cameraRay
        {
            get
            {
                Transform t = standing ? topCam.transform : bottomCam.transform;
                return new Ray(
                    t.position,
                    t.TransformDirection(0, 0, 1));
            }
        }

		#region Init
		void Awake()
        {
            xRotation = 0;
            InputActionMap inputMap = asset.actionMaps[0];
            interactAction = inputMap.FindAction("Interact");
            lookAction = inputMap.FindAction("Look");
            crouchAction = inputMap.FindAction("Crouch");
            
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;

            topCam = transform.GetChild(0).GetComponent<CinemachineCamera>();
            bottomCam = transform.GetChild(1).GetComponent<CinemachineCamera>();
        }
        void Start()
        {
            yRotation = transform.parent.rotation.eulerAngles.y;
            propertyChanged?.Invoke(this, new(nameof(yRotation)));
        }
		#endregion
		private void OnDrawGizmos()
        {
            if (topCam == null)
                Awake();
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(cameraRay.origin, cameraRay.GetPoint(range));
        }


        private void Update()
        {
            #region Interaction
            if (interactAction.WasPressedThisFrame())
                CrosshairImage.StartHold();
            else if (interactAction.WasReleasedThisFrame())
                CrosshairImage.EndHold();

            if (interactAction.triggered)
                StartCoroutine(Interact());
            #endregion


            bool checkRayCast = false;
            
            #region Camera
            Vector2 input = lookAction.ReadValue<Vector2>() * lookSpeed;
            if (input.y != 0)
            {
                xRotation -= input.y;
                xRotation = Mathf.Clamp(xRotation, -60, 60);
                if (standing)
                    topCam.transform.localRotation = Quaternion.Euler(new(xRotation, 0, 0));
                else
                    bottomCam.transform.localRotation = Quaternion.Euler(new(xRotation, 0, 0));
                checkRayCast = true;
            }
            if (input.x != 0)
            {
                transform.parent.Rotate(Vector3.up, input.x);
                yRotation = transform.parent.rotation.eulerAngles.y;
                //Debug.Log(yRotation);
                propertyChanged?.Invoke(this, new(nameof(yRotation)));
                checkRayCast = true;
            }
            #endregion

            #region Crouch
            if (crouchAction.WasPressedThisFrame())
            {
                standing = false;
                bottomCam.Priority = 2;
                bottomCam.transform.rotation = topCam.transform.rotation;
                checkRayCast = true;
            }
            else if (crouchAction.WasReleasedThisFrame())
            {
                standing = true;
                bottomCam.Priority = 0;
                topCam.transform.rotation = bottomCam.transform.rotation;
                checkRayCast = true;
            }
            #endregion

            if (checkRayCast)
            {
                RayCastUpdate();
            }
        }


        /// <summary>
        /// If there wasn't a raycast this frame then does a raycast and updates the croshair.
        /// </summary>
        public void RayCastUpdate()
        {
            if (lastFrame == Time.frameCount)
                return;
            lastFrame = Time.frameCount;
            RaycastHit hit;
            Physics.Raycast(cameraRay, out hit, range, 128);
            if (hit.transform)
            {
                //Debug.Log("hit");
                if (!hasItem)
                {
                    CrosshairImage.Enter();
                    hasItem = true;
                }
                item = hit.transform.gameObject;
                interactAction.Enable();
            }
            else
            {
                //Debug.Log("miss");
                if (hasItem)
                {
                    CrosshairImage.Exit();
                    hasItem = false;
                }
                interactAction.Disable();
                item = null;
            }
        }

        /// <summary>
        /// Starts interaction process by loading the interact scene.
        /// </summary>
        IEnumerator Interact()
        {
            CrosshairImage.Toggle();
            asset.actionMaps[0].Disable();
            AsyncOperationHandle<SceneInstance> sceneLoading = 
                Addressables.LoadSceneAsync(inspectScene, LoadSceneMode.Additive, false);
            yield return sceneLoading;
            if (sceneLoading.Status == AsyncOperationStatus.Succeeded)
                yield return sceneLoading.Result.ActivateAsync();
            else
			{
				Debug.LogError(sceneLoading.Status);
                yield break;
			}
            GameObject.FindAnyObjectByType<InpectionInit>().Init(item.transform, asset, sceneLoading);
        }

        /// <summary>
        /// Called after ending the interaction, reset the camera if crouched and Raycast.
        /// </summary>
        public void EndIteract()
        {
            if(standing == false){
                standing = true;
                bottomCam.Priority = 0;
                topCam.transform.rotation = bottomCam.transform.rotation;
            }
            if(item)
                hasItem = false;
            RayCastUpdate();
        }
    }

}