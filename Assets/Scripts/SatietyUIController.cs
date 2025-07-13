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
        // Обновляем прогресс-бар каждый кадр
        if (gameManager != null)
        {
            // Получаем процент сытости (может быть > 1.0) и ограничиваем для отображения
            float fill = gameManager.GetSatietyPercentage();
            satietyProgressBar.fillAmount = Mathf.Clamp01(fill); // Clamp01 не дает значению выйти за пределы 0-1
        }

        // Включаем/выключаем кнопку в зависимости от того, хватает ли денег
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