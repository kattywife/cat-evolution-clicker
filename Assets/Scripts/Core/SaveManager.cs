using UnityEngine;
using System;

[Serializable]
public class GameData
{
    public double score;
    public int levelIndex;
    public bool introWatched; // Видел ли игрок интро
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private float autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 30f; // Сохраняем каждые 30 секунд

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // При старте просим загрузить данные
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

    // Сохраняем при уходе со вкладки (сворачивание браузера)
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Save();
        }
    }

    public void Save()
    {
        // Защита от сохранения до загрузки сцены
        if (EconomyManager.Instance == null || ProgressionManager.Instance == null) return;

        GameData data = new GameData();
        
        // Берем данные из новых менеджеров:
        data.score = EconomyManager.Instance.score;
        data.levelIndex = ProgressionManager.Instance.GetCurrentLevel();
        data.introWatched = CutsceneManager.Instance.hasWatchedIntro;

        string json = JsonUtility.ToJson(data);

        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.SaveData(json);
        }
    }

    private void HandleLoad(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "{}") return;

        try
        {
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Раздаем загруженные данные каждому менеджеру:
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.LoadEconomy(data.score);

            if (ProgressionManager.Instance != null)
                ProgressionManager.Instance.LoadLevel(data.levelIndex);

            if (CutsceneManager.Instance != null)
                CutsceneManager.Instance.hasWatchedIntro = data.introWatched;
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing save data: " + e.Message);
        }
    }
}