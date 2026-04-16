using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Настройки экономики")]
    public double score = 0;
    public double scorePerClick = 1;
    public double scorePerSecond = 0;

    [Header("Множители")]
    public double clickMultiplier = 1.0;
    public double passiveMultiplier = 1.0;

    // Событие, на которое можно подписаться, чтобы обновлять UI
    public event Action OnScoreChanged;

    private void Awake()
    {
        Instance = this;
    }

    // Добавить очки
    public void AddScore(double amount)
    {
        if (amount <= 0) return;
        score += amount;
        OnScoreChanged?.Invoke();
    }

    // Потратить очки (возвращает true, если успешно)
    public bool SpendScore(double amount)
    {
        if (score >= amount)
        {
            score -= amount;
            OnScoreChanged?.Invoke();
            return true;
        }
        return false;
    }

    // Получить текущую прибыль за клик (с учетом всех множителей)
    public double GetFinalClickValue()
    {
        return scorePerClick * clickMultiplier;
    }

    // Получить текущую пассивную прибыль (с учетом множителей, но БЕЗ штрафа сытости)
    // Штраф мы применим в SatietyManager или GameManager
    public double GetBasePassiveValue()
    {
        return scorePerSecond * passiveMultiplier;
    }

    // Установка данных при загрузке
    public void LoadEconomy(double loadedScore)
    {
        score = loadedScore;
        OnScoreChanged?.Invoke();
    }

    #region --- УТИЛИТЫ (ФОРМАТИРОВАНИЕ ЧИСЕЛ) ---

    public string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "K";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "M";
        if (number < 1_000_000_000_000) return (number / 1_000_000_000).ToString("F1") + "Б";
        if (number < 1_000_000_000_000_000) return (number / 1_000_000_000_000).ToString("F1") + "Т";
        return (number / 1_000_000_000_000_000).ToString("F1") + "Кв";
    }

    #endregion
}