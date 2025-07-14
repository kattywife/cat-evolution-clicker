// GameManager.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections; // <<< НОВОЕ: Добавлено для работы с корутинами (IEnumerator)


// Убираем IPointerClickHandler из списка интерфейсов, чтобы избежать двойного клика
public class UpgradeButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI priceText;
    public Image iconImage;
    public Button purchaseButton;

    // <<< НОВОЕ: Добавлены настройки для анимации наведения >>>
    [Header("Настройки анимации наведения")]
    [Tooltip("Насколько увеличивать кнопку при наведении. 1.05 = 5%")]
    public float scaleFactor = 1.05f;
    [Tooltip("За сколько секунд должна проходить анимация")]
    public float animationDuration = 0.1f;


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

    // <<< ИЗМЕНЕНО: Теперь этот метод запускает плавную анимацию увеличения >>>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Увеличиваем кнопку при наведении, только если она активна
        if (purchaseButton.interactable)
        {
            // Останавливаем предыдущие анимации, чтобы избежать конфликтов
            StopAllCoroutines();
            // Запускаем корутину для плавного увеличения
            StartCoroutine(ScaleOverTime(originalScale * scaleFactor, animationDuration));
        }
    }

    // <<< ИЗМЕНЕНО: Теперь этот метод запускает плавную анимацию возврата к исходному размеру >>>
    public void OnPointerExit(PointerEventData eventData)
    {
        // Останавливаем предыдущие анимации
        StopAllCoroutines();
        // Запускаем корутину для плавного возврата к оригинальному размеру
        StartCoroutine(ScaleOverTime(originalScale, animationDuration));
    }

    // <<< НОВОЕ: Корутина для плавного изменения размера во времени >>>
    private IEnumerator ScaleOverTime(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            // Используем Lerp для плавного перехода от initialScale к targetScale
            transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
            yield return null; // Ждем следующего кадра
        }

        // Гарантируем, что в конце будет установлен точный целевой размер
        transform.localScale = targetScale;
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