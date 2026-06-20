using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Ссылки на облачка (расставь в сцене)")]
    public TutorialTooltip tooltipClickCat;    // 1. Кликай на котика
    public TutorialTooltip tooltipEarnMoney;   // 2. Зарабатывай монетки
    public TutorialTooltip tooltipBuyUpgrade;  // 3. Купи улучшение
    public TutorialTooltip tooltipFeedCat;     // 4. Покорми котика

    [Header("Настройки")]
    public float step1Delay = 4.0f;    
    public float step2Duration = 7.0f; 

    // Флаги состояния
    private bool step1Done = false;
    private bool step2Done = false;
    private bool step3Shown = false;
    private bool step3Done = false;
    private bool step4Shown = false;
    private bool step4Done = false;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        // Ждем немного, чтобы все менеджеры (Economy, Progression) успели проснуться
        yield return new WaitForSeconds(0.6f);

        if (GameManager.Instance == null)
        {
            Debug.LogError("<color=red>[Tutorial]</color> GameManager.Instance не найден! Туториал не может быть запущен.");
            yield break;
        }

        // Если новая игра (0 очков и 0 уровень)
        if (EconomyManager.Instance.score <= 0 && ProgressionManager.Instance.GetCurrentLevel() == 0)
        {
            Debug.Log("<color=cyan>[Tutorial]</color> Новая игра обнаружена. Запускаю таймер первого шага.");
            StartCoroutine(Step1DelayRoutine());
        }
        else
        {
            Debug.Log("<color=cyan>[Tutorial]</color> Загружено сохранение. Пропускаю первые шаги обучения.");
            step1Done = true;
            step2Done = true;
            step3Done = true;
            // Шаг 4 (голод) оставляем, он может понадобиться позже
        }
    }

    private IEnumerator Step1DelayRoutine()
    {
        yield return new WaitForSeconds(step1Delay);

        if (!step1Done)
        {
            Debug.Log("<color=white>[Tutorial]</color> Показываю Шаг 1: Клик по коту.");
            ShowStep1_ClickCat();
        }
    }

    private void Update()
    {
        // Если геймплей еще не начался или GameManager выключен (например, в конце игры)
        if (GameManager.Instance == null || !GameManager.Instance.enabled) return;

        // ЛОГИКА ШАГА 3 (Магазин)
        if (step2Done && !step3Done && !step3Shown)
        {
            if (ShopManager.Instance != null && ShopManager.Instance.upgrades.Count > 0)
            {
                double cost = ShopManager.Instance.upgrades[0].baseCost;
                if (EconomyManager.Instance.score >= cost)
                {
                    Debug.Log("<color=white>[Tutorial]</color> Очков достаточно для покупки. Показываю Шаг 3.");
                    ShowStep3_BuyUpgrade();
                }
            }
        }

        // ЛОГИКА ШАГА 4 (Голод)
        if (step2Done && !step4Done && !step4Shown)
        {
            if (SatietyManager.Instance != null && SatietyManager.Instance.currentSatiety <= 0f)
            {
                Debug.Log("<color=white>[Tutorial]</color> Котик проголодался. Показываю Шаг 4.");
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
            Debug.Log("<color=green>[Tutorial]</color> Шаг 1 выполнен (Клик).");
            
            if (tooltipClickCat) tooltipClickCat.Hide();
            ShowStep2_EarnMoney();
        }
    }

    // --- ШАГ 2: МОНЕТКИ ---
    public void ShowStep2_EarnMoney()
    {
        Debug.Log("<color=white>[Tutorial]</color> Показываю Шаг 2: Рассказ про монетки.");
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
        Debug.Log("<color=green>[Tutorial]</color> Шаг 2 завершен по времени.");
    }

    // --- ШАГ 3: ПОКУПКА ---
    public void ShowStep3_BuyUpgrade()
    {
        step3Shown = true;
        if (tooltipBuyUpgrade) tooltipBuyUpgrade.Show();
    }

    public void OnUpgradePurchased()
    {
        if (!step3Done)
        {
            Debug.Log("<color=green>[Tutorial]</color> Шаг 3 выполнен (Апгрейд куплен).");
            if (tooltipBuyUpgrade) tooltipBuyUpgrade.Hide();
            step3Done = true;
            step3Shown = true;
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
            Debug.Log("<color=green>[Tutorial]</color> Шаг 4 выполнен (Кот сыт).");
            if (tooltipFeedCat) tooltipFeedCat.Hide();
            step4Done = true;
        }
    }
}