using UnityEngine;
using System;

public class SatietyManager : MonoBehaviour
{
    public static SatietyManager Instance { get; private set; }

    [Header("Настройки сытости")]
    public float maxSatiety = 100f;
    public float currentSatiety;
    public float satietyDepletionRate = 0.5f;
    public float satietyPenaltyMultiplier = 0.1f;

    [Header("Визуальные эффекты")]
    public GameObject tearEffectObject;

    // Событие для обновления UI (слайдера)
    public event Action<float> OnSatietyChanged;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentSatiety = maxSatiety;
    }

    private void Update()
    {
        // Если котик на максимальном уровне (финал), голод отключается
        if (satietyDepletionRate <= 0) return;

        if (currentSatiety > 0)
        {
            currentSatiety -= satietyDepletionRate * Time.deltaTime;
            if (currentSatiety < 0) currentSatiety = 0;
            
            OnSatietyChanged?.Invoke(GetSatietyPercentage());
        }

        UpdateTearEffect();
    }

    // Проверка: котик голоден?
    public bool IsStarving() => currentSatiety <= 0;

    // Получить текущий множитель для пассивного дохода
    public double GetCurrentSatietyMultiplier()
    {
        return IsStarving() ? (double)satietyPenaltyMultiplier : 1.0;
    }

    private void UpdateTearEffect()
    {
        if (tearEffectObject == null) return;

        bool shouldShowTears = IsStarving();
        if (tearEffectObject.activeSelf != shouldShowTears)
        {
            tearEffectObject.SetActive(shouldShowTears);
        }
    }

    // Обычная кормежка
    public void Feed(float amount)
    {
        currentSatiety = Mathf.Min(maxSatiety, currentSatiety + amount);
        OnSatietyChanged?.Invoke(GetSatietyPercentage());
        
        // Уведомляем туториал
        if (TutorialManager.Instance) TutorialManager.Instance.OnCatFed();
    }

    // Супер-кормежка (за рекламу)
    public void SuperFeed()
    {
        currentSatiety = maxSatiety * 2.0f; // Можно сделать больше максимума
        OnSatietyChanged?.Invoke(GetSatietyPercentage());

        if (TutorialManager.Instance) TutorialManager.Instance.OnCatFed();
    }

    public float GetSatietyPercentage()
    {
        return maxSatiety == 0 ? 0 : currentSatiety / maxSatiety;
    }

    // Выключаем голод (для концовки игры)
    public void DisableHunger()
    {
        satietyDepletionRate = 0;
        currentSatiety = maxSatiety;
        if (tearEffectObject) tearEffectObject.SetActive(false);
        OnSatietyChanged?.Invoke(1f);
    }
}