using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // --- ���������� ��� �������� ---
    public double score = 0;

    // --- ������ �� ������� �� ����� ---
    public TextMeshProUGUI scoreText; // ���� ������������� ����� ��� �����
    public Transform catTransform;    // ���� ������������� ������

    // ���� ����� ���������� ��� ����� �� ������
    public void OnCatClicked()
    {
        // ��������� �������� ��������� � �������, ����� ���������, ��� ���� ��������
        Debug.Log("���� �� ������! ������� ����: " + score);

        // ����������� ���� �� 1
        score = score + 1;

        // ��������� ����� �� ������
        UpdateScoreText();

        // �������� �����: ������� ����������� ������
        catTransform.localScale = new Vector3(1.1f, 1.1f, 1f); // ���� ������ ��������� �������

        // ����� 0.1 ������� �������� �����, ����� ������� ������ �������
        Invoke("ResetCatScale", 0.1f);
    }

    // ���� ����� ���������� ������ � ��� �������� �������
    private void ResetCatScale()
    {
        // �������, ��� ����� ������ ���� �������� ������ ������!
        // ���� �� ��� ������, �������� ���� ��������. 5.3316 - ��� � ������ ���������.
        catTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    // ���� ����� ��������� ����� ����� �� ������
    private void UpdateScoreText()
    {
        // ���������, ��� ������ �� ����� �� ������, ����� �������� ������
        if (scoreText != null)
        {
            scoreText.text = score.ToString("F0"); // "F0" �������� �������� ����� ��� �������
        }
    }
}