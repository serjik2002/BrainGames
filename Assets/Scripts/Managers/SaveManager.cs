using UnityEngine;


public class SaveManager : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_EDITOR
        Options.UseEncryption = false;
#else
    Options.UseEncryption = true;
#endif


        DontDestroyOnLoad(this.gameObject);
    }

    // Вызывается, когда игра закрывается
    private void OnApplicationQuit()
    {
        if (Options.HasUnsavedChanges())
        {
            Options.Save();
            Debug.Log("Настройки сохранены при выходе.");
        }
    }

    // Вызывается на мобилках, когда приложение сворачивается (pause)
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && Options.HasUnsavedChanges())
        {
            Options.Save();
        }
    }
}