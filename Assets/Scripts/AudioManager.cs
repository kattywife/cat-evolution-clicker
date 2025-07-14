// AudioManager.cs

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Это "синглтон" - простой способ сделать менеджер доступным из любого другого скрипта.
    public static AudioManager Instance;

    // Сюда мы перетащим компонент, который будет проигрывать звуки.
    [Tooltip("Источник звука для одиночных эффектов (UI, клики и т.д.)")]
    public AudioSource sfxSource;

    private void Awake()
    {
        // Настройка синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Чтобы менеджер не удалялся при смене сцен
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Публичный метод, который сможет вызвать любой другой скрипт
    public void PlaySound(AudioClip clip)
    {
        // Проверяем, что есть и звук, и источник, чтобы избежать ошибок
        if (clip != null && sfxSource != null)
        {
            // PlayOneShot позволяет проигрывать звуки, не прерывая друг друга.
            // Идеально для частых кликов!
            sfxSource.PlayOneShot(clip);
        }
    }
}