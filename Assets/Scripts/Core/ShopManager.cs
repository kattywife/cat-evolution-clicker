using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Данные")]
    public List<UpgradeData> upgrades;
    public GameObject upgradeButtonPrefab;
    public Transform shopContentParent;
    public ScrollRect shopScrollRect;

    [Header("Настройки анимации")]
    public float animationScrollSpeed = 3f;
    public float animationBounceAmount = 10f; // Уменьшили отскок
    public int initialItemsToIgnore = 4;

    private List<UpgradeButtonUI> shopButtons = new List<UpgradeButtonUI>();
    private RectTransform shopContentRectTransform;
    private int unlockedItemsCount = 1;
    private bool isShopAnimating = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (shopContentParent != null) shopContentRectTransform = shopContentParent.GetComponent<RectTransform>();
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
            if (i < shopButtons.Count) shopButtons[i].UpdateInteractableState(currentScore);
        }
    }

    private void CreateShop()
    {
        foreach (var upgrade in upgrades)
        {
            GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopContentParent);
            UpgradeButtonUI buttonUI = newButtonGO.GetComponent<UpgradeButtonUI>();
            buttonUI.Setup(upgrade, this); 
            shopButtons.Add(buttonUI);
        }
        // Убрали принудительный сброс, теперь его делает SaveManager через LoadScrollPosition
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
                    StartCoroutine(AnimateScrollToShowItem(shopButtons[unlockedItemsCount - 1].GetComponent<RectTransform>()));
            }
            button.OnPurchaseSuccess();
            UpdateAllShopButtonsState();
            SaveManager.Instance?.Save();
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
            if (shopButtons[i] != null) shopButtons[i].SetLockedState(i >= unlockedItemsCount);
    }

    // --- МЕТОДЫ ДЛЯ СОХРАНЕНИЯ ПРОКРУТКИ ---

    public float GetScrollPosition() => shopScrollRect != null ? shopScrollRect.verticalNormalizedPosition : 1f;

    public void LoadScrollPosition(float pos)
    {
        if (pos <= 0) pos = 1f; // Если данных нет, начинаем сверху
        StartCoroutine(ApplyScrollRoutine(pos));
    }

    private IEnumerator ApplyScrollRoutine(float pos)
    {
        yield return new WaitForEndOfFrame(); // Ждем, пока UI построится
        if (shopScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            shopScrollRect.verticalNormalizedPosition = pos;
        }
    }

    private IEnumerator AnimateScrollToShowItem(RectTransform targetItem)
    {
        isShopAnimating = true;
        shopScrollRect.enabled = false;
        Canvas.ForceUpdateCanvases();
        Vector2 startPos = shopContentRectTransform.anchoredPosition;
        float targetY = Mathf.Max(0, -targetItem.anchoredPosition.y - 150f); 
        Vector2 targetPos = new Vector2(startPos.x, targetY);
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