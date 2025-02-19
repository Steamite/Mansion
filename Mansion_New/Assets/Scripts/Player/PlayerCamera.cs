using System;
using Unity.Cinemachine;
using Unity.Collections;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PlayerCamera : MonoBehaviour
{
    [Header("Assign")]
    [SerializeField] InputActionAsset asset;
    [SerializeField] CrosshairImage targetImage;
    
    [SerializeField][Range(0.5f, 1.5f)] float range = 1;
    [SerializeField][Range(0, 10)] float lookSpeed;
    [SerializeField][Range(300, 360)] float lookLockMax;
    [SerializeField][Range(0, 60)] float lookLockMin;

    [SerializeField][ReadOnly()] GameObject item;

    CinemachineCamera topCam;
    CinemachineCamera bottomCam;

    InputAction interactAction;
    InputAction lookAction;
    [HideInInspector]public InputAction crouchAction;

    float xRotation;
    int lastFrame;
    bool standing = true;

    void Start()
    {
        xRotation = 0;
        InputActionMap inputMap = asset.actionMaps[0];
        interactAction = inputMap.FindAction("Interact");
        lookAction = inputMap.FindAction("Look");
        crouchAction = inputMap.FindAction("Crouch");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        topCam = transform.GetChild(0).GetComponent<CinemachineCamera>();
        bottomCam = transform.GetChild(1).GetComponent<CinemachineCamera>();
    }

    private void OnDrawGizmos()
    {
        Vector3 startPos = transform.position + transform.TransformDirection(0, 0, 0.5f);
        Gizmos.DrawLine(
            startPos,
            startPos + transform.TransformDirection(0, 0, range));
    }


    private void Update()
    {
        if (interactAction.WasPressedThisFrame())
            targetImage.StartHold();
        else if (interactAction.WasReleasedThisFrame())
            targetImage.EndHold();

        if (interactAction.triggered)
        {
            targetImage.Exit();
            interactAction.Disable();
            GameObject.Destroy(item);
        }


        Vector2 input = lookAction.ReadValue<Vector2>();
        if (input.x != 0 || input.y != 0)
        {
            input *= lookSpeed;
            transform.parent.Rotate(Vector3.up, input.x);


            xRotation -= input.y;
            xRotation = Mathf.Clamp(xRotation, -60, 60);
            if(standing)
                topCam.transform.localRotation = Quaternion.Euler(new(xRotation, 0, 0));
            else
                bottomCam.transform.localRotation = Quaternion.Euler(new(xRotation, 0, 0));
            RayCastUpdate();
        }

        if (crouchAction.WasPressedThisFrame())
        {
            standing = false;
            bottomCam.Priority = 2;
            bottomCam.transform.rotation = topCam.transform.rotation;
            RayCastUpdate();
        }
        else if(crouchAction.WasReleasedThisFrame())
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
        Ray ray = new(transform.position + transform.TransformDirection(0, 0, 0.5f),
            transform.TransformDirection(0, 0, 1));
        RaycastHit hit;
        Physics.Raycast(ray, out hit, range);
        if (hit.transform)
        {
            if (!item)
                targetImage.Enter();
            item = hit.transform.gameObject;
            interactAction.Enable();
        }
        else
        {
            if (item)
                targetImage.Exit();
            interactAction.Disable();
            item = null;
        }
    }
}
