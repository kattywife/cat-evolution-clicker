using UnityEngine;
using System;

[Serializable]
public class GameData
{
    public double score;
    public int levelIndex;
    public bool introWatched; // <--- НОВОЕ ПОЛЕ: Видел ли игрок интро
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public GameManager gameManager;

    private float autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 30f; // Сохраняем каждые 30 секунд

    private void Awake()
    {
        if (Instance == null) Instance = this;
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
        if (gameManager == null) return;

        GameData data = new GameData();
        data.score = gameManager.score;
        data.levelIndex = gameManager.GetCurrentLevel();

        // Берём информацию о просмотре интро из GameManager
        data.introWatched = gameManager.hasWatchedIntro;

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
            if (gameManager != null)
            {
                // Передаем также статус интро
                gameManager.LoadGameState(data.score, data.levelIndex, data.introWatched);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing save data: " + e.Message);
        }
    }
}