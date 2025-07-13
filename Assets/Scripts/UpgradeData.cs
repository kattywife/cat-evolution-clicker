//UpgradeData

using UnityEngine;

public enum UpgradeType { PerClick, PerSecond }

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game Data/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    public Sprite icon;

    [Header("Stats")]
    public UpgradeType type;
    public long power;

    [Header("Cost")]
    public double baseCost;
    public float costMultiplier;
}