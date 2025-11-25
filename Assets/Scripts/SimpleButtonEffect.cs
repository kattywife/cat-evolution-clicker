using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SimpleButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Настройки")]
    public float scaleFactor = 1.1f; // Насколько увеличивать (1.1 = на 10%)
    public float duration = 0.1f;    // Скорость анимации

    [Header("Звуки")]
    public AudioClip hoverSound;     // Звук наведения
    public AudioClip clickSound;     // Звук нажатия

    private Vector3 originalScale;
    private Button btn;

    void Start()
    {
        originalScale = transform.localScale;
        btn = GetComponent<Button>(); // Скрипт сам найдет кнопку на этом объекте
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Если кнопка выключена (неактивна), анимации и звука не будет
        if (btn != null && !btn.interactable) return;

        // Воспроизводим звук (используем ваш AudioManager)
        if (hoverSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(hoverSound);
        }

        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale * scaleFactor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;

        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(clickSound);
        }
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        float timer = 0f;
        Vector3 start = transform.localScale;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // unscaledDeltaTime работает даже если игра на паузе
            transform.localScale = Vector3.Lerp(start, target, timer / duration);
            yield return null;
        }
        transform.localScale = target;
    }
}