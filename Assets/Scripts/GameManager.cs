using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // --- ДАННЫЕ ИГРЫ ---
    [Header("Настройки уровней")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("Настройки улучшений")]
    public List<UpgradeData> upgrades;

    [Header("Звуки")]
    public AudioClip catClickSound;
    [Tooltip("Звук при переходе на новый уровень")]
    public AudioClip levelUpSound;

    // --- ПЕРЕМЕННЫЕ ГЕЙМПЛЕЯ ---
    [Header("Текущее состояние")]
    public double score = 0;
    public double scorePerClick = 1;      // Используем double для гибкости
    public double scorePerSecond = 0;     // Используем double для гибкости

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

    [Header("Эффекты")]
    public ParticleSystem levelUpEffect;
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
    private RectTransform shopContentRectTransform;
    private List<UpgradeButtonUI> shopButtons = new List<UpgradeButtonUI>();
    private bool isShopAnimating = false;
    private bool hasShownNewItemAnimation = false;

    // --- МНОЖИТЕЛИ ---
    private double clickMultiplier = 1.0;   // 1.0 = 100% (без бонуса)
    private double passiveMultiplier = 1.0; // 1.0 = 100% (без бонуса)


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

        CreateShop();
        ApplyLevelUp();
    }

    void Update()
    {
        // Сначала вычисляем итоговый пассивный доход с учетом множителя
        double finalScorePerSecond = scorePerSecond * passiveMultiplier;

        if (currentSatiety > 0)
        {
            currentSatiety -= satietyDepletionRate * Time.deltaTime;
        }
        else
        {
            currentSatiety = 0;
        }

        // Применяем штраф за голод, если он есть
        double effectiveSps = finalScorePerSecond;
        if (currentSatiety <= 0)
        {
            effectiveSps *= satietyPenaltyMultiplier;
        }

        // Начисляем очки
        if (effectiveSps > 0)
        {
            score += effectiveSps * Time.deltaTime;
        }

        // Обновляем UI
        UpdateAllShopButtonsState();
        CheckForShopAnimation();
        UpdateAllUITexts();
        UpdateProgressBar();
    }

    private void UpdateAllShopButtonsState()
    {
        foreach (var button in shopButtons)
        {
            if (button != null)
            {
                button.UpdateInteractableState(score);
            }
        }
    }

    private void CheckForShopAnimation()
    {
        if (isShopAnimating || hasShownNewItemAnimation)
        {
            return;
        }

        for (int i = 0; i < shopButtons.Count; i++)
        {
            UpgradeButtonUI button = shopButtons[i];
            if (button == null)
            {
                continue;
            }

            if (button.IsInteractable())
            {
                if (i >= initialItemsToIgnore && !IsItemVisible(button.GetComponent<RectTransform>()))
                {
                    StartCoroutine(AnimateScrollToShowItem(button.GetComponent<RectTransform>()));
                    hasShownNewItemAnimation = true;
                    return;
                }
            }
        }
    }

    public void OnCatClicked(BaseEventData baseData)
    {
        PointerEventData eventData = baseData as PointerEventData;
        if (eventData == null)
        {
            return;
        }

        AudioManager.Instance.PlaySound(catClickSound);

        // Вычисляем итоговый доход от клика с учетом множителя
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

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (score >= cost)
        {
            score -= cost;

            // Используем switch для обработки разных типов улучшений
            switch (upgrade.type)
            {
                case UpgradeType.PerClick:
                    scorePerClick += upgrade.power;
                    break;

                case UpgradeType.PerSecond:
                    scorePerSecond += upgrade.power;
                    break;

                case UpgradeType.ClickMultiplier:
                    // Прибавляем процент к множителю. Например: 1.0 + 0.15 = 1.15 (т.е. +15%)
                    clickMultiplier += upgrade.power;
                    break;

                case UpgradeType.PassiveMultiplier:
                    passiveMultiplier += upgrade.power;
                    break;

                case UpgradeType.GlobalMultiplier:
                    clickMultiplier += upgrade.power;
                    passiveMultiplier += upgrade.power;
                    break;
            }

            button.OnPurchaseSuccess();
        }
    }

    public void FeedCat(double cost, float amount)
    {
        if (score >= cost)
        {
            score -= cost;
            bool wasAlreadySuperFed = currentSatiety > maxSatiety;
            currentSatiety += amount;
            if (!wasAlreadySuperFed && currentSatiety > maxSatiety)
            {
                currentSatiety = maxSatiety;
            }
        }
    }

    public void SuperFeedCat()
    {
        currentSatiety = maxSatiety * 2.0f;
    }

    public float GetSatietyPercentage()
    {
        if (maxSatiety == 0)
        {
            return 0;
        }
        return currentSatiety / maxSatiety;
    }

    private void UpdateAllUITexts()
    {
        if (totalScoreText != null)
        {
            totalScoreText.text = FormatNumber(score);
        }
        if (perSecondText != null)
        {
            perSecondText.text = $"{FormatNumber(scorePerSecond * passiveMultiplier)}/сек";
        }
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

    private bool IsItemVisible(RectTransform item)
    {
        Vector3[] viewportCorners = new Vector3[4];
        shopScrollRect.viewport.GetWorldCorners(viewportCorners);
        Vector3 viewportBottomLeft = viewportCorners[0];
        Vector3 viewportTopRight = viewportCorners[2];
        Vector3[] itemCorners = new Vector3[4];
        item.GetWorldCorners(itemCorners);
        Vector3 itemBottomLeft = itemCorners[0];
        Vector3 itemTopRight = itemCorners[2];
        return itemTopRight.y < viewportTopRight.y && itemBottomLeft.y > viewportBottomLeft.y;
    }

    private IEnumerator AnimateScrollToShowItem(RectTransform targetItem)
    {
        isShopAnimating = true;
        shopScrollRect.enabled = false;
        Canvas.ForceUpdateCanvases();
        Vector3[] viewportCorners = new Vector3[4];
        shopScrollRect.viewport.GetWorldCorners(viewportCorners);
        float viewportBottomY = viewportCorners[0].y;
        Vector3[] itemCorners = new Vector3[4];
        targetItem.GetWorldCorners(itemCorners);
        float itemTopY = itemCorners[2].y;
        float scrollDistance = viewportBottomY - itemTopY;
        scrollDistance += 20f;
        Vector2 startPosition = shopContentRectTransform.anchoredPosition;
        Vector2 targetPosition = startPosition + new Vector2(0, scrollDistance);
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

    private void CheckForLevelUp()
    {
        if (currentLevelIndex + 1 < levels.Count)
        {
            if (score >= levels[currentLevelIndex + 1].scoreToReach)
            {
                currentLevelIndex++;
                ApplyLevelUp();
            }
        }
    }

    private void ApplyLevelUp()
    {
        AudioManager.Instance.PlaySound(levelUpSound, 0.8f);

        if (levels.Count > 0 && currentLevelIndex < levels.Count)
        {
            catImage.sprite = levels[currentLevelIndex].catSprite;
            catImage.SetNativeSize();
        }

        if (levelUpEffect != null)
        {
            levelUpEffect.Play();
        }
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "K";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "M";
        if (number < 1_000_000_000_000) return (number / 1_000_000_000).ToString("F1") + "B";
        if (number < 1_000_000_000_000_000) return (number / 1_000_000_000_000).ToString("F1") + "T";
        return (number / 1_000_000_000_000_000).ToString("F1") + "Qa";
    }

    private void ResetCatScale()
    {
        catImage.transform.localScale = Vector3.one;
    }
}