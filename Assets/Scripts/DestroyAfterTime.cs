using UnityEngine;
using System.Collections; // Обязательно добавьте эту строку для работы с корутинами!

public class DestroyAfterTime : MonoBehaviour
{
    [Tooltip("Общее время жизни объекта в секундах")]
    public float lifeTime = 3f;

    [Tooltip("За сколько секунд до уничтожения объект должен плавно исчезнуть")]
    public float fadeDuration = 1f;

    // Ссылка на компонент, который отвечает за отрисовку спрайта
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // При создании объекта сразу находим и запоминаем его SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Запускаем нашу главную "программу" исчезновения
        StartCoroutine(FadeOutAndDestroy());
    }

    // Это "корутина" - функция, которая может растягивать свои действия во времени
    private IEnumerator FadeOutAndDestroy()
    {
        // 1. Сначала ждем основное время, пока слеза просто видна
        // Мы вычитаем время на исчезание из общего времени жизни
        float initialWaitTime = lifeTime - fadeDuration;

        // Убедимся, что время ожидания не отрицательное
        if (initialWaitTime > 0)
        {
            yield return new WaitForSeconds(initialWaitTime);
        }

        // 2. Теперь начинаем процесс плавного исчезновения
        float timer = 0f;
        Color startColor = spriteRenderer.color; // Запоминаем исходный цвет

        // Этот цикл будет работать, пока не пройдет время, отведенное на исчезание
        while (timer < fadeDuration)
        {
            // Увеличиваем таймер на время, прошедшее с прошлого кадра
            timer += Time.deltaTime;

            // Высчитываем новую прозрачность (альфа-канал).
            // Mathf.Lerp плавно изменяет значение от 1 (полностью видим) до 0 (невидим)
            float newAlpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            // Применяем новый цвет с новой прозрачностью
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            // Ждем следующего кадра, чтобы продолжить цикл
            yield return null;
        }

        // 3. После того, как объект стал полностью невидимым, уничтожаем его
        Destroy(gameObject);
    }
}