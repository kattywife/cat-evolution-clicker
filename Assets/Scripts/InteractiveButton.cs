// InteractiveButton.cs
// Скрипт для добавления "жизни" кнопкам через код.

using UnityEngine;
using UnityEngine.EventSystems; // Обязательно для работы с событиями мыши
using UnityEngine.UI;           // Обязательно для работы с UI

[RequireComponent(typeof(Button))] // Гарантирует, что на объекте есть компонент Button
public class InteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Настройки эффектов")]
    [Tooltip("Звук при наведении курсора на кнопку")]
    public AudioClip hoverSound;

    [Tooltip("Звук при нажатии на кнопку")]
    public AudioClip clickSound;

    [Tooltip("Насколько кнопка увеличится при нажатии (1.1 = 110%)")]
    public float pressedScale = 1.1f;

    // Приватные переменные для хранения состояний
    private RectTransform rectTransform;
    private Vector3 originalScale;

    // Awake вызывается один раз при создании объекта
    private void Awake()
    {
        // Кэшируем компоненты для производительности, чтобы не искать их каждый раз
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    // --- Реализация интерфейсов EventSystem ---

    // Вызывается, когда курсор мыши входит в область кнопки
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            // Проигрываем звук через наш AudioManager
            AudioManager.Instance.PlaySound(hoverSound, 0.7f); // Громкость можно настроить
        }
    }

    // Вызывается в момент, когда кнопка мыши НАЖАТА над объектом
    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.localScale = originalScale * pressedScale;
    }

    // Вызывается в момент, когда кнопка мыши ОТПУЩЕНА над объектом
    public void OnPointerUp(PointerEventData eventData)
    {
        rectTransform.localScale = originalScale;
    }

    // Вызывается, когда курсор покидает область кнопки
    // Это нужно, чтобы кнопка вернулась в нормальный размер, 
    // если пользователь нажал, но увёл мышь в сторону и отпустил
    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.localScale = originalScale;
    }

    // Вызывается при полном клике (нажал и отпустил на одном объекте)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            AudioManager.Instance.PlaySound(clickSound);
        }
    }
}