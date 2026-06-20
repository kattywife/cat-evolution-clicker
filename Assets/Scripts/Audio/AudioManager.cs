using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public event Action<bool> OnMuteChanged; // Событие для мгновенной синхронизации

    [Header("Источники звука")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    private bool isMuted = false;
    private const string MutePrefKey = "IsMuted";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            isMuted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyMuteState(); // Применяем состояние при старте
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && sfxSource != null && !isMuted)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || musicSource == null) return;
        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.Stop();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.mute = isMuted; // Ставим актуальный статус сразу
        musicSource.Play();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyMuteState();

        PlayerPrefs.SetInt(MutePrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        // Оповещаем всех слушателей (например, AudioMuteSync)
        OnMuteChanged?.Invoke(isMuted);
    }

    private void ApplyMuteState()
    {
        if (sfxSource != null) sfxSource.mute = isMuted;
        if (musicSource != null) musicSource.mute = isMuted;
    }

    public bool IsMuted() => isMuted;
}