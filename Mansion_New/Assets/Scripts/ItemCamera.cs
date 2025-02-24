using UnityEngine;

public class ItemCamera : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = 90;
        QualitySettings.vSyncCount = 0;
    }
}
