using UnityEngine;

public class AudioMuteSync : MonoBehaviour
{
    private AudioSource myAudioSource;

    void Start()
    {
        // Находим звук на этом объекте
        myAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Если у нас есть связь с главным менеджером звука
        if (myAudioSource != null && AudioManager.Instance != null)
        {
            // Копируем настройку "Mute" оттуда
            // Если в главном меню звук выключен (IsMuted = true), то и мы выключаемся
            myAudioSource.mute = AudioManager.Instance.IsMuted();
        }
    }
}