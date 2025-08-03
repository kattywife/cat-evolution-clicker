// SoundToggleButton.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // �������� ��� ������������ ��������� ����
using System.Collections;       // �������� ��� ������������� �������

// ��������� ���������� ��� ������������ �������
[RequireComponent(typeof(Button))]
public class SoundToggleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("������ ���������")]
    [Tooltip("������, ����� ���� �������")]
    public Sprite soundOnSprite;

    [Tooltip("������, ����� ���� ��������")]
    public Sprite soundOffSprite;

    [Header("������")]
    [Tooltip("Image ���������, �� ������� ����� �������� ������")]
    public Image iconImage;

    // --- ����� ���� ��� �������� � ������ ---
    [Header("��������� �������� ���������")]
    [Tooltip("��������� ������ ���������� ��� ��������� (1.1 = �� 10%)")]
    public float hoverScaleFactor = 1.1f;

    [Tooltip("�������� �������� ����������/����������")]
    public float scaleDuration = 0.1f;

    [Header("�����")]
    [Tooltip("���� ��� ��������� ������� �� ������")]
    public AudioClip hoverSound;

    [Tooltip("���� ��� ������� �� ������")]
    public AudioClip clickSound;
    // --- ����� ����� ����� ---

    private Button toggleButton;
    private Vector3 originalScale; // ��������� �������� ������

    void Awake()
    {
        // ���������� ������������ ������ ������ ���� ��� ��� �������
        originalScale = transform.localScale;
    }

    void Start()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(OnButtonClick);

        // ������������� ���������� ������ ��� ������� ����
        UpdateIcon();
    }

    void OnButtonClick()
    {
        // --- �������� ���� ����� ---
        if (clickSound != null)
        {
            AudioManager.Instance.PlaySound(clickSound);
        }

        // ������� AudioManager'� ����������� ����
        AudioManager.Instance.ToggleMute();

        // ��������� ������ �� ������
        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (iconImage == null || AudioManager.Instance == null)
        {
            Debug.LogWarning("�� ��������� ������ ��� �� ������ AudioManager!");
            return;
        }

        if (AudioManager.Instance.IsMuted())
        {
            iconImage.sprite = soundOffSprite;
        }
        else
        {
            iconImage.sprite = soundOnSprite;
        }
    }

    // --- ����� ������ ��� ��������������� ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ����� ������ ������� �� ������
        if (hoverSound != null)
        {
            AudioManager.Instance.PlaySound(hoverSound);
        }

        // ��������� �������� ����������
        StopAllCoroutines(); // ������������� ������ ��������, ���� ��� ����
        StartCoroutine(AnimateScale(originalScale * hoverScaleFactor, scaleDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ����� ������ ���� � ������
        // ��������� �������� �������� � ��������� �������
        StopAllCoroutines();
        StartCoroutine(AnimateScale(originalScale, scaleDuration));
    }

    private IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}