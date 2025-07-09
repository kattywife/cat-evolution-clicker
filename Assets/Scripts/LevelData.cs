using UnityEngine;

// Эта строка позволяет создавать ассеты этого типа прямо в меню Unity
[CreateAssetMenu(fileName = "NewLevel", menuName = "Game Data/Level")]
public class LevelData : ScriptableObject
{
    [Tooltip("Счет, необходимый для достижения этого уровня")]
    public int scoreToReach;

    [Tooltip("Спрайт котика для этого уровня")]
    public Sprite catSprite;
}