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

    // Событие для обновления UI
    public event Action OnScoreChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Добавить очки
    public void AddScore(double amount)
    {
        if (amount <= 0) return;
        score += amount;
        OnScoreChanged?.Invoke();
    }

    // Потратить очки
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

    // --- МЕТОДЫ ДЛЯ GAMEMANAGER (которые вызывали ошибку) ---

    public double GetFinalClickValue()
    {
        return scorePerClick * clickMultiplier;
    }


    public double GetBasePassiveValue()
    {
        return scorePerSecond * passiveMultiplier;
    }

    // --- МЕТОДЫ ЗАГРУЗКИ ---

    public void LoadFullEconomy(double s, double spc, double sps, double cm, double pm)
    {
        score = s;
        scorePerClick = spc;
        scorePerSecond = sps;
        clickMultiplier = cm;
        passiveMultiplier = pm;
        
        Debug.Log("[EconomyManager] Full economy loaded and applied");
        OnScoreChanged?.Invoke();
    }

    public void LoadEconomy(double loadedScore)
    {
        score = loadedScore;
        OnScoreChanged?.Invoke();
    }

    // --- ФОРМАТИРОВАНИЕ ЧИСЕЛ ---

    public string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "K";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "M";
        if (number < 1_000_000_000_000) return (number / 1_000_000_000).ToString("F1") + "B";
        return (number / 1_000_000_000_000).ToString("F1") + "T";
    }
}