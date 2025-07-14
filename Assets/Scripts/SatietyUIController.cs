// SatietyUIController.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class SatietyUIController : MonoBehaviour
{
    // --- ������ �� ���������� ---

    [Header("�������� ������ �� �����")]
    public GameManager gameManager; // <--- �������, ��� ��� ������ �� �����!
    public Image satietyProgressBar;
    public Button feedButton;
    public Button superFeedButton;
    public TextMeshProUGUI satietyText;
    public TextMeshProUGUI feedCostText;

    [Header("���������� ���������")]
    public Image bowlImage;
    public Image cloudImage;
    public Animator catAnimator;
    public GameObject tear1;
    public GameObject tear3;
    [Tooltip("�������� ����� �� ������ (��� �������� ��� 0%)")]
    public AudioSource catHungryAudioSource;
    [Tooltip("�������� ����� �� ����� (��� ��������� 1-20%)")]
    public AudioSource bowlPulsingAudioSource;

    [Header("�����")]
    public AudioClip feedButtonSound;
    public AudioClip feedButtonHoverSound;
    public AudioClip cloudFreezeSound;
    public AudioClip tearAppearSound;
    [Tooltip("��� ����� (� ��������) ������ ����������� ���� �����")]
    public float tearSoundInterval = 4.0f;

    [Header("������� �����")]
    public Sprite bowlFullSprite;
    public Sprite bowlLowSprite;
    public Sprite bowlEmptySprite;

    [Header("������� �������")]
    public Sprite cloudNormalSprite;
    public Sprite cloudGreySprite;

    [Header("��������� ���������")]
    public float pulseMagnitude = 1.1f;
    public float pulseSpeed = 3f;

    // --- ��������� ���������� ---
    private bool isPulsating = false;
    private Vector3 originalBowlScale;
    private double feedCost = 10;
    private float costIncreaseMultiplier = 1.15f;
    private float feedAmount = 50f;
    private bool isCrying = false;
    private bool isCloudFrozen = false;
    private Coroutine tearSoundCoroutine;

    // --- ������ UNITY ---
    void Start()
    {
        feedButton.onClick.AddListener(OnFeedButtonClicked);
        superFeedButton.onClick.AddListener(OnSuperFeedButtonClicked);
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
        UpdateHungerEffects(satietyPercentage);
    }

    // --- ������ ����� ������� � ������ ---
    private void UpdateHungerEffects(float satietyPercentage)
    {
        if (tear1 == null || tear3 == null) return;

        // ������� 1: ������� 0%
        if (satietyPercentage <= 0)
        {
            // ������
            bowlImage.sprite = bowlEmptySprite;
            cloudImage.sprite = cloudGreySprite;
            tear1.SetActive(false);
            tear3.SetActive(true);
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 2);

            // ������ ������
            if (catHungryAudioSource != null && !catHungryAudioSource.isPlaying) catHungryAudioSource.Play();
            if (bowlPulsingAudioSource != null && bowlPulsingAudioSource.isPlaying) bowlPulsingAudioSource.Stop();
            if (!isCloudFrozen)
            {
                AudioManager.Instance.PlaySound(cloudFreezeSound);
                isCloudFrozen = true;
            }
            if (!isCrying)
            {
                isCrying = true;
                tearSoundCoroutine = StartCoroutine(PlayTearSoundRepeatedly());
            }
            if (!isPulsating) StartPulsing();
        }
        // ������� 2: ������� 1-20%
        else if (satietyPercentage <= 0.20f)
        {
            // ������
            bowlImage.sprite = bowlLowSprite;
            cloudImage.sprite = cloudNormalSprite;
            tear1.SetActive(true);
            tear3.SetActive(false);
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 1);

            // ������ ������
            if (catHungryAudioSource != null && catHungryAudioSource.isPlaying) catHungryAudioSource.Stop();
            if (bowlPulsingAudioSource != null && !bowlPulsingAudioSource.isPlaying) bowlPulsingAudioSource.Play();
            if (!isCrying)
            {
                isCrying = true;
                tearSoundCoroutine = StartCoroutine(PlayTearSoundRepeatedly());
            }
            isCloudFrozen = false;
            if (!isPulsating) StartPulsing();
        }
        // ������� 3: �Ѩ � ������� (> 20%)
        else
        {
            // ������
            bowlImage.sprite = bowlFullSprite;
            cloudImage.sprite = cloudNormalSprite;
            tear1.SetActive(false);
            tear3.SetActive(false);
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 0);

            // ������ ������
            if (catHungryAudioSource != null && catHungryAudioSource.isPlaying) catHungryAudioSource.Stop();
            if (bowlPulsingAudioSource != null && bowlPulsingAudioSource.isPlaying) bowlPulsingAudioSource.Stop();
            if (isCrying)
            {
                isCrying = false;
                if (tearSoundCoroutine != null) StopCoroutine(tearSoundCoroutine);
            }
            isCloudFrozen = false;
            if (isPulsating) StopPulsing();
        }
    }

    // --- ������������� ��������� (��������) ��� �������������� ����� ���� ---
    private IEnumerator PlayTearSoundRepeatedly()
    {
        while (true)
        {
            AudioManager.Instance.PlaySound(tearAppearSound);
            yield return new WaitForSeconds(tearSoundInterval);
        }
    }

    // --- ���������� ���������� (������ ������) ---
    void StartPulsing()
    {
        isPulsating = true;
        StartCoroutine(PulseEffect());
    }

    void StopPulsing()
    {
        isPulsating = false;
        // ����� �� ������������� ������ �������� ���������, ����� �� ������ �������� ����
        StopCoroutine(PulseEffect());
        if (bowlImage != null)
        {
            bowlImage.transform.localScale = originalBowlScale;
        }
    }

    private IEnumerator PulseEffect()
    {
        while (isPulsating)
        {
            if (bowlImage != null)
            {
                float scale = originalBowlScale.x + Mathf.PingPong(Time.time * pulseSpeed, pulseMagnitude - originalBowlScale.x);
                bowlImage.transform.localScale = new Vector3(scale, scale, scale);
            }
            yield return null;
        }
    }

    // --- ��������� ������ ---
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