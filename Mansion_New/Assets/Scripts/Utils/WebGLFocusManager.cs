using Player;
using System.Collections;
using UnityEngine;

public class WebGLFocusManager : MonoBehaviour
{
    void Awake()
    {
        // Tells Unity not to destroy this specific GameObject when a new scene loads.
        DontDestroyOnLoad(this.gameObject);
    }

    // This method will be called from JavaScript.
    // It must be public to be accessible.
    public void DeactivateInput()
    {
        StartCoroutine(DeactivateAfterDelay());
    }

    // This coroutine will execute the logic after a short, controlled delay.
    private IEnumerator DeactivateAfterDelay()
    {
        // This is the magic line:
        // It pauses the function until the end of the current frame,
        // after Unity has finished its own input processing.
        yield return new WaitForEndOfFrame();

        // Now, it's safe to set the value without it being immediately overwritten.
        WebGLInput.captureAllKeyboardInput = false;
        GameObject cameras = GameObject.Find("Cameras");
        if (cameras != null)
        {
            PlayerCamera cam = cameras.GetComponent<PlayerCamera>();
            cam.enabled = false;
        }
        Debug.Log("Input deactivated after one frame delay.");
    }
    public void ActivateInput()
    {
        WebGLInput.captureAllKeyboardInput = true;
        GameObject cameras = GameObject.Find("Cameras");
        if (cameras != null)
        {
            PlayerCamera cam = cameras.GetComponent<PlayerCamera>();
            cam.enabled = true;
        }
    }
}
