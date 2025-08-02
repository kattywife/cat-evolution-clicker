// UpgradeButtonUI.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class UpgradeButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI priceText;
    public Image iconImage;
    public Button purchaseButton;

    [Header("��������� �������� ���������")]
    [Tooltip("��������� ����������� ������ ��� ���������. 1.05 = 5%")]
    public float scaleFactor = 1.05f;
    [Tooltip("�� ������� ������ ������ ��������� ��������")]
    public float animationDuration = 0.1f;

    // --- �����: ��������� ���� ��� ������ ---
    [Header("�����")]
    [Tooltip("���� ��� ��������� ������� �� ������")]
    public AudioClip hoverSound;
    [Tooltip("���� ��� �������� ������� ���������")]
    public AudioClip purchaseSound;


    // ��������� ���������� ��� �������� ��������� ������
    private UpgradeData currentUpgradeData;
    private int currentLevel = 0;
    private double currentCost = 0;
    private GameManager gameManager;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Setup(UpgradeData data, GameManager manager)
    {
        currentUpgradeData = data;
        gameManager = manager;
        currentLevel = 0;
        currentCost = currentUpgradeData.baseCost;

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchaseClicked);

        UpdateTextAndIcons();
    }

    public void OnPurchaseClicked()
    {
        if (gameManager.score >= currentCost)
        {
            gameManager.PurchaseUpgrade(currentUpgradeData, currentCost, this);
        }
    }

    public void OnPurchaseSuccess()
    {
        // --- ��������: ����������� ���� �������� ������� ---
        AudioManager.Instance.PlaySound(purchaseSound);

        currentLevel++;
        currentCost *= currentUpgradeData.costMultiplier;

        UpdateTextAndIcons();
    }

    public void UpdateInteractableState(double currentScore)
    {
        purchaseButton.interactable = currentScore >= currentCost;
    }

    // <--- ��� ����� �����, ������� �� �������� --- >
    /// <summary>
    /// ����������, �������� �� ������ ��� ��������������.
    /// </summary>
    /// <returns>true, ���� ������ �������, ����� false.</returns>
    public bool IsInteractable()
    {
        // ������ ���������� ������� ��������� interactable � ����� �������� ������
        if (purchaseButton != null)
        {
            return purchaseButton.interactable;
        }
        return false;
    }

    public void UpdateTextAndIcons()
    {
        nameText.text = currentUpgradeData.upgradeName;
        iconImage.sprite = currentUpgradeData.icon;
        priceText.text = FormatNumber(currentCost);

        if (currentUpgradeData.type == UpgradeType.PerClick)
        {
            effectText.text = $"+{currentUpgradeData.power} �� ����";
        }
        else if (currentUpgradeData.type == UpgradeType.PerSecond)
        {
            effectText.text = $"+{currentUpgradeData.power} � �������";
        }
    }

    // --- ����������� ��������� ���� ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (purchaseButton.interactable)
        {
            // --- ��������: ����������� ���� ��������� ---
            AudioManager.Instance.PlaySound(hoverSound);

            StopAllCoroutines();
            StartCoroutine(ScaleOverTime(originalScale * scaleFactor, animationDuration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleOverTime(originalScale, animationDuration));
    }

    private IEnumerator ScaleOverTime(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1000000) return (number / 1000).ToString("F1") + "K";
        if (number < 1000000000) return (number / 1000000).ToString("F1") + "M";
        return (number / 1000000000).ToString("F1") + "B";
    }
}