using UnityEngine;
using UnityEngine.UI;

public class PauseButtonUI : MonoBehaviour
{
    [Header("Настройки UI")]
    [Tooltip("Картинка иконки внутри кнопки паузы")]
    public Image iconImage;

    [Tooltip("Спрайт паузы (две полоски)")]
    public Sprite pauseSprite;

    [Tooltip("Спрайт продолжения (треугольник/плей)")]
    public Sprite playSprite;

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
        if (GameManager.Instance != null)
        {
            // Переключаем паузу в GameManager
            GameManager.Instance.TogglePause();
            
            // Обновляем иконку
            UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        if (GameManager.Instance != null && iconImage != null)
        {
            // Если игра на паузе -> показываем иконку Play (чтобы нажать и продолжить)
            // Если игра идет -> показываем иконку Pause (чтобы нажать и остановить)
            bool isPaused = GameManager.Instance.isGamePaused;
            iconImage.sprite = isPaused ? playSprite : pauseSprite;
        }
    }

    // Метод на случай, если меню закрывается/открывается, чтобы обновить состояние
    private void OnEnable()
    {
        UpdateIcon();
    }
}