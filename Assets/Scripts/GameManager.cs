using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;


public class GameManager : MonoBehaviour
{
    // =========================================================
    // НАСТРОЙКИ И ДАННЫЕ
    // =========================================================

    [Header("Данные игры")]
    public List<LevelData> levels;
    public List<UpgradeData> upgrades;

    [Header("Звуки")]
    public AudioClip catClickSound;
    public AudioClip levelUpSound;

    // --- ГЕЙМПЛЕЙ ---
    [Header("Состояние")]
    public double score = 0;
    private double scorePerClick = 1;
    private double scorePerSecond = 0;

    [Header("Сытость")]
    public float maxSatiety = 100f;
    public float currentSatiety;
    public float satietyDepletionRate = 0.5f;
    public float satietyPenaltyMultiplier = 0.1f;

    // --- UI ЭЛЕМЕНТЫ ---
    [Header("UI Ссылки")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI perSecondText;
    public Image catImage;
    public Slider levelProgressBar;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI progressText;
    public Camera uiCamera;

    [Header("Кнопки (WebGL)")]
    public GameObject exitButton;
    public GameObject restartButton;

    // --- УДВОЕНИЕ ОЧКОВ ---
    [Header("Бонус за рекламу")]
    public Button doubleScoreButton;
    public TextMeshProUGUI doubleScoreButtonText;
    public float doubleScoreCooldown = 180f; // 3 минуты
    private float currentDoubleScoreTimer = 0f;

    // --- ЭКРАНЫ ---
    [Header("Экран Загрузки")]
    public bool enableLoadingScreen = true;
    public GameObject loadingPanel;
    [Tooltip("Картинка типа Filled для прогресс-бара")]
    public Image loadingFillImage;
    public VideoPlayer loadingCatVideoPlayer;
    public float loadingDuration = 3.0f;

    [Header("Переход через Белое")]
    public GameObject whiteFadePanel;
    public float whiteFadeInDuration = 1.0f;
    public float whiteFadeOutDuration = 1.0f;

    [Header("Интро")]
    public bool enableIntro = true;
    public GameObject introPanel;
    public VideoPlayer introVideoPlayer;
    public float introFadeDuration = 1.5f;

    [Header("Концовка")]
    public float endingDelay = 3.0f;
    public float endingFadeDuration = 2.0f;
    public GameObject mainGamePanel;
    public GameObject endingPanel;
    public VideoPlayer endingVideoPlayer;
    public GameObject postVideoUI;
    public AudioClip endingMusic;

    // --- ЭФФЕКТЫ ---
    [Header("Эффекты")]
    public ParticleSystem levelUpEffect;
    public GameObject tearEffectObject;
    public GameObject clickTextPrefab;
    public Transform canvasTransform;

    // --- МАГАЗИН ---
    [Header("Магазин")]
    public GameObject upgradeButtonPrefab;
    public Transform shopContentParent;
    public ScrollRect shopScrollRect;
    public float animationScrollSpeed = 3f;
    public float animationBounceAmount = 50f;
    public int initialItemsToIgnore = 4;

    // --- ПРИВАТНЫЕ ПЕРЕМЕННЫЕ ---
    private int currentLevelIndex = 0;
    private RectTransform shopContentRectTransform;
    private List<UpgradeButtonUI> shopButtons = new List<UpgradeButtonUI>();
    private bool isShopAnimating = false;
    private int unlockedItemsCount = 1;
    private double clickMultiplier = 1.0;
    private double passiveMultiplier = 1.0;

    // Типы наград за рекламу
    private enum AdRewardType { None, SuperFood, DoubleScore }
    private AdRewardType pendingAdReward = AdRewardType.None;


    // =========================================================
    // СТАРТ И ИНИЦИАЛИЗАЦИЯ
    // =========================================================

    void Start()
    {
        // Подписка на награду за рекламу
        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.OnRewardGranted += OnAdRewarded;
        }

        // Инициализация
        currentLevelIndex = 0;
        scorePerClick = 1;
        scorePerSecond = 0;
        score = 0;
        currentSatiety = maxSatiety;

        if (shopContentParent != null)
            shopContentRectTransform = shopContentParent.GetComponent<RectTransform>();

        // Скрываем панели
        if (endingPanel) endingPanel.SetActive(false);
        if (postVideoUI) postVideoUI.SetActive(false);
        if (introPanel) introPanel.SetActive(false);
        if (loadingPanel) loadingPanel.SetActive(false);
        if (whiteFadePanel) whiteFadePanel.SetActive(false);

        CreateShop();
        UpdateAllShopButtonsState();
        ApplyLevelUp(false); // Без звука при старте

        // Адаптация под WebGL (Яндекс)
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (exitButton) exitButton.SetActive(false);
            if (restartButton)
            {
                RectTransform rt = restartButton.GetComponent<RectTransform>();
                if (rt) rt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y);
            }
        }

        // Запуск последовательности экранов
        if (enableLoadingScreen && loadingPanel)
        {
            StartCoroutine(PlayLoadingSequence());
        }
        else if (enableIntro && introPanel)
        {
            StartCoroutine(StartIntroSequence(false));
        }
        else
        {
            StartGameImmediately();
        }
    }

    void OnDestroy()
    {
        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.OnRewardGranted -= OnAdRewarded;
        }
    }


    // =========================================================
    // ОСНОВНОЙ ИГРОВОЙ ЦИКЛ (UPDATE)
    // =========================================================

    void Update()
    {
        if (!this.enabled) return;

        // --- 1. ЛОГИКА ИСЧЕЗНОВЕНИЯ КНОПКИ Х2 ---
        if (currentDoubleScoreTimer > 0)
        {
            currentDoubleScoreTimer -= Time.deltaTime;

            // Если таймер идет - ПРЯЧЕМ кнопку (если она еще видна)
            if (doubleScoreButton != null && doubleScoreButton.gameObject.activeSelf)
            {
                doubleScoreButton.gameObject.SetActive(false);
            }
        }
        else
        {
            // Таймер кончился - ПОКАЗЫВАЕМ кнопку (если она скрыта)
            if (doubleScoreButton != null && !doubleScoreButton.gameObject.activeSelf)
            {
                doubleScoreButton.gameObject.SetActive(true);
            }
        }
        // ----------------------------------------

        // 2. Клик пробелом
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (endingPanel == null || !endingPanel.activeSelf)
            {
                if (catImage != null)
                {
                    Vector3 clickPos = catImage.transform.position;
                    clickPos.x += Random.Range(-50f, 50f);
                    clickPos.y += Random.Range(-50f, 50f);
                    HandleClick(clickPos);
                }
            }
        }

        // 3. Экономика
        double finalScorePerSecond = scorePerSecond * passiveMultiplier;

        if (currentSatiety > 0)
            currentSatiety -= satietyDepletionRate * Time.deltaTime;
        else
            currentSatiety = 0;

        double effectiveSps = finalScorePerSecond;

        // Голод и слезы
        if (currentSatiety <= 0)
        {
            effectiveSps *= satietyPenaltyMultiplier;
            if (tearEffectObject != null && !tearEffectObject.activeSelf) tearEffectObject.SetActive(true);
        }
        else
        {
            if (tearEffectObject != null && tearEffectObject.activeSelf) tearEffectObject.SetActive(false);
        }

        if (effectiveSps > 0)
            score += effectiveSps * Time.deltaTime;

        // Магазин
        for (int i = 0; i < unlockedItemsCount; i++)
        {
            if (i < shopButtons.Count && shopButtons[i] != null)
                shopButtons[i].UpdateInteractableState(score);
        }

        UpdateAllUITexts();
        UpdateProgressBar();
    }


    // =========================================================
    // ПОКУПКИ (С ИСПРАВЛЕННОЙ МАТЕМАТИКОЙ)
    // =========================================================

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (score >= cost)
        {
            score -= cost;

            switch (upgrade.type)
            {
                // ИСПРАВЛЕНИЕ: Делим добавку на текущий множитель, чтобы
                // итоговый прирост (после умножения) был ровно таким, как написано на кнопке.
                case UpgradeType.PerClick:
                    if (clickMultiplier > 0)
                        scorePerClick += upgrade.power / clickMultiplier;
                    else
                        scorePerClick += upgrade.power;
                    break;

                case UpgradeType.PerSecond:
                    if (passiveMultiplier > 0)
                        scorePerSecond += upgrade.power / passiveMultiplier;
                    else
                        scorePerSecond += upgrade.power;
                    break;

                // Множители работают через умножение (*=)
                case UpgradeType.ClickMultiplier:
                    clickMultiplier *= upgrade.power;
                    break;

                case UpgradeType.PassiveMultiplier:
                    passiveMultiplier *= upgrade.power;
                    break;

                case UpgradeType.GlobalMultiplier:
                    clickMultiplier *= upgrade.power;
                    passiveMultiplier *= upgrade.power;
                    break;
            }

            int purchasedIndex = shopButtons.IndexOf(button);
            if (purchasedIndex == unlockedItemsCount - 1 && unlockedItemsCount < shopButtons.Count)
            {
                unlockedItemsCount++;
                if (unlockedItemsCount - 1 >= initialItemsToIgnore && !isShopAnimating)
                    StartCoroutine(AnimateScrollToShowItem(shopButtons[unlockedItemsCount - 1].GetComponent<RectTransform>()));
            }
            button.OnPurchaseSuccess();
            UpdateAllShopButtonsState();
        }
    }


    // =========================================================
    // РЕКЛАМА И НАГРАДЫ
    // =========================================================

    public void WatchAdForSuperFood()
    {
        pendingAdReward = AdRewardType.SuperFood;
        if (YandexManager.Instance != null) YandexManager.Instance.ShowRewardAd();
    }

    public void WatchAdForDoubleScore()
    {
        pendingAdReward = AdRewardType.DoubleScore;
        if (YandexManager.Instance != null) YandexManager.Instance.ShowRewardAd();
    }

    private void OnAdRewarded()
    {
        if (pendingAdReward == AdRewardType.SuperFood)
        {
            SuperFeedCat();
        }
        else if (pendingAdReward == AdRewardType.DoubleScore)
        {
            if (score > 0) score *= 2;
            else score += 100;

            // Запускаем таймер (кнопка исчезнет в Update)
            currentDoubleScoreTimer = doubleScoreCooldown;
        }

        pendingAdReward = AdRewardType.None;
        UpdateAllUITexts();
    }

    public void SuperFeedCat()
    {
        currentSatiety = maxSatiety * 2.0f;
    }


    // =========================================================
    // ЗАПУСК И ЭКРАНЫ
    // =========================================================

    private IEnumerator PlayLoadingSequence()
    {
        this.enabled = false;
        loadingPanel.SetActive(true);
        SetupCanvasGroup(loadingPanel);

        if (mainGamePanel) mainGamePanel.SetActive(false);
        if (loadingCatVideoPlayer) loadingCatVideoPlayer.Play();

        float timer = 0f;
        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;
            if (loadingFillImage)
                loadingFillImage.fillAmount = Mathf.Clamp01(timer / loadingDuration);
            yield return null;
        }

        if (YandexManager.Instance != null) YandexManager.Instance.ReportGameReady();

        yield return new WaitForSeconds(0.2f);

        if (whiteFadePanel)
        {
            whiteFadePanel.SetActive(true);
            CanvasGroup w = SetupCanvasGroup(whiteFadePanel);
            w.alpha = 0;
            float t = 0;
            while (t < whiteFadeInDuration)
            {
                t += Time.deltaTime;
                w.alpha = Mathf.Lerp(0, 1, t / whiteFadeInDuration);
                yield return null;
            }
            w.alpha = 1;
        }

        if (loadingCatVideoPlayer) loadingCatVideoPlayer.Stop();
        loadingPanel.SetActive(false);

        if (enableIntro && introPanel)
            StartCoroutine(StartIntroSequence(true));
        else
        {
            StartGameImmediately();
            if (whiteFadePanel) StartCoroutine(FadeOutWhite());
        }
    }

    private IEnumerator StartIntroSequence(bool startedFromWhite)
    {
        this.enabled = false;
        introPanel.SetActive(true);
        SetupCanvasGroup(introPanel);

        if (mainGamePanel) mainGamePanel.SetActive(false);

        if (introVideoPlayer)
        {
            introVideoPlayer.isLooping = false;
            introVideoPlayer.Prepare();
            while (!introVideoPlayer.isPrepared) yield return null;
            introVideoPlayer.Play();
        }

        if (startedFromWhite && whiteFadePanel) StartCoroutine(FadeOutWhite());

        if (introVideoPlayer)
        {
            while (introVideoPlayer.isPlaying) yield return null;
        }

        if (mainGamePanel) mainGamePanel.SetActive(true);

        AudioSource videoAudio = (introVideoPlayer != null) ? introVideoPlayer.GetComponent<AudioSource>() : null;
        float startVolume = (videoAudio != null) ? videoAudio.volume : 1f;

        CanvasGroup introCg = introPanel.GetComponent<CanvasGroup>();
        float t = 0f;
        bool playedEffect = false;

        while (t < introFadeDuration)
        {
            t += Time.deltaTime;
            float progress = t / introFadeDuration;

            introCg.alpha = Mathf.Lerp(1, 0, progress);

            if (videoAudio != null)
                videoAudio.volume = Mathf.Lerp(startVolume, 0f, progress);

            if (!playedEffect && t > introFadeDuration * 0.2f)
            {
                if (levelUpEffect) levelUpEffect.Play();
                playedEffect = true;
            }
            yield return null;
        }

        introPanel.SetActive(false);
        if (!playedEffect && levelUpEffect) levelUpEffect.Play();

        this.enabled = true;
    }

    private IEnumerator FadeOutWhite()
    {
        if (whiteFadePanel)
        {
            CanvasGroup c = whiteFadePanel.GetComponent<CanvasGroup>();
            float t = 0;
            while (t < whiteFadeOutDuration)
            {
                t += Time.deltaTime;
                c.alpha = Mathf.Lerp(1, 0, t / whiteFadeOutDuration);
                yield return null;
            }
            whiteFadePanel.SetActive(false);
        }
    }

    private void StartGameImmediately()
    {
        if (mainGamePanel) mainGamePanel.SetActive(true);
        if (levelUpEffect) levelUpEffect.Play();
        this.enabled = true;
    }


    // =========================================================
    // СИСТЕМА СОХРАНЕНИЙ
    // =========================================================

    public int GetCurrentLevel() { return currentLevelIndex; }

    public void LoadGameState(double loadedScore, int loadedLevel)
    {
        score = loadedScore;
        currentLevelIndex = loadedLevel;
        UpdateAllUITexts();
        UpdateProgressBar();
        ApplyLevelUp(false);
    }


    // =========================================================
    // МЕХАНИКИ ИГРЫ
    // =========================================================

    public void OnCatClicked(BaseEventData baseData)
    {
        if (!this.enabled) return;
        PointerEventData eventData = baseData as PointerEventData;
        if (eventData == null) return;
        HandleClick(eventData.position);
    }

    private void HandleClick(Vector2 clickPosition)
    {
        AudioManager.Instance.PlaySound(catClickSound);
        double finalScorePerClick = scorePerClick * clickMultiplier;
        score += finalScorePerClick;

        CheckForLevelUp();

        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        CancelInvoke("ResetCatScale");
        Invoke("ResetCatScale", 0.1f);

        if (clickTextPrefab != null && canvasTransform != null)
        {
            GameObject textGO = Instantiate(clickTextPrefab, canvasTransform);
            RectTransform canvasRect = canvasTransform.GetComponent<RectTransform>();
            Vector2 localPoint;
            Camera cam = (uiCamera != null) ? uiCamera : Camera.main;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, clickPosition, cam, out localPoint);
            textGO.GetComponent<RectTransform>().localPosition = localPoint;
            textGO.GetComponent<TextMeshProUGUI>().text = "+" + FormatNumber(finalScorePerClick);
        }
    }

    private void CheckForLevelUp()
    {
        if (currentLevelIndex + 1 < levels.Count)
        {
            if (score >= levels[currentLevelIndex + 1].scoreToReach)
            {
                currentLevelIndex++;
                ApplyLevelUp(true);
            }
        }
    }

    private void ApplyLevelUp(bool playEffects = true)
    {
        if (playEffects && levelUpSound)
            AudioManager.Instance.PlaySound(levelUpSound, 0.8f);

        if (levels.Count > 0 && currentLevelIndex < levels.Count)
        {
            catImage.sprite = levels[currentLevelIndex].catSprite;
            catImage.SetNativeSize();
            if (tearEffectObject) tearEffectObject.transform.localPosition = levels[currentLevelIndex].tearPosition;
        }

        if (playEffects && levelUpEffect)
            levelUpEffect.Play();

        // Реклама при повышении уровня
        if (playEffects && YandexManager.Instance != null)
        {
            YandexManager.Instance.ShowInterstitialAd();
        }

        // Финал
        if (currentLevelIndex == levels.Count - 1)
        {
            satietyDepletionRate = 0f;
            currentSatiety = maxSatiety;
            if (tearEffectObject) tearEffectObject.SetActive(false);
            if (playEffects) StartCoroutine(WaitAndStartEnding());
        }
    }

    public void FeedCat(double cost, float amount)
    {
        if (score >= cost)
        {
            score -= cost;
            currentSatiety = Mathf.Min(maxSatiety, currentSatiety + amount);
        }
    }

    // --- МАГАЗИН И UI ---

    private void CreateShop()
    {
        foreach (var upgrade in upgrades)
        {
            GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopContentParent);
            UpgradeButtonUI buttonUI = newButtonGO.GetComponent<UpgradeButtonUI>();
            buttonUI.Setup(upgrade, this);
            shopButtons.Add(buttonUI);
        }
    }

    private void UpdateAllShopButtonsState()
    {
        for (int i = 0; i < shopButtons.Count; i++)
        {
            if (shopButtons[i] == null) continue;
            bool isUnlocked = (i < unlockedItemsCount);
            shopButtons[i].SetLockedState(!isUnlocked);
            if (isUnlocked) shopButtons[i].UpdateInteractableState(score);
        }
    }

    private void UpdateAllUITexts()
    {
        if (totalScoreText) totalScoreText.text = FormatNumber(score);
        if (perSecondText) perSecondText.text = $"{FormatNumber(scorePerSecond * passiveMultiplier)}/сек";
    }

    private void UpdateProgressBar()
    {
        if (!levelProgressBar) return;

        if (currentLevelIndex >= levels.Count - 1 && levels.Count > 1)
        {
            levelProgressBar.value = 1;
            if (levelNumberText) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}";
            if (progressText) progressText.text = "МАКС.";
            return;
        }
        double barEndValue = levels[currentLevelIndex + 1].scoreToReach;
        levelProgressBar.minValue = 0f;
        levelProgressBar.maxValue = (float)barEndValue;
        levelProgressBar.value = (float)score;
        if (levelNumberText) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}";
        if (progressText) progressText.text = $"{FormatNumber(score)} / {FormatNumber(barEndValue)}";
    }

    private IEnumerator AnimateScrollToShowItem(RectTransform targetItem)
    {
        isShopAnimating = true;
        shopScrollRect.enabled = false;
        Canvas.ForceUpdateCanvases();

        Vector2 startPos = shopContentRectTransform.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, -targetItem.anchoredPosition.y);
        Vector2 overshoot = targetPos + new Vector2(0, animationBounceAmount);

        float t = 0;
        while (t < 1f) { t += Time.deltaTime * animationScrollSpeed; shopContentRectTransform.anchoredPosition = Vector2.Lerp(startPos, overshoot, t); yield return null; }
        t = 0;
        while (t < 1f) { t += Time.deltaTime * animationScrollSpeed * 1.5f; shopContentRectTransform.anchoredPosition = Vector2.Lerp(overshoot, targetPos, t); yield return null; }

        shopContentRectTransform.anchoredPosition = targetPos;
        isShopAnimating = false;
        shopScrollRect.enabled = true;
    }

    // --- ЛОГИКА КОНЦОВКИ ---

    private IEnumerator WaitAndStartEnding()
    {
        yield return new WaitForSeconds(endingDelay);
        StartEndingSequence();
    }

    private void StartEndingSequence()
    {
        if (endingPanel)
        {
            CanvasGroup cg = SetupCanvasGroup(endingPanel);
            cg.alpha = 0f;
            endingPanel.SetActive(true);

            if (endingVideoPlayer)
            {
                endingVideoPlayer.isLooping = false;
                endingVideoPlayer.loopPointReached += OnVideoFinished;
                endingVideoPlayer.Play();
            }
            if (endingMusic) AudioManager.Instance.PlayMusic(endingMusic);

            StartCoroutine(FadeInEndingPanel(cg));
        }
        else this.enabled = false;
    }

    private IEnumerator FadeInEndingPanel(CanvasGroup cg)
    {
        float t = 0f;
        while (t < endingFadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / endingFadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
        if (mainGamePanel) mainGamePanel.SetActive(false);
        this.enabled = false;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Пауза вместо стопа, чтобы не было черного экрана
        if (vp.frameCount > 0) vp.frame = (long)(vp.frameCount - 5);
        vp.Pause();

        if (postVideoUI) postVideoUI.SetActive(true);
        vp.loopPointReached -= OnVideoFinished;
    }

    // --- КНОПКИ ---
    public void RestartGame() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }

    // --- УТИЛИТЫ ---
    public float GetSatietyPercentage() { return maxSatiety == 0 ? 0 : currentSatiety / maxSatiety; }
    private void ResetCatScale() { catImage.transform.localScale = Vector3.one; }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "К";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "М";
        if (number < 1_000_000_000_000) return (number / 1_000_000_000).ToString("F1") + "Б";
        if (number < 1_000_000_000_000_000) return (number / 1_000_000_000_000).ToString("F1") + "Т";
        return (number / 1_000_000_000_000_000).ToString("F1") + "Кв";
    }

    private CanvasGroup SetupCanvasGroup(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        return cg;
    }
}