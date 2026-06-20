using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Элементы UI")]
    public Button gearButton;           // Кнопка с шестеренкой
    public RectTransform gearIcon;      // Картинка шестеренки (для вращения)
    public RectTransform cloudPanel;    // Выпадающее облачко
    public Button closeBlockerButton;   // Невидимая кнопка на весь экран

    [Header("Анимация")]
    public float animationDuration = 0.3f;
    public Vector3 openRotation = new Vector3(0, 0, -180f); // На сколько градусов крутить

    private bool isOpen = false;
    private bool isAnimating = false;
    private Vector3 closedRotation = Vector3.zero;

    private void Start()
    {
        // Изначально прячем облачко и блокировщик
        cloudPanel.localScale = new Vector3(1, 0, 1); 
        closeBlockerButton.gameObject.SetActive(false);

        // Назначаем клики
        gearButton.onClick.AddListener(ToggleMenu);
        closeBlockerButton.onClick.AddListener(CloseMenu);
    }

    public void ToggleMenu()
    {
        if (isAnimating) return;

        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    private void OpenMenu()
    {
        isOpen = true;
        closeBlockerButton.gameObject.SetActive(true); // Включаем зону клика "мимо"
        StartCoroutine(AnimateMenu(1f, openRotation));
    }

    private void CloseMenu()
    {
        isOpen = false;
        closeBlockerButton.gameObject.SetActive(false); // Выключаем зону клика "мимо"
        StartCoroutine(AnimateMenu(0f, closedRotation));
    }

    private IEnumerator AnimateMenu(float targetScaleY, Vector3 targetRotation)
    {
        isAnimating = true;

        Vector3 startScale = cloudPanel.localScale;
        Vector3 targetScale = new Vector3(targetScaleY, 1, 1);

        Quaternion startRot = gearIcon.localRotation;
        Quaternion targetRot = Quaternion.Euler(targetRotation);

        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime; // unscaledDeltaTime позволяет анимации работать даже на паузе!
            float t = timer / animationDuration;
            
            // Плавное сглаживание
            t = t * t * (3f - 2f * t);

            cloudPanel.localScale = Vector3.Lerp(startScale, targetScale, t);
            gearIcon.localRotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        cloudPanel.localScale = targetScale;
        gearIcon.localRotation = targetRot;
        isAnimating = false;
    }

    // --- ФУНКЦИИ ДЛЯ КНОПОК ВНУТРИ МЕНЮ ---

    public void OnPauseClicked()
    {
        // Вызываем паузу из GameManager
        GameManager.Instance.TogglePause();
        
        // По желанию: закрывать меню при нажатии на паузу
        // CloseMenu(); 
    }

    public void OnRestartClicked()
    {
        // Вызываем уже готовый метод рестарта
        GameManager.Instance.RestartGame();
    }
}