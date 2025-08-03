// InteractiveButton.cs
// ������ ��� ���������� "�����" ������� ����� ���.

using UnityEngine;
using UnityEngine.EventSystems; // ����������� ��� ������ � ��������� ����
using UnityEngine.UI;           // ����������� ��� ������ � UI

[RequireComponent(typeof(Button))] // �����������, ��� �� ������� ���� ��������� Button
public class InteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("��������� ��������")]
    [Tooltip("���� ��� ��������� ������� �� ������")]
    public AudioClip hoverSound;

    [Tooltip("���� ��� ������� �� ������")]
    public AudioClip clickSound;

    [Tooltip("��������� ������ ���������� ��� ������� (1.1 = 110%)")]
    public float pressedScale = 1.1f;

    // ��������� ���������� ��� �������� ���������
    private RectTransform rectTransform;
    private Vector3 originalScale;

    // Awake ���������� ���� ��� ��� �������� �������
    private void Awake()
    {
        // �������� ���������� ��� ������������������, ����� �� ������ �� ������ ���
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    // --- ���������� ����������� EventSystem ---

    // ����������, ����� ������ ���� ������ � ������� ������
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            // ����������� ���� ����� ��� AudioManager
            AudioManager.Instance.PlaySound(hoverSound, 0.7f); // ��������� ����� ���������
        }
    }

    // ���������� � ������, ����� ������ ���� ������ ��� ��������
    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.localScale = originalScale * pressedScale;
    }

    // ���������� � ������, ����� ������ ���� �������� ��� ��������
    public void OnPointerUp(PointerEventData eventData)
    {
        rectTransform.localScale = originalScale;
    }

    // ����������, ����� ������ �������� ������� ������
    // ��� �����, ����� ������ ��������� � ���������� ������, 
    // ���� ������������ �����, �� ��� ���� � ������� � ��������
    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.localScale = originalScale;
    }

    // ���������� ��� ������ ����� (����� � �������� �� ����� �������)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            AudioManager.Instance.PlaySound(clickSound);
        }
    }
}