using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Ссылки")]
    public GameObject mainGamePanel;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI perSecondText;
    public Camera uiCamera;
    public Transform canvasTransform;

    [Header("Настройки Клика")]
    public Image catImage;
    public GameObject clickTextPrefab;
    private Vector3 initialCatScale;

    [Header("Звуки")]
    public AudioClip catClickSound;

    [Header("Бонус х2")]
    public GameObject doubleScoreButton;
    public float doubleScoreCooldown = 180f;
    private float currentDoubleScoreTimer = 0f;

    [Header("Кнопки (WebGL)")]
    public GameObject exitButton;
    public GameObject restartButton;

    [Header("Эффекты старта")]
    public AudioSource startMeowSource; // Ссылка на AudioSource с мяуканьем
    public GameObject startLevelEffect; // Ссылка на эффект начала уровня (частицы или анимация)
    public AudioClip levelUpSound; 


    private enum AdRewardType { None, SuperFood, DoubleScore }
    private AdRewardType pendingAdReward = AdRewardType.None;

    private void Awake()
    {
        Instance = this;

        if (catImage != null) initialCatScale = catImage.transform.localScale;
        else initialCatScale = Vector3.one;
    }

    void Start()
    {
        // Подписка на рекламу
        if (YandexManager.Instance != null)
            YandexManager.Instance.OnRewardGranted += OnAdRewarded;

        // Настройка кнопок для WebGL
        SetupPlatformButtons();

        // Запуск загрузки
        this.enabled = false; 
        if (CutsceneManager.Instance != null)
        {
            StartCoroutine(CutsceneManager.Instance.PlayLoadingSequence("loading.webm"));
        }
        else
        {
            this.enabled = true;
        }
    }

    void Update()
    {
        if (EconomyManager.Instance == null || SatietyManager.Instance == null) return;

        // 1. Пассивный доход
        double finalSps = EconomyManager.Instance.GetBasePassiveValue() * SatietyManager.Instance.GetCurrentSatietyMultiplier();
        if (finalSps > 0) EconomyManager.Instance.AddScore(finalSps * Time.deltaTime);

        // 2. Тексты очков
        UpdateUITexts(finalSps);

        // 3. Проверка уровня и ОБНОВЛЕНИЕ ПОЛОСКИ (Добавь/проверь эти строки)
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.CheckForLevelUp(EconomyManager.Instance.score);
            ProgressionManager.Instance.UpdateUI(EconomyManager.Instance.score); // <--- ВОТ ЭТА СТРОЧКА
        }

        UpdateDoubleScoreTimer();
        if (Input.GetKeyDown(KeyCode.Space)) HandleClick(Input.mousePosition);
    }

    #region --- КЛИКИ И ВВОД ---

    public void OnCatClicked(BaseEventData baseData)
    {
        PointerEventData eventData = baseData as PointerEventData;
        if (eventData != null) HandleClick(eventData.position);
    }

    private void HandleClick(Vector2 clickPosition)
    {
        if (EconomyManager.Instance == null) return;

        // --- НОВОЕ: Включаем голод при первом клике ---
        if (SatietyManager.Instance != null)
        {
            SatietyManager.Instance.StartHunger();
        }
        // ----------------------------------------------

        AudioManager.Instance.PlaySound(catClickSound);
        
        double clickValue = EconomyManager.Instance.GetFinalClickValue();
        EconomyManager.Instance.AddScore(clickValue);

        // Анимация котика
        if (catImage != null)
        {
            catImage.transform.localScale = initialCatScale * 1.1f;
            CancelInvoke("ResetCatScale");
            Invoke("ResetCatScale", 0.1f);
        }

        // Всплывающий текст
        SpawnClickText(clickPosition, clickValue);

        if (TutorialManager.Instance) TutorialManager.Instance.OnCatClicked();
    }

    private void ResetCatScale() => catImage.transform.localScale = initialCatScale;

    private void SpawnClickText(Vector2 pos, double val)
    {
        if (clickTextPrefab == null || canvasTransform == null) return;
        GameObject textGO = Instantiate(clickTextPrefab, canvasTransform);
        RectTransform canvasRect = canvasTransform.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pos, uiCamera, out Vector2 localPoint);
        textGO.GetComponent<RectTransform>().localPosition = localPoint;
        textGO.GetComponent<TextMeshProUGUI>().text = "+" + EconomyManager.Instance.FormatNumber(val);
    }

    #endregion

    #region --- РЕКЛАМА ---

    public void WatchAdForSuperFood() { pendingAdReward = AdRewardType.SuperFood; YandexManager.Instance?.ShowRewardAd(); }
    public void WatchAdForDoubleScore() { pendingAdReward = AdRewardType.DoubleScore; YandexManager.Instance?.ShowRewardAd(); }

    private void OnAdRewarded()
    {
        if (pendingAdReward == AdRewardType.SuperFood) SatietyManager.Instance.SuperFeed();
        else if (pendingAdReward == AdRewardType.DoubleScore)
        {
            EconomyManager.Instance.AddScore(EconomyManager.Instance.score > 0 ? EconomyManager.Instance.score : 100);
            currentDoubleScoreTimer = doubleScoreCooldown;
        }
        pendingAdReward = AdRewardType.None;
        SaveManager.Instance?.Save();
    }

    #endregion

    #region --- СИСТЕМНЫЕ МЕТОДЫ ---

    private void UpdateUITexts(double currentSps)
    {
        if (totalScoreText) totalScoreText.text = EconomyManager.Instance.FormatNumber(EconomyManager.Instance.score);
        if (perSecondText) perSecondText.text = $"{EconomyManager.Instance.FormatNumber(currentSps)}/сек";
    }

    private void UpdateDoubleScoreTimer()
    {
        if (doubleScoreButton == null) return;

        if (currentDoubleScoreTimer > 0)
        {
            currentDoubleScoreTimer -= Time.deltaTime;
            if (doubleScoreButton.activeSelf) doubleScoreButton.SetActive(false);
        }
        else
        {
            if (!doubleScoreButton.activeSelf) doubleScoreButton.SetActive(true);
        }
    }

    public void OnReachingMaxLevel()
    {
        // Запускаем корутину для задержки
        StartCoroutine(VictorySequenceRoutine());
    }

    private IEnumerator VictorySequenceRoutine()
    {
        Debug.Log("<color=magenta>[GameManager]</color> Максимальный уровень! Даем игроку 3 секунды на радость.");
        
        // 1. Можно здесь запустить какой-то эффект салюта или просто звук победы
        // AudioManager.Instance.PlaySound(victorySound); 

        // 2. Ждем 3 секунды
        yield return new WaitForSeconds(3.0f);

        // 3. Теперь выключаем логику кликов
        this.enabled = false; 

        // 4. Просим CutsceneManager начать подготовку финала
        if (CutsceneManager.Instance != null)
            CutsceneManager.Instance.StartEndingSequence("ending.webm");
    }

    private void SetupPlatformButtons()
    {
        // Проверяем, запущена ли игра как WebGL (для Яндекс.Игр)
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("<color=yellow>[GameManager]</color> Режим WebGL: Скрываю кнопку выхода.");

            if (exitButton != null) 
            {
                exitButton.SetActive(false); // Выключаем кнопку выхода
            }

            // Если ты не используешь Layout Group, мы можем центрировать кнопку Рестарт кодом
            if (restartButton != null)
            {
                RectTransform rt = restartButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Устанавливаем X в 0 (центр родителя), сохраняя текущий Y
                    rt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y);
                }
            }
        }
    }


    public void RestartGame() 
    {
        Debug.Log("<color=yellow>[GameManager]</color> Полный сброс прогресса...");
        
        // Сбрасываем время (важно, если нажали рестарт во время паузы!)
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // 1. СОЗДАЕМ ЧИСТОЕ СОХРАНЕНИЕ
        GameData resetData = new GameData();
        resetData.score = 0;
        resetData.levelIndex = 0;
        resetData.introWatched = true; // Чтобы не смотреть интро опять, но можно поставить false
        resetData.unlockedItemsCount = 1;
        resetData.shopScrollPosition = 1f;
        resetData.currentSatiety = 100f;

        resetData.scorePerClick = 1;
        resetData.scorePerSecond = 0;
        resetData.clickMultiplier = 1;
        resetData.passiveMultiplier = 1;

        // ЭТО ОБНУЛИТ ЦЕНЫ:
        resetData.shopItemCosts = null; // Магазин увидит null и возьмет базовые цены из UpgradeData
        resetData.foodCost = 10;        // Твоя самая первая цена корма

        // 2. ОТПРАВЛЯЕМ В ЯНДЕКС
        if (YandexManager.Instance != null)
        {
            string json = JsonUtility.ToJson(resetData);
            YandexManager.Instance.SaveData(json);
        }

        // 3. ПЕРЕЗАГРУЖАЕМ СЦЕНУ
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame() => Application.Quit();

    private void OnDestroy()
    {
        if (YandexManager.Instance != null) YandexManager.Instance.OnRewardGranted -= OnAdRewarded;
    }


    public void PlayStartEffects()
    {

        if (ProgressionManager.Instance.currentLevelIndex == 0)
        {
            // 1. Включаем звук мяуканья
            if (startMeowSource != null)
            {
                startMeowSource.Play();
            }

            // 2. Включаем визуальный эффект
            if (startLevelEffect != null)
            {
                startLevelEffect.SetActive(true);
                
                // Если это частицы (Particle System), на всякий случай принудительно запустим
                ParticleSystem ps = startLevelEffect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }

            if (levelUpSound != null)
            {
                AudioManager.Instance.PlaySound(levelUpSound);
            }
        }
        
    }

    [HideInInspector] public bool isGamePaused = false;

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Debug.Log("<color=yellow>[GameManager]</color> Игра на паузе");
            Time.timeScale = 0f;           // Останавливаем время (голод, пассивный доход)
            AudioListener.pause = true;    // Останавливаем все звуки
        }
        else
        {
            Debug.Log("<color=green>[GameManager]</color> Игра снята с паузы");
            Time.timeScale = 1f;           // Возвращаем время
            AudioListener.pause = false;   // Возвращаем звуки
        }
    }

    #endregion

    // --- НОВЫЕ МЕТОДЫ ДЛЯ СОХРАНЕНИЯ КУЛДАУНА ---
    public float GetDoubleScoreTimer()
    {
        return currentDoubleScoreTimer;
    }

    public void LoadDoubleScoreTimer(float savedTimerValue)
    {
        // Загружаем сохраненный таймер. Если он больше нуля — кнопка х2 автоматически скроется
        currentDoubleScoreTimer = savedTimerValue;
    }
}