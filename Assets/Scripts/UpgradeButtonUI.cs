// UpgradeButttonUI
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


// Убираем IPointerClickHandler из списка интерфейсов, чтобы избежать двойного клика
public class UpgradeButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI priceText;
    public Image iconImage;
    public Button purchaseButton;



    // Приватные переменные для хранения состояния кнопки
    private UpgradeData currentUpgradeData;
    private int currentLevel = 0;
    private double currentCost = 0;
    private GameManager gameManager;
    private Vector3 originalScale;

    void Awake()
    {
        // Запоминаем оригинальный размер, чтобы использовать его для анимации наведения
        originalScale = transform.localScale;
    }

    // Метод для первоначальной настройки кнопки при создании магазина
    public void Setup(UpgradeData data, GameManager manager)
    {
        currentUpgradeData = data;
        gameManager = manager;
        currentLevel = 0;
        currentCost = currentUpgradeData.baseCost;

        // Назначаем обработчик клика ТОЛЬКО через компонент Button
        // Сначала очищаем старые подписки на всякий случай
        purchaseButton.onClick.RemoveAllListeners();
        // Добавляем новый вызов нашего метода
        purchaseButton.onClick.AddListener(OnPurchaseClicked);

        // Обновляем весь текст на кнопке
        UpdateTextAndIcons();
    }

    // Этот метод вызывается, когда игрок нажимает на кнопку
    public void OnPurchaseClicked()
    {
        // GameManager сам проверит, хватает ли очков,
        // но для надежности можно и здесь проверить, прежде чем вызывать метод
        if (gameManager.score >= currentCost)
        {
            gameManager.PurchaseUpgrade(currentUpgradeData, currentCost, this);
        }
    }

    // Этот метод вызывается из GameManager после успешной покупки
    public void OnPurchaseSuccess()
    {
        currentLevel++;
        currentCost *= currentUpgradeData.costMultiplier;

        // После покупки нужно обновить текст с новой ценой
        UpdateTextAndIcons();
    }

    // Этот метод вызывается каждый кадр из GameManager,
    // чтобы включать и выключать кнопку в зависимости от счета игрока
    public void UpdateInteractableState(double currentScore)
    {
        purchaseButton.interactable = currentScore >= currentCost;
    }

    // Вспомогательный метод для обновления всех текстовых полей и иконок
    public void UpdateTextAndIcons()
    {
        nameText.text = currentUpgradeData.upgradeName;
        iconImage.sprite = currentUpgradeData.icon;
        priceText.text = FormatNumber(currentCost);

        if (currentUpgradeData.type == UpgradeType.PerClick)
        {
            effectText.text = $"+{currentUpgradeData.power} за клик";
        }
        else if (currentUpgradeData.type == UpgradeType.PerSecond)
        {
            effectText.text = $"+{currentUpgradeData.power} в секунду";
        }
    }

    // --- Обработчики наведения мыши (для визуальных эффектов) ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Увеличиваем кнопку при наведении, только если она активна
        if (purchaseButton.interactable)
        {
            transform.localScale = originalScale * 1.05f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Возвращаем кнопке исходный размер, когда мышь уходит
        transform.localScale = originalScale;
    }

    // Форматирование чисел для красивого отображения (K, M)
    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1000000) return (number / 1000).ToString("F1") + "K";
        if (number < 1000000000) return (number / 1000000).ToString("F1") + "M";
        return (number / 1000000000).ToString("F1") + "B"; // Я заменил G3 на B для миллиардов, это более стандартно
    }
}