using UnityEngine;

public class UnlockFrameRate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
    }
}
