using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections; 

public class YandexManager : MonoBehaviour
{
    public static YandexManager Instance;
    public bool isAdPlaying = false;
    public bool isSdkReady = false; 
    public string currentLanguage = "rus"; // Язык по умолчанию

    [DllImport("__Internal")] private static extern void GameReady();
    [DllImport("__Internal")] private static extern void ShowYandexRewardAd();
    [DllImport("__Internal")] private static extern void ShowYandexInterstitialAd();
    [DllImport("__Internal")] private static extern void SaveToYandex(string data);
    [DllImport("__Internal")] private static extern void LoadFromYandex();
    [DllImport("__Internal")] private static extern string GetLang();
    [DllImport("__Internal")] private static extern bool CheckYandexSDKReady();

    public Action OnRewardGranted;      
    public Action<string> OnDataLoaded; 

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

    private IEnumerator Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[YandexManager] Ожидание инициализации Yandex SDK...");
        
        float timeout = 7.0f; // Таймаут 7 секунд для защиты от AdBlock
        while (!CheckYandexSDKReady() && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (timeout <= 0)
        {
            Debug.LogWarning("[YandexManager] Превышено время ожидания SDK. Возможно, включен AdBlock. Запуск в оффлайн-режиме.");
        }
#else
        yield return null; 
#endif

        isSdkReady = true; // В любом случае разблокируем игру
        Debug.Log("<color=green>[YandexManager] Статус готовности SDK установлен.</color>");

        RequestLanguage();
    }

    public void ReportGameReady()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (isSdkReady) GameReady();
#else
        Debug.Log("[YandexManager] Game Ready Sent (Editor)");
#endif
    }

    private void RequestLanguage()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        try {
            if (isSdkReady) {
                currentLanguage = GetLang(); // Записываем язык Яндекса (ru, en, tr и т.д.)
                Debug.Log("[YandexManager] Detected Language: " + currentLanguage);
            }
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
        if (isSdkReady) ShowYandexRewardAd();
#else
        Debug.Log("[YandexManager] Show Reward Ad -> Fake Reward Granted");
        OnRewardedAdReward(); 
#endif
    }

    public void ShowInterstitialAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (isSdkReady) ShowYandexInterstitialAd();
#else
        Debug.Log("[YandexManager] Show Interstitial Ad");
#endif
    }

    public void SaveData(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (isSdkReady) SaveToYandex(json);
#else
        Debug.Log("[YandexManager] Save Data To Cloud: " + json);
#endif
    }

    public void LoadData()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (isSdkReady) LoadFromYandex();
        else OnLoadDataReceived("{}"); // Если SDK не готов, сразу отдаем пустой объект
#else
        Debug.Log("[YandexManager] Load Data Requested (Fake Empty)");
        OnLoadDataReceived("{}");
#endif
    }

    public void OnAdOpen()
    {
        isAdPlaying = true;
        Time.timeScale = 0f;
        AudioListener.pause = true; 
        AudioListener.volume = 0f;
    }

    public void OnAdClose()
    {
        isAdPlaying = false;
        Time.timeScale = 1f;
        AudioListener.pause = false; 
        AudioListener.volume = 1f;
    }

    public void OnRewardedAdReward()
    {
        OnRewardGranted?.Invoke();
    }

    public void OnLoadDataReceived(string json)
    {
        Debug.Log("[YandexManager] Cloud Data Received: " + json);
        OnDataLoaded?.Invoke(json);
    }
}