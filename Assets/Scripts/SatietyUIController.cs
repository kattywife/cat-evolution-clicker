// SatietyUIController.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class SatietyUIController : MonoBehaviour
{
    // --- ������ �� ����������, ������� �� ���������� � ���������� ---

    [Header("�������� ������ �� �����")]
    public GameManager gameManager;
    public Image satietyProgressBar;    // ProgressBar_Fill
    public Button feedButton;           // ������ FeedButton
    public Button superFeedButton;      // ������ SuperFeedButton
    public TextMeshProUGUI satietyText; // SatietyPercentageText
    public TextMeshProUGUI feedCostText;  // PriceText ��� ������� FeedButton

    [Header("���������� ���������")]
    public Image bowlImage;      // ���� ���������� FeedButton
    public Image cloudImage;     // ���� ���������� IncomeCloudImage (������� �� �������)
    public Animator catAnimator; // ���� ���������� CatImage
    // --- ������ ��������� ---
    [Tooltip("������ �� �������� ����� ����� (��� ������� 1-20%)")]
    public GameObject tear1; // ���� ���������� ������ ������_1
    [Tooltip("������ �� �������� ���� ���� (��� ������� 0%)")]
    public GameObject tear3; // ���� ���������� ������ ������_3
    // --- ����� ��������� ---


    [Header("������� �����")]
    public Sprite bowlFullSprite;     // ���������� (100-21%)
    public Sprite bowlLowSprite;      // ������� (20-1%)
    public Sprite bowlEmptySprite;    // ������� (0%)

    [Header("������� �������")]
    public Sprite cloudNormalSprite;  // �������
    public Sprite cloudGreySprite;    // ����� (0%)

    [Header("��������� ���������")]
    public float pulseMagnitude = 1.1f; // ��������� ������������� (1.1 = 110%)
    public float pulseSpeed = 3f;       // �������� ���������

    // --- ��������� ���������� ��� ������ ������� ---

    private bool isPulsating = false;
    private Vector3 originalBowlScale;
    private double feedCost = 10;
    private float costIncreaseMultiplier = 1.15f;
    private float feedAmount = 50f;

    // --- ������ UNITY ---

    void Start()
    {
        feedButton.onClick.AddListener(OnFeedButtonClicked);
        superFeedButton.onClick.AddListener(OnSuperFeedButtonClicked);

        // ���������� ������������ ������ �����, ����� ��������� ���� ����������
        if (bowlImage != null)
        {
            originalBowlScale = bowlImage.transform.localScale;
        }
    }

    void Update()
    {
        if (gameManager == null) return;

        float satietyPercentage = gameManager.GetSatietyPercentage();
        satietyProgressBar.fillAmount = Mathf.Clamp01(satietyPercentage);
        satietyText.text = (satietyPercentage * 100).ToString("F0") + "%";
        feedButton.interactable = gameManager.score >= feedCost;
        if (feedCostText != null)
        {
            feedCostText.text = FormatNumber(feedCost);
        }

        // ������� �������, ������� ������ ��� � ����������� �� �������
        UpdateHungerEffects(satietyPercentage);
    }

    // --- ������ ����� ������� ---

    // --- ����������� ����� ---
    private void UpdateHungerEffects(float satietyPercentage)
    {
        // ���������, ��� ������ �� ���� �����������, ����� �������� ������
        if (tear1 == null || tear3 == null)
        {
            Debug.LogError("�� ������ ���������� ������� ������ � ���������� �� ������ SatietyUIController!");
            return;
        }

        // ������� 1: ������� 0% (������� �����, ����� ���, ������_3, ���������)
        if (satietyPercentage <= 0)
        {
            bowlImage.sprite = bowlEmptySprite;
            cloudImage.sprite = cloudGreySprite;

            // ���������� ������ �����, ������ ���������
            tear1.SetActive(false);
            tear3.SetActive(true);

            // ������������� ��������� "3 �����" ��� ������ ��������, ���� ��� ����
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 2);

            if (!isPulsating) StartPulsing();
        }
        // ������� 2: ������� 20% � ���� (������� �����, ������_1, ���������)
        else if (satietyPercentage <= 0.20f)
        {
            bowlImage.sprite = bowlLowSprite;
            cloudImage.sprite = cloudNormalSprite;

            // ���������� ������ �����, ������ ���������
            tear1.SetActive(true);
            tear3.SetActive(false);

            // ������������� ��������� "1 �����"
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 1);

            if (!isPulsating) StartPulsing();
        }
        // ������� 3: �Ѩ � ������� (���������� �����, ��� ����, �� ��������)
        else
        {
            bowlImage.sprite = bowlFullSprite;
            cloudImage.sprite = cloudNormalSprite;

            // ������ ��� �����
            tear1.SetActive(false);
            tear3.SetActive(false);

            // ������������� ��������� "�� ������"
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 0);

            if (isPulsating) StopPulsing();
        }
    }

    // --- ���������� ���������� ---

    void StartPulsing()
    {
        isPulsating = true;
        StartCoroutine(PulseEffect());
    }

    void StopPulsing()
    {
        isPulsating = false;
        StopAllCoroutines(); // �������� ���������� ��� �������� �� ���� �������
        if (bowlImage != null)
        {
            bowlImage.transform.localScale = originalBowlScale;
        }
    }

    private IEnumerator PulseEffect()
    {
        while (isPulsating) // �������� ������� ��� ������� ����������
        {
            float scale = originalBowlScale.x + Mathf.PingPong(Time.time * pulseSpeed, pulseMagnitude - originalBowlScale.x);
            if (bowlImage != null)
            {
                bowlImage.transform.localScale = new Vector3(scale, scale, scale);
            }
            yield return null;
        }
    }

    // --- ��������� ������ ---

    void OnFeedButtonClicked()
    {
        if (gameManager.score >= feedCost)
        {
            gameManager.FeedCat(feedCost, feedAmount);
            feedCost *= costIncreaseMultiplier;
        }
    }

    void OnSuperFeedButtonClicked()
    {
        gameManager.SuperFeedCat();
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1000000) return (number / 1000).ToString("F1") + "K";
        return (number / 1000000).ToString("F1") + "M";
    }
}