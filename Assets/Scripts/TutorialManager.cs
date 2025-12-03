using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public GameManager gameManager;

    [Header("Ссылки на облачка (расставь в сцене)")]
    public TutorialTooltip tooltipClickCat;    // 1. Кликай на котика
    public TutorialTooltip tooltipEarnMoney;   // 2. Зарабатывай монетки
    public TutorialTooltip tooltipBuyUpgrade;  // 3. Купи улучшение
    public TutorialTooltip tooltipFeedCat;     // 4. Покорми котика

    [Header("Настройки")]
    public float step2Duration = 7.0f; // Сколько висит совет про монетки

    // Флаги состояния (чтобы не показывать повторно)
    private bool step1Done = false;
    private bool step2Done = false;
    private bool step3Shown = false; // Показали, но еще не купили
    private bool step3Done = false;
    private bool step4Shown = false;
    private bool step4Done = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private IEnumerator Start()
    {
        // Ждем инициализации GameManager
        yield return new WaitForSeconds(1.0f);

        // Если это новая игра (счет 0 и уровень 0), начинаем туториал
        if (gameManager.score == 0 && gameManager.GetCurrentLevel() == 0)
        {
            ShowStep1_ClickCat();
        }
        else
        {
            // Если игрок вернулся (загрузка), считаем первые шаги пройденными
            step1Done = true;
            step2Done = true;
            step3Done = true; // Магазин скорее всего уже понятен
        }
    }

    private void Update()
    {
        // ЛОГИКА ШАГА 3 (Магазин)
        // Если шаг 2 пройден, шаг 3 еще не пройден, и мы его еще не показываем
        if (step2Done && !step3Done && !step3Shown)
        {
            // Проверяем, хватает ли денег на ПЕРВЫЙ товар
            if (gameManager.upgrades.Count > 0)
            {
                double cost = gameManager.upgrades[0].baseCost;
                if (gameManager.score >= cost)
                {
                    ShowStep3_BuyUpgrade();
                }
            }
        }

        // ЛОГИКА ШАГА 4 (Голод)
        // Если шаг 2 пройден, шаг 4 не пройден и не показан
        if (step2Done && !step4Done && !step4Shown)
        {
            // Если сытость упала ниже 70%
            if (gameManager.GetSatietyPercentage() < 0.7f)
            {
                ShowStep4_FeedCat();
            }
        }
    }

    // --- ШАГ 1: КЛИК ---
    public void ShowStep1_ClickCat()
    {
        if (tooltipClickCat) tooltipClickCat.Show();
    }

    public void OnCatClicked()
    {
        // Если мы на 1 этапе
        if (!step1Done)
        {
            step1Done = true;
            if (tooltipClickCat) tooltipClickCat.Hide();

            // Сразу запускаем шаг 2
            ShowStep2_EarnMoney();
        }
    }

    // --- ШАГ 2: МОНЕТКИ (Таймер) ---
    public void ShowStep2_EarnMoney()
    {
        if (tooltipEarnMoney)
        {
            tooltipEarnMoney.Show();
            StartCoroutine(HideStep2Routine());
        }
        else
        {
            step2Done = true; // Если облачка нет, сразу считаем пройденным
        }
    }

    private IEnumerator HideStep2Routine()
    {
        yield return new WaitForSeconds(step2Duration);
        if (tooltipEarnMoney) tooltipEarnMoney.Hide();
        step2Done = true;
    }

    // --- ШАГ 3: ПОКУПКА ---
    public void ShowStep3_BuyUpgrade()
    {
        step3Shown = true;
        if (tooltipBuyUpgrade) tooltipBuyUpgrade.Show();
    }

    public void OnUpgradePurchased()
    {
        // Если подсказка висит - убираем
        if (step3Shown && !step3Done)
        {
            if (tooltipBuyUpgrade) tooltipBuyUpgrade.Hide();
            step3Done = true;
        }
    }

    // --- ШАГ 4: КОРМЛЕНИЕ ---
    public void ShowStep4_FeedCat()
    {
        step4Shown = true;
        if (tooltipFeedCat) tooltipFeedCat.Show();
    }

    public void OnCatFed()
    {
        if (step4Shown && !step4Done)
        {
            if (tooltipFeedCat) tooltipFeedCat.Hide();
            step4Done = true;
        }
    }
}