using UnityEngine;
using TMPro; // ����������� �������� ��� ������ ��� ������ � TextMeshPro

public class FloatingText : MonoBehaviour
{
    [Tooltip("��� ������ ����� ����� ������ (��� �����������, ���� �������� �������������)")]
    public float moveSpeed = 150f;

    [Tooltip("��� ������ ����� ����� ��������")]
    public float fadeSpeed = 1f;

    [Tooltip("������� ������ ����� �������� ����� ������������")]
    public float lifeTime = 1f;

    // ������ �� ��������� ������, ����� ������ ��� ���������� � ����
    private TextMeshProUGUI textMesh;
    private Color startColor;

    void Awake()
    {
        // ������� ��������� ������ � ���������� ��� �������� ����
        textMesh = GetComponent<TextMeshProUGUI>();
        startColor = textMesh.color;
    }

    void Start()
    {
        // ����� ����� �������� ��������� ������ �� ���������������
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. �������� ������ ����
        // ������� ������ ���� �� ��������� moveSpeed �������� � �������
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        // 2. ������� ������������
        // ��������� �����-����� (������������) ����� �� ��������
        startColor.a -= fadeSpeed * Time.deltaTime;

        // ��������� �����, ����� ���������� ���� � ������
        textMesh.color = startColor;
    }
}