using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Источники звука")]
    [Tooltip("Источник звука для одиночных эффектов (UI, клики и т.д.)")]
    public AudioSource sfxSource;

    // --- ДОБАВЛЕНО: Отдельный источник для фоновой музыки ---
    [Tooltip("Источник звука для фоновой музыки")]
    public AudioSource musicSource;

    private bool isMuted = false;
    private const string MutePrefKey = "IsMuted";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            isMuted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
            ApplyMuteState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Метод для коротких звуков остался без изменений
    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // --- ДОБАВЛЕНО: Новый метод для проигрывания фоновой музыки ---
    /// <summary>
    /// Проигрывает фоновую музыку. Останавливает предыдущую, если она играла.
    /// </summary>
    /// <param name="musicClip">Аудиоклип для проигрывания.</param>
    public void PlayMusic(AudioClip musicClip)
    {
        // Проверяем, что есть и источник, и клип
        if (musicClip != null && musicSource != null)
        {
            // Если уже играет та же музыка, ничего не делаем
            if (musicSource.clip == musicClip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.Stop(); // Останавливаем текущую музыку
            musicSource.clip = musicClip; // Назначаем новый клип
            musicSource.loop = true; // Делаем музыку зацикленной
            musicSource.Play(); // Включаем!
        }
    }


    // --- Управление общим звуком (без изменений) ---

    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyMuteState();

        PlayerPrefs.SetInt(MutePrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyMuteState()
    {
        // AudioListener.volume - это глобальная громкость, она повлияет на ВСЕ источники звука
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    public bool IsMuted()
    {
        return isMuted;
    }
}