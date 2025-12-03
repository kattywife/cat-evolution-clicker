using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TutorialTooltip : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        Init();
        // При старте скрываем (делаем прозрачным), но не выключаем сам объект
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Init()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        // 1. Включаем объект (ставим галочку)
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Init();

        // 2. ГЛАВНАЯ ПРОВЕРКА: Активен ли объект в сцене?
        // (Если родитель объекта выключен, то activeInHierarchy будет false,
        // и запуск корутины вызовет ошибку. Мы это предотвращаем).
        if (!gameObject.activeInHierarchy)
        {
            // Если объект всё равно выключен (из-за родителя),
            // просто ставим альфу на 1, чтобы он появился сразу, как только включится родитель.
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
            return; // Выходим, не запуская корутину
        }

        // 3. Если всё ок — запускаем плавную анимацию
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f));

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        // Если объект уже выключен в иерархии, анимацию играть нельзя и не нужно
        if (!gameObject.activeInHierarchy)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            return;
        }

        Init();

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f));

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;

        // Если полностью скрыли - можно выключить объект для экономии ресурсов
        if (targetAlpha == 0f)
        {
            gameObject.SetActive(false);
        }
    }
}