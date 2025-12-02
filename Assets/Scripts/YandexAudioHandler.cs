using UnityEngine;

public class YandexAudioHandler : MonoBehaviour
{
    // Вызывается, когда вкладка теряет/получает фокус
    void OnApplicationFocus(bool hasFocus)
    {
        Silence(!hasFocus);
    }

    // Вызывается при паузе (сворачивании на мобилках)
    void OnApplicationPause(bool isPaused)
    {
        Silence(isPaused);
    }

    void Silence(bool silence)
    {
        // 0 - полная тишина, 1 - обычная громкость
        AudioListener.volume = silence ? 0f : 1f;
    }
}