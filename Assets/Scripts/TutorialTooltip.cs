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
        canvasGroup = GetComponent<CanvasGroup>();

        // При запуске игры сразу делаем облачко невидимым и прозрачным для кликов
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void Show()
    {
        // Прерываем исчезновение, если оно шло
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f));
        canvasGroup.blocksRaycasts = true; // (Опционально) Блокировать клики сквозь облачко
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f));
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}