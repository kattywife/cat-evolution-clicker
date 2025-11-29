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
    private double currentCost = 0;
    private GameManager gameManager;
    private Vector3 originalScale;

    // --- ЗАЩИТА ОТ ДВОЙНОГО КЛИКА ---
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.2f; // Задержка 0.2 секунды

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

        // Чистим старые и добавляем новые события
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }

        UpdateTextAndIcons();
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
        // --- ПРОВЕРКА ЗАЩИТЫ ---
        // Если с прошлого клика прошло слишком мало времени - игнорируем этот клик
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            return;
        }
        lastClickTime = Time.time; // Обновляем время последнего клика
        // -----------------------

        if (gameManager != null && gameManager.score >= currentCost)
        {
            gameManager.PurchaseUpgrade(currentUpgradeData, currentCost, this);
        }
    }

    public void OnPurchaseSuccess()
    {
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

    public bool IsInteractable()
    {
        return purchaseButton != null && purchaseButton.interactable;
    }

    public void UpdateTextAndIcons()
    {
        if (currentUpgradeData == null) return;

        if (nameText != null) nameText.text = currentUpgradeData.upgradeName;
        if (iconImage != null) iconImage.sprite = currentUpgradeData.icon;
        if (priceText != null) priceText.text = FormatNumber(currentCost);

        if (effectText != null)
        {
            switch (currentUpgradeData.type)
            {
                case UpgradeType.PerClick:
                    effectText.text = $"+{FormatNumber(currentUpgradeData.power)} за клик";
                    break;
                case UpgradeType.PerSecond:
                    effectText.text = $"+{FormatNumber(currentUpgradeData.power)} в секунду";
                    break;
                case UpgradeType.ClickMultiplier:
                    effectText.text = $"+{(currentUpgradeData.power * 100 - 100).ToString("F0")}% за клик";
                    break;
                case UpgradeType.PassiveMultiplier:
                    effectText.text = $"+{(currentUpgradeData.power * 100 - 100).ToString("F0")}% в секунду";
                    break;
                case UpgradeType.GlobalMultiplier:
                    effectText.text = $"+{(currentUpgradeData.power * 100 - 100).ToString("F0")}% ко всему";
                    break;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (purchaseButton != null && purchaseButton.interactable)
        {
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
        if (number < 1_000_000) return (number / 1000).ToString("F1") + "К";
        if (number < 1_000_000_000) return (number / 1_000_000).ToString("F1") + "М";
        if (number < 1_000_000_000_000) return (number / 1_000_000_000).ToString("F1") + "Б";
        if (number < 1_000_000_000_000_000) return (number / 1_000_000_000_000).ToString("F1") + "Т";
        return (number / 1_000_000_000_000_000).ToString("F1") + "Кв";
    }
}