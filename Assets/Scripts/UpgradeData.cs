// UpgradeData.cs

using UnityEngine;

// --- ���������: ��������� ����� ���� ��� ���������� ---
public enum UpgradeType
{
    PerClick,          // ������� ���������� � �����
    PerSecond,         // ������� ���������� � ���������� ������
    ClickMultiplier,   // ��������� ��� ����� (+15% -> 0.15)
    PassiveMultiplier, // ��������� ��� ���������� ������ (+15% -> 0.15)
    GlobalMultiplier   // ��������� ��� ����� (+50% -> 0.50)
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game Data/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    public Sprite icon;

    [Header("Stats")]
    public UpgradeType type;
    // --- ���������: ������ long �� double, ����� ������� � ����� �����, � �������� ---
    public double power;

    [Header("Cost")]
    public double baseCost;
    public float costMultiplier;
}