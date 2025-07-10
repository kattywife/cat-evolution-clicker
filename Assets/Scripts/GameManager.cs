using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    // --- ДАННЫЕ ИГРЫ (МОДЕЛЬ) ---
    [Header("Настройки уровней")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("Настройки улучшений")]
    public List<UpgradeData> upgrades; // Список всех доступных улучшений

    // --- ПЕРЕМЕННЫЕ ГЕЙМПЛЕЯ ---
    [Header("Текущее состояние")]
    public double score = 0;
    public long scorePerClick = 1; // Добавляем силу клика
    public long scorePerSecond = 0; // Добавляем пассивный доход

    // --- ССЫЛКИ НА UI (ПРЕДСТАВЛЕНИЕ) ---
    [Header("Ссылки на UI элементы")]
    public TextMeshProUGUI scoreText;
    public Image catImage;

    [Header("Магазин")]
    public GameObject upgradeButtonPrefab; // Сюда перетащим наш префаб кнопки
    public Transform shopPanel; // Панель, куда будут добавляться кнопки

    void Start()
    {
        // Устанавливаем начальные значения
        score = 0;
        currentLevelIndex = 0;
        ApplyLevelUp();
        UpdateScoreText();

        // Создаем магазин при старте
        CreateShop();
    }

    void Update()
    {
        // Пассивный доход
        score += scorePerSecond * Time.deltaTime;
        UpdateScoreText();
        // Здесь можно будет добавить логику обновления состояния кнопок (серые/цветные)
    }

    // --- ОСНОВНЫЕ МЕТОДЫ ---

    public void OnCatClicked()
    {
        score += scorePerClick; // Используем переменную
        // ... остальной код клика
        UpdateScoreText();
        CheckForLevelUp();
        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        Debug.Log(">>> GameManager получил запрос на покупку: " + upgrade.upgradeName + " <<<"); // ЛОВУШКА 5

        if (score >= cost)
        {
            Debug.Log("$$$ Денег хватает! Покупаем! $$$"); // ЛОВУШКА 6
            score -= cost;
            if (upgrade.type == UpgradeType.PerClick)
            {
                scorePerClick += upgrade.power;
            }
            else if (upgrade.type == UpgradeType.PerSecond)
            {
                scorePerSecond += upgrade.power;
            }

            button.OnPurchaseSuccess();
        }
        else
        {
            Debug.Log("!!! Денег не хватает. Нужно " + cost + ", а у меня только " + score); // ЛОВУШКА 7
        }
    }

    // --- МЕТОДЫ-ПОМОЩНИКИ ---

    private void CreateShop()
    {
        Debug.Log("--- Начинаю создавать магазин. Улучшений в списке: " + upgrades.Count + " ---"); // ЛОВУШКА 2

        foreach (var upgrade in upgrades)
        {
            GameObject newButton = Instantiate(upgradeButtonPrefab, shopPanel);
            Debug.Log("Создаю кнопку для: " + upgrade.upgradeName); // ЛОВУШКА 3
            newButton.GetComponent<UpgradeButtonUI>().Setup(upgrade, this);
        }
    }

    private void CheckForLevelUp()
    {
        if (currentLevelIndex + 1 >= levels.Count) return;
        if (score >= levels[currentLevelIndex + 1].scoreToReach)
        {
            currentLevelIndex++;
            ApplyLevelUp();
        }
    }

    private void ApplyLevelUp()
    {
        catImage.sprite = levels[currentLevelIndex].catSprite;
    }

    private void ResetCatScale()
    {
        catImage.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("F0");
        }
    }
}