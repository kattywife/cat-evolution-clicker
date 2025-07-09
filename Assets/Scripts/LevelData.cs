using UnityEngine;

// ��� ������ ��������� ��������� ������ ����� ���� ����� � ���� Unity
[CreateAssetMenu(fileName = "NewLevel", menuName = "Game Data/Level")]
public class LevelData : ScriptableObject
{
    [Tooltip("����, ����������� ��� ���������� ����� ������")]
    public int scoreToReach;

    [Tooltip("������ ������ ��� ����� ������")]
    public Sprite catSprite;
}