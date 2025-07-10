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

        // Подписываемся на событие изменения счета
        gameManager.OnScoreChanged += UpdateUI;

        // Вызываем один раз для первоначальной настройки
        UpdateUI();
    }

    // Этот метод будет вызываться, когда объект кнопки уничтожается
    void OnDestroy()
    {
        // Отписываемся от события, чтобы избежать ошибок
        if (gameManager != null)
        {
            gameManager.OnScoreChanged -= UpdateUI;
        }
    }

    // Этот метод теперь вызывается автоматически по событию от GameManager
    public void UpdateUI()
    {
        if (currentUpgradeData == null) return;

        nameText.text = currentUpgradeData.upgradeName;
        iconImage.sprite = currentUpgradeData.icon;
        priceText.text = FormatNumber(currentCost);

        if (currentUpgradeData.type == UpgradeType.PerClick)
            effectText.text = $"+{currentUpgradeData.power} за клик";
        else if (currentUpgradeData.type == UpgradeType.PerSecond)
            effectText.text = $"+{currentUpgradeData.power} в секунду";

        // Включаем или выключаем кнопку в зависимости от количества очков
        purchaseButton.interactable = gameManager.score >= currentCost;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Вызываем логику покупки только если кнопка активна
        if (purchaseButton.interactable)
        {
            OnPurchaseClicked();
        }
    }

    public void OnPurchaseClicked()
    {
        gameManager.PurchaseUpgrade(currentUpgradeData, currentCost, this);
    }

    public void OnPurchaseSuccess()
    {
        currentLevel++;
        currentCost *= currentUpgradeData.costMultiplier;
        UpdateUI();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Увеличиваем, только если кнопка активна, для лучшего отклика
        if (purchaseButton.interactable)
        {
            transform.localScale = originalScale * 1.05f;
        }
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