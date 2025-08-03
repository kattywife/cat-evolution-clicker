// SoundToggleButton.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Добавили для отслеживания наведения мыши
using System.Collections;       // Добавили для использования корутин

// Добавляем интерфейсы для отслеживания курсора
[RequireComponent(typeof(Button))]
public class SoundToggleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Иконки состояния")]
    [Tooltip("Иконка, когда звук включен")]
    public Sprite soundOnSprite;

    [Tooltip("Иконка, когда звук выключен")]
    public Sprite soundOffSprite;

    [Header("Ссылки")]
    [Tooltip("Image компонент, на котором будет меняться иконка")]
    public Image iconImage;

    // --- НОВЫЕ ПОЛЯ ДЛЯ АНИМАЦИИ И ЗВУКОВ ---
    [Header("Настройки анимации наведения")]
    [Tooltip("Насколько кнопка увеличится при наведении (1.1 = на 10%)")]
    public float hoverScaleFactor = 1.1f;

    [Tooltip("Скорость анимации увеличения/уменьшения")]
    public float scaleDuration = 0.1f;

    [Header("Звуки")]
    [Tooltip("Звук при наведении курсора на кнопку")]
    public AudioClip hoverSound;

    [Tooltip("Звук при нажатии на кнопку")]
    public AudioClip clickSound;
    // --- КОНЕЦ НОВЫХ ПОЛЕЙ ---

    private Button toggleButton;
    private Vector3 originalScale; // Сохраняем исходный размер

    void Awake()
    {
        // Запоминаем оригинальный размер кнопки один раз при запуске
        originalScale = transform.localScale;
    }

    void Start()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(OnButtonClick);

        // Устанавливаем правильную иконку при запуске игры
        UpdateIcon();
    }

    void OnButtonClick()
    {
        // --- ДОБАВИЛИ ЗВУК КЛИКА ---
        if (clickSound != null)
        {
            AudioManager.Instance.PlaySound(clickSound);
        }

        // Говорим AudioManager'у переключить звук
        AudioManager.Instance.ToggleMute();

        // Обновляем иконку на кнопке
        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (iconImage == null || AudioManager.Instance == null)
        {
            Debug.LogWarning("Не назначена иконка или не найден AudioManager!");
            return;
        }

        if (AudioManager.Instance.IsMuted())
        {
            iconImage.sprite = soundOffSprite;
        }
        else
        {
            iconImage.sprite = soundOnSprite;
        }
    }

    // --- НОВЫЕ МЕТОДЫ ДЛЯ ИНТЕРАКТИВНОСТИ ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Когда курсор НАВЕЛСЯ на кнопку
        if (hoverSound != null)
        {
            AudioManager.Instance.PlaySound(hoverSound);
        }

        // Запускаем анимацию увеличения
        StopAllCoroutines(); // Останавливаем другие анимации, если они есть
        StartCoroutine(AnimateScale(originalScale * hoverScaleFactor, scaleDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Когда курсор УШЕЛ с кнопки
        // Запускаем анимацию возврата к исходному размеру
        StopAllCoroutines();
        StartCoroutine(AnimateScale(originalScale, scaleDuration));
    }

    private IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}