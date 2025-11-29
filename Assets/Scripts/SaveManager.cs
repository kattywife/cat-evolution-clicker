using UnityEngine;
using System;

[Serializable]
public class GameData
{
    public double score;
    public int levelIndex;
    // Сюда потом можно добавить купленные товары
}

public class SaveManager : MonoBehaviour
{
    public GameManager gameManager; // Ссылка на Гейм Менеджер

    private void Start()
    {
        // Подписываемся на событие загрузки от YandexManager
        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.OnDataLoaded += HandleLoad;

            // Просим загрузить данные при старте
            YandexManager.Instance.LoadData();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void Save()
    {
        // 1. Собираем данные
        GameData data = new GameData();
        data.score = gameManager.score;
        data.levelIndex = gameManager.GetCurrentLevel();

        // 2. Превращаем в текст (JSON)
        string json = JsonUtility.ToJson(data);

        // 3. Отправляем в Яндекс
        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.SaveData(json);
        }
    }

    // Этот метод сработает, когда Яндекс вернет данные
    private void HandleLoad(string json)
    {
        if (string.IsNullOrEmpty(json)) return; // Если сохранений нет, ничего не делаем

        GameData data = JsonUtility.FromJson<GameData>(json);

        // Применяем данные к игре
        if (gameManager != null)
        {
            gameManager.LoadGameState(data.score, data.levelIndex);
        }
    }
}