using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI priceText;
    public Image iconImage;
    public Button purchaseButton;

    private UpgradeData currentUpgradeData;
    private int currentLevel = 0;
    private double currentCost = 0;
    private GameManager gameManager; // ������ �� ������� ������

    public void Setup(UpgradeData data, GameManager manager)
    {
        currentUpgradeData = data;
        gameManager = manager;
        currentLevel = 0;
        currentCost = currentUpgradeData.baseCost;

        purchaseButton.onClick.AddListener(OnPurchaseClicked);
        UpdateUI();
    }

    public void UpdateUI()
    {
        nameText.text = $"{currentUpgradeData.upgradeName} (��. {currentLevel})";
        iconImage.sprite = currentUpgradeData.icon;
        priceText.text = FormatNumber(currentCost);

        if (currentUpgradeData.type == UpgradeType.PerClick)
            effectText.text = $"+{currentUpgradeData.power} �� ����";
        else if (currentUpgradeData.type == UpgradeType.PerSecond)
            effectText.text = $"+{currentUpgradeData.power} � �������";

        // ���������, ����� �� �� ��������� ���� �������, � ������ ��������� ������
        purchaseButton.interactable = gameManager.score >= currentCost;
    }

    private void OnPurchaseClicked()
    {
        gameManager.PurchaseUpgrade(currentUpgradeData, currentCost, this);
    }

    // ���� ����� ������� GameManager ����� �������� �������
    public void OnPurchaseSuccess()
    {
        currentLevel++;
        currentCost *= currentUpgradeData.costMultiplier;
        UpdateUI();
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1000000) return (number / 1000).ToString("F1") + "K";
        if (number < 1000000000) return (number / 1000000).ToString("F1") + "M";
        return number.ToString("F0");
    }
}