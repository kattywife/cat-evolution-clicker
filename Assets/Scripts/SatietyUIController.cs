// SatietyUIController.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class SatietyUIController : MonoBehaviour
{
    // --- ССЫЛКИ НА КОМПОНЕНТЫ, КОТОРЫЕ ТЫ ПЕРЕТАЩИШЬ В ИНСПЕКТОРЕ ---

    [Header("Основные ссылки из сцены")]
    public GameManager gameManager;
    public Image satietyProgressBar;    // ProgressBar_Fill
    public Button feedButton;           // Кнопка FeedButton
    public Button superFeedButton;      // Кнопка SuperFeedButton
    public TextMeshProUGUI satietyText; // SatietyPercentageText
    public TextMeshProUGUI feedCostText;  // PriceText под кнопкой FeedButton

    [Header("Визуальные состояния")]
    public Image bowlImage;      // СЮДА ПЕРЕТАЩИШЬ FeedButton
    public Image cloudImage;     // СЮДА ПЕРЕТАЩИШЬ IncomeCloudImage (который мы создали)
    public Animator catAnimator; // СЮДА ПЕРЕТАЩИШЬ CatImage
    // --- НАЧАЛО ИЗМЕНЕНИЙ ---
    [Tooltip("Объект со спрайтом одной слезы (для сытости 1-20%)")]
    public GameObject tear1; // Сюда перетащишь объект слезка_1
    [Tooltip("Объект со спрайтом трех слез (для сытости 0%)")]
    public GameObject tear3; // Сюда перетащишь объект слезка_3
    // --- КОНЕЦ ИЗМЕНЕНИЙ ---


    [Header("Спрайты Миски")]
    public Sprite bowlFullSprite;     // Фиолетовая (100-21%)
    public Sprite bowlLowSprite;      // Розовая (20-1%)
    public Sprite bowlEmptySprite;    // Красная (0%)

    [Header("Спрайты Облачка")]
    public Sprite cloudNormalSprite;  // Обычное
    public Sprite cloudGreySprite;    // Серое (0%)

    [Header("Настройки Пульсации")]
    public float pulseMagnitude = 1.1f; // Насколько увеличивается (1.1 = 110%)
    public float pulseSpeed = 3f;       // Скорость пульсации

    // --- ПРИВАТНЫЕ ПЕРЕМЕННЫЕ ДЛЯ РАБОТЫ СКРИПТА ---

    private bool isPulsating = false;
    private Vector3 originalBowlScale;
    private double feedCost = 10;
    private float costIncreaseMultiplier = 1.15f;
    private float feedAmount = 50f;

    // --- МЕТОДЫ UNITY ---

    void Start()
    {
        feedButton.onClick.AddListener(OnFeedButtonClicked);
        superFeedButton.onClick.AddListener(OnSuperFeedButtonClicked);

        // Запоминаем оригинальный размер миски, чтобы пульсация была корректной
        if (bowlImage != null)
        {
            originalBowlScale = bowlImage.transform.localScale;
        }
    }

    void Update()
    {
        if (gameManager == null) return;

        float satietyPercentage = gameManager.GetSatietyPercentage();
        satietyProgressBar.fillAmount = Mathf.Clamp01(satietyPercentage);
        satietyText.text = (satietyPercentage * 100).ToString("F0") + "%";
        feedButton.interactable = gameManager.score >= feedCost;
        if (feedCostText != null)
        {
            feedCostText.text = FormatNumber(feedCost);
        }

        // Главная функция, которая меняет вид в зависимости от сытости
        UpdateHungerEffects(satietyPercentage);
    }

    // --- ЛОГИКА СМЕНЫ ВИЗУАЛА ---

    // --- ОБНОВЛЕННЫЙ МЕТОД ---
    private void UpdateHungerEffects(float satietyPercentage)
    {
        // Проверяем, что ссылки на слёзы установлены, чтобы избежать ошибок
        if (tear1 == null || tear3 == null)
        {
            Debug.LogError("Не забудь перетащить объекты слезок в инспекторе на скрипт SatietyUIController!");
            return;
        }

        // Условие 1: СЫТОСТЬ 0% (Красная миска, серый фон, слезка_3, пульсация)
        if (satietyPercentage <= 0)
        {
            bowlImage.sprite = bowlEmptySprite;
            cloudImage.sprite = cloudGreySprite;

            // Показываем нужную слезу, прячем остальные
            tear1.SetActive(false);
            tear3.SetActive(true);

            // Устанавливаем состояние "3 слезы" для других анимаций, если они есть
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 2);

            if (!isPulsating) StartPulsing();
        }
        // Условие 2: СЫТОСТЬ 20% И НИЖЕ (Розовая миска, слезка_1, пульсация)
        else if (satietyPercentage <= 0.20f)
        {
            bowlImage.sprite = bowlLowSprite;
            cloudImage.sprite = cloudNormalSprite;

            // Показываем нужную слезу, прячем остальные
            tear1.SetActive(true);
            tear3.SetActive(false);

            // Устанавливаем состояние "1 слеза"
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 1);

            if (!isPulsating) StartPulsing();
        }
        // Условие 3: ВСЁ В ПОРЯДКЕ (Фиолетовая миска, нет слез, всё спокойно)
        else
        {
            bowlImage.sprite = bowlFullSprite;
            cloudImage.sprite = cloudNormalSprite;

            // Прячем обе слезы
            tear1.SetActive(false);
            tear3.SetActive(false);

            // Устанавливаем состояние "не плачет"
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 0);

            if (isPulsating) StopPulsing();
        }
    }

    // --- УПРАВЛЕНИЕ ПУЛЬСАЦИЕЙ ---

    void StartPulsing()
    {
        isPulsating = true;
        StartCoroutine(PulseEffect());
    }

    void StopPulsing()
    {
        isPulsating = false;
        StopAllCoroutines(); // Надежнее остановить все корутины на этом скрипте
        if (bowlImage != null)
        {
            bowlImage.transform.localScale = originalBowlScale;
        }
    }

    private IEnumerator PulseEffect()
    {
        while (isPulsating) // Изменено условие для большей надежности
        {
            float scale = originalBowlScale.x + Mathf.PingPong(Time.time * pulseSpeed, pulseMagnitude - originalBowlScale.x);
            if (bowlImage != null)
            {
                bowlImage.transform.localScale = new Vector3(scale, scale, scale);
            }
            yield return null;
        }
    }

    // --- ОСТАЛЬНЫЕ МЕТОДЫ ---

    void OnFeedButtonClicked()
    {
        if (gameManager.score >= feedCost)
        {
            gameManager.FeedCat(feedCost, feedAmount);
            feedCost *= costIncreaseMultiplier;
        }
    }

    void OnSuperFeedButtonClicked()
    {
        gameManager.SuperFeedCat();
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1000000) return (number / 1000).ToString("F1") + "K";
        return (number / 1000000).ToString("F1") + "M";
    }
}