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
    public GameObject lockIcon;

    [Header("Настройки анимации наведения")]
    public float scaleFactor = 1.05f;
    public float animationDuration = 0.1f;

    [Header("Звуки")]
    public AudioClip hoverSound;
    public AudioClip purchaseSound;

    private UpgradeData currentUpgradeData;
    private int currentLevel = 0;
    public double currentCost = 0;
    
    // Сменили GameManager на ShopManager
    private ShopManager shopManager; 
    private Vector3 originalScale;

    // --- ЗАЩИТА ОТ ДВОЙНОГО КЛИКА ---
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.2f;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    // ИСПРАВЛЕНО: Теперь принимает ShopManager
    public void Setup(UpgradeData data, ShopManager manager)
    {
        currentUpgradeData = data;
        shopManager = manager;
        currentLevel = 0;
        currentCost = currentUpgradeData.baseCost;

        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }

        UpdateTextAndIcons();
        
        if (currentUpgradeData != null)
            Debug.Log($"<color=white>[Button UI]</color> Товар '{currentUpgradeData.upgradeName}' инициализирован.");
    }

    public void SetLockedState(bool isLocked)
    {
        if (lockIcon != null) lockIcon.SetActive(isLocked);
        if (nameText != null) nameText.gameObject.SetActive(!isLocked);
        if (effectText != null) effectText.gameObject.SetActive(!isLocked);
        if (priceText != null) priceText.gameObject.SetActive(!isLocked);
        if (iconImage != null) iconImage.gameObject.SetActive(!isLocked);

        if (purchaseButton != null) purchaseButton.interactable = !isLocked;
    }

    public void OnPurchaseClicked()
    {
        if (Time.time - lastClickTime < CLICK_COOLDOWN) return;
        lastClickTime = Time.time;

        if (shopManager == null)
        {
            Debug.LogError("<color=red>[Button UI]</color> Ошибка: ShopManager не привязан к кнопке!");
            return;
        }

        if (EconomyManager.Instance != null && EconomyManager.Instance.score >= currentCost)
        {
            Debug.Log($"<color=yellow>[Button UI]</color> Запрос на покупку: {currentUpgradeData.upgradeName}");
            shopManager.PurchaseUpgrade(currentUpgradeData, currentCost, this);
        }
        else
        {
            Debug.Log("<color=orange>[Button UI]</color> Недостаточно средств для покупки.");
        }
    }

    public void OnPurchaseSuccess()
    {
        Debug.Log($"<color=green>[Button UI]</color> Покупка '{currentUpgradeData.upgradeName}' успешна!");
        
        if (purchaseSound && AudioManager.Instance) 
            AudioManager.Instance.PlaySound(purchaseSound);

        currentLevel++;
        currentCost *= currentUpgradeData.costMultiplier;
        UpdateTextAndIcons();
    }

    public void UpdateInteractableState(double currentScore)
    {
        if (purchaseButton != null)
        {
            purchaseButton.interactable = currentScore >= currentCost;
        }
    }

    public void UpdateTextAndIcons()
    {
        if (currentUpgradeData == null) return;

        // 1. ПЕРЕВОДИМ НАЗВАНИЕ ТОВАРА
        // (будет искать ключ, например "shop_item_ball_title", в вашем JSON)
        if (nameText != null)
        {
            nameText.text = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetTranslation(currentUpgradeData.upgradeName) 
                : currentUpgradeData.upgradeName;
        }

        if (iconImage != null) iconImage.sprite = currentUpgradeData.icon;
        if (priceText != null) priceText.text = FormatNumber(currentCost);

        // 2. ДИНАМИЧЕСКИ ПЕРЕВОДИМ ЭФФЕКТЫ
        if (effectText != null)
        {
            string suffix = "";
            string powerValue = "";

            switch (currentUpgradeData.type)
            {
                case UpgradeType.PerClick:
                    powerValue = $"+{FormatNumber(currentUpgradeData.power)}";
                    suffix = LocalizationManager.Instance != null 
                        ? LocalizationManager.Instance.GetTranslation("shop_effect_per_click") 
                        : " за клик";
                    break;

                case UpgradeType.PerSecond:
                    powerValue = $"+{FormatNumber(currentUpgradeData.power)}";
                    suffix = LocalizationManager.Instance != null 
                        ? LocalizationManager.Instance.GetTranslation("shop_effect_per_second") 
                        : " в секунду";
                    break;

                case UpgradeType.ClickMultiplier:
                    powerValue = $"+{(currentUpgradeData.power * 100 - 100).ToString("F0")}%";
                    suffix = LocalizationManager.Instance != null 
                        ? LocalizationManager.Instance.GetTranslation("shop_effect_per_click") 
                        : " за клик";
                    break;

                case UpgradeType.PassiveMultiplier:
                    powerValue = $"+{(currentUpgradeData.power * 100 - 100).ToString("F0")}%";
                    suffix = LocalizationManager.Instance != null 
                        ? LocalizationManager.Instance.GetTranslation("shop_effect_per_second") 
                        : " в секунду";
                    break;

                case UpgradeType.GlobalMultiplier:
                    powerValue = $"+{(currentUpgradeData.power * 100 - 100).ToString("F0")}%";
                    suffix = LocalizationManager.Instance != null 
                        ? LocalizationManager.Instance.GetTranslation("shop_effect_global") 
                        : " ко всему";
                    break;
            }

            // Собираем число и переведенный текст вместе
            effectText.text = powerValue + suffix;
        }
    }

    #region --- АНИМАЦИИ И ФОРМАТИРОВАНИЕ ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (purchaseButton != null && purchaseButton.interactable)
        {
            if (hoverSound && AudioManager.Instance) AudioManager.Instance.PlaySound(hoverSound);
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
        // Используем форматирование из EconomyManager, если он доступен
        if (EconomyManager.Instance != null) return EconomyManager.Instance.FormatNumber(number);

        // Запасной вариант, если EconomyManager еще не проснулся
        if (number < 1000) return number.ToString("F0");
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "К";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "М";
        return (number / 1_000_000_000).ToString("F1") + "Б";
    }
    
    public void LoadCost(double savedCost)
    {
        currentCost = savedCost;
        UpdateTextAndIcons(); // Сразу обновляем текст цены на кнопке
    }
    #endregion
}