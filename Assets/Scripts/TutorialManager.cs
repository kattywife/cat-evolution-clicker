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
    public float step1Delay = 4.0f;    // <--- НОВОЕ: Задержка появления первого совета
    public float step2Duration = 7.0f; // Сколько висит совет про монетки

    // Флаги состояния
    private bool step1Done = false;
    private bool step2Done = false;
    private bool step3Shown = false;
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
        yield return new WaitForSeconds(0.5f);

        if (gameManager == null)
        {
            Debug.LogError("TutorialManager: Не привязан GameManager!");
            yield break;
        }

        // Если новая игра
        if (gameManager.score == 0 && gameManager.GetCurrentLevel() == 0)
        {
            // Вместо мгновенного показа запускаем таймер
            StartCoroutine(Step1DelayRoutine());
        }
        else
        {
            // Если загрузка - пропускаем начало
            step1Done = true;
            step2Done = true;
            step3Done = true;
        }
    }

    // <--- НОВАЯ ЛОГИКА: Таймер первого шага
    private IEnumerator Step1DelayRoutine()
    {
        // Ждем 4 секунды (или сколько настроишь)
        yield return new WaitForSeconds(step1Delay);

        // ВАЖНО: Если игрок за эти 4 секунды еще НЕ начал кликать сам,
        // тогда показываем совет. Если уже начал - не мешаем.
        if (!step1Done)
        {
            ShowStep1_ClickCat();
        }
    }

    private void Update()
    {
        if (gameManager == null) return;

        // ЛОГИКА ШАГА 3 (Магазин)
        if (step2Done && !step3Done && !step3Shown)
        {
            if (gameManager.upgrades != null && gameManager.upgrades.Count > 0)
            {
                double cost = gameManager.upgrades[0].baseCost;
                if (gameManager.score >= cost)
                {
                    ShowStep3_BuyUpgrade();
                }
            }
        }

        // ЛОГИКА ШАГА 4 (Голод)
        if (step2Done && !step4Done && !step4Shown)
        {
            // <--- ИЗМЕНЕНИЕ: Теперь строго, когда упало до 0 (или ниже)
            if (gameManager.currentSatiety <= 0f)
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
        if (!step1Done)
        {
            step1Done = true;

            // Скрываем совет (если он успел появиться)
            if (tooltipClickCat) tooltipClickCat.Hide();

            // Сразу переходим к шагу 2
            ShowStep2_EarnMoney();
        }
    }

    // --- ШАГ 2: МОНЕТКИ ---
    public void ShowStep2_EarnMoney()
    {
        if (tooltipEarnMoney)
        {
            tooltipEarnMoney.Show();
            StartCoroutine(HideStep2Routine());
        }
        else
        {
            step2Done = true;
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