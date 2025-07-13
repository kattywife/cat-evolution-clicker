using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SatietyUIController : MonoBehaviour
{
    [Header("������ �� ����������")]
    public GameManager gameManager;
    public Image satietyProgressBar; // ���� �������� ProgressBar_Fill
    public Button feedButton;
    public Button superFeedButton; // ������ ��� �������
    public TextMeshProUGUI satietyText; // ����� ��������� ������


    [Header("��������� ���������")]
    public double feedCost = 10;
    public float feedAmount = 50f; // ������� ������� ��������������� ������� ���

    void Start()
    {
        // ��������� �������� ��� ������ ����� ���
        feedButton.onClick.AddListener(OnFeedButtonClicked);
        superFeedButton.onClick.AddListener(OnSuperFeedButtonClicked);
    }

    void Update()
    {
        if (gameManager != null)
        {
            float fillPercentage = gameManager.GetSatietyPercentage();

            // ��������� ��������-���
            satietyProgressBar.fillAmount = Mathf.Clamp01(fillPercentage);

            // <<< ��� ����� ������ >>>
            // �������� �� 100, ����� �������� ��������, � ����������� ��� ����� �����
            satietyText.text = (fillPercentage * 100).ToString("F0") + "%";
        }

        // ��������/��������� ������
        feedButton.interactable = gameManager.score >= feedCost;
    }

    void OnFeedButtonClicked()
    {
        gameManager.FeedCat(feedCost, feedAmount);
    }

    void OnSuperFeedButtonClicked()
    {
        // TODO: ����� ����� ������ ������ �������
        Debug.Log("������ ������ '����� ����'. ����� �������� �������.");

        // ��������� �������� ��� �����: ����� ���� �������
        gameManager.SuperFeedCat();
    }
}