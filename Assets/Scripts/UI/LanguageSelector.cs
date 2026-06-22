using UnityEngine;
using UnityEngine.UI;

public class LanguageSelector : MonoBehaviour
{
    [Header("Кнопки выбора языка")]
    public Button ruButton;
    public Button enButton;
    public Button trButton;

    [Header("Настройки цветов")]
    public Color activeColor = Color.white; // 100% яркость для активной кнопки
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Слегка серая для неактивных

    private void Start()
    {
        // Программно подписываемся на клики кнопок
        if (ruButton) ruButton.onClick.AddListener(() => ChangeLanguage("ru"));
        if (enButton) enButton.onClick.AddListener(() => ChangeLanguage("en"));
        if (trButton) trButton.onClick.AddListener(() => ChangeLanguage("tr"));

        // Обновляем внешний вид кнопок под текущий автоопределенный язык
        UpdateVisuals();
    }

    private void ChangeLanguage(string langCode)
    {
        if (LocalizationManager.Instance != null)
        {
            // Переключаем язык в игре и обновляем все тексты
            LocalizationManager.Instance.SetLanguage(langCode);
            
            // Запоминаем выбор в YandexManager
            if (YandexManager.Instance != null)
            {
                YandexManager.Instance.currentLanguage = langCode;
            }
        }

        // Обновляем визуальное выделение кнопок
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (LocalizationManager.Instance == null) return;

        // Узнаем, какой язык активен в данный момент
        string currentLang = LocalizationManager.Instance.GetActiveLanguage();

        // Подсвечиваем активную кнопку и приглушаем остальные
        SetButtonState(ruButton, currentLang == "ru");
        SetButtonState(enButton, currentLang == "en");
        SetButtonState(trButton, currentLang == "tr");
    }

    private void SetButtonState(Button button, bool isActive)
    {
        if (button == null) return;

        // Меняем цвет картинки кнопки (активная — яркая деревянная, неактивные — затемненные)
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? activeColor : inactiveColor;
        }

        // Слегка увеличиваем активную кнопку в масштабе (1.05), а неактивные возвращаем к 1.0
        button.transform.localScale = isActive ? Vector3.one * 1.05f : Vector3.one;
    }
}