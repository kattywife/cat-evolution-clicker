using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class LocalizationItem
{
    public string key;
    public string ru;
    public string en;
    public string tr;
}

[Serializable]
public class LocalizationData
{
    public List<LocalizationItem> items;
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private Dictionary<string, LocalizationItem> localizedDictionary;
    private string activeLanguage = "ru"; // "ru", "en", "tr"

    [Header("Язык для тестов в редакторе Unity")]
    public string testLanguageInEditor = "tr";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Отвязываем объект от родителя, чтобы DontDestroyOnLoad работал без ошибок
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject);
            
            // Загружаем словарь мгновенно при старте игры
            LoadLocalization();
            
            // Сразу же определяем язык
            DetermineActiveLanguage(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadLocalization()
    {
        localizedDictionary = new Dictionary<string, LocalizationItem>();

        TextAsset jsonFile = Resources.Load<TextAsset>("localization");
        if (jsonFile == null)
        {
            Debug.LogError("[LocalizationManager] Не найден файл Resources/localization.json!");
            return;
        }

        try
        {
            LocalizationData data = JsonUtility.FromJson<LocalizationData>(jsonFile.text);
            foreach (var item in data.items)
            {
                if (!localizedDictionary.ContainsKey(item.key))
                {
                    localizedDictionary.Add(item.key, item);
                }
            }
            Debug.Log($"[LocalizationManager] Успешно загружено {localizedDictionary.Count} фраз.");
        }
        catch (Exception e)
        {
            Debug.LogError("[LocalizationManager] Ошибка чтения JSON: " + e.Message);
        }
    }

    private void DetermineActiveLanguage()
    {
#if UNITY_EDITOR
        activeLanguage = testLanguageInEditor.ToLower();
        Debug.Log("[LocalizationManager] EDITOR MODE: Используем тестовый язык: " + activeLanguage);
#else
        if (YandexManager.Instance != null)
        {
            activeLanguage = YandexManager.Instance.currentLanguage.ToLower();
        }
        else
        {
            if (Application.systemLanguage == SystemLanguage.Russian) activeLanguage = "ru";
            else if (Application.systemLanguage == SystemLanguage.Turkish) activeLanguage = "tr";
            else activeLanguage = "en";
        }
#endif

        if (activeLanguage != "ru" && activeLanguage != "tr" && activeLanguage != "en")
        {
            activeLanguage = "en";
        }

        Debug.Log("[LocalizationManager] Активный язык локализации: " + activeLanguage);
    }

    public void SetLanguage(string lang)
    {
        activeLanguage = lang.ToLower();
        if (activeLanguage != "ru" && activeLanguage != "tr" && activeLanguage != "en")
        {
            activeLanguage = "en";
        }

        Debug.Log("[LocalizationManager] Язык принудительно переключен на: " + activeLanguage);

        LocalizeText[] allTexts = FindObjectsByType<LocalizeText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var txt in allTexts)
        {
            txt.Localize(); 
        }
    }

    public string GetTranslation(string key)
    {
        if (localizedDictionary == null || !localizedDictionary.ContainsKey(key))
        {
            Debug.LogWarning("[LocalizationManager] Ключ не найден: " + key);
            return key;
        }

        LocalizationItem item = localizedDictionary[key];

        switch (activeLanguage)
        {
            case "ru": return item.ru;
            case "en": return item.en;
            case "tr": return item.tr;
            default: return item.en;
        }
    }
}