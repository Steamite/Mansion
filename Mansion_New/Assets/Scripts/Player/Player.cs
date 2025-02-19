using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] InputActionAsset asset;
    [SerializeField] CharacterController controller;
    [SerializeField] Transform viewCamera;
    [SerializeField] Transform groundPos;

    [SerializeField] ItemRaycasts itemRaycast;


    [Header("Configures")]
    [SerializeField][Range(0, 10)] float moveSpeed;
    [SerializeField][Range(0, 10)] float lookSpeed;
    [SerializeField][Range(300, 360)] float lookLockMax;
    [SerializeField][Range(0, 60)] float lookLockMin;

    [Header("Camera positions")]
    [SerializeField][Range(-1, 1)] float cameraTop = 0.45f;
    [SerializeField][Range(-1, 1)] float cameraBottom = -0.45f;



    InputActionMap inputMap;

    InputAction moveAction;
    InputAction lookAction;
    InputAction crouchAction;


    float xRotation;
    Vector3 gravity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xRotation = 0;
        gravity = new();
        inputMap = asset.actionMaps[0];
        moveAction = inputMap.FindAction("Move");
        lookAction = inputMap.FindAction("Look");
        crouchAction = inputMap.FindAction("Crouch");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        itemRaycast.Init(inputMap);
    }

    // Update is called once per frame
    void Update()
    {
        bool changed = false; ;
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input.x != 0 || input.y != 0)
        {

            Vector3 test = transform.TransformDirection(Vector3.forward);
            Vector3 moveDir = transform.TransformDirection(Vector3.forward) * input.y + transform.TransformDirection(Vector3.right) * input.x;
            controller.Move(new Vector3(moveDir.x * moveSpeed, 0, moveDir.z * moveSpeed) * (crouchAction.inProgress ? 0.5f : 1));
            //Debug.Log(moveDir);
            changed = true;
        }

        //Debug.Log($"Update: {input}");
        input = lookAction.ReadValue<Vector2>();
        if (input.x != 0 || input.y != 0)
        {
            input *= lookSpeed;
            transform.Rotate(Vector3.up, input.x);


            xRotation -= input.y;
            xRotation = Mathf.Clamp(xRotation, -60, 60);
            viewCamera.localRotation = Quaternion.Euler(new(xRotation, 0, 0));
            changed = true;
        }

        if (crouchAction.WasReleasedThisFrame())
        {
            viewCamera.localPosition = new(viewCamera.localPosition.x, cameraTop, viewCamera.localPosition.z);
        }
        else if (crouchAction.WasPressedThisFrame())
        {
            viewCamera.localPosition = new(viewCamera.localPosition.x, cameraBottom, viewCamera.localPosition.z);
        }


        if (changed)
        {
            itemRaycast.RayCastUpdate();
        }
    }

    private void FixedUpdate()
    {
        /*if (Physics.Raycast(groundPos.position, Vector3.down, 0.1f))
        {
            gravity.y = 0;
        }
        else
        {
            gravity.y -= 9.8f * Time.fixedDeltaTime;
            controller.Move(gravity);
        }*/
    }
}
