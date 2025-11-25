using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Источники звука")]
    [Tooltip("Источник звука для одиночных эффектов (UI, клики и т.д.)")]
    public AudioSource sfxSource;

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

            // Загружаем состояние (1 = выключено, 0 = включено)
            isMuted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
            ApplyMuteState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && sfxSource != null)
        {
            // PlayOneShot играет, даже если source.loop = false
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip != null && musicSource != null)
        {
            if (musicSource.clip == musicClip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.Stop();
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyMuteState();

        PlayerPrefs.SetInt(MutePrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyMuteState()
    {
        // ВАЖНОЕ ИЗМЕНЕНИЕ:
        // Мы больше не трогаем AudioListener.volume (это выключило бы всё).
        // Мы выключаем только конкретные источники звука ИГРЫ.

        if (sfxSource != null)
        {
            sfxSource.mute = isMuted;
        }

        if (musicSource != null)
        {
            musicSource.mute = isMuted;
        }
    }

    public bool IsMuted()
    {
        return isMuted;
    }
}