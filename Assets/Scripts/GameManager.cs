using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // --- ������ ���� (������) ---
    [Header("��������� �������")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("��������� ���������")]
    public List<UpgradeData> upgrades; // ������ ���� ��������� ���������

    // --- ���������� �������� ---
    [Header("������� ���������")]
    public double score = 0;
    public long scorePerClick = 1; // ��������� ���� �����
    public long scorePerSecond = 0; // ��������� ��������� �����

    // --- ������ �� UI (�������������) ---
    [Header("������ �� UI ��������")]
    public TextMeshProUGUI scoreText;
    public Image catImage;

    [Header("�������")]
    public GameObject upgradeButtonPrefab; // ���� ��������� ��� ������ ������
    public Transform shopPanel; // ������, ���� ����� ����������� ������

    void Start()
    {
        // ������������� ��������� ��������
        score = 0;
        currentLevelIndex = 0;
        ApplyLevelUp();
        UpdateScoreText();

        // ������� ������� ��� ������
        CreateShop();
    }

    void Update()
    {
        // ��������� �����
        score += scorePerSecond * Time.deltaTime;
        UpdateScoreText();
        // ����� ����� ����� �������� ������ ���������� ��������� ������ (�����/�������)
    }

    // --- �������� ������ ---

    public void OnCatClicked()
    {
        score += scorePerClick; // ���������� ����������
        // ... ��������� ��� �����
        UpdateScoreText();
        CheckForLevelUp();
        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
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

            // �������� ������, ��� ������� ���������
            button.OnPurchaseSuccess();
        }
    }

    // --- ������-��������� ---

    private void CreateShop()
    {
        foreach (var upgrade in upgrades)
        {
            // ������� ����� �������
            GameObject newButton = Instantiate(upgradeButtonPrefab, shopPanel);
            // �������� �� ������ � �����������
            newButton.GetComponent<UpgradeButtonUI>().Setup(upgrade, this); // �������� ������ � ������ �� ����
        }
    }

    private void CheckForLevelUp()
    {
        if (currentLevelIndex + 1 >= levels.Count) return;
        if (score >= levels[currentLevelIndex + 1].scoreToReach)
        {
            currentLevelIndex++;
            ApplyLevelUp();
        }
    }

    private void ApplyLevelUp()
    {
        catImage.sprite = levels[currentLevelIndex].catSprite;
    }

    private void ResetCatScale()
    {
        catImage.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("F0");
        }
    }
}