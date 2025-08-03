//LevelData

using UnityEngine;

// ��� ������ ��������� ��������� ������ ����� ���� ����� � ���� Unity
[CreateAssetMenu(fileName = "NewLevel", menuName = "Game Data/Level")]
public class LevelData : ScriptableObject
{
    [Tooltip("����, ����������� ��� ���������� ����� ������")]
    public double scoreToReach;

    [Tooltip("������ ������ ��� ����� ������")]
    public Sprite catSprite;
}