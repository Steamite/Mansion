using Rooms;
using System;
using System.Collections;
using Unity.Cinemachine;
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
    /// <summary>Handles player movement and map resizing.</summary>
    public class PlayerMovement : MonoBehaviour, INotifyBindablePropertyChanged
    {
		#region Variables
		/// <summary>Reference to the input assets.</summary>
		[Header("Assets")][SerializeField] InputActionAsset asset;

        /// <summary>Player controller for easier move handling.</summary>
        CharacterController controller;
        /// <summary>GroundPosition for gravity.</summary>
		Transform groundPos;
		PlayerCamera playerCamera;

		/// <summary>Movement speed.</summary>
		[Header("Configures")][SerializeField][Range(0, 10)] float moveSpeed = 5f;
		/// <summary>Min and max range limits for minimap zooming.</summary>
        [SerializeField][MinMaxRangeSlider(0, 10)] Vector2 mapZoomLimit;
		/// <summary>Base room to load scenes from</summary>
        [SerializeField] AssetReference startingScene;
		/// <summary>Current velocity of on -y.</summary>
		Vector3 gravity;

		/// <summary>Input for moving.</summary>
		InputAction moveAction;
		/// <summary>Input for zooming minmap.</summary>
		InputAction mapZoomAction;
		#endregion

		#region Binding Properies
		/// <summary>Character position for moving minimap.</summary>
		[CreateProperty] public Vector2 Position = new();
		/// <summary>Active room for displayText under the minimap.</summary>
        [CreateProperty] public Room ActiveRoom;
		/// <summary>Current zoom level.</summary>
		[CreateProperty] public float mapZoom = 1;

		public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
		#endregion

		#region Init
		void Awake()
        {
            asset.Disable();
            gravity = new();
            InputActionMap inputMap = asset.actionMaps[0];
            moveAction = inputMap.FindAction("Move");
            mapZoomAction = asset.actionMaps[1].FindAction("MapZoom");

            controller = GetComponent<CharacterController>();
            groundPos = transform.GetChild(1);
            playerCamera = transform.GetChild(0).GetComponent<PlayerCamera>();
        }

        public void Activate()
        {
			UnityEngine.Cursor.lockState = CursorLockMode.Locked;
			UnityEngine.Cursor.visible = false;
			StartCoroutine(InitInput());
		}

        IEnumerator InitInput()
        {
            if (Init.toLoad == "Player")
            {
                AsyncOperationHandle<SceneInstance> initialLoad =
                    Addressables.LoadSceneAsync(startingScene, LoadSceneMode.Additive, false);
                yield return initialLoad;
                if (initialLoad.Status == AsyncOperationStatus.Succeeded)
                    yield return initialLoad.Result.ActivateAsync();
            }

            ActiveRoom = FindFirstObjectByType<Room>().EnterRoom(null);
            propertyChanged?.Invoke(this, new(nameof(ActiveRoom)));

            Position.x = -transform.position.x;
            Position.y = transform.position.z;
            propertyChanged?.Invoke(this, new(nameof(Position)));

            asset.Enable();
            //Debug.Log("Enabled:" + moveAction.enabled);
            yield break;
        }
        #endregion

        /// <summary>
        /// Player movement and map resize.
        /// </summary>
        void Update()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            //Debug.Log("moving by:" + input);
            if (input.x != 0 || input.y != 0)
            {
                Vector3 moveDir = transform.TransformDirection(Vector3.forward) * input.y + transform.TransformDirection(Vector3.right) * input.x;
                controller.Move((playerCamera.crouchAction.inProgress ? 0.5f : 1) * moveSpeed * Time.deltaTime * new Vector3(moveDir.x, 0, moveDir.z));
                playerCamera.RayCastUpdate();

                Position.x = -transform.position.x;
                Position.y = transform.position.z;
                propertyChanged?.Invoke(this, new(nameof(Position)));
            }

            #region Map resize
            if (mapZoomAction.ReadValue<float>() != 0)
            {
                mapZoom -= mapZoomAction.ReadValue<float>() * Time.deltaTime;
                float zoom = Mathf.Clamp(mapZoom, mapZoomLimit.x, mapZoomLimit.y);
                if (zoom == mapZoom)
                {
                    mapZoom = zoom;
                    propertyChanged?.Invoke(this, new(nameof(mapZoom)));
                    propertyChanged?.Invoke(this, new(nameof(Position)));
                }
                else
                    mapZoom = zoom;
            }
            #endregion
        }

        /// <summary>
        /// Checks for entering different rooms.
        /// </summary>
        /// <param name="hit">The object that was hit.</param>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if(hit.gameObject.CompareTag("Entrance"))
            {
                ActiveRoom = hit.transform.parent.parent.GetComponent<Room>().EnterRoom(ActiveRoom);
                propertyChanged?.Invoke(this, new(nameof(ActiveRoom)));
            }
        }

        /// <summary>
        /// Handles gravity.f
        /// </summary>
        private void FixedUpdate()
        {
            if (Physics.Raycast(groundPos.position, Vector3.down, 0.1f))
            {
                gravity.y = 0;
            }
            else
            {
                gravity.y -= 9.8f * Time.fixedDeltaTime;
                controller.Move(gravity);
            }
        }
    }
}