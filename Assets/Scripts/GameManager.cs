using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region --- НАСТРОЙКИ И ДАННЫЕ ---

    [Header("Данные игры")]
    public List<LevelData> levels;
    public List<UpgradeData> upgrades;

    [Header("Звуки")]
    public AudioClip catClickSound;
    public AudioClip levelUpSound;
    public AudioClip endingMusic;

    #endregion

    #region --- ГЕЙМПЛЕЙ ---

    [Header("Состояние Экономики")]
    public double score = 0;
    private double scorePerClick = 1;
    private double scorePerSecond = 0;

    [Header("Механика Сытости")]
    public float maxSatiety = 100f;
    public float currentSatiety;
    public float satietyDepletionRate = 0.5f;
    public float satietyPenaltyMultiplier = 0.1f;

    private double clickMultiplier = 1.0;
    private double passiveMultiplier = 1.0;
    private int currentLevelIndex = 0;

    [HideInInspector] public bool hasWatchedIntro = false;

    #endregion

    #region --- UI ЭЛЕМЕНТЫ ---

    [Header("UI Ссылки")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI perSecondText;
    public Image catImage;
    public Slider levelProgressBar;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI progressText;
    public Camera uiCamera;
    public Transform canvasTransform;

    [Header("Кнопки (WebGL)")]
    public GameObject exitButton;
    public GameObject restartButton;

    [Header("Бонус за рекламу (х2)")]
    public Button doubleScoreButton;
    public TextMeshProUGUI doubleScoreButtonText;
    public float doubleScoreCooldown = 180f;
    private float currentDoubleScoreTimer = 0f;

    #endregion

    #region --- ВИДЕО И ЭКРАНЫ ---

    [Header("Имена файлов")]
    public string loadingVideoName = "loading.mp4";
    public string introVideoName = "intro.mp4";
    public string endingVideoName = "ending.mp4";

    [Header("Экран Загрузки - Объекты")]
    public bool enableLoadingScreen = true;
    public GameObject loadingPanel;           // Вся панель загрузки целиком

    [Tooltip("Перетащи сюда ВЕСЬ объект слайдера (родительский)")]
    public GameObject loadingProgressBarObject;

    [Tooltip("Перетащи сюда картинку Fill (которая ползет)")]
    public Image loadingFillImage;

    [Tooltip("Перетащи сюда кнопку Старт")]
    public GameObject loadingStartButton;

    public VideoPlayer loadingCatVideoPlayer;
    public float loadingDuration = 3.0f;

    private bool isStartButtonClicked = false;

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

    private bool isWatchingEnding = false;

    #endregion

    #region --- ЭФФЕКТЫ И МАГАЗИН ---

    [Header("Эффекты")]
    public ParticleSystem levelUpEffect;
    public GameObject tearEffectObject;
    public GameObject clickTextPrefab;

    [Header("Магазин")]
    public GameObject upgradeButtonPrefab;
    public Transform shopContentParent;
    public ScrollRect shopScrollRect;
    public float animationScrollSpeed = 3f;
    public float animationBounceAmount = 50f;
    public int initialItemsToIgnore = 4;

    private RectTransform shopContentRectTransform;
    private List<UpgradeButtonUI> shopButtons = new List<UpgradeButtonUI>();
    private int unlockedItemsCount = 1;
    private bool isShopAnimating = false;

    #endregion

    private enum AdRewardType { None, SuperFood, DoubleScore }
    private AdRewardType pendingAdReward = AdRewardType.None;


    #region --- СТАРТ И ИНИЦИАЛИЗАЦИЯ ---

    void Start()
    {
        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.OnRewardGranted += OnAdRewarded;
        }

        currentLevelIndex = 0;
        scorePerClick = 1;
        scorePerSecond = 0;
        score = 0;
        currentSatiety = maxSatiety;
        hasWatchedIntro = false;
        isStartButtonClicked = false;

        // Инициализация загрузочного экрана
        if (loadingStartButton) loadingStartButton.SetActive(false);
        if (loadingProgressBarObject) loadingProgressBarObject.SetActive(true);

        if (shopContentParent != null)
            shopContentRectTransform = shopContentParent.GetComponent<RectTransform>();

        if (endingPanel) endingPanel.SetActive(false);
        if (postVideoUI) postVideoUI.SetActive(false);
        if (introPanel) introPanel.SetActive(false);
        if (loadingPanel) loadingPanel.SetActive(false);
        if (whiteFadePanel) whiteFadePanel.SetActive(false);

        CreateShop();
        UpdateAllShopButtonsState();
        ApplyLevelUp(false);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (exitButton) exitButton.SetActive(false);
            if (restartButton)
            {
                RectTransform rt = restartButton.GetComponent<RectTransform>();
                if (rt) rt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y);
            }
        }

        if (enableLoadingScreen && loadingPanel)
            StartCoroutine(PlayLoadingSequence());
        else if (enableIntro && introPanel)
            StartCoroutine(StartIntroSequence(false));
        else
            StartGameImmediately();
    }

    void OnDestroy()
    {
        if (YandexManager.Instance != null)
            YandexManager.Instance.OnRewardGranted -= OnAdRewarded;
    }

    #endregion


    #region --- ЛОГИКА ЗАГРУЗКИ ---

    public void OnLoadingStartClicked()
    {
        isStartButtonClicked = true;
        AudioManager.Instance.PlaySound(catClickSound);
    }

    private IEnumerator PlayLoadingSequence()
    {
        this.enabled = false;
        loadingPanel.SetActive(true);
        SetupCanvasGroup(loadingPanel);

        // 1. Скрываем кнопку, показываем ВЕСЬ прогресс-бар
        if (loadingStartButton) loadingStartButton.SetActive(false);
        if (loadingProgressBarObject) loadingProgressBarObject.SetActive(true);

        isStartButtonClicked = false;
        if (mainGamePanel) mainGamePanel.SetActive(false);

        if (loadingCatVideoPlayer)
        {
            yield return StartCoroutine(PrepareVideoSafe(loadingCatVideoPlayer, loadingVideoName));
            if (loadingCatVideoPlayer.isPrepared) loadingCatVideoPlayer.Play();
        }

        // --- ЭТАП 1: Заполнение ---
        float timer = 0f;
        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;
            // Двигаем Fill внутри бара
            if (loadingFillImage)
                loadingFillImage.fillAmount = Mathf.Clamp01(timer / loadingDuration);
            yield return null;
        }

        // --- ЭТАП 2: Переключение (Бар ИСЧЕЗАЕТ, Кнопка ПОЯВЛЯЕТСЯ) ---

        // Выключаем ВЕСЬ объект прогресс-бара (включая фон)
        if (loadingProgressBarObject) loadingProgressBarObject.SetActive(false);

        // Включаем кнопку
        if (loadingStartButton) loadingStartButton.SetActive(true);

        if (YandexManager.Instance != null) YandexManager.Instance.ReportGameReady();

        // --- ЭТАП 3: Ждем клика ---
        yield return new WaitUntil(() => isStartButtonClicked);

        // --- ЭТАП 4: Переход ---
        if (loadingStartButton) loadingStartButton.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        bool shouldShowIntro = (enableIntro && introPanel && !hasWatchedIntro);

        if (shouldShowIntro)
        {
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
        }

        if (loadingCatVideoPlayer) loadingCatVideoPlayer.Stop();
        loadingPanel.SetActive(false);

        if (shouldShowIntro)
        {
            StartCoroutine(StartIntroSequence(true));
        }
        else
        {
            StartGameImmediately();
            if (whiteFadePanel && whiteFadePanel.activeSelf) StartCoroutine(FadeOutWhite());
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
            yield return StartCoroutine(PrepareVideoSafe(introVideoPlayer, introVideoName));
            if (introVideoPlayer.isPrepared) introVideoPlayer.Play();
        }

        if (startedFromWhite && whiteFadePanel) StartCoroutine(FadeOutWhite());

        if (introVideoPlayer && introVideoPlayer.isPrepared)
        {
            while (!introVideoPlayer.isPlaying) yield return null;
            while (introVideoPlayer.isPlaying) yield return null;
        }

        hasWatchedIntro = true;
        if (SaveManager.Instance != null) SaveManager.Instance.Save();

        AudioSource videoAudio = (introVideoPlayer != null) ? introVideoPlayer.GetComponent<AudioSource>() : null;
        float startVolume = (videoAudio != null) ? videoAudio.volume : 1f;

        if (whiteFadePanel)
        {
            whiteFadePanel.SetActive(true);
            CanvasGroup w = SetupCanvasGroup(whiteFadePanel);
            w.alpha = 0;
            float t = 0;
            while (t < introFadeDuration)
            {
                t += Time.deltaTime;
                float progress = t / introFadeDuration;
                w.alpha = Mathf.Lerp(0, 1, progress);
                if (videoAudio != null) videoAudio.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }
            w.alpha = 1;
        }

        introPanel.SetActive(false);
        StartGameImmediately();
    }

    private void StartGameImmediately()
    {
        if (mainGamePanel) mainGamePanel.SetActive(true);
        if (whiteFadePanel) StartCoroutine(FadeOutWhiteAndPlaySound());
        else this.enabled = true;
    }

    private IEnumerator FadeOutWhiteAndPlaySound()
    {
        CanvasGroup w = whiteFadePanel.GetComponent<CanvasGroup>();
        float t = 0;
        bool playedEffect = false;
        while (t < whiteFadeOutDuration)
        {
            t += Time.deltaTime;
            w.alpha = Mathf.Lerp(1, 0, t / whiteFadeOutDuration);
            if (!playedEffect && t > whiteFadeOutDuration * 0.2f)
            {
                if (levelUpEffect) levelUpEffect.Play();
                if (levelUpSound) AudioManager.Instance.PlaySound(levelUpSound, 0.8f);
                playedEffect = true;
            }
            yield return null;
        }
        whiteFadePanel.SetActive(false);
        this.enabled = true;
    }

    private IEnumerator PrepareVideoSafe(VideoPlayer vp, string fileName)
    {
        if (vp == null) yield break;
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        vp.source = VideoSource.Url;
        vp.url = videoPath;
        vp.waitForFirstFrame = false;
        vp.Prepare();
        float timeout = 20.0f;
        while (!vp.isPrepared && timeout > 0) { timeout -= Time.deltaTime; yield return null; }
    }

    private IEnumerator FadeOutWhite()
    {
        if (whiteFadePanel)
        {
            CanvasGroup c = whiteFadePanel.GetComponent<CanvasGroup>();
            float t = 0;
            while (t < whiteFadeOutDuration) { t += Time.deltaTime; c.alpha = Mathf.Lerp(1, 0, t / whiteFadeOutDuration); yield return null; }
            whiteFadePanel.SetActive(false);
        }
    }

    #endregion


    #region --- UPDATE И ИГРОВОЙ ЦИКЛ ---

    void Update()
    {
        if (!this.enabled) return;
        if (isWatchingEnding && endingVideoPlayer != null && endingVideoPlayer.isPlaying)
        {
            if (endingVideoPlayer.length > 0 && endingVideoPlayer.time >= endingVideoPlayer.length - 0.1f)
            {
                endingVideoPlayer.Pause();
                isWatchingEnding = false;
                if (postVideoUI) postVideoUI.SetActive(true);
            }
        }
        if (currentDoubleScoreTimer > 0)
        {
            currentDoubleScoreTimer -= Time.deltaTime;
            if (doubleScoreButton && doubleScoreButton.gameObject.activeSelf) doubleScoreButton.gameObject.SetActive(false);
        }
        else
        {
            if (doubleScoreButton && !doubleScoreButton.gameObject.activeSelf) doubleScoreButton.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (endingPanel == null || !endingPanel.activeSelf)
                if (catImage != null) HandleClick(catImage.transform.position);
        }
        double finalScorePerSecond = scorePerSecond * passiveMultiplier;
        if (currentSatiety > 0) currentSatiety -= satietyDepletionRate * Time.deltaTime; else currentSatiety = 0;
        double effectiveSps = finalScorePerSecond;
        if (currentSatiety <= 0) { effectiveSps *= satietyPenaltyMultiplier; if (tearEffectObject && !tearEffectObject.activeSelf) tearEffectObject.SetActive(true); }
        else { if (tearEffectObject && tearEffectObject.activeSelf) tearEffectObject.SetActive(false); }
        if (effectiveSps > 0) score += effectiveSps * Time.deltaTime;
        for (int i = 0; i < unlockedItemsCount; i++)
            if (i < shopButtons.Count && shopButtons[i] != null) shopButtons[i].UpdateInteractableState(score);
        UpdateAllUITexts();
        UpdateProgressBar();
    }

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

        // --- ТУТОРИАЛ: КЛИК ---
        if (TutorialManager.Instance) TutorialManager.Instance.OnCatClicked();
        // ---------------------

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

    #endregion


    #region --- МАГАЗИН И ПРОКАЧКА ---

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (score >= cost)
        {
            score -= cost;
            switch (upgrade.type) { case UpgradeType.PerClick: if (clickMultiplier > 0) scorePerClick += upgrade.power / clickMultiplier; else scorePerClick += upgrade.power; break; case UpgradeType.PerSecond: if (passiveMultiplier > 0) scorePerSecond += upgrade.power / passiveMultiplier; else scorePerSecond += upgrade.power; break; case UpgradeType.ClickMultiplier: clickMultiplier *= upgrade.power; break; case UpgradeType.PassiveMultiplier: passiveMultiplier *= upgrade.power; break; case UpgradeType.GlobalMultiplier: clickMultiplier *= upgrade.power; passiveMultiplier *= upgrade.power; break; }
            int purchasedIndex = shopButtons.IndexOf(button);
            if (purchasedIndex == unlockedItemsCount - 1 && unlockedItemsCount < shopButtons.Count) { unlockedItemsCount++; if (unlockedItemsCount - 1 >= initialItemsToIgnore && !isShopAnimating) StartCoroutine(AnimateScrollToShowItem(shopButtons[unlockedItemsCount - 1].GetComponent<RectTransform>())); }

            button.OnPurchaseSuccess();
            UpdateAllShopButtonsState();

            // --- ТУТОРИАЛ: ПОКУПКА ---
            if (TutorialManager.Instance) TutorialManager.Instance.OnUpgradePurchased();
            // -------------------------

            if (SaveManager.Instance != null) SaveManager.Instance.Save();
        }
    }

    public void FeedCat(double cost, float amount)
    {
        if (score >= cost)
        {
            score -= cost;
            currentSatiety = Mathf.Min(maxSatiety, currentSatiety + amount);

            // --- ТУТОРИАЛ: КОРМЕЖКА ---
            if (TutorialManager.Instance) TutorialManager.Instance.OnCatFed();
            // -------------------------
        }
    }

    public void SuperFeedCat()
    {
        currentSatiety = maxSatiety * 2.0f;
        // --- ТУТОРИАЛ: СУПЕР КОРМЕЖКА ---
        if (TutorialManager.Instance) TutorialManager.Instance.OnCatFed();
        // -------------------------
    }

    private void CreateShop() { foreach (var upgrade in upgrades) { GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopContentParent); UpgradeButtonUI buttonUI = newButtonGO.GetComponent<UpgradeButtonUI>(); buttonUI.Setup(upgrade, this); shopButtons.Add(buttonUI); } }
    private void UpdateAllShopButtonsState() { for (int i = 0; i < shopButtons.Count; i++) { if (shopButtons[i] == null) continue; bool isUnlocked = (i < unlockedItemsCount); shopButtons[i].SetLockedState(!isUnlocked); if (isUnlocked) shopButtons[i].UpdateInteractableState(score); } }
    private IEnumerator AnimateScrollToShowItem(RectTransform targetItem) { isShopAnimating = true; shopScrollRect.enabled = false; Canvas.ForceUpdateCanvases(); Vector2 startPos = shopContentRectTransform.anchoredPosition; Vector2 targetPos = new Vector2(startPos.x, -targetItem.anchoredPosition.y); Vector2 overshoot = targetPos + new Vector2(0, animationBounceAmount); float t = 0; while (t < 1f) { t += Time.deltaTime * animationScrollSpeed; shopContentRectTransform.anchoredPosition = Vector2.Lerp(startPos, overshoot, t); yield return null; } t = 0; while (t < 1f) { t += Time.deltaTime * animationScrollSpeed * 1.5f; shopContentRectTransform.anchoredPosition = Vector2.Lerp(overshoot, targetPos, t); yield return null; } shopContentRectTransform.anchoredPosition = targetPos; isShopAnimating = false; shopScrollRect.enabled = true; }

    #endregion


    #region --- УРОВНИ И КОНЦОВКА ---

    private void CheckForLevelUp() { if (currentLevelIndex + 1 < levels.Count) { if (score >= levels[currentLevelIndex + 1].scoreToReach) { currentLevelIndex++; ApplyLevelUp(true); } } }
    private void ApplyLevelUp(bool playEffects = true)
    {
        if (playEffects && levelUpSound) AudioManager.Instance.PlaySound(levelUpSound, 0.8f);
        if (levels.Count > 0 && currentLevelIndex < levels.Count) { catImage.sprite = levels[currentLevelIndex].catSprite; catImage.SetNativeSize(); if (tearEffectObject) tearEffectObject.transform.localPosition = levels[currentLevelIndex].tearPosition; }
        if (playEffects && levelUpEffect) levelUpEffect.Play();
        if (playEffects) { if (YandexManager.Instance) YandexManager.Instance.ShowInterstitialAd(); if (SaveManager.Instance) SaveManager.Instance.Save(); }
        if (currentLevelIndex == levels.Count - 1) { satietyDepletionRate = 0f; currentSatiety = maxSatiety; if (tearEffectObject) tearEffectObject.SetActive(false); if (playEffects) StartCoroutine(WaitAndStartEnding()); }
    }
    private IEnumerator WaitAndStartEnding() { yield return new WaitForSeconds(endingDelay); StartEndingSequence(); }
    private void StartEndingSequence() { if (endingPanel) { CanvasGroup cg = SetupCanvasGroup(endingPanel); cg.alpha = 0f; endingPanel.SetActive(true); if (endingVideoPlayer) { endingVideoPlayer.isLooping = false; StartCoroutine(PrepareAndPlayEnding(endingVideoPlayer)); } if (endingMusic) AudioManager.Instance.PlayMusic(endingMusic); StartCoroutine(FadeInEndingPanel(cg)); } else this.enabled = false; }
    private IEnumerator PrepareAndPlayEnding(VideoPlayer vp) { yield return StartCoroutine(PrepareVideoSafe(vp, endingVideoName)); if (vp.isPrepared) { vp.Play(); isWatchingEnding = true; } else { if (postVideoUI) postVideoUI.SetActive(true); } }
    private IEnumerator FadeInEndingPanel(CanvasGroup cg) { float t = 0f; while (t < endingFadeDuration) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(0f, 1f, t / endingFadeDuration); yield return null; } cg.alpha = 1f; if (mainGamePanel) mainGamePanel.SetActive(false); }

    #endregion


    #region --- РЕКЛАМА И SAVE ---

    public void WatchAdForSuperFood() { pendingAdReward = AdRewardType.SuperFood; if (YandexManager.Instance) YandexManager.Instance.ShowRewardAd(); }
    public void WatchAdForDoubleScore() { pendingAdReward = AdRewardType.DoubleScore; if (YandexManager.Instance) YandexManager.Instance.ShowRewardAd(); }
    private void OnAdRewarded() { if (pendingAdReward == AdRewardType.SuperFood) SuperFeedCat(); else if (pendingAdReward == AdRewardType.DoubleScore) { if (score > 0) score *= 2; else score += 100; currentDoubleScoreTimer = doubleScoreCooldown; } pendingAdReward = AdRewardType.None; UpdateAllUITexts(); if (SaveManager.Instance != null) SaveManager.Instance.Save(); }
    public int GetCurrentLevel() { return currentLevelIndex; }
    public void LoadGameState(double loadedScore, int loadedLevel, bool introSeen) { score = loadedScore; currentLevelIndex = loadedLevel; hasWatchedIntro = introSeen; UpdateAllUITexts(); UpdateProgressBar(); ApplyLevelUp(false); }

    #endregion


    #region --- УТИЛИТЫ И UI ---

    private void UpdateAllUITexts() { if (totalScoreText) totalScoreText.text = FormatNumber(score); if (perSecondText) perSecondText.text = $"{FormatNumber(scorePerSecond * passiveMultiplier)}/сек"; }
    private void UpdateProgressBar() { if (!levelProgressBar) return; if (currentLevelIndex >= levels.Count - 1 && levels.Count > 1) { levelProgressBar.value = 1; if (levelNumberText) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}"; if (progressText) progressText.text = "МАКС."; return; } double barEndValue = levels[currentLevelIndex + 1].scoreToReach; levelProgressBar.minValue = 0f; levelProgressBar.maxValue = (float)barEndValue; levelProgressBar.value = (float)score; if (levelNumberText) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}"; if (progressText) progressText.text = $"{FormatNumber(score)} / {FormatNumber(barEndValue)}"; }
    public float GetSatietyPercentage() { return maxSatiety == 0 ? 0 : currentSatiety / maxSatiety; }
    private void ResetCatScale() { catImage.transform.localScale = Vector3.one; }
    private CanvasGroup SetupCanvasGroup(GameObject panel) { CanvasGroup cg = panel.GetComponent<CanvasGroup>(); if (cg == null) cg = panel.AddComponent<CanvasGroup>(); cg.alpha = 1f; cg.blocksRaycasts = true; return cg; }
    private string FormatNumber(double number) { if (number < 1000) return number.ToString("F0"); if (number < 1_000_000) return (number / 1000).ToString("F1") + "K"; if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "M"; if (number < 1_000_000_000_000) return (number / 1_000_000_000).ToString("F1") + "Б"; if (number < 1_000_000_000_000_000) return (number / 1_000_000_000_000).ToString("F1") + "Т"; return (number / 1_000_000_000_000_000).ToString("F1") + "Кв"; }
    public void RestartGame() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void ExitGame() { Application.Quit(); }

    #endregion
}