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

    // --- НОВОЕ: Переменная для задержки голода ---
    private bool canDeplete = false; 

    public event Action<float> OnSatietyChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Начинаем всегда со 100%
        currentSatiety = maxSatiety;
    }

    private void Update()
{
    // --- НОВОЕ: Ждем, пока закончится экран загрузки и интро ---
    // (CutsceneManager включает GameManager только когда начинается геймплей)
    if (GameManager.Instance == null || !GameManager.Instance.enabled) return;

    // Если голод не активирован ИЛИ мы в финале — выходим
    if (!canDeplete || satietyDepletionRate <= 0) return;

    if (currentSatiety > 0)
    {
        currentSatiety -= satietyDepletionRate * Time.deltaTime;
        if (currentSatiety < 0) currentSatiety = 0;
        
        OnSatietyChanged?.Invoke(GetSatietyPercentage());
    }

    UpdateTearEffect();
}

    // --- НОВОЕ: Метод, который вызывает GameManager при первом клике ---
    public void StartHunger()
    {
        if (!canDeplete)
        {
            canDeplete = true;
            Debug.Log("[SatietyManager] Голод активирован первым кликом!");
        }
    }

    public bool IsStarving() => currentSatiety <= 0;

    public double GetCurrentSatietyMultiplier()
    {
        return IsStarving() ? (double)satietyPenaltyMultiplier : 1.0;
    }

    private void UpdateTearEffect()
    {
        if (tearEffectObject == null) return;
        bool shouldShowTears = IsStarving();
        if (tearEffectObject.activeSelf != shouldShowTears)
            tearEffectObject.SetActive(shouldShowTears);
    }

    public void Feed(float amount)
    {
        currentSatiety = Mathf.Min(maxSatiety, currentSatiety + amount);
        OnSatietyChanged?.Invoke(GetSatietyPercentage());
        if (TutorialManager.Instance) TutorialManager.Instance.OnCatFed();
    }

    public void SuperFeed()
    {
        currentSatiety = maxSatiety * 2.0f; 
        OnSatietyChanged?.Invoke(GetSatietyPercentage());
        if (TutorialManager.Instance) TutorialManager.Instance.OnCatFed();
    }

    public float GetSatietyPercentage()
    {
        return maxSatiety == 0 ? 0 : currentSatiety / maxSatiety;
    }

    public void DisableHunger()
    {
        satietyDepletionRate = 0;
        currentSatiety = maxSatiety;
        if (tearEffectObject) tearEffectObject.SetActive(false);
        OnSatietyChanged?.Invoke(1f);
    }

    public float GetCurrentSatiety() => currentSatiety;

    public void LoadSatiety(float value) 
    { 
        currentSatiety = value; 
        
        // Защита от микропогрешностей. Проверяем не ровно 100, а хотя бы < 99.5
        // Это спасет, если в Яндекс сохранилось 99.999 вместо 100
        if (currentSatiety < maxSatiety - 0.5f) 
        {
            canDeplete = true;
        }

        OnSatietyChanged?.Invoke(GetSatietyPercentage());
        UpdateTearEffect(); 
    }

    
}