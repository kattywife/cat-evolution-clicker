using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class YandexManager : MonoBehaviour
{
    public static YandexManager Instance;

    // --- ИМПОРТ ФУНКЦИЙ ИЗ JSLIB ---
    [DllImport("__Internal")] private static extern void GameReady();
    [DllImport("__Internal")] private static extern void ShowYandexRewardAd();
    [DllImport("__Internal")] private static extern void ShowYandexInterstitialAd();
    [DllImport("__Internal")] private static extern void SaveToYandex(string data);
    [DllImport("__Internal")] private static extern void LoadFromYandex();

    // --- СОБЫТИЯ (Для подписки из других скриптов) ---
    public Action OnRewardGranted; // Игрок посмотрел рекламу до конца
    public Action<string> OnDataLoaded; // Пришли данные сохранения

    private void Awake()
    {
        // Паттерн Синглтон (чтобы YandexManager был один на всю игру)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожать при смене сцен
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // ПУБЛИЧНЫЕ МЕТОДЫ (Вызываются из GameManager / SaveManager)
    // =========================================================

    public void ReportGameReady()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameReady();
#else
        Debug.Log("YandexManager: Game Ready Sent (Editor)");
#endif
    }

    public void ShowRewardAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowYandexRewardAd();
#else
        Debug.Log("YandexManager: Show Reward Ad -> Fake Reward Granted");
        OnRewardedAdReward(); // В редакторе сразу даем награду для теста
#endif
    }

    public void ShowInterstitialAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowYandexInterstitialAd();
#else
        Debug.Log("YandexManager: Show Interstitial Ad");
#endif
    }

    public void SaveData(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveToYandex(json);
#else
        Debug.Log("YandexManager: Save Data -> " + json);
#endif
    }

    public void LoadData()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        LoadFromYandex();
#else
        Debug.Log("YandexManager: Load Data (Fake Empty)");
        OnLoadDataReceived("{}"); // В редакторе возвращаем пустой JSON
#endif
    }


    // =========================================================
    // CALLBACKS ИЗ JAVASCRIPT (Вызываются через SendMessage)
    // =========================================================

    // 1. Реклама открылась
    public void OnAdOpen()
    {
        Debug.Log("YandexManager: Ad Opened -> Pausing Game");
        Time.timeScale = 0f;       // Останавливаем время
        AudioListener.volume = 0f; // Выключаем весь звук
    }

    // 2. Реклама закрылась (любая)
    public void OnAdClose()
    {
        Debug.Log("YandexManager: Ad Closed -> Resuming Game");
        Time.timeScale = 1f;       // Возвращаем время
        AudioListener.volume = 1f; // Возвращаем звук
    }

    // 3. Награда получена (Только для Rewarded Video)
    public void OnRewardedAdReward()
    {
        Debug.Log("YandexManager: Reward Granted!");
        // Сообщаем всем подписчикам (GameManager), что пора давать бонус
        OnRewardGranted?.Invoke();
    }

    // 4. Пришли данные сохранения
    public void OnLoadDataReceived(string json)
    {
        Debug.Log("YandexManager: Data Received -> " + json);
        OnDataLoaded?.Invoke(json);
    }
}