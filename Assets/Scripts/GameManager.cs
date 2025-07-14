// GameManager.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;


public class GameManager : MonoBehaviour
{
    // --- ������ ���� ---
    [Header("��������� �������")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("��������� ���������")]
    public List<UpgradeData> upgrades;

    // --- ���������� �������� ---
    [Header("������� ���������")]
    public double score = 0;
    public long scorePerClick = 1;
    public long scorePerSecond = 0;

    // <<< ���������: ��������� ���������� ��� ������� ������� >>>
    [Header("��������� �������")]
    public float maxSatiety = 100f; // ������������ �������� ������� (100%)
    public float currentSatiety; // ������� �������
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

    [Header("�������")]
    public ParticleSystem levelUpEffect;

    [Header("�������")]
    public GameObject upgradeButtonPrefab;
    public Transform shopPanel;
    private List<UpgradeButtonUI> shopButtons = new List<UpgradeButtonUI>();

    // --- �������� ������ UNITY ---

    void Start()
    {
        currentLevelIndex = 0;
        scorePerClick = 1;
        scorePerSecond = 0;
        score = 0;

        // <<< ���������: ������������� ������� ��� ������ >>>
        currentSatiety = maxSatiety;

        CreateShop();
        ApplyLevelUp();
    }

    void Update()
    {
        // <<< ���������: ��������� �������������� ������ ���������� ���������� ������ >>>
        // 1. ��������� ������� �� ��������
        if (currentSatiety > 0)
        {
            currentSatiety -= satietyDepletionRate * Time.deltaTime;
        }
        else
        {
            currentSatiety = 0; // �� ���� ���� � �����
        }

        // 2. ������������ ����������� ����� � �������
        double effectiveSps = scorePerSecond;
        if (currentSatiety <= 0)
        {
            effectiveSps *= satietyPenaltyMultiplier; // ��������� �����, ���� �������
        }

        // 3. ��������� ����
        if (effectiveSps > 0)
        {
            score += effectiveSps * Time.deltaTime;
        }

        // 4. ��������� ���� UI, ������� ����� ��������, ������� ����������� �� ������ ��������
        UpdateAllUI();
    }

    // --- ��������� ������ ---

    public void OnCatClicked()
    {
        score += scorePerClick;
        CheckForLevelUp();

        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        Debug.Log($"������� ������� '{upgrade.name}'. " +
                  $"������� ��������� �����: {scorePerSecond}. " +
                  $"���� ��������� (power): {upgrade.power}. " +
                  $"���������: {cost}");

        if (score >= cost)
        {
            score -= cost;

            if (upgrade.type == UpgradeType.PerClick)
            {
                scorePerClick += upgrade.power;
            }
            else if (upgrade.type == UpgradeType.PerSecond)
            {
                scorePerSecond += upgrade.power;
            }

            button.OnPurchaseSuccess();
        }
    }

    // <<< ���������: ��������� ����� ��������� ������ ��� ���������� �������� >>>
    public void FeedCat(double cost, float amount)
    {
        if (score >= cost)
        {
            score -= cost;

            // ����������, ���� �� ������� ��� ���� 100%
            bool wasAlreadySuperFed = currentSatiety > maxSatiety;

            currentSatiety += amount;

            // ������������ �� 100% ������ ���� ������� �� ���� "�����" �� �����
            if (!wasAlreadySuperFed && currentSatiety > maxSatiety)
            {
                currentSatiety = maxSatiety;
            }

            Debug.Log("����� ���������. ������� �������: " + currentSatiety);
        }
    }

    public void SuperFeedCat()
    {
        // ���� ����� ����� ������ ����� ��������� ��������� �������
        // ���� ��� �� ���������� �������� �� �������� � SatietyUIController
        currentSatiety = maxSatiety * 2.0f; // ���������� �� 200%
        Debug.Log("����� ������� �����-����! ������� �������: " + currentSatiety);
    }

    // ��������������� ����� ��� UI
    public float GetSatietyPercentage()
    {
        if (maxSatiety == 0) return 0; // ������ �� ������� �� ����
        return currentSatiety / maxSatiety;
    }


    // --- ��������� ������-��������� ---

    private void UpdateAllUI()
    {
        UpdateAllUITexts();
        UpdateProgressBar();
        UpdateAllShopButtons();
    }

    private void UpdateAllUITexts()
    {
        if (totalScoreText != null) totalScoreText.text = FormatNumber(score);
        // <<< ���������: ���������� ������� �����, � �� �����������, ����� ����� �����, �� ��� �� ������ >>>
        if (perSecondText != null) perSecondText.text = $"{FormatNumber(scorePerSecond)}/���";
    }

    private void UpdateProgressBar()
    {
        if (levelProgressBar == null) return;

        if (currentLevelIndex >= levels.Count - 1 && levels.Count > 1)
        {
            levelProgressBar.minValue = 0;
            levelProgressBar.maxValue = 1;
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
        if (progressText != null)
        {
            progressText.text = $"{FormatNumber(score)} / {FormatNumber(barEndValue)}";
        }
    }

    private void UpdateAllShopButtons()
    {
        foreach (var button in shopButtons)
        {
            button.UpdateInteractableState(score);
        }
    }

    private void CreateShop()
    {
        foreach (var upgrade in upgrades)
        {
            GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopPanel);
            UpgradeButtonUI buttonUI = newButtonGO.GetComponent<UpgradeButtonUI>();
            buttonUI.Setup(upgrade, this);
            shopButtons.Add(buttonUI);
        }
    }

    private void CheckForLevelUp()
    {
        while (currentLevelIndex + 1 < levels.Count && score >= levels[currentLevelIndex + 1].scoreToReach)
        {
            currentLevelIndex++;
            ApplyLevelUp();
        }
    }

    private void ApplyLevelUp()
    {
        if (levels.Count > 0 && currentLevelIndex < levels.Count)
        {
            catImage.sprite = levels[currentLevelIndex].catSprite;
        }

        if (levelUpEffect != null)
        {
            levelUpEffect.Play();
        }

        UpdateAllUI();
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "K";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "M";
        return (number / 1_000_000_000).ToString("F1") + "B";
    }

    private void ResetCatScale()
    {
        catImage.transform.localScale = new Vector3(1f, 1f, 1f);
    }
}