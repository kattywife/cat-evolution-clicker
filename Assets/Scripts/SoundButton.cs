using UnityEngine;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
    [Header("Настройки UI")]
    [Tooltip("Сюда перетащи картинку (Image), которая показывает иконку динамика")]
    public Image iconImage;

    [Tooltip("Спрайт включенного звука")]
    public Sprite soundOnSprite;

    [Tooltip("Спрайт выключенного звука")]
    public Sprite soundOffSprite;

    private Button btn;

    void Start()
    {
        btn = GetComponent<Button>();

        // Добавляем функцию нажатия
        btn.onClick.AddListener(OnButtonClick);

        // Сразу выставляем правильную картинку при запуске
        UpdateIcon();
    }

    void OnButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        if (AudioManager.Instance != null && iconImage != null)
        {
            bool isMuted = AudioManager.Instance.IsMuted();
            // Если звук выключен (Muted) -> ставим картинку Off
            // Если включен -> ставим картинку On
            iconImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }
}