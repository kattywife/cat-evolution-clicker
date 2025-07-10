using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System; // ОБЯЗАТЕЛЬНО для работы с Action (событиями)

public class GameManager : MonoBehaviour
{
    // --- СОБЫТИЕ ---
    // Это событие будет вызываться каждый раз, когда меняется счет.
    public event Action OnScoreChanged;

    // --- ДАННЫЕ ИГРЫ (МОДЕЛЬ) ---
    [Header("Настройки уровней")]
    public List<LevelData> levels;
    private int currentLevelIndex = 0;

    [Header("Настройки улучшений")]
    public List<UpgradeData> upgrades;

    // --- ПЕРЕМЕННЫЕ ГЕЙМПЛЕЯ ---
    [Header("Текущее состояние")]
    private double _score = 0; // Приватная переменная для хранения счета
    public double score // Публичное "свойство" для доступа к счету
    {
        get { return _score; } // Когда кто-то читает score, он получает значение _score
        private set // Когда кто-то пытается записать в score, выполняется этот код
        {
            _score = value;
            OnScoreChanged?.Invoke(); // Вызываем событие, чтобы оповестить всех подписчиков
        }
    }
    public long scorePerClick = 1;
    public long scorePerSecond = 0;

    // --- ССЫЛКИ НА UI (ПРЕДСТАВЛЕНИЕ) ---
    [Header("Ссылки на UI элементы")]
    public TextMeshProUGUI scoreText;
    public Image catImage;

    [Header("Магазин")]
    public GameObject upgradeButtonPrefab;
    public Transform shopPanel;

    void Start()
    {
        currentLevelIndex = 0;
        ApplyLevelUp();

        CreateShop();
        score = 0; // Устанавливаем счет в 0. Это вызовет событие и обновит кнопки.
        UpdateScoreText();
    }

    void Update()
    {
        if (scorePerSecond > 0)
        {
            // Начисляем пассивный доход
            // Важно: здесь мы обращаемся к _score напрямую, чтобы не вызывать событие каждый кадр
            _score += scorePerSecond * Time.deltaTime;
            UpdateScoreText(); // Но текст на экране нужно обновлять постоянно
        }
    }

    public void OnCatClicked()
    {
        score += scorePerClick; // Увеличиваем счет (вызовет событие)
        UpdateScoreText();
        CheckForLevelUp();

        // Анимация клика
        catImage.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetCatScale", 0.1f);
    }

    public void PurchaseUpgrade(UpgradeData upgrade, double cost, UpgradeButtonUI button)
    {
        if (score >= cost)
        {
            score -= cost; // Уменьшаем счет (вызовет событие)
            UpdateScoreText();

            if (upgrade.type == UpgradeType.PerClick)
            {
                scorePerClick += upgrade.power;
            }
            else if (upgrade.type == UpgradeType.PerSecond)
            {
                scorePerSecond += upgrade.power;
            }

            button.OnPurchaseSuccess();
        }
    }

    private void CreateShop()
    {
        foreach (var upgrade in upgrades)
        {
            GameObject newButtonGO = Instantiate(upgradeButtonPrefab, shopPanel);
            newButtonGO.GetComponent<UpgradeButtonUI>().Setup(upgrade, this);
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = _score.ToString("F0");
        }
    }

    // --- Остальные методы-помощники без изменений ---
    private void CheckForLevelUp() { if (currentLevelIndex + 1 < levels.Count && score >= levels[currentLevelIndex + 1].scoreToReach) { currentLevelIndex++; ApplyLevelUp(); } }
    private void ApplyLevelUp() { if (levels.Count > 0) catImage.sprite = levels[currentLevelIndex].catSprite; }
    private void ResetCatScale() { catImage.transform.localScale = new Vector3(1f, 1f, 1f); }
}