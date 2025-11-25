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
    // --- ДАННЫЕ ИГРЫ ---
    [Header("Настройки уровней")]
    public List<LevelData> levels;

    [Header("Настройки улучшений")]
    public List<UpgradeData> upgrades;

    [Header("Звуки")]
    public AudioClip catClickSound;
    [Tooltip("Звук при переходе на новый уровень")]
    public AudioClip levelUpSound;


    // --- ПЕРЕМЕННЫЕ ГЕЙМПЛЕЯ ---
    [Header("Текущее состояние")]
    public double score = 0;
    public double scorePerClick = 1;
    public double scorePerSecond = 0;

    [Header("Настройки Сытости")]
    public float maxSatiety = 100f;
    public float currentSatiety;
    [Tooltip("Сколько единиц сытости котик теряет в секунду")]
    public float satietyDepletionRate = 0.5f;
    [Tooltip("Множитель дохода, когда котик голоден (0.1 = 10%)")]
    public float satietyPenaltyMultiplier = 0.1f;


    // --- ССЫЛКИ НА UI ---
    [Header("Ссылки на UI элементы")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI perSecondText;
    public Image catImage;
    public Slider levelProgressBar;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI progressText;
    [Tooltip("Сюда нужно перетащить камеру, которая рендерит ваш UI")]
    public Camera uiCamera;


    // --- НАСТРОЙКИ ВСТУПЛЕНИЯ ---
    [Header("Настройки Вступления")]
    [Tooltip("Включить ли начальную заставку?")]
    public bool enableIntro = true;
    public GameObject introPanel;
    public VideoPlayer introVideoPlayer;
    [Tooltip("Сколько секунд панель будет исчезать (Fade Out)")]
    public float introFadeDuration = 1.5f;


    // --- ССЫЛКИ НА ЭЛЕМЕНТЫ КОНЦОВКИ ---
    [Header("Ссылки на элементы Концовки")]
    public float endingDelay = 3.0f;
    public float endingFadeDuration = 2.0f;

    [Tooltip("Панель с основным игровым UI")]
    public GameObject mainGamePanel;
    [Tooltip("Главная панель концовки")]
    public GameObject endingPanel;
    public VideoPlayer endingVideoPlayer;
    public GameObject postVideoUI;
    public AudioClip endingMusic;


    [Header("Эффекты")]
    public ParticleSystem levelUpEffect;
    [Tooltip("Объект с эффектом слез, который будем двигать")]
    public GameObject tearEffectObject;

    public GameObject clickTextPrefab;
    public Transform canvasTransform;

    [Header("Магазин")]
    public GameObject upgradeButtonPrefab;
    public Transform shopContentParent;
    public ScrollRect shopScrollRect;

    [Header("Настройки анимации магазина")]
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
        currentLevelIndex = 0;
        scorePerClick = 1;
        scorePerSecond = 0;
        score = 0;
        currentSatiety = maxSatiety;

        if (shopContentParent != null)
        {
            shopContentRectTransform = shopContentParent.GetComponent<RectTransform>();
        }

        if (endingPanel != null) endingPanel.SetActive(false);
        if (postVideoUI != null) postVideoUI.SetActive(false);

        CreateShop();
        UpdateAllShopButtonsState();

        // 1. Применяем уровень БЕЗ эффектов
        ApplyLevelUp(false);

        // --- ЛОГИКА ЗАПУСКА ---
        if (enableIntro && introPanel != null && introVideoPlayer != null)
        {
            this.enabled = false;
            introPanel.SetActive(true);
            CanvasGroup cg = introPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = introPanel.AddComponent<CanvasGroup>();

            cg.alpha = 1f;
            cg.blocksRaycasts = true;

            if (mainGamePanel != null) mainGamePanel.SetActive(false);

            StartCoroutine(PlayIntroSequence());
        }
        else
        {
            // Если интро нет
            if (introPanel != null) introPanel.SetActive(false);
            if (mainGamePanel != null) mainGamePanel.SetActive(true);

            // Если интро нет, сразу показываем эффект
            if (levelUpEffect != null) levelUpEffect.Play();

            this.enabled = true;
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        introVideoPlayer.isLooping = false;
        introVideoPlayer.Prepare();

        while (!introVideoPlayer.isPrepared)
        {
            yield return null;
        }

        introVideoPlayer.Play();

        while (introVideoPlayer.isPlaying)
        {
            yield return null;
        }

        // --- ПЕРЕХОД К ИГРЕ ---

        if (mainGamePanel != null) mainGamePanel.SetActive(true);

        CanvasGroup cg = introPanel.GetComponent<CanvasGroup>();

        float timer = 0f;
        bool hasPlayedEffect = false; // Флаг, чтобы эффект сработал один раз

        while (timer < introFadeDuration)
        {
            timer += Time.deltaTime;

            // Плавно меняем прозрачность
            cg.alpha = Mathf.Lerp(1f, 0f, timer / introFadeDuration);

            // --- НОВОЕ: Запускаем эффект ВОВРЕМЯ исчезновения ---
            // Сработает, когда пройдет 20% времени затемнения.
            // Экран еще черный, но уже светлеет -> ПУФ! -> Котик появляется из дымки
            if (!hasPlayedEffect && timer > (introFadeDuration * 0.2f))
            {
                if (levelUpEffect != null) levelUpEffect.Play();
                hasPlayedEffect = true;
            }
            // ----------------------------------------------------

            yield return null;
        }

        introPanel.SetActive(false);

        // Страховка: если вдруг эффект не сработал в цикле (например, время introFadeDuration = 0)
        if (!hasPlayedEffect && levelUpEffect != null)
        {
            levelUpEffect.Play();
        }

        this.enabled = true;
    }

    void Update()
    {
        double finalScorePerSecond = scorePerSecond * passiveMultiplier;

        if (currentSatiety > 0)
        {
            currentSatiety -= satietyDepletionRate * Time.deltaTime;
        }
        else
        {
            currentSatiety = 0;
        }

        double effectiveSps = finalScorePerSecond;

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
        {
            score += effectiveSps * Time.deltaTime;
        }

        for (int i = 0; i < unlockedItemsCount; i++)
        {
            if (i < shopButtons.Count && shopButtons[i] != null)
            {
                shopButtons[i].UpdateInteractableState(score);
            }
        }

        UpdateAllUITexts();
        UpdateProgressBar();
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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    // --- ИГРОВАЯ МЕХАНИКА ---

    public void OnCatClicked(BaseEventData baseData)
    {
        if (!this.enabled) return;

        PointerEventData eventData = baseData as PointerEventData;
        if (eventData == null) return;

        AudioManager.Instance.PlaySound(catClickSound);

        double finalScorePerClick = scorePerClick * clickMultiplier;
        score += finalScorePerClick;

        CheckForLevelUp();
        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);

        if (clickTextPrefab != null && canvasTransform != null)
        {
            GameObject textGO = Instantiate(clickTextPrefab, canvasTransform);
            RectTransform canvasRect = canvasTransform.GetComponent<RectTransform>();
            Vector2 localPoint;
            Camera cam = (uiCamera != null) ? uiCamera : Camera.main;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, cam, out localPoint);
            textGO.GetComponent<RectTransform>().localPosition = localPoint;
            TextMeshProUGUI textMesh = textGO.GetComponent<TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.text = "+" + FormatNumber(finalScorePerClick);
            }
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
        {
            AudioManager.Instance.PlaySound(levelUpSound, 0.8f);
        }

        if (levels.Count > 0 && currentLevelIndex < levels.Count)
        {
            catImage.sprite = levels[currentLevelIndex].catSprite;
            catImage.SetNativeSize();
            if (tearEffectObject != null) tearEffectObject.transform.localPosition = levels[currentLevelIndex].tearPosition;
        }

        if (playEffects && levelUpEffect != null)
        {
            levelUpEffect.Play();
        }

        if (currentLevelIndex == levels.Count - 1)
        {
            satietyDepletionRate = 0f;
            currentSatiety = maxSatiety;
            if (tearEffectObject != null) tearEffectObject.SetActive(false);

            if (playEffects)
            {
                StartCoroutine(WaitAndStartEnding());
            }
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
            if (purchasedIndex == unlockedItemsCount - 1)
            {
                if (unlockedItemsCount < shopButtons.Count)
                {
                    unlockedItemsCount++;
                    if (unlockedItemsCount - 1 >= initialItemsToIgnore && !isShopAnimating)
                    {
                        var newItemButton = shopButtons[unlockedItemsCount - 1];
                        StartCoroutine(AnimateScrollToShowItem(newItemButton.GetComponent<RectTransform>()));
                    }
                }
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
        while (timer < 1f)
        {
            timer += Time.deltaTime * animationScrollSpeed;
            shopContentRectTransform.anchoredPosition = Vector2.Lerp(startPosition, overshootPosition, timer);
            yield return null;
        }
        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * animationScrollSpeed * 1.5f;
            shopContentRectTransform.anchoredPosition = Vector2.Lerp(overshootPosition, targetPosition, timer);
            yield return null;
        }
        shopContentRectTransform.anchoredPosition = targetPosition;
        isShopAnimating = false;
        shopScrollRect.enabled = true;
    }

    public float GetSatietyPercentage()
    {
        if (maxSatiety == 0) return 0;
        return currentSatiety / maxSatiety;
    }

    private void ResetCatScale() { catImage.transform.localScale = Vector3.one; }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "K";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "M";
        if (number < 1_000_000_000_000) return (number / 1_000_000_000).ToString("F1") + "B";
        if (number < 1_000_000_000_000_000) return (number / 1_000_000_000_000).ToString("F1") + "T";
        return (number / 1_000_000_000_000_000).ToString("F1") + "Qa";
    }
}