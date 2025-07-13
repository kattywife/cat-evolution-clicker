// UpgradeButttonUI
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


// ������� IPointerClickHandler �� ������ �����������, ����� �������� �������� �����
public class UpgradeButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI priceText;
    public Image iconImage;
    public Button purchaseButton;



    // ��������� ���������� ��� �������� ��������� ������
    private UpgradeData currentUpgradeData;
    private int currentLevel = 0;
    private double currentCost = 0;
    private GameManager gameManager;
    private Vector3 originalScale;

    void Awake()
    {
        // ���������� ������������ ������, ����� ������������ ��� ��� �������� ���������
        originalScale = transform.localScale;
    }

    // ����� ��� �������������� ��������� ������ ��� �������� ��������
    public void Setup(UpgradeData data, GameManager manager)
    {
        currentUpgradeData = data;
        gameManager = manager;
        currentLevel = 0;
        currentCost = currentUpgradeData.baseCost;

        // ��������� ���������� ����� ������ ����� ��������� Button
        // ������� ������� ������ �������� �� ������ ������
        purchaseButton.onClick.RemoveAllListeners();
        // ��������� ����� ����� ������ ������
        purchaseButton.onClick.AddListener(OnPurchaseClicked);

        // ��������� ���� ����� �� ������
        UpdateTextAndIcons();
    }

    // ���� ����� ����������, ����� ����� �������� �� ������
    public void OnPurchaseClicked()
    {
        // GameManager ��� ��������, ������� �� �����,
        // �� ��� ���������� ����� � ����� ���������, ������ ��� �������� �����
        if (gameManager.score >= currentCost)
        {
            gameManager.PurchaseUpgrade(currentUpgradeData, currentCost, this);
        }
    }

    // ���� ����� ���������� �� GameManager ����� �������� �������
    public void OnPurchaseSuccess()
    {
        currentLevel++;
        currentCost *= currentUpgradeData.costMultiplier;

        // ����� ������� ����� �������� ����� � ����� �����
        UpdateTextAndIcons();
    }

    // ���� ����� ���������� ������ ���� �� GameManager,
    // ����� �������� � ��������� ������ � ����������� �� ����� ������
    public void UpdateInteractableState(double currentScore)
    {
        purchaseButton.interactable = currentScore >= currentCost;
    }

    // ��������������� ����� ��� ���������� ���� ��������� ����� � ������
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

    // --- ����������� ��������� ���� (��� ���������� ��������) ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ����������� ������ ��� ���������, ������ ���� ��� �������
        if (purchaseButton.interactable)
        {
            transform.localScale = originalScale * 1.05f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ���������� ������ �������� ������, ����� ���� ������
        transform.localScale = originalScale;
    }

    // �������������� ����� ��� ��������� ����������� (K, M)
    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1000000) return (number / 1000).ToString("F1") + "K";
        if (number < 1000000000) return (number / 1000000).ToString("F1") + "M";
        return (number / 1000000000).ToString("F1") + "B"; // � ������� G3 �� B ��� ����������, ��� ����� ����������
    }
}