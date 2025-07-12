using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;


public class GameManager : MonoBehaviour
{
    // --- ДАННЫЕ ИГРЫ ---
    [Header("Настройки уровней")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("Настройки улучшений")]
    public List<UpgradeData> upgrades;

    // --- ПЕРЕМЕННЫЕ ГЕЙМПЛЕЯ ---
    [Header("Текущее состояние")]
    public double score = 0;
    public long scorePerClick = 1;
    public long scorePerSecond = 0;

    // --- ССЫЛКИ НА UI ---
    [Header("Ссылки на UI элементы")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI perSecondText;
    public Image catImage;
    public Slider levelProgressBar;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI progressText;

    [Header("Эффекты")]
    public ParticleSystem levelUpEffect;

    [Header("Магазин")]
    public GameObject upgradeButtonPrefab;
    public Transform shopPanel;
    private List<UpgradeButtonUI> shopButtons = new List<UpgradeButtonUI>();

    // --- ОСНОВНЫЕ МЕТОДЫ UNITY ---

    void Start()
    {
        currentLevelIndex = 0;
        scorePerClick = 1;
        scorePerSecond = 0;
        score = 0;

        CreateShop();
        ApplyLevelUp();
    }

    void Update()
    {
        if (scorePerSecond > 0)
        {
            score += scorePerSecond * Time.deltaTime;
        }

        UpdateAllUI();
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ---

    public void OnCatClicked()
    {
        score += scorePerClick;
        CheckForLevelUp();

        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {

        // --- НАЧАЛО НАШЕЙ ПРОВЕРКИ ---
        Debug.Log($"Попытка покупки '{upgrade.name}'. " +
                  $"Текущий пассивный доход: {scorePerSecond}. " +
                  $"Сила улучшения (power): {upgrade.power}. " +
                  $"Стоимость: {cost}");
        // --- КОНЕЦ ПРОВЕРКИ ---
        if (score >= cost)
        {
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
    }

    // --- ПРИВАТНЫЕ МЕТОДЫ-ПОМОЩНИКИ ---

    private void UpdateAllUI()
    {
        UpdateAllUITexts();
        UpdateProgressBar();
        UpdateAllShopButtons();
    }

    private void UpdateAllUITexts()
    {
        if (totalScoreText != null) totalScoreText.text = FormatNumber(score);
        if (perSecondText != null) perSecondText.text = $"{FormatNumber(scorePerSecond)}/сек";
    }

    // --- ФИНАЛЬНОЕ ИСПРАВЛЕНИЕ ---
    private void UpdateProgressBar()
    {
        if (levelProgressBar == null) return;

        // 1. Обрабатываем случай максимального уровня
        if (currentLevelIndex >= levels.Count - 1 && levels.Count > 1)
        {
            levelProgressBar.minValue = 0;
            levelProgressBar.maxValue = 1;
            levelProgressBar.value = 1;
            if (levelNumberText != null) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}";
            if (progressText != null) progressText.text = "МАКС.";
            return;
        }

        // 2. Определяем конечную точку - цель для следующего уровня
        double barEndValue = levels[currentLevelIndex + 1].scoreToReach;

        // 3. Настраиваем сам слайдер. Начало всегда 0.
        levelProgressBar.minValue = 0f; // <--- ИЗМЕНЕНИЕ: Начало всегда ноль
        levelProgressBar.maxValue = (float)barEndValue;
        levelProgressBar.value = (float)score;

        // 4. Обновляем текст (эта строка уже работает как надо для новой логики)
        if (levelNumberText != null) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}";
        if (progressText != null)
        {
            progressText.text = $"{FormatNumber(score)} / {FormatNumber(barEndValue)}";
        }
    }


    private void UpdateAllShopButtons()
    {
        foreach (var button in shopButtons)
        {
            button.UpdateInteractableState(score);
        }
    }

    private void CreateShop()
    {
        foreach (var upgrade in upgrades)
        {
            GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopPanel);
            UpgradeButtonUI buttonUI = newButtonGO.GetComponent<UpgradeButtonUI>();
            buttonUI.Setup(upgrade, this);
            shopButtons.Add(buttonUI);
        }
    }

    private void CheckForLevelUp()
    {
        while (currentLevelIndex + 1 < levels.Count && score >= levels[currentLevelIndex + 1].scoreToReach)
        {
            currentLevelIndex++;
            ApplyLevelUp();
        }
    }

    private void ApplyLevelUp()
    {
        if (levels.Count > 0 && currentLevelIndex < levels.Count)
        {
            catImage.sprite = levels[currentLevelIndex].catSprite;
        }


        if (levelUpEffect != null)
        {
            levelUpEffect.Play();
        }

        // Принудительное обновление UI сразу после левел апа очень важно,
        // чтобы прогресс-бар перенастроился с новыми minValue/maxValue.
        UpdateAllUI();
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "K";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "M";
        return (number / 1_000_000_000).ToString("F1") + "B";
    }

    private void ResetCatScale()
    {
        catImage.transform.localScale = new Vector3(1f, 1f, 1f);
    }
}