using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SatietyUIController : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    public GameManager gameManager;
    public Image satietyProgressBar; // Сюда перетащи ProgressBar_Fill
    public Button feedButton;
    public Button superFeedButton; // Кнопка для рекламы
    public TextMeshProUGUI satietyText; // цифры процентов голода


    [Header("Настройки кормления")]
    public double feedCost = 10;
    public float feedAmount = 50f; // Сколько сытости восстанавливает обычная еда

    void Start()
    {
        // Назначаем действия для кнопок через код
        feedButton.onClick.AddListener(OnFeedButtonClicked);
        superFeedButton.onClick.AddListener(OnSuperFeedButtonClicked);
    }

    void Update()
    {
        if (gameManager != null)
        {
            float fillPercentage = gameManager.GetSatietyPercentage();

            // Обновляем прогресс-бар
            satietyProgressBar.fillAmount = Mathf.Clamp01(fillPercentage);

            // <<< ВОТ НОВАЯ СТРОКА >>>
            // Умножаем на 100, чтобы получить проценты, и форматируем как целое число
            satietyText.text = (fillPercentage * 100).ToString("F0") + "%";
        }

        // Включаем/выключаем кнопку
        feedButton.interactable = gameManager.score >= feedCost;
    }

    void OnFeedButtonClicked()
    {
        gameManager.FeedCat(feedCost, feedAmount);
    }

    void OnSuperFeedButtonClicked()
    {
        // TODO: Здесь будет логика вызова рекламы
        Debug.Log("Нажата кнопка 'Супер корм'. Нужно показать рекламу.");

        // Временная заглушка для теста: сразу даем награду
        gameManager.SuperFeedCat();
    }
}