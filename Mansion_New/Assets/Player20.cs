using UnityEngine;
using UnityEngine.InputSystem;

public class Player20 : MonoBehaviour
{
    public InputActionAsset inputActions;
    public float globalSpeed = 1.5f;
    /// <summary>
    /// This is variable called playerMove <see cref="Start"/>
    /// </summary>
    private InputAction playerMove;
    private InputAction playerView;

    [Header("Camera")][SerializeField] Transform cameraTransform;
    private CharacterController characterController;

    void Awake()
    {
        playerMove = inputActions.FindActionMap("Player").FindAction("Pohyb");
        playerView = inputActions.FindActionMap("Player").FindAction("Camera");
        characterController = GetComponent<CharacterController>();
        inputActions.Enable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        characterController.Move(playerMove.ReadValue<Vector2>());  

    }
   /// <summary>
   /// Hello dear developer, happy coding!
   /// </summary>
   /// <param name="naza">This should be an integer value-this is </param>
   /// <returns></returns>
    string aha(int naza)
    {
        return "baf";
    }
}

