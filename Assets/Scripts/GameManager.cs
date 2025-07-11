using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    // --- ������� ---
    public event Action OnScoreChanged;

    // --- ������ ���� ---
    [Header("��������� �������")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("��������� ���������")]
    public List<UpgradeData> upgrades;

    // --- ���������� �������� ---
    [Header("������� ���������")]
    private double _score = 0;
    public double score
    {
        get { return _score; }
        private set
        {
            if (_score != value)
            {
                _score = value;
                OnScoreChanged?.Invoke();
            }
        }
    }
    public long scorePerClick = 1;
    public long scorePerSecond = 0;

    // --- ������ �� UI ---
    [Header("������ �� UI ��������")]
    public TextMeshProUGUI oldScoreText; // ������ �����, ������� �� ������. ����� �������, ���� �� �����.
    public Image catImage;
    public Slider levelProgressBar;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI totalScoreText;  // ����� �� ������� ��� ������ �����
    public TextMeshProUGUI perSecondText;   // ����� �� ������� ��� ���������� ������

    [Header("�������")]
    public GameObject upgradeButtonPrefab;
    public Transform shopPanel;

    // --- �������� ������ UNITY ---
    void Start()
    {
        currentLevelIndex = 0;
        scorePerClick = 1;
        scorePerSecond = 0;

        ApplyLevelUp();
        CreateShop();
        score = 0;
        UpdateAllUITexts(); // �������������� ���������� ���� �������
        UpdateProgressBar();
    }

    void Update()
    {
        if (scorePerSecond > 0)
        {
            _score += scorePerSecond * Time.deltaTime;
            UpdateAllUITexts(); // ��������� ������ ���������
            UpdateProgressBar();
        }
    }

    // --- ��������� ������ ---
    public void OnCatClicked()
    {
        score += scorePerClick;
        UpdateAllUITexts();
        CheckForLevelUp();
        UpdateProgressBar();

        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (score >= cost)
        {
            score -= cost;

            if (upgrade.type == UpgradeType.PerClick)
                scorePerClick += upgrade.power;
            else if (upgrade.type == UpgradeType.PerSecond)
                scorePerSecond += upgrade.power;

            UpdateAllUITexts(); // ����� �������� ����� ��������� scorePerSecond
            UpdateProgressBar();
            button.OnPurchaseSuccess();
        }
    }

    // --- ��������� ������-��������� ---
    private void UpdateAllUITexts()
    {
        // ��������� ������ ������� (���� �� ����)
        if (oldScoreText != null)
        {
            oldScoreText.text = _score.ToString("F0");
        }

        // ��������� ����� ������� �� ������
        if (totalScoreText != null)
        {
            totalScoreText.text = FormatNumber(_score);
        }

        // ��������� ����� ���������� ������
        if (perSecondText != null)
        {
            perSecondText.text = $"{FormatNumber(scorePerSecond)}/���";
        }
    }

    private void UpdateProgressBar()
    {
        if (levelProgressBar == null) return;

        double currentLevelScore = levels[currentLevelIndex].scoreToReach;
        double nextLevelScore = (currentLevelIndex + 1 < levels.Count) ? levels[currentLevelIndex + 1].scoreToReach : currentLevelScore;

        levelProgressBar.minValue = (float)currentLevelScore;
        levelProgressBar.maxValue = (float)nextLevelScore;
        levelProgressBar.value = (float)score;

        if (levelNumberText != null)
            levelNumberText.text = $"�������: {currentLevelIndex + 1}";

        if (progressText != null)
        {
            if (levelProgressBar.value >= levelProgressBar.maxValue && currentLevelIndex + 1 >= levels.Count)
                progressText.text = "����.";
            else
                progressText.text = $"{score.ToString("F0")} / {nextLevelScore.ToString("F0")}";
        }
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "K";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "M";
        return number.ToString("G3");
    }

    private void CreateShop() { foreach (var upgrade in upgrades) { GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopPanel); newButtonGO.GetComponent<UpgradeButtonUI>().Setup(upgrade, this); } }
    private void CheckForLevelUp() { if (currentLevelIndex + 1 < levels.Count && score >= levels[currentLevelIndex + 1].scoreToReach) { currentLevelIndex++; ApplyLevelUp(); UpdateProgressBar(); } }
    private void ApplyLevelUp() { if (levels.Count > 0) catImage.sprite = levels[currentLevelIndex].catSprite; }
    private void ResetCatScale() { catImage.transform.localScale = new Vector3(1f, 1f, 1f); }
}