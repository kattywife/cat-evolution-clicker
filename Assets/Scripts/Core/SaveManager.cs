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

        // В методе Save
    public void Save()
    {
        if (!isDataLoaded) return; 
        if (EconomyManager.Instance == null) return;

        GameData data = new GameData();
        data.score = EconomyManager.Instance.score;
        
        // ЗАЩИТА: Если по какой-то причине множители стали 0, принудительно ставим 1
        data.scorePerClick = Math.Max(1.0, EconomyManager.Instance.scorePerClick);
        data.scorePerSecond = EconomyManager.Instance.scorePerSecond;
        data.clickMultiplier = Math.Max(1.0, EconomyManager.Instance.clickMultiplier);
        data.passiveMultiplier = Math.Max(1.0, EconomyManager.Instance.passiveMultiplier);
        
        data.levelIndex = ProgressionManager.Instance.GetCurrentLevel();
        data.unlockedItemsCount = ShopManager.Instance.GetUnlockedCount();
        data.introWatched = CutsceneManager.Instance.hasWatchedIntro;

        string json = JsonUtility.ToJson(data);
        YandexManager.Instance.SaveData(json);
    }

    // В методе HandleLoad
    private void HandleLoad(string json)
    {
        isDataLoaded = true;
        if (string.IsNullOrEmpty(json) || json == "{}") return;

        try
        {
            GameData data = JsonUtility.FromJson<GameData>(json);

            // ЗАЩИТА ПРИ ЗАГРУЗКЕ: Не даем нулям из облака испортить игру
            if (data.scorePerClick < 1) data.scorePerClick = 1;
            if (data.clickMultiplier < 1) data.clickMultiplier = 1;
            if (data.passiveMultiplier < 1) data.passiveMultiplier = 1;

            EconomyManager.Instance.LoadFullEconomy(
                data.score, data.scorePerClick, data.scorePerSecond, 
                data.clickMultiplier, data.passiveMultiplier
            );
            
            ShopManager.Instance.LoadUnlockedCount(data.unlockedItemsCount);
            ProgressionManager.Instance.LoadLevel(data.levelIndex);
            CutsceneManager.Instance.hasWatchedIntro = data.introWatched;
        }
        catch (Exception e) { Debug.LogError(e.Message); }
    }
}