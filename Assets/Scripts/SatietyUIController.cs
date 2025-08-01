// SatietyUIController.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SatietyUIController : MonoBehaviour
{
    // --- ССЫЛКИ НА КОМПОНЕНТЫ ---
    [Header("Основные ссылки из сцены")]
    public GameManager gameManager;
    public Image satietyProgressBar;
    public Button feedButton;
    public Button superFeedButton;
    public TextMeshProUGUI satietyText;
    public TextMeshProUGUI feedCostText;

    [Header("Визуальные состояния")]
    public Image bowlImage;
    public Image cloudImage;
    public Animator catAnimator;
    public GameObject tearPrefab; // --- ИЗМЕНЕНО: Префаб слезы вместо отдельных объектов ---
    public Transform tearSpawnPointLeft; // --- НОВОЕ: Точка для появления слезы слева ---
    public Transform tearSpawnPointRight; // --- НОВОЕ: Точка для появления слезы справа ---
    [Tooltip("Источник звука для мяуканья (сытость 0%)")]
    public AudioSource catHungryAudioSource;

    [Header("Звуки (Одиночные аудиофайлы)")]
    public AudioClip feedButtonSound;
    public AudioClip feedButtonHoverSound;
    public AudioClip cloudFreezeSound;
    [Tooltip("Звук пульсации миски (1-20%)")]
    public AudioClip bowlPulseSound;
    [Tooltip("Звук капающих слез")]
    public AudioClip tearDropSound;

    [Header("Настройки частоты звуков (в секундах)")]
    public float meowInterval = 5.0f;
    public float bowlPulseInterval = 1.2f;

    [Header("Настройки анимации пульсации")]
    public float pulseScaleAmount = 1.15f;
    public float pulseAnimationDuration = 0.2f;

    [Header("Спрайты...")]
    public Sprite bowlFullSprite, bowlLowSprite, bowlEmptySprite;
    public Sprite cloudNormalSprite, cloudGreySprite;

    // --- ПРИВАТНЫЕ ПЕРЕМЕННЫЕ ---
    private Vector3 originalBowlScale;
    private double feedCost = 10;
    private float costIncreaseMultiplier = 1.15f;
    private float feedAmount = 50f;
    private bool isCloudFrozen = false;
    private Coroutine meowCoroutine, bowlPulseCoroutine;

    // --- МЕТОДЫ UNITY ---

    void Start()
    {
        feedButton.onClick.AddListener(OnFeedButtonClicked);
        superFeedButton.onClick.AddListener(OnSuperFeedButtonClicked);
        if (bowlImage != null) originalBowlScale = bowlImage.transform.localScale;
    }

    void Update()
    {
        if (gameManager == null) return;
        float satietyPercentage = gameManager.GetSatietyPercentage();
        satietyProgressBar.fillAmount = Mathf.Clamp01(satietyPercentage);
        satietyText.text = (satietyPercentage * 100).ToString("F0") + "%";
        feedButton.interactable = gameManager.score >= feedCost;
        if (feedCostText != null) feedCostText.text = FormatNumber(feedCost);
        UpdateHungerEffects(satietyPercentage);
    }

    // --- ГЛАВНАЯ ЛОГИКА СОСТОЯНИЙ ---
    private void UpdateHungerEffects(float satietyPercentage)
    {
        // Состояние 1: Голод 0%
        if (satietyPercentage <= 0)
        {
            // Визуал
            bowlImage.sprite = bowlEmptySprite;
            cloudImage.sprite = cloudGreySprite;
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 2);

            // Звуки и пульсация
            StopAndNullifyCoroutine(ref bowlPulseCoroutine);
            StartCoroutineIfNotRunning(ref meowCoroutine, PlayMeowSoundRepeatedly());
            if (!isCloudFrozen)
            {
                AudioManager.Instance.PlaySound(cloudFreezeSound);
                isCloudFrozen = true;
            }
        }
        // --- ИЗМЕНЕНО: Логика для 1-20% сытости ---
        // Состояние 2: Голод 1-20%
        else if (satietyPercentage <= 0.20f)
        {
            // Визуал
            bowlImage.sprite = bowlLowSprite;
            cloudImage.sprite = cloudNormalSprite;
            // УБРАНЫ СЛЕЗЫ, но анимация грусти остается
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 1);

            // Звуки и пульсация
            StopAndNullifyCoroutine(ref meowCoroutine);
            StartCoroutineIfNotRunning(ref bowlPulseCoroutine, PulseAndPlaySound(bowlImage.transform, originalBowlScale, bowlPulseSound, bowlPulseInterval));
            isCloudFrozen = false;
        }
        // Состояние 3: Все в порядке
        else
        {
            // Визуал
            bowlImage.sprite = bowlFullSprite;
            cloudImage.sprite = cloudNormalSprite;
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 0);

            // Звуки и пульсация: выключаем всё
            StopAndNullifyCoroutine(ref meowCoroutine);
            StopAndNullifyCoroutine(ref bowlPulseCoroutine);
            isCloudFrozen = false;
        }
    }

    // --- НОВЫЙ ПУБЛИЧНЫЙ МЕТОД ДЛЯ ВЫЗОВА ИЗ АНИМАЦИИ ---
    /// <summary>
    /// Создает слезу в указанной точке и проигрывает звук падения.
    /// Вызывается через Animation Event.
    /// </summary>
    /// <param name="side">0 для левой стороны, 1 для правой</param>
    public void DropTear(int side)
    {
        // !! ГЛАВНАЯ ПРОВЕРКА !!
        // Если сытость БОЛЬШЕ НУЛЯ, то мы ничего не делаем и просто выходим из функции.
        if (gameManager.GetSatietyPercentage() > 0)
        {
            return;
        }

        // Этот код выполнится ТОЛЬКО ЕСЛИ сытость равна 0.
        Transform spawnPoint = (side == 0) ? tearSpawnPointLeft : tearSpawnPointRight;

        if (tearPrefab != null && spawnPoint != null)
        {
            Instantiate(tearPrefab, spawnPoint.position, Quaternion.identity);
            AudioManager.Instance.PlaySound(tearDropSound);
        }
    }


    private IEnumerator PlayMeowSoundRepeatedly()
    {
        while (true)
        {
            if (catHungryAudioSource != null && catHungryAudioSource.clip != null)
            {
                catHungryAudioSource.Play();
                yield return new WaitForSeconds(catHungryAudioSource.clip.length + meowInterval);
            }
            else
            {
                yield return new WaitForSeconds(meowInterval);
            }
        }
    }

    private IEnumerator PulseAndPlaySound(Transform targetTransform, Vector3 originalScale, AudioClip clip, float interval)
    {
        while (true)
        {
            if (targetTransform != null)
            {
                float timer = 0f;
                float halfDuration = pulseAnimationDuration / 2;
                Vector3 targetScale = originalScale * pulseScaleAmount;

                while (timer < halfDuration)
                {
                    targetTransform.localScale = Vector3.Lerp(originalScale, targetScale, timer / halfDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }

                AudioManager.Instance.PlaySound(clip);

                timer = 0;
                while (timer < halfDuration)
                {
                    targetTransform.localScale = Vector3.Lerp(targetScale, originalScale, timer / halfDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }
                targetTransform.localScale = originalScale;
            }
            else
            {
                AudioManager.Instance.PlaySound(clip);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private void StartCoroutineIfNotRunning(ref Coroutine coroutineRef, IEnumerator routine)
    {
        if (coroutineRef == null) coroutineRef = StartCoroutine(routine);
    }

    private void StopAndNullifyCoroutine(ref Coroutine coroutineRef)
    {
        if (coroutineRef != null)
        {
            StopCoroutine(coroutineRef);
            coroutineRef = null;
        }
    }

    void OnFeedButtonClicked()
    {
        if (gameManager.score >= feedCost)
        {
            AudioManager.Instance.PlaySound(feedButtonSound);
            gameManager.FeedCat(feedCost, feedAmount);
            feedCost *= costIncreaseMultiplier;
        }
    }

    void OnSuperFeedButtonClicked()
    {
        gameManager.SuperFeedCat();
    }

    public void PlayFeedButtonHoverSound()
    {
        AudioManager.Instance.PlaySound(feedButtonHoverSound);
    }

    private string FormatNumber(double number)
    {
        if (number < 1000) return number.ToString("F0");
        if (number < 1000000) return (number / 1000).ToString("F1") + "K";
        return (number / 1000000).ToString("F1") + "M";
    }
}