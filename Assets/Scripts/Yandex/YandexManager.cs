using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class YandexManager : MonoBehaviour
{
    public static YandexManager Instance;

    // Импорт функций из JS
    [DllImport("__Internal")] private static extern void GameReady();
    [DllImport("__Internal")] private static extern void ShowYandexRewardAd();
    [DllImport("__Internal")] private static extern void ShowYandexInterstitialAd();
    [DllImport("__Internal")] private static extern void SaveToYandex(string data);
    [DllImport("__Internal")] private static extern void LoadFromYandex();

    // События, на которые подпишется GameManager
    public Action OnRewardGranted; // Игрок получил награду
    public Action<string> OnDataLoaded; // Данные загрузились

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ (Вызывает GameManager) ---

    public void ReportGameReady()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameReady();
#endif
    }

    public void ShowRewardAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowYandexRewardAd();
#else
        Debug.Log("EDITOR: Show Reward Ad -> Fake Reward");
        OnRewardedAdReward(); // В редакторе сразу даем награду
#endif
    }

    public void ShowInterstitialAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowYandexInterstitialAd();
#else
        Debug.Log("EDITOR: Show Interstitial Ad");
#endif
    }

    public void SaveData(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveToYandex(json);
#else
        Debug.Log("EDITOR: Saving JSON -> " + json);
#endif
    }

    public void LoadData()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        LoadFromYandex();
#else
        Debug.Log("EDITOR: Loading Data... (Fake Empty)");
        OnLoadDataReceived("");
#endif
    }

    // --- CALLBACKS ИЗ JS (Вызываются через SendMessage) ---

    public void OnAdOpen()
    {
        Time.timeScale = 0; // Пауза
        AudioListener.volume = 0; // Выкл звук
    }

    public void OnAdClose()
    {
        Time.timeScale = 1; // Снятие паузы
        AudioListener.volume = 1; // Вкл звук
    }

    public void OnRewardedAdReward()
    {
        // Сообщаем всем подписчикам (GameManager), что пора кормить кота
        OnRewardGranted?.Invoke();
    }

    public void OnLoadDataReceived(string json)
    {
        // Передаем данные тому, кто просил (SaveManager)
        OnDataLoaded?.Invoke(json);
    }
}