using UnityEngine;

public class YandexAudioHandler : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        SetPause(!hasFocus);
    }

    void OnApplicationPause(bool isPaused)
    {
        SetPause(isPaused);
    }

   void SetPause(bool doPause)
    {
        // Если сейчас идет реклама, мы вообще не реагируем на потерю/получение фокуса!
        // Реклама сама управляет временем и звуком.
        if (YandexManager.Instance != null && YandexManager.Instance.isAdPlaying)
        {
            return; 
        }

        AudioListener.volume = doPause ? 0f : 1f;
        Time.timeScale = doPause ? 0f : 1f;
    }
}