using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Данные уровней")]
    public List<LevelData> levels;
    public int currentLevelIndex = 0;

    [Header("UI Ссылки")]
    public Image catImage;
    public Slider levelProgressBar;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI progressText;

    [Header("Эффекты")]
    public ParticleSystem levelUpEffect;
    public AudioClip levelUpSound;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Принудительно ставим первый уровень без эффектов
        ApplyLevelUp(false);
    }

    // Проверка, пора ли повысить уровень
    public void CheckForLevelUp(double currentScore)
    {

        // Внутри метода, где уровень повышается:
        if (currentLevelIndex == 5)
        {
            if (CutsceneManager.Instance != null)
            {
                CutsceneManager.Instance.PreloadEndingVideo("ending.webm"); // используй .webm если переименовала
            }
        }

        if (currentLevelIndex + 1 < levels.Count)
        {
            if (currentScore >= levels[currentLevelIndex + 1].scoreToReach)
            {
                currentLevelIndex++;
                ApplyLevelUp(true);
            }
        }
    }

    private void ApplyLevelUp(bool playEffects)
    {
        if (levels == null || levels.Count == 0) return;

        // 1. Меняем внешний вид кота
        var currentLevel = levels[currentLevelIndex];
        if (catImage != null)
        {
            catImage.sprite = currentLevel.catSprite;
            catImage.SetNativeSize();
        }

        // 2. Двигаем слезки (позиция для этого спрайта)
        if (SatietyManager.Instance != null && SatietyManager.Instance.tearEffectObject != null)
        {
            SatietyManager.Instance.tearEffectObject.transform.localPosition = currentLevel.tearPosition;
        }

        // 3. Эффекты
        if (playEffects)
        {
            if (levelUpEffect) levelUpEffect.Play();
            if (levelUpSound) AudioManager.Instance.PlaySound(levelUpSound, 0.8f);
            
            // Сохраняем прогресс
            if (SaveManager.Instance) SaveManager.Instance.Save();
        }

        // 4. Проверка на финальный уровень
        if (currentLevelIndex == levels.Count - 1)
        {
            HandleFinalLevel(playEffects);
        }

        UpdateUI(EconomyManager.Instance != null ? EconomyManager.Instance.score : 0);
    }

    private void HandleFinalLevel(bool playEffects)
    {
        // Отключаем голод навсегда
        if (SatietyManager.Instance) SatietyManager.Instance.DisableHunger();

        // Если это только что произошло (playEffects == true), запускаем финал
        if (playEffects)
        {
            // Здесь мы позовем GameManager или CutsceneManager
            GameManager.Instance.OnReachingMaxLevel();
        }
    }

    // Обновление полоски прогресса
    public void UpdateUI(double currentScore)
    {
        if (levelProgressBar == null) return;

        // Получаем переведенное слово "Уровень" (если переводчик не готов, пишем дефолтное "Уровень")
        string levelWord = "Уровень";
        if (LocalizationManager.Instance != null)
        {
            levelWord = LocalizationManager.Instance.GetTranslation("ui_level");
        }

        // Если это последний уровень
        if (currentLevelIndex >= levels.Count - 1)
        {
            levelProgressBar.minValue = 0;
            levelProgressBar.maxValue = 1;
            levelProgressBar.value = 1;
            
            if (levelNumberText) levelNumberText.text = $"{levelWord}: {currentLevelIndex + 1}";
            
            if (progressText)
            {
                string maxWord = LocalizationManager.Instance != null 
                    ? LocalizationManager.Instance.GetTranslation("ui_max_level") 
                    : "МАКСИМУМ";
                progressText.text = maxWord;
            }
            return;
        }

        // Берем данные текущего и следующего уровня
        double scoreToReachNext = levels[currentLevelIndex + 1].scoreToReach;
        
        // Настраиваем слайдер
        levelProgressBar.minValue = 0; 
        levelProgressBar.maxValue = (float)scoreToReachNext;
        levelProgressBar.value = (float)currentScore;

        // Обновляем текст уровня
        if (levelNumberText) levelNumberText.text = $"{levelWord}: {currentLevelIndex + 1}";
        
        if (progressText && EconomyManager.Instance != null)
        {
            progressText.text = $"{EconomyManager.Instance.FormatNumber(currentScore)} / {EconomyManager.Instance.FormatNumber(scoreToReachNext)}";
        }
    }

    public int GetCurrentLevel() => currentLevelIndex;

    public void LoadLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        ApplyLevelUp(false);
    }
}