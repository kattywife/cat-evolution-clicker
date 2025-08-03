// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Tooltip("Источник звука для одиночных эффектов (UI, клики и т.д.)")]
    public AudioSource sfxSource;

    // --- НАШИ ИЗМЕНЕНИЯ ---
    private bool isMuted = false;
    private const string MutePrefKey = "IsMuted"; // Ключ для сохранения состояния звука

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Загружаем сохраненное состояние звука при запуске игры
            isMuted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
            ApplyMuteState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Метод для проигрывания звука (без изменений в логике вызова)
    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // --- НОВЫЕ МЕТОДЫ ДЛЯ УПРАВЛЕНИЯ ЗВУКОМ ---

    /// <summary>
    /// Переключает состояние звука (вкл/выкл).
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyMuteState();

        // Сохраняем выбор пользователя между сессиями
        PlayerPrefs.SetInt(MutePrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Применяет текущее состояние (isMuted) к глобальному звуку.
    /// </summary>
    private void ApplyMuteState()
    {
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    /// <summary>
    /// Возвращает текущее состояние звука.
    /// </summary>
    public bool IsMuted()
    {
        return isMuted;
    }
}