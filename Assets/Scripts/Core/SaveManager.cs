using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public double score;
    public int levelIndex;
    public bool introWatched;
    public int unlockedItemsCount;
    public float shopScrollPosition; 
    public float currentSatiety; 

    public double scorePerClick;
    public double scorePerSecond;
    public double clickMultiplier;
    public double passiveMultiplier;

    public double[] shopItemCosts; // Массив цен товаров в магазине
    public double foodCost;        // Текущая цена корма

    public float doubleScoreTimer; // Сюда мы запишем оставшееся время кулдауна х2
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private float autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 20f;
    public bool isDataLoaded = false;

    private void Awake() => Instance = this;

    private IEnumerator Start()
    {
        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.OnDataLoaded += HandleLoad;
            
            // Ждем готовности Яндекс SDK (сработает быстро, либо по таймауту оффлайна)
            yield return new WaitUntil(() => YandexManager.Instance.isSdkReady);
            
            YandexManager.Instance.LoadData();

            // Вводим таймаут 5 секунд на получение данных из облака Яндекса
            float loadTimeout = 5.0f;
            while (!isDataLoaded && loadTimeout > 0)
            {
                loadTimeout -= Time.deltaTime;
                yield return null;
            }

            if (!isDataLoaded)
            {
                Debug.LogWarning("[SaveManager] Превышено время ожидания сохранений от Яндекса. Запускаем локальную сессию.");
                isDataLoaded = true; // Принудительно снимаем блокировку, чтобы игра не зависла
            }
        }
        else
        {
            isDataLoaded = true;
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
        if (!hasFocus) Save();
    }

    public void Save()
    {
        if (!isDataLoaded || EconomyManager.Instance == null) return;

        GameData data = new GameData();
        data.score = EconomyManager.Instance.score;
        
        // Защита множителей
        data.scorePerClick = Math.Max(1.0, EconomyManager.Instance.scorePerClick);
        data.scorePerSecond = EconomyManager.Instance.scorePerSecond;
        data.clickMultiplier = Math.Max(1.0, EconomyManager.Instance.clickMultiplier);
        data.passiveMultiplier = Math.Max(1.0, EconomyManager.Instance.passiveMultiplier);
        
        data.levelIndex = ProgressionManager.Instance.GetCurrentLevel();
        data.introWatched = CutsceneManager.Instance.hasWatchedIntro;
        
        // Данные из других менеджеров
        if (ShopManager.Instance != null)
        {
            data.unlockedItemsCount = ShopManager.Instance.GetUnlockedCount();
            data.shopScrollPosition = ShopManager.Instance.GetScrollPosition();
            data.shopItemCosts = ShopManager.Instance.GetShopItemCosts();
        }

        if (SatietyUIController.Instance != null)
        {
            data.foodCost = SatietyUIController.Instance.GetFoodCost();
        }
        
        if (SatietyManager.Instance != null)
            data.currentSatiety = SatietyManager.Instance.GetCurrentSatiety();

        // --- СОХРАНЕНИЕ КУЛДАУНА Х2 ---
        if (GameManager.Instance != null)
        {
            data.doubleScoreTimer = GameManager.Instance.GetDoubleScoreTimer();
        }

        string json = JsonUtility.ToJson(data);
        YandexManager.Instance.SaveData(json);
    }

    private void HandleLoad(string json)
    {
        // Предотвращаем повторную загрузку, если ранее сработал таймаут защиты
        if (isDataLoaded) return;

        isDataLoaded = true;
        if (string.IsNullOrEmpty(json) || json == "{}") return;

        try
        {
            GameData data = JsonUtility.FromJson<GameData>(json);

            // 1. Экономика
            if (data.scorePerClick < 1) data.scorePerClick = 1;
            if (data.clickMultiplier < 1) data.clickMultiplier = 1;
            EconomyManager.Instance.LoadFullEconomy(data.score, data.scorePerClick, data.scorePerSecond, data.clickMultiplier, data.passiveMultiplier);

            // 2. Магазин
            if (ShopManager.Instance != null)
            {
                // Загружаем количество открытых товаров и позицию скролла
                ShopManager.Instance.LoadUnlockedCount(data.unlockedItemsCount < 1 ? 1 : data.unlockedItemsCount);
                ShopManager.Instance.LoadScrollPosition(data.shopScrollPosition);

                // Загружаем ЦЕНЫ магазина (проверяем, что массив не пустой)
                if (data.shopItemCosts != null && data.shopItemCosts.Length > 0)
                {
                    ShopManager.Instance.LoadShopItemCosts(data.shopItemCosts);
                }
            }

            // 3. Сытость и Корм
            if (SatietyManager.Instance != null)
            {
                SatietyManager.Instance.LoadSatiety(data.currentSatiety > 0 ? data.currentSatiety : 100f);
            }

            // Загружаем ЦЕНУ корма
            if (SatietyUIController.Instance != null && data.foodCost > 0)
            {
                SatietyUIController.Instance.LoadFoodCost(data.foodCost);
            }

            // 4. Прогресс и Интро
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.LoadLevel(data.levelIndex);
            }

            if (CutsceneManager.Instance != null)
            {
                CutsceneManager.Instance.hasWatchedIntro = data.introWatched;
            }

            // --- ЗАГРУЗКА КУЛДАУНА Х2 ---
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadDoubleScoreTimer(data.doubleScoreTimer);
            }
        }
        catch (Exception e) 
        { 
            Debug.LogError("[SaveManager] Load Error: " + e.Message); 
        }
    }
}