using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // --- ДАННЫЕ ИГРЫ (МОДЕЛЬ) ---
    [Header("Настройки уровней")]
    public List<LevelData> levels; // Список всех наших уровней (перетащим сюда ассеты)
    private int currentLevelIndex = 0;

    // --- ПЕРЕМЕННЫЕ ГЕЙМПЛЕЯ ---
    [Header("Текущее состояние")]
    public double score = 0;

    // --- ССЫЛКИ НА UI (ПРЕДСТАВЛЕНИЕ) ---
    [Header("Ссылки на UI элементы")]
    public TextMeshProUGUI scoreText;
    public Image catImage; // ИЗМЕНЕНИЕ: теперь ссылка на Image, а не Transform

    // Start вызывается один раз при запуске игры
    void Start()
    {
        // Устанавливаем начальные значения
        score = 0;
        currentLevelIndex = 0;
        ApplyLevelUp(); // Применяем спрайт самого первого уровня
        UpdateScoreText();
    }

    public void OnCatClicked()
    {
        score++;
        UpdateScoreText();
        CheckForLevelUp();

        // Анимация клика
        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    private void CheckForLevelUp()
    {
        // Проверяем, не последний ли это уровень
        if (currentLevelIndex + 1 >= levels.Count)
        {
            return; // Мы уже на максимальном уровне, выходим
        }

        // Проверяем, набрали ли мы достаточно очков для СЛЕДУЮЩЕГО уровня
        if (score >= levels[currentLevelIndex + 1].scoreToReach)
        {
            currentLevelIndex++; // Повышаем уровень
            ApplyLevelUp(); // Применяем изменения
        }
    }

    private void ApplyLevelUp()
    {
        // Получаем данные текущего уровня
        LevelData currentLevel = levels[currentLevelIndex];

        // Меняем спрайт котика
        catImage.sprite = currentLevel.catSprite;
    }

    private void ResetCatScale()
    {
        catImage.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("F0");
        }
    }
}