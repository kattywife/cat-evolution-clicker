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
            StartCoroutine(CutsceneManager.Instance.PlayLoadingSequence("loading.mp4"));
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
            CutsceneManager.Instance.StartEndingSequence("ending.mp4");
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
        Debug.Log("<color=yellow>[GameManager]</color> Перезапуск...");
        
        // Сбрасываем время на случай пауз
        Time.timeScale = 1f;

        // Вместо просто LoadScene, лучше использовать индекс текущей сцены
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void ExitGame() => Application.Quit();

    private void OnDestroy()
    {
        if (YandexManager.Instance != null) YandexManager.Instance.OnRewardGranted -= OnAdRewarded;
    }

    #endregion
}