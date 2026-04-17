using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public double score;
    public int levelIndex;
    public bool introWatched;
    public int unlockedItemsCount;
    public float shopScrollPosition; 
    public float currentSatiety; 

    public double scorePerClick;
    public double scorePerSecond;
    public double clickMultiplier;
    public double passiveMultiplier;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private float autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 20f;
    private bool isDataLoaded = false;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.OnDataLoaded += HandleLoad;
            YandexManager.Instance.LoadData();
        }
    }

    private void Update()
    {
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            Save();
            autoSaveTimer = 0f;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) Save();
    }

    public void Save()
    {
        if (!isDataLoaded || EconomyManager.Instance == null) return;

        GameData data = new GameData();
        data.score = EconomyManager.Instance.score;
        
        // Защита множителей
        data.scorePerClick = Math.Max(1.0, EconomyManager.Instance.scorePerClick);
        data.scorePerSecond = EconomyManager.Instance.scorePerSecond;
        data.clickMultiplier = Math.Max(1.0, EconomyManager.Instance.clickMultiplier);
        data.passiveMultiplier = Math.Max(1.0, EconomyManager.Instance.passiveMultiplier);
        
        data.levelIndex = ProgressionManager.Instance.GetCurrentLevel();
        data.introWatched = CutsceneManager.Instance.hasWatchedIntro;
        
        // Данные из других менеджеров
        if (ShopManager.Instance != null)
        {
            data.unlockedItemsCount = ShopManager.Instance.GetUnlockedCount();
            data.shopScrollPosition = ShopManager.Instance.GetScrollPosition();
        }
        
        if (SatietyManager.Instance != null)
            data.currentSatiety = SatietyManager.Instance.GetCurrentSatiety();

        string json = JsonUtility.ToJson(data);
        YandexManager.Instance.SaveData(json);
    }

    private void HandleLoad(string json)
    {
        isDataLoaded = true;
        if (string.IsNullOrEmpty(json) || json == "{}") return;

        try
        {
            GameData data = JsonUtility.FromJson<GameData>(json);

            // 1. Экономика
            if (data.scorePerClick < 1) data.scorePerClick = 1;
            if (data.clickMultiplier < 1) data.clickMultiplier = 1;
            EconomyManager.Instance.LoadFullEconomy(data.score, data.scorePerClick, data.scorePerSecond, data.clickMultiplier, data.passiveMultiplier);

            // 2. Магазин (Загружаем количество И позицию)
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.LoadUnlockedCount(data.unlockedItemsCount < 1 ? 1 : data.unlockedItemsCount);
                ShopManager.Instance.LoadScrollPosition(data.shopScrollPosition);
            }

            // 3. Сытость
            if (SatietyManager.Instance != null)
                SatietyManager.Instance.LoadSatiety(data.currentSatiety > 0 ? data.currentSatiety : 100f);

            // 4. Прогресс и Интро
            ProgressionManager.Instance.LoadLevel(data.levelIndex);
            CutsceneManager.Instance.hasWatchedIntro = data.introWatched;
        }
        catch (Exception e) { Debug.LogError("[SaveManager] Load Error: " + e.Message); }
    }
}