using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System; // ����������� ��� ������ � Action (���������)

public class GameManager : MonoBehaviour
{
    // --- ������� ---
    // ��� ������� ����� ���������� ������ ���, ����� �������� ����.
    public event Action OnScoreChanged;

    // --- ������ ���� (������) ---
    [Header("��������� �������")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("��������� ���������")]
    public List<UpgradeData> upgrades;

    // --- ���������� �������� ---
    [Header("������� ���������")]
    private double _score = 0; // ��������� ���������� ��� �������� �����
    public double score // ��������� "��������" ��� ������� � �����
    {
        get { return _score; } // ����� ���-�� ������ score, �� �������� �������� _score
        private set // ����� ���-�� �������� �������� � score, ����������� ���� ���
        {
            _score = value;
            OnScoreChanged?.Invoke(); // �������� �������, ����� ���������� ���� �����������
        }
    }
    public long scorePerClick = 1;
    public long scorePerSecond = 0;

    // --- ������ �� UI (�������������) ---
    [Header("������ �� UI ��������")]
    public TextMeshProUGUI scoreText;
    public Image catImage;

    [Header("�������")]
    public GameObject upgradeButtonPrefab;
    public Transform shopPanel;

    void Start()
    {
        currentLevelIndex = 0;
        ApplyLevelUp();

        CreateShop();
        score = 0; // ������������� ���� � 0. ��� ������� ������� � ������� ������.
        UpdateScoreText();
    }

    void Update()
    {
        if (scorePerSecond > 0)
        {
            // ��������� ��������� �����
            // �����: ����� �� ���������� � _score ��������, ����� �� �������� ������� ������ ����
            _score += scorePerSecond * Time.deltaTime;
            UpdateScoreText(); // �� ����� �� ������ ����� ��������� ���������
        }
    }

    public void OnCatClicked()
    {
        score += scorePerClick; // ����������� ���� (������� �������)
        UpdateScoreText();
        CheckForLevelUp();

        // �������� �����
        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (score >= cost)
        {
            score -= cost; // ��������� ���� (������� �������)
            UpdateScoreText();

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

    private void CreateShop()
    {
        foreach (var upgrade in upgrades)
        {
            GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopPanel);
            newButtonGO.GetComponent<UpgradeButtonUI>().Setup(upgrade, this);
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = _score.ToString("F0");
        }
    }

    // --- ��������� ������-��������� ��� ��������� ---
    private void CheckForLevelUp() { if (currentLevelIndex + 1 < levels.Count && score >= levels[currentLevelIndex + 1].scoreToReach) { currentLevelIndex++; ApplyLevelUp(); } }
    private void ApplyLevelUp() { if (levels.Count > 0) catImage.sprite = levels[currentLevelIndex].catSprite; }
    private void ResetCatScale() { catImage.transform.localScale = new Vector3(1f, 1f, 1f); }
}