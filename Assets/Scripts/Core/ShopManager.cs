using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Данные и Префиксы")]
    public List<UpgradeData> upgrades;
    public GameObject upgradeButtonPrefab;
    public Transform shopContentParent;
    public ScrollRect shopScrollRect;

    [Header("Настройки анимации")]
    public float animationScrollSpeed = 3f;
    public float animationBounceAmount = 50f;
    public int initialItemsToIgnore = 4;

    private List<UpgradeButtonUI> shopButtons = new List<UpgradeButtonUI>();
    private RectTransform shopContentRectTransform;
    private int unlockedItemsCount = 1;
    private bool isShopAnimating = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (shopContentParent != null)
            shopContentRectTransform = shopContentParent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        CreateShop();
        UpdateAllShopButtonsState();
    }

    private void Update()
    {
        if (EconomyManager.Instance == null) return;
        
        double currentScore = EconomyManager.Instance.score;
        for (int i = 0; i < unlockedItemsCount; i++)
        {
            if (i < shopButtons.Count && shopButtons[i] != null)
                shopButtons[i].UpdateInteractableState(currentScore);
        }
    }

    private void CreateShop()
    {
        if (upgradeButtonPrefab == null || shopContentParent == null) {
            Debug.LogError("[ShopManager] Не назначен префаб кнопки или родитель (Content)!");
            return;
        }

        foreach (var upgrade in upgrades)
        {
            GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopContentParent);
            UpgradeButtonUI buttonUI = newButtonGO.GetComponent<UpgradeButtonUI>();
            
            // Теперь передаем 'this' (сам ShopManager), чтобы кнопка могла обращаться к нему
            buttonUI.Setup(upgrade, this); 
            shopButtons.Add(buttonUI);
        }

            // Сбрасываем прокрутку в самый верх (1.0f — это верх, 0.0f — низ)
        if (shopScrollRect != null) 
        {
            shopScrollRect.verticalNormalizedPosition = 1f;
        }
        
        Debug.Log($"[ShopManager] Создано товаров: {shopButtons.Count}");
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (EconomyManager.Instance.SpendScore(cost))
        {
            ApplyUpgradeEffect(upgrade);

            int purchasedIndex = shopButtons.IndexOf(button);
            if (purchasedIndex == unlockedItemsCount - 1 && unlockedItemsCount < shopButtons.Count)
            {
                unlockedItemsCount++;
                if (unlockedItemsCount - 1 >= initialItemsToIgnore && !isShopAnimating)
                {
                    StartCoroutine(AnimateScrollToShowItem(shopButtons[unlockedItemsCount - 1].GetComponent<RectTransform>()));
                }
            }

            button.OnPurchaseSuccess();
            UpdateAllShopButtonsState();

            if (TutorialManager.Instance) TutorialManager.Instance.OnUpgradePurchased();
            if (SaveManager.Instance) SaveManager.Instance.Save();
        }
    }

    private void ApplyUpgradeEffect(UpgradeData upgrade)
    {
        var eco = EconomyManager.Instance;
        switch (upgrade.type)
        {
            case UpgradeType.PerClick: eco.scorePerClick += upgrade.power / eco.clickMultiplier; break;
            case UpgradeType.PerSecond: eco.scorePerSecond += upgrade.power / eco.passiveMultiplier; break;
            case UpgradeType.ClickMultiplier: eco.clickMultiplier *= upgrade.power; break;
            case UpgradeType.PassiveMultiplier: eco.passiveMultiplier *= upgrade.power; break;
            case UpgradeType.GlobalMultiplier: eco.clickMultiplier *= upgrade.power; eco.passiveMultiplier *= upgrade.power; break;
        }
    }

    public void UpdateAllShopButtonsState()
    {
        for (int i = 0; i < shopButtons.Count; i++)
        {
            if (shopButtons[i] == null) continue;
            bool isUnlocked = (i < unlockedItemsCount);
            shopButtons[i].SetLockedState(!isUnlocked);
        }
    }

    private IEnumerator AnimateScrollToShowItem(RectTransform targetItem)
    {
        isShopAnimating = true;
        shopScrollRect.enabled = false;
        Canvas.ForceUpdateCanvases();
        Vector2 startPos = shopContentRectTransform.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, -targetItem.anchoredPosition.y);
        Vector2 overshoot = targetPos + new Vector2(0, animationBounceAmount);
        float t = 0;
        while (t < 1f) { t += Time.deltaTime * animationScrollSpeed; shopContentRectTransform.anchoredPosition = Vector2.Lerp(startPos, overshoot, t); yield return null; }
        t = 0;
        while (t < 1f) { t += Time.deltaTime * animationScrollSpeed * 1.5f; shopContentRectTransform.anchoredPosition = Vector2.Lerp(overshoot, targetPos, t); yield return null; }
        shopContentRectTransform.anchoredPosition = targetPos;
        isShopAnimating = false;
        shopScrollRect.enabled = true;
    }

    public int GetUnlockedCount() => unlockedItemsCount;
    public void LoadUnlockedCount(int count) { unlockedItemsCount = count; UpdateAllShopButtonsState(); }
}