// UpgradeData.cs

using UnityEngine;

// --- ОБНОВЛЕНО: Добавляем новые типы для множителей ---
public enum UpgradeType
{
    PerClick,          // Простое добавление к клику
    PerSecond,         // Простое добавление к пассивному доходу
    ClickMultiplier,   // Множитель для клика (+15% -> 0.15)
    PassiveMultiplier, // Множитель для пассивного дохода (+15% -> 0.15)
    GlobalMultiplier   // Множитель для всего (+50% -> 0.50)
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game Data/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    public Sprite icon;

    [Header("Stats")]
    public UpgradeType type;
    // --- ОБНОВЛЕНО: Меняем long на double, чтобы хранить и целые числа, и проценты ---
    public double power;

    [Header("Cost")]
    public double baseCost;
    public float costMultiplier;
}