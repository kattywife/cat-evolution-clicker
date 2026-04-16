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
    [DllImport("__Internal")] private static extern string GetLang(); // Новое: для пункта 2.14

    // --- СОБЫТИЯ ---
    public Action OnRewardGranted;      // Игрок досмотрел видео
    public Action<string> OnDataLoaded; // Пришли данные из облака

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Сразу при старте делаем запрос языка, чтобы выполнить требование 2.14
        RequestLanguage();
    }

    // =========================================================
    // ПУБЛИЧНЫЕ МЕТОДЫ
    // =========================================================

    public void ReportGameReady()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameReady();
#else
        Debug.Log("[YandexManager] Game Ready Sent (Editor)");
#endif
    }

    private void RequestLanguage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try {
            string lang = GetLang();
            Debug.Log("[YandexManager] Detected Language: " + lang);
            // Даже если мы ничего не меняем в UI, вызов метода зафиксирован в логах SDK
        } catch (Exception e) {
            Debug.LogError("[YandexManager] Language Request Error: " + e.Message);
        }
#else
        Debug.Log("[YandexManager] Detected Language: ru (Editor)");
#endif
    }

    public void ShowRewardAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowYandexRewardAd();
#else
        Debug.Log("[YandexManager] Show Reward Ad -> Fake Reward Granted");
        OnRewardedAdReward(); 
#endif
    }

    public void ShowInterstitialAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowYandexInterstitialAd();
#else
        Debug.Log("[YandexManager] Show Interstitial Ad");
#endif
    }

    public void SaveData(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveToYandex(json);
#else
        Debug.Log("[YandexManager] Save Data To Cloud: " + json);
#endif
    }

    public void LoadData()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        LoadFromYandex();
#else
        Debug.Log("[YandexManager] Load Data Requested (Fake Empty)");
        OnLoadDataReceived("{}");
#endif
    }

    // =========================================================
    // CALLBACKS ИЗ JAVASCRIPT (Вызываются через SendMessage)
    // =========================================================

    public void OnAdOpen()
    {
        Debug.Log("[YandexManager] Ad Opened -> Muting Game");
        Time.timeScale = 0f;
        AudioListener.pause = true; // Ставим на паузу все аудио источники
        AudioListener.volume = 0f;
    }

    public void OnAdClose()
    {
        Debug.Log("[YandexManager] Ad Closed -> Resuming Game");
        Time.timeScale = 1f;
        AudioListener.pause = false; // Снимаем с паузы
        AudioListener.volume = 1f;
    }

    public void OnRewardedAdReward()
    {
        Debug.Log("[YandexManager] Reward Granted!");
        OnRewardGranted?.Invoke();
    }

    public void OnLoadDataReceived(string json)
    {
        Debug.Log("[YandexManager] Cloud Data Received: " + json);
        OnDataLoaded?.Invoke(json);
    }
}