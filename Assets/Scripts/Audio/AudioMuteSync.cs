using UnityEngine;

public class AudioMuteSync : MonoBehaviour
{
    private AudioSource myAudioSource;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();

        if (AudioManager.Instance != null)
        {
            // Подписываемся на событие изменения звука
            AudioManager.Instance.OnMuteChanged += HandleMuteChanged;
            
            // Устанавливаем текущее состояние сразу
            myAudioSource.mute = AudioManager.Instance.IsMuted();
        }
    }

    // Этот метод вызывается ТОЛЬКО когда нажали кнопку Mute
    private void HandleMuteChanged(bool muted)
    {
        if (myAudioSource != null)
        {
            myAudioSource.mute = muted;
        }
    }

    private void OnDestroy()
    {
        // Отписываемся, чтобы не было ошибок при удалении объекта
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnMuteChanged -= HandleMuteChanged;
        }
    }
}