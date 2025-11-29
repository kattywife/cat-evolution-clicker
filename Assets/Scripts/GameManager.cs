using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices; // Нужно для связи с Яндексом


public class GameManager : MonoBehaviour
{
    // --- СВЯЗЬ С ЯНДЕКСОМ (Функция из .jslib) ---
    [DllImport("__Internal")]
    private static extern void GameReady();

    // --- ДАННЫЕ ИГРЫ ---
    [Header("Настройки уровней")]
    public List<LevelData> levels;

    [Header("Настройки улучшений")]
    public List<UpgradeData> upgrades;

    [Header("Звуки")]
    public AudioClip catClickSound;
    public AudioClip levelUpSound;


    // --- ПЕРЕМЕННЫЕ ГЕЙМПЛЕЯ ---
    [Header("Текущее состояние")]
    public double score = 0;
    public double scorePerClick = 1;
    public double scorePerSecond = 0;

    [Header("Настройки Сытости")]
    public float maxSatiety = 100f;
    public float currentSatiety;
    public float satietyDepletionRate = 0.5f;
    public float satietyPenaltyMultiplier = 0.1f;


    // --- ССЫЛКИ НА UI ---
    [Header("Ссылки на UI элементы")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI perSecondText;
    public Image catImage;
    public Slider levelProgressBar;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI progressText;
    public Camera uiCamera;


    // --- КНОПКИ (ДЛЯ ЯНДЕКСА) ---
    [Header("Кнопки (Настройка для WebGL)")]
    public GameObject exitButton;
    public GameObject restartButton;


    // --- НАСТРОЙКИ ЗАГРУЗКИ ---
    [Header("Экран Загрузки")]
    public bool enableLoadingScreen = true;
    public GameObject loadingPanel;
    [Tooltip("Картинка с типом Filled для прогресс-бара")]
    public Image loadingFillImage;
    public VideoPlayer loadingCatVideoPlayer;
    public float loadingDuration = 3.0f;

    // --- БЕЛАЯ ВСПЫШКА ---
    [Header("Переход через Белое")]
    public GameObject whiteFadePanel;
    public float whiteFadeInDuration = 1.0f;
    public float whiteFadeOutDuration = 1.0f;


    // --- НАСТРОЙКИ ВСТУПЛЕНИЯ ---
    [Header("Настройки Вступления")]
    public bool enableIntro = true;
    public GameObject introPanel;
    public VideoPlayer introVideoPlayer;
    public float introFadeDuration = 1.5f;


    // --- ССЫЛКИ НА ЭЛЕМЕНТЫ КОНЦОВКИ ---
    [Header("Ссылки на элементы Концовки")]
    public float endingDelay = 3.0f;
    public float endingFadeDuration = 2.0f;

    public GameObject mainGamePanel;
    public GameObject endingPanel;
    public VideoPlayer endingVideoPlayer;
    public GameObject postVideoUI;
    public AudioClip endingMusic;


    [Header("Эффекты")]
    public ParticleSystem levelUpEffect;
    public GameObject tearEffectObject;

    public GameObject clickTextPrefab;
    public Transform canvasTransform;

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


    // --- ОСНОВНЫЕ МЕТОДЫ UNITY ---

    void Start()
    {
        // Инициализация
        currentLevelIndex = 0;
        scorePerClick = 1;
        scorePerSecond = 0;
        score = 0;
        currentSatiety = maxSatiety;

        if (shopContentParent != null)
        {
            shopContentRectTransform = shopContentParent.GetComponent<RectTransform>();
        }

        // Скрываем все лишние панели
        if (endingPanel != null) endingPanel.SetActive(false);
        if (postVideoUI != null) postVideoUI.SetActive(false);
        if (introPanel != null) introPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (whiteFadePanel != null) whiteFadePanel.SetActive(false);

        CreateShop();
        UpdateAllShopButtonsState();

        // Применяем уровень без звуковых эффектов при старте
        ApplyLevelUp(false);

        // --- ЛОГИКА АДАПТАЦИИ ПОД БРАУЗЕР (ЯНДЕКС) ---
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // 1. Прячем кнопку выхода
            if (exitButton != null) exitButton.SetActive(false);

            // 2. Двигаем рестарт в центр
            if (restartButton != null)
            {
                RectTransform rt = restartButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Ставим X в 0 (центр), Y оставляем как есть
                    rt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y);
                }
            }
        }
        // ---------------------------------------------

        // Запуск цепочки экранов
        if (enableLoadingScreen && loadingPanel != null)
        {
            StartCoroutine(PlayLoadingSequence());
        }
        else if (enableIntro && introPanel != null)
        {
            StartCoroutine(StartIntroSequence(false));
        }
        else
        {
            StartGameImmediately();
        }
    }

    // --- 1. ЭКРАН ЗАГРУЗКИ ---
    private IEnumerator PlayLoadingSequence()
    {
        this.enabled = false;

        loadingPanel.SetActive(true);
        CanvasGroup cg = loadingPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = loadingPanel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;

        if (mainGamePanel != null) mainGamePanel.SetActive(false);

        // Запуск видео-котика на загрузке
        if (loadingCatVideoPlayer != null) loadingCatVideoPlayer.Play();

        float timer = 0f;
        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / loadingDuration);
            if (loadingFillImage != null) loadingFillImage.fillAmount = progress;
            yield return null;
        }

        // --- ЗАГРУЗКА ЗАВЕРШЕНА: СООБЩАЕМ ЯНДЕКСУ ---
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            try
            {
                GameReady();
            }
            catch
            {
                Debug.Log("GameReady called (in Editor it fails, that's ok)");
            }
        }
        // --------------------------------------------

        yield return new WaitForSeconds(0.2f);

        // Переход в белое (если настроен)
        if (whiteFadePanel != null)
        {
            whiteFadePanel.SetActive(true);
            CanvasGroup whiteCg = whiteFadePanel.GetComponent<CanvasGroup>();
            if (whiteCg == null) whiteCg = whiteFadePanel.AddComponent<CanvasGroup>();
            whiteCg.alpha = 0f;
            whiteCg.blocksRaycasts = true;

            float whiteTimer = 0f;
            while (whiteTimer < whiteFadeInDuration)
            {
                whiteTimer += Time.deltaTime;
                whiteCg.alpha = Mathf.Lerp(0f, 1f, whiteTimer / whiteFadeInDuration);
                yield return null;
            }
            whiteCg.alpha = 1f;
        }

        if (loadingCatVideoPlayer != null) loadingCatVideoPlayer.Stop();
        loadingPanel.SetActive(false);

        if (enableIntro && introPanel != null)
        {
            StartCoroutine(StartIntroSequence(true));
        }
        else
        {
            StartGameImmediately();
            if (whiteFadePanel != null) StartCoroutine(FadeOutWhite());
        }
    }

    // --- 2. ЭКРАН ИНТРО ---
    private IEnumerator StartIntroSequence(bool startedFromWhite)
    {
        this.enabled = false;

        introPanel.SetActive(true);
        CanvasGroup introCg = introPanel.GetComponent<CanvasGroup>();
        if (introCg == null) introCg = introPanel.AddComponent<CanvasGroup>();
        introCg.alpha = 1f;
        introCg.blocksRaycasts = true;

        if (mainGamePanel != null) mainGamePanel.SetActive(false);

        // Подготовка и запуск видео
        if (introVideoPlayer != null)
        {
            introVideoPlayer.isLooping = false;
            introVideoPlayer.Prepare();
            while (!introVideoPlayer.isPrepared) yield return null;
            introVideoPlayer.Play();
        }

        // Если пришли через белую вспышку - убираем её
        if (startedFromWhite && whiteFadePanel != null) StartCoroutine(FadeOutWhite());

        // Ждем пока видео играет
        if (introVideoPlayer != null)
        {
            while (introVideoPlayer.isPlaying) yield return null;
        }

        // Включаем игру на фоне
        if (mainGamePanel != null) mainGamePanel.SetActive(true);

        float timer = 0f;
        bool hasPlayedEffect = false;

        // Плавное исчезновение интро
        while (timer < introFadeDuration)
        {
            timer += Time.deltaTime;
            introCg.alpha = Mathf.Lerp(1f, 0f, timer / introFadeDuration);

            // Эффект появления кота на 20% прозрачности
            if (!hasPlayedEffect && timer > (introFadeDuration * 0.2f))
            {
                if (levelUpEffect != null) levelUpEffect.Play();
                hasPlayedEffect = true;
            }
            yield return null;
        }

        introPanel.SetActive(false);
        if (!hasPlayedEffect && levelUpEffect != null) levelUpEffect.Play();

        this.enabled = true; // Включаем логику игры
    }

    private IEnumerator FadeOutWhite()
    {
        if (whiteFadePanel == null) yield break;
        CanvasGroup whiteCg = whiteFadePanel.GetComponent<CanvasGroup>();

        float timer = 0f;
        while (timer < whiteFadeOutDuration)
        {
            timer += Time.deltaTime;
            whiteCg.alpha = Mathf.Lerp(1f, 0f, timer / whiteFadeOutDuration);
            yield return null;
        }
        whiteFadePanel.SetActive(false);
    }

    private void StartGameImmediately()
    {
        if (mainGamePanel != null) mainGamePanel.SetActive(true);
        if (levelUpEffect != null) levelUpEffect.Play();
        this.enabled = true;
    }


    // --- ИГРОВОЙ ЦИКЛ UPDATE ---

    void Update()
    {
        // Клик пробелом
        if (this.enabled && Input.GetKeyDown(KeyCode.Space))
        {
            if (endingPanel == null || !endingPanel.activeSelf)
            {
                if (catImage != null)
                {
                    Vector3 clickPos = catImage.transform.position;
                    // Небольшой разброс для красоты
                    clickPos.x += Random.Range(-50f, 50f);
                    clickPos.y += Random.Range(-50f, 50f);
                    HandleClick(clickPos);
                }
            }
        }

        // Расчет очков
        double finalScorePerSecond = scorePerSecond * passiveMultiplier;

        if (currentSatiety > 0)
            currentSatiety -= satietyDepletionRate * Time.deltaTime;
        else
            currentSatiety = 0;

        double effectiveSps = finalScorePerSecond;

        // Логика голода и слез
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

        // Обновление кнопок магазина
        for (int i = 0; i < unlockedItemsCount; i++)
        {
            if (i < shopButtons.Count && shopButtons[i] != null)
                shopButtons[i].UpdateInteractableState(score);
        }

        UpdateAllUITexts();
        UpdateProgressBar();
    }


    // --- МЕТОДЫ КЛИКОВ И ЛОГИКА ---

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
        if (playEffects && levelUpSound != null)
            AudioManager.Instance.PlaySound(levelUpSound, 0.8f);

        if (levels.Count > 0 && currentLevelIndex < levels.Count)
        {
            catImage.sprite = levels[currentLevelIndex].catSprite;
            catImage.SetNativeSize();
            if (tearEffectObject != null) tearEffectObject.transform.localPosition = levels[currentLevelIndex].tearPosition;
        }

        if (playEffects && levelUpEffect != null)
            levelUpEffect.Play();

        // Проверка на последний уровень
        if (currentLevelIndex == levels.Count - 1)
        {
            satietyDepletionRate = 0f;
            currentSatiety = maxSatiety;
            if (tearEffectObject != null) tearEffectObject.SetActive(false);

            if (playEffects) StartCoroutine(WaitAndStartEnding());
        }
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (score >= cost)
        {
            score -= cost;
            switch (upgrade.type)
            {
                case UpgradeType.PerClick: scorePerClick += upgrade.power; break;
                case UpgradeType.PerSecond: scorePerSecond += upgrade.power; break;
                case UpgradeType.ClickMultiplier: clickMultiplier += upgrade.power; break;
                case UpgradeType.PassiveMultiplier: passiveMultiplier += upgrade.power; break;
                case UpgradeType.GlobalMultiplier: clickMultiplier += upgrade.power; passiveMultiplier += upgrade.power; break;
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

    public void FeedCat(double cost, float amount)
    {
        if (score >= cost)
        {
            score -= cost;
            currentSatiety = Mathf.Min(maxSatiety, currentSatiety + amount);
        }
    }

    public void SuperFeedCat()
    {
        currentSatiety = maxSatiety * 2.0f;
    }

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
        if (totalScoreText != null) totalScoreText.text = FormatNumber(score);
        if (perSecondText != null) perSecondText.text = $"{FormatNumber(scorePerSecond * passiveMultiplier)}/сек";
    }

    private void UpdateProgressBar()
    {
        if (levelProgressBar == null) return;
        if (currentLevelIndex >= levels.Count - 1 && levels.Count > 1)
        {
            levelProgressBar.value = 1;
            if (levelNumberText != null) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}";
            if (progressText != null) progressText.text = "МАКС.";
            return;
        }
        double barEndValue = levels[currentLevelIndex + 1].scoreToReach;
        levelProgressBar.minValue = 0f;
        levelProgressBar.maxValue = (float)barEndValue;
        levelProgressBar.value = (float)score;
        if (levelNumberText != null) levelNumberText.text = $"Уровень: {currentLevelIndex + 1}";
        if (progressText != null) progressText.text = $"{FormatNumber(score)} / {FormatNumber(barEndValue)}";
    }

    private IEnumerator AnimateScrollToShowItem(RectTransform targetItem)
    {
        isShopAnimating = true;
        shopScrollRect.enabled = false;
        Canvas.ForceUpdateCanvases();

        Vector2 startPosition = shopContentRectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, -targetItem.anchoredPosition.y);
        Vector2 overshootPosition = targetPosition + new Vector2(0, animationBounceAmount);

        float timer = 0f;
        while (timer < 1f) { timer += Time.deltaTime * animationScrollSpeed; shopContentRectTransform.anchoredPosition = Vector2.Lerp(startPosition, overshootPosition, timer); yield return null; }
        timer = 0f;
        while (timer < 1f) { timer += Time.deltaTime * animationScrollSpeed * 1.5f; shopContentRectTransform.anchoredPosition = Vector2.Lerp(overshootPosition, targetPosition, timer); yield return null; }

        shopContentRectTransform.anchoredPosition = targetPosition;
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
        if (endingPanel != null)
        {
            CanvasGroup cg = endingPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = endingPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = true;
            endingPanel.SetActive(true);

            if (endingVideoPlayer != null)
            {
                endingVideoPlayer.isLooping = false;
                endingVideoPlayer.loopPointReached += OnVideoFinished;
                endingVideoPlayer.Play();
            }

            if (endingMusic != null) AudioManager.Instance.PlayMusic(endingMusic);

            StartCoroutine(FadeInEndingPanel(cg));
        }
        else
        {
            this.enabled = false;
        }
    }

    private IEnumerator FadeInEndingPanel(CanvasGroup cg)
    {
        float timer = 0f;
        while (timer < endingFadeDuration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, timer / endingFadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
        if (mainGamePanel != null) mainGamePanel.SetActive(false);
        this.enabled = false;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (postVideoUI != null) postVideoUI.SetActive(true);
        vp.loopPointReached -= OnVideoFinished;
    }

    public float GetSatietyPercentage()
    {
        return maxSatiety == 0 ? 0 : currentSatiety / maxSatiety;
    }

    private void ResetCatScale()
    {
        catImage.transform.localScale = Vector3.one;
    }

    // --- РУСИФИКАЦИЯ ЧИСЕЛ ---
    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "К"; // Тысячи
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "М"; // Миллионы
        if (number < 1_000_000_000_000) return (number / 1_000_000_000).ToString("F1") + "Б"; // Миллиарды
        if (number < 1_000_000_000_000_000) return (number / 1_000_000_000_000).ToString("F1") + "Т"; // Триллионы
        return (number / 1_000_000_000_000_000).ToString("F1") + "Кв"; // Квадриллионы
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ КНОПОК ---

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Debug.Log("Нажата кнопка Выход");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}