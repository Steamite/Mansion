using Rooms;
using System;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Player
{
    public class PlayerMovement : MonoBehaviour, INotifyBindablePropertyChanged
    {
        [Header("Assets")]
        [SerializeField] InputActionAsset asset;

        CharacterController controller;
        Transform groundPos;
        PlayerCamera playerCamera;

        [Header("Configures")]
        [SerializeField][Range(0, 10)] float moveSpeed = 5f;
        [SerializeField] string startingScene;
        Vector3 gravity;

        InputAction moveAction;

        [CreateProperty] public Vector2 Position = new();
        [CreateProperty] public Room ActiveRoom;


        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;


        void Awake()
        {
            gravity = new();
            InputActionMap inputMap = asset.actionMaps[0];
            moveAction = inputMap.FindAction("Move");

            controller = GetComponent<CharacterController>();
            groundPos = transform.GetChild(1);
            playerCamera = transform.GetChild(0).GetComponent<PlayerCamera>();
            
        }

        async void Start()
        {
            if (SceneManager.sceneCount > 1)
            {
                for (int i = SceneManager.sceneCount - 1; i > 0; i--)
                {
                    await SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i).name);
                }
            }
            await SceneManager.LoadSceneAsync(startingScene, LoadSceneMode.Additive);
            ActiveRoom = FindFirstObjectByType<Room>().EnterRoom(null);
            propertyChanged?.Invoke(this, new(nameof(ActiveRoom)));

            Position.x = -transform.position.x;
            Position.y = transform.position.z;
            propertyChanged?.Invoke(this, new(nameof(Position)));
        }


        // Update is called once per frame
        void Update()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            if (input.x != 0 || input.y != 0)
            {
                Vector3 moveDir = transform.TransformDirection(Vector3.forward) * input.y + transform.TransformDirection(Vector3.right) * input.x;
                controller.Move((playerCamera.crouchAction.inProgress ? 0.5f : 1) * moveSpeed * Time.deltaTime * new Vector3(moveDir.x, 0, moveDir.z));
                playerCamera.RayCastUpdate();

                Position.x = -transform.position.x;
                Position.y = transform.position.z;
                propertyChanged?.Invoke(this, new(nameof(Position)));
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if(hit.gameObject.tag == "Entrance")
            {
                ActiveRoom = hit.transform.parent.parent.GetComponent<Room>().EnterRoom(ActiveRoom);
                propertyChanged?.Invoke(this, new(nameof(ActiveRoom)));
            }
        }

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