using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class UpgradeButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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

        UpdateUI(); // Правильный вызов
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPurchaseClicked();
    }

    public void OnPurchaseClicked()
    {
        gameManager.PurchaseUpgrade(currentUpgradeData, currentCost, this);
    }

    public void OnPurchaseSuccess()
    {
        currentLevel++;
        currentCost *= currentUpgradeData.costMultiplier;
        UpdateUI(); // Правильный вызов
    }

    // Вот правильное объявление метода
    public void UpdateUI()
    {
        nameText.text = currentUpgradeData.upgradeName; // Убрали (Ур. 0)
        iconImage.sprite = currentUpgradeData.icon;
        priceText.text = FormatNumber(currentCost);

        if (currentUpgradeData.type == UpgradeType.PerClick)
            effectText.text = $"+{currentUpgradeData.power} за клик";
        else if (currentUpgradeData.type == UpgradeType.PerSecond)
            effectText.text = $"+{currentUpgradeData.power} в секунду";

        purchaseButton.interactable = gameManager.score >= currentCost;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1000000) return (number / 1000).ToString("F1") + "K";
        if (number < 1000000000) return (number / 1000000).ToString("F1") + "M";
        return number.ToString("F0");
    }
}