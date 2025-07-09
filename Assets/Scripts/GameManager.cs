using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // --- ������ ���� (������) ---
    [Header("��������� �������")]
    public List<LevelData> levels; // ������ ���� ����� ������� (��������� ���� ������)
    private int currentLevelIndex = 0;

    // --- ���������� �������� ---
    [Header("������� ���������")]
    public double score = 0;

    // --- ������ �� UI (�������������) ---
    [Header("������ �� UI ��������")]
    public TextMeshProUGUI scoreText;
    public Image catImage; // ���������: ������ ������ �� Image, � �� Transform

    // Start ���������� ���� ��� ��� ������� ����
    void Start()
    {
        // ������������� ��������� ��������
        score = 0;
        currentLevelIndex = 0;
        ApplyLevelUp(); // ��������� ������ ������ ������� ������
        UpdateScoreText();
    }

    public void OnCatClicked()
    {
        score++;
        UpdateScoreText();
        CheckForLevelUp();

        // �������� �����
        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    private void CheckForLevelUp()
    {
        // ���������, �� ��������� �� ��� �������
        if (currentLevelIndex + 1 >= levels.Count)
        {
            return; // �� ��� �� ������������ ������, �������
        }

        // ���������, ������� �� �� ���������� ����� ��� ���������� ������
        if (score >= levels[currentLevelIndex + 1].scoreToReach)
        {
            currentLevelIndex++; // �������� �������
            ApplyLevelUp(); // ��������� ���������
        }
    }

    private void ApplyLevelUp()
    {
        // �������� ������ �������� ������
        LevelData currentLevel = levels[currentLevelIndex];

        // ������ ������ ������
        catImage.sprite = currentLevel.catSprite;
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