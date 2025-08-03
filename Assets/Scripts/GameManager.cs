using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // --- ������ ���� ---
    [Header("��������� �������")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("��������� ���������")]
    public List<UpgradeData> upgrades;

    [Header("�����")]
    public AudioClip catClickSound;
    [Tooltip("���� ��� �������� �� ����� �������")]
    public AudioClip levelUpSound;

    // --- ���������� �������� ---
    [Header("������� ���������")]
    public double score = 0;
    public double scorePerClick = 1;
    public double scorePerSecond = 0;

    [Header("��������� �������")]
    public float maxSatiety = 100f;
    public float currentSatiety;
    [Tooltip("������� ������ ������� ����� ������ � �������")]
    public float satietyDepletionRate = 0.5f;
    [Tooltip("��������� ������, ����� ����� ������� (0.1 = 10%)")]
    public float satietyPenaltyMultiplier = 0.1f;

    // --- ������ �� UI ---
    [Header("������ �� UI ��������")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI perSecondText;
    public Image catImage;
    public Slider levelProgressBar;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI progressText;
    [Tooltip("���� ����� ���������� ������, ������� �������� ��� UI")]
    public Camera uiCamera;

    [Header("�������")]
    public ParticleSystem levelUpEffect;
    public GameObject clickTextPrefab;
    public Transform canvasTransform;

    [Header("�������")]
    public GameObject upgradeButtonPrefab;
    public Transform shopContentParent;
    public ScrollRect shopScrollRect;

    [Header("��������� �������� ��������")]
    public float animationScrollSpeed = 3f;
    public float animationBounceAmount = 50f;
    public int initialItemsToIgnore = 4;

    // --- ��������� ���������� ---
    private RectTransform shopContentRectTransform;
    private List<UpgradeButtonUI> shopButtons = new List<UpgradeButtonUI>();
    private bool isShopAnimating = false;

    // �����������, ������� ������� ��������������
    private int unlockedItemsCount = 1; // �������� � 1, ����� ������ ����� ��� ��������

    // --- ��������� ---
    private double clickMultiplier = 1.0;
    private double passiveMultiplier = 1.0;


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
        UpdateAllShopButtonsState(); // �������������� ��������� ������
        ApplyLevelUp();
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
        }

        if (effectiveSps > 0)
        {
            score += effectiveSps * Time.deltaTime;
        }

        // ��������� ������ ��������� "������� �� �����" ��� ��� ���������������� ������
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

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (score >= cost)
        {
            score -= cost;

            switch (upgrade.type)
            {
                case UpgradeType.PerClick:
                    scorePerClick += upgrade.power;
                    break;
                case UpgradeType.PerSecond:
                    scorePerSecond += upgrade.power;
                    break;
                case UpgradeType.ClickMultiplier:
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

            int purchasedIndex = shopButtons.IndexOf(button);

            // ���� �� ������ ��������� �� ��������� �������, ��������� ���������
            if (purchasedIndex == unlockedItemsCount - 1)
            {
                if (unlockedItemsCount < shopButtons.Count)
                {
                    unlockedItemsCount++;

                    // ��������� �������� ��� ������ ������, ���� �� �� ������ � ����� ������������
                    if (unlockedItemsCount - 1 >= initialItemsToIgnore && !isShopAnimating)
                    {
                        var newItemButton = shopButtons[unlockedItemsCount - 1];
                        StartCoroutine(AnimateScrollToShowItem(newItemButton.GetComponent<RectTransform>()));
                    }
                }
            }

            button.OnPurchaseSuccess();
            // ����� ����� ������� ��������� ��������� ���� ������ (����� � �����������)
            UpdateAllShopButtonsState();
        }
    }

    private void UpdateAllShopButtonsState()
    {
        for (int i = 0; i < shopButtons.Count; i++)
        {
            if (shopButtons[i] == null)
            {
                continue;
            }

            // ����� �������������, ���� ��� ������ ������ �������� ����������������
            bool isUnlocked = (i < unlockedItemsCount);

            shopButtons[i].SetLockedState(!isUnlocked);

            // ���� �� �������������, ������������� ���������, ������� �� �����
            if (isUnlocked)
            {
                shopButtons[i].UpdateInteractableState(score);
            }
        }
    }

    private IEnumerator AnimateScrollToShowItem(RectTransform targetItem)
    {
        isShopAnimating = true;
        shopScrollRect.enabled = false;
        Canvas.ForceUpdateCanvases();

        Vector2 startPosition = shopContentRectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, -targetItem.anchoredPosition.y);
        Vector2 overshootPosition = targetPosition + new Vector2(0, animationBounceAmount);

        // ���� "�������" �� ������� ���� ������ ����
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * animationScrollSpeed;
            shopContentRectTransform.anchoredPosition = Vector2.Lerp(startPosition, overshootPosition, timer);
            yield return null;
        }

        // ���� "��������" �� ������� �������
        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * animationScrollSpeed * 1.5f; // ������������ ���� �������
            shopContentRectTransform.anchoredPosition = Vector2.Lerp(overshootPosition, targetPosition, timer);
            yield return null;
        }

        // ��������� �� ����� � ��������� ��������
        shopContentRectTransform.anchoredPosition = targetPosition;
        isShopAnimating = false;
        shopScrollRect.enabled = true;
    }

    public void OnCatClicked(BaseEventData baseData)
    {
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
            perSecondText.text = $"{FormatNumber(scorePerSecond * passiveMultiplier)}/���";
        }
    }

    private void UpdateProgressBar()
    {
        if (levelProgressBar == null) return;

        if (currentLevelIndex >= levels.Count - 1 && levels.Count > 1)
        {
            levelProgressBar.value = 1;
            if (levelNumberText != null) levelNumberText.text = $"�������: {currentLevelIndex + 1}";
            if (progressText != null) progressText.text = "����.";
            return;
        }

        double barEndValue = levels[currentLevelIndex + 1].scoreToReach;
        levelProgressBar.minValue = 0f;
        levelProgressBar.maxValue = (float)barEndValue;
        levelProgressBar.value = (float)score;
        if (levelNumberText != null) levelNumberText.text = $"�������: {currentLevelIndex + 1}";
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