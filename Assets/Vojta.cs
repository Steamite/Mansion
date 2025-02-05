using UnityEngine;
using UnityEngine.InputSystem;

public class Vojta : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    public InputActionAsset inputActions;
    public float globalSpeed = 5f;

    CharacterController controller;
    InputAction move;
    InputAction camera;


    private void Awake()
    {
        move = inputActions.FindActionMap("Player").FindAction("Movement");
        camera = inputActions.FindActionMap("Player").FindAction("Camera");
        controller = GetComponent<CharacterController>();
        
        inputActions.Enable();

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(camera.ReadValue<Vector2>());
        controller.Move(move.ReadValue<Vector2>());

        Vector2 vector = camera.ReadValue<Vector2>();

        transform.Rotate(Vector3.up, vector.x);

        cameraTransform.Rotate(Vector3.right, vector.y);
    }
}
