using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class ItemRaycasts : MonoBehaviour
{
    [SerializeField] CrosshairImage targetImage;
    [SerializeField][Range(0.5f, 1.5f)] float range = 1;

    [SerializeField] GameObject item;

    InputAction interactAction;


    private void OnDrawGizmos()
    {
        /*+
            transform.TransformDirection(Vector3.right)*2);*/
        //Gizmos.DrawRay(ray);
        Vector3 startPos = transform.position + transform.TransformDirection(0, 0, 0.5f);
        Gizmos.DrawLine(
            startPos,
            startPos + transform.TransformDirection(0, 0, range));
    }

    public void Init(InputActionMap inputMap)
    {
        interactAction = inputMap.FindAction("Interact");
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
    }


    public void RayCastUpdate()
    {
        Ray ray = new(transform.position + transform.TransformDirection(0, 0, 0.5f),
            transform.TransformDirection(0, 0, 1));
        RaycastHit hit;
        Physics.Raycast(ray, out hit, range);
        if (hit.transform)
        {
            if(!item)
                targetImage.Enter();
            item = hit.transform.gameObject;
            interactAction.Enable();
        }
        else
        {
            if(item)
                targetImage.Exit();
            interactAction.Disable();
            item = null;
        }
    }
}
