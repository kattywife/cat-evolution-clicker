// SatietyUIController.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SatietyUIController : MonoBehaviour
{
    // --- ������ �� ���������� ---
    [Header("�������� ������ �� �����")]
    public GameManager gameManager;
    public Image satietyProgressBar;
    public Button feedButton;
    public Button superFeedButton;
    public TextMeshProUGUI satietyText;
    public TextMeshProUGUI feedCostText;

    [Header("���������� ���������")]
    public Image bowlImage;
    public Image cloudImage;
    public Animator catAnimator;
    public GameObject tearPrefab; // --- ��������: ������ ����� ������ ��������� �������� ---
    public Transform tearSpawnPointLeft; // --- �����: ����� ��� ��������� ����� ����� ---
    public Transform tearSpawnPointRight; // --- �����: ����� ��� ��������� ����� ������ ---
    [Tooltip("�������� ����� ��� �������� (������� 0%)")]
    public AudioSource catHungryAudioSource;

    [Header("����� (��������� ����������)")]
    public AudioClip feedButtonSound;
    public AudioClip feedButtonHoverSound;
    public AudioClip cloudFreezeSound;
    [Tooltip("���� ��������� ����� (1-20%)")]
    public AudioClip bowlPulseSound;
    [Tooltip("���� �������� ����")]
    public AudioClip tearDropSound;

    [Header("��������� ������� ������ (� ��������)")]
    public float meowInterval = 5.0f;
    public float bowlPulseInterval = 1.2f;

    [Header("��������� �������� ���������")]
    public float pulseScaleAmount = 1.15f;
    public float pulseAnimationDuration = 0.2f;

    [Header("�������...")]
    public Sprite bowlFullSprite, bowlLowSprite, bowlEmptySprite;
    public Sprite cloudNormalSprite, cloudGreySprite;

    // --- ��������� ���������� ---
    private Vector3 originalBowlScale;
    private double feedCost = 10;
    private float costIncreaseMultiplier = 1.15f;
    private float feedAmount = 50f;
    private bool isCloudFrozen = false;
    private Coroutine meowCoroutine, bowlPulseCoroutine;

    // --- ������ UNITY ---

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

    // --- ������� ������ ��������� ---
    private void UpdateHungerEffects(float satietyPercentage)
    {
        // ��������� 1: ����� 0%
        if (satietyPercentage <= 0)
        {
            // ������
            bowlImage.sprite = bowlEmptySprite;
            cloudImage.sprite = cloudGreySprite;
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 2);

            // ����� � ���������
            StopAndNullifyCoroutine(ref bowlPulseCoroutine);
            StartCoroutineIfNotRunning(ref meowCoroutine, PlayMeowSoundRepeatedly());
            if (!isCloudFrozen)
            {
                AudioManager.Instance.PlaySound(cloudFreezeSound);
                isCloudFrozen = true;
            }
        }
        // --- ��������: ������ ��� 1-20% ������� ---
        // ��������� 2: ����� 1-20%
        else if (satietyPercentage <= 0.20f)
        {
            // ������
            bowlImage.sprite = bowlLowSprite;
            cloudImage.sprite = cloudNormalSprite;
            // ������ �����, �� �������� ������ ��������
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 1);

            // ����� � ���������
            StopAndNullifyCoroutine(ref meowCoroutine);
            StartCoroutineIfNotRunning(ref bowlPulseCoroutine, PulseAndPlaySound(bowlImage.transform, originalBowlScale, bowlPulseSound, bowlPulseInterval));
            isCloudFrozen = false;
        }
        // ��������� 3: ��� � �������
        else
        {
            // ������
            bowlImage.sprite = bowlFullSprite;
            cloudImage.sprite = cloudNormalSprite;
            if (catAnimator != null) catAnimator.SetInteger("CryingState", 0);

            // ����� � ���������: ��������� ��
            StopAndNullifyCoroutine(ref meowCoroutine);
            StopAndNullifyCoroutine(ref bowlPulseCoroutine);
            isCloudFrozen = false;
        }
    }

    // --- ����� ��������� ����� ��� ������ �� �������� ---
    /// <summary>
    /// ������� ����� � ��������� ����� � ����������� ���� �������.
    /// ���������� ����� Animation Event.
    /// </summary>
    /// <param name="side">0 ��� ����� �������, 1 ��� ������</param>
    public void DropTear(int side)
    {
        // !! ������� �������� !!
        // ���� ������� ������ ����, �� �� ������ �� ������ � ������ ������� �� �������.
        if (gameManager.GetSatietyPercentage() > 0)
        {
            return;
        }

        // ���� ��� ���������� ������ ���� ������� ����� 0.
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