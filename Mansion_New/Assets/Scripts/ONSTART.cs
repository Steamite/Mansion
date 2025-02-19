using UnityEngine;

public class ONSTART : MonoBehaviour
{
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;
    }
}
