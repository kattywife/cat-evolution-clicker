// GameManager.cs

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

    // <<< ИЗМЕНЕНИЕ: Добавлены переменные для системы сытости >>>
    [Header("Настройки Сытости")]
    public float maxSatiety = 100f; // Максимальное значение сытости (100%)
    public float currentSatiety; // Текущая сытость
    [Tooltip("Сколько единиц сытости котик теряет в секунду")]
    public float satietyDepletionRate = 0.5f;
    [Tooltip("Множитель дохода, когда котик голоден (0.1 = 10%)")]
    public float satietyPenaltyMultiplier = 0.1f;


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

        // <<< ИЗМЕНЕНИЕ: Инициализация сытости при старте >>>
        currentSatiety = maxSatiety;

        CreateShop();
        ApplyLevelUp();
    }

    void Update()
    {
        // <<< ИЗМЕНЕНИЕ: Полностью переработанная логика начисления пассивного дохода >>>
        // 1. Уменьшаем сытость со временем
        if (currentSatiety > 0)
        {
            currentSatiety -= satietyDepletionRate * Time.deltaTime;
        }
        else
        {
            currentSatiety = 0; // Не даем уйти в минус
        }

        // 2. Рассчитываем эффективный доход в секунду
        double effectiveSps = scorePerSecond;
        if (currentSatiety <= 0)
        {
            effectiveSps *= satietyPenaltyMultiplier; // Применяем штраф, если голоден
        }

        // 3. Начисляем очки
        if (effectiveSps > 0)
        {
            score += effectiveSps * Time.deltaTime;
        }

        // 4. Обновляем весь UI, включая новые элементы, которые управляются из других скриптов
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
        Debug.Log($"Попытка покупки '{upgrade.name}'. " +
                  $"Текущий пассивный доход: {scorePerSecond}. " +
                  $"Сила улучшения (power): {upgrade.power}. " +
                  $"Стоимость: {cost}");

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

    // <<< ИЗМЕНЕНИЕ: Добавлены новые публичные методы для управления сытостью >>>
    public void FeedCat(double cost, float amount)
    {
        if (score >= cost)
        {
            score -= cost;

            // Запоминаем, была ли сытость уже выше 100%
            bool wasAlreadySuperFed = currentSatiety > maxSatiety;

            currentSatiety += amount;

            // Ограничиваем до 100% только если сытость не была "супер" до этого
            if (!wasAlreadySuperFed && currentSatiety > maxSatiety)
            {
                currentSatiety = maxSatiety;
            }

            Debug.Log("Котик покормлен. Текущая сытость: " + currentSatiety);
        }
    }

    public void SuperFeedCat()
    {
        // Этот метод будет вызван ПОСЛЕ успешного просмотра рекламы
        // Пока что он вызывается напрямую из заглушки в SatietyUIController
        currentSatiety = maxSatiety * 2.0f; // Восполняем до 200%
        Debug.Log("Котик получил супер-корм! Текущая сытость: " + currentSatiety);
    }

    // Вспомогательный метод для UI
    public float GetSatietyPercentage()
    {
        if (maxSatiety == 0) return 0; // Защита от деления на ноль
        return currentSatiety / maxSatiety;
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
        // <<< ИЗМЕНЕНИЕ: Отображаем базовый доход, а не эффективный, чтобы игрок видел, на что он влияет >>>
        if (perSecondText != null) perSecondText.text = $"{FormatNumber(scorePerSecond)}/сек";
    }

    private void UpdateProgressBar()
    {
        if (levelProgressBar == null) return;

        if (currentLevelIndex >= levels.Count - 1 && levels.Count > 1)
        {
            levelProgressBar.minValue = 0;
            levelProgressBar.maxValue = 1;
            levelProgressBar.value = 1;
            if (levelNumberText != null) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}";
            if (progressText != null) progressText.text = "МАКС.";
            return;
        }

        double barEndValue = levels[currentLevelIndex + 1].scoreToReach;
        levelProgressBar.minValue = 0f;
        levelProgressBar.maxValue = (float)barEndValue;
        levelProgressBar.value = (float)score;

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