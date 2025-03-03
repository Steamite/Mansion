using System;
using UI;
using UI.Inspect;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Player
{
    public class PlayerCamera : MonoBehaviour, INotifyBindablePropertyChanged
    {
        [Header("Assign")]
        [SerializeField] InputActionAsset asset;
        

        [SerializeField][Range(0.5f, 1.5f)] float range = 1;
        [SerializeField][Range(0, 10)] float lookSpeed;
        [SerializeField][Range(300, 360)] float lookLockMax;
        [SerializeField][Range(0, 60)] float lookLockMin;

        [SerializeField][ReadOnly()] GameObject item;
        bool hasItem;

        Texture2D destinationTexture;

        CinemachineCamera topCam;
        CinemachineCamera bottomCam;

        InputAction interactAction;
        InputAction lookAction;
        [HideInInspector] public InputAction crouchAction;

        float xRotation;
        int lastFrame;
        bool standing = true;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
        [CreateProperty] public float yRotation;
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

        private void OnDrawGizmos()
        {
            if (topCam == null)
                Awake();
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(cameraRay.origin, cameraRay.GetPoint(range));
        }


        private void Update()
        {
            if (interactAction.WasPressedThisFrame())
                CrosshairImage.StartHold();
            else if (interactAction.WasReleasedThisFrame())
                CrosshairImage.EndHold();

            if (interactAction.triggered)
                Interact();


            Vector2 input = lookAction.ReadValue<Vector2>()*lookSpeed;
            if (input.y != 0)
            {
                xRotation -= input.y;
                xRotation = Mathf.Clamp(xRotation, -60, 60);
                if (standing)
                    topCam.transform.localRotation = Quaternion.Euler(new(xRotation, 0, 0));
                else
                    bottomCam.transform.localRotation = Quaternion.Euler(new(xRotation, 0, 0));
                RayCastUpdate();
            }
            if (input.x != 0)
            {
                transform.parent.Rotate(Vector3.up, input.x);
                yRotation = transform.parent.rotation.eulerAngles.y;
                //Debug.Log(yRotation);
                propertyChanged?.Invoke(this, new(nameof(yRotation)));
                RayCastUpdate();
            }

            if (crouchAction.WasPressedThisFrame())
            {
                standing = false;
                bottomCam.Priority = 2;
                bottomCam.transform.rotation = topCam.transform.rotation;
                RayCastUpdate();
            }
            else if (crouchAction.WasReleasedThisFrame())
            {
                standing = true;
                bottomCam.Priority = 0;
                topCam.transform.rotation = bottomCam.transform.rotation;
                RayCastUpdate();
            }
        }


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

        async void Interact()
        {
            CrosshairImage.Toggle();
            asset.actionMaps[0].Disable();
            await SceneManager.LoadSceneAsync("Interact", LoadSceneMode.Additive);
            GameObject.FindAnyObjectByType<ItemInteract>().Init(item.transform, asset);
        }
    }

}