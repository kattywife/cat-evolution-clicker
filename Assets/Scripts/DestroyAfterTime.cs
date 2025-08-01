using UnityEngine;
using System.Collections; // ����������� �������� ��� ������ ��� ������ � ����������!

public class DestroyAfterTime : MonoBehaviour
{
    [Tooltip("����� ����� ����� ������� � ��������")]
    public float lifeTime = 3f;

    [Tooltip("�� ������� ������ �� ����������� ������ ������ ������ ���������")]
    public float fadeDuration = 1f;

    // ������ �� ���������, ������� �������� �� ��������� �������
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // ��� �������� ������� ����� ������� � ���������� ��� SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // ��������� ���� ������� "���������" ������������
        StartCoroutine(FadeOutAndDestroy());
    }

    // ��� "��������" - �������, ������� ����� ����������� ���� �������� �� �������
    private IEnumerator FadeOutAndDestroy()
    {
        // 1. ������� ���� �������� �����, ���� ����� ������ �����
        // �� �������� ����� �� ��������� �� ������ ������� �����
        float initialWaitTime = lifeTime - fadeDuration;

        // ��������, ��� ����� �������� �� �������������
        if (initialWaitTime > 0)
        {
            yield return new WaitForSeconds(initialWaitTime);
        }

        // 2. ������ �������� ������� �������� ������������
        float timer = 0f;
        Color startColor = spriteRenderer.color; // ���������� �������� ����

        // ���� ���� ����� ��������, ���� �� ������� �����, ���������� �� ���������
        while (timer < fadeDuration)
        {
            // ����������� ������ �� �����, ��������� � �������� �����
            timer += Time.deltaTime;

            // ����������� ����� ������������ (�����-�����).
            // Mathf.Lerp ������ �������� �������� �� 1 (��������� �����) �� 0 (�������)
            float newAlpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            // ��������� ����� ���� � ����� �������������
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            // ���� ���������� �����, ����� ���������� ����
            yield return null;
        }

        // 3. ����� ����, ��� ������ ���� ��������� ���������, ���������� ���
        Destroy(gameObject);
    }
}