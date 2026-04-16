using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    // Основное
    public double score;
    public int levelIndex;
    public bool introWatched;

    // Прогресс магазина
    public int unlockedItemsCount;

    // Статы экономики (чтобы не пересчитывать, сохраним готовые значения)
    public double scorePerClick;
    public double scorePerSecond;
    public double clickMultiplier;
    public double passiveMultiplier;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private float autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 20f; // Сохраняем чуть чаще для надежности

    private bool isDataLoaded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (YandexManager.Instance != null)
        {
            Debug.Log("[SaveManager] Requesting data from Yandex SDK...");
            YandexManager.Instance.OnDataLoaded += HandleLoad;
            YandexManager.Instance.LoadData();
        }
        else
        {
            Debug.LogWarning("[SaveManager] YandexManager not found!");
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
        // Когда игрок переключает вкладку или сворачивает браузер
        if (!hasFocus)
        {
            Save();
        }
    }

    public void Save()
    {
        // Если данные еще не загрузились при старте, не сохраняем (чтобы не затереть облако нулями)
        if (!isDataLoaded) return; 

        if (EconomyManager.Instance == null || ShopManager.Instance == null) return;

        try
        {
            GameData data = new GameData();
            
            // 1. Базовый прогресс
            data.score = EconomyManager.Instance.score;
            data.levelIndex = ProgressionManager.Instance.GetCurrentLevel();
            data.introWatched = CutsceneManager.Instance.hasWatchedIntro;

            // 2. Магазин
            data.unlockedItemsCount = ShopManager.Instance.GetUnlockedCount();

            // 3. Экономика
            data.scorePerClick = EconomyManager.Instance.scorePerClick;
            data.scorePerSecond = EconomyManager.Instance.scorePerSecond;
            data.clickMultiplier = EconomyManager.Instance.clickMultiplier;
            data.passiveMultiplier = EconomyManager.Instance.passiveMultiplier;

            string json = JsonUtility.ToJson(data);
            
            if (YandexManager.Instance != null)
            {
                YandexManager.Instance.SaveData(json);
                Debug.Log("[SaveManager] Game Saved: " + json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveManager] Save Error: " + e.Message);
        }
    }

    private void HandleLoad(string json)
    {
        Debug.Log("[SaveManager] HandleLoad received: " + json);
        
        isDataLoaded = true; // Разрешаем сохранения теперь

        if (string.IsNullOrEmpty(json) || json == "{}")
        {
            Debug.Log("[SaveManager] First time play or empty save.");
            return;
        }

        try
        {
            GameData data = JsonUtility.FromJson<GameData>(json);

            // 1. Восстанавливаем экономику
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.LoadFullEconomy(
                    data.score, 
                    data.scorePerClick, 
                    data.scorePerSecond, 
                    data.clickMultiplier, 
                    data.passiveMultiplier
                );
            }

            // 2. Восстанавливаем магазин
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.LoadUnlockedCount(data.unlockedItemsCount);
            }

            // 3. Восстанавливаем уровень кота
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.LoadLevel(data.levelIndex);
            }

            // 4. Интро
            if (CutsceneManager.Instance != null)
            {
                CutsceneManager.Instance.hasWatchedIntro = data.introWatched;
            }

            Debug.Log("[SaveManager] Data successfully applied!");
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveManager] Load Parsing Error: " + e.Message);
        }
    }
}