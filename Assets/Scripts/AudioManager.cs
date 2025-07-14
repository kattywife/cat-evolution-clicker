// AudioManager.cs

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // ��� "��������" - ������� ������ ������� �������� ��������� �� ������ ������� �������.
    public static AudioManager Instance;

    // ���� �� ��������� ���������, ������� ����� ����������� �����.
    [Tooltip("�������� ����� ��� ��������� �������� (UI, ����� � �.�.)")]
    public AudioSource sfxSource;

    private void Awake()
    {
        // ��������� ���������
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ����� �������� �� �������� ��� ����� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ��������� �����, ������� ������ ������� ����� ������ ������
    public void PlaySound(AudioClip clip)
    {
        // ���������, ��� ���� � ����, � ��������, ����� �������� ������
        if (clip != null && sfxSource != null)
        {
            // PlayOneShot ��������� ����������� �����, �� �������� ���� �����.
            // �������� ��� ������ ������!
            sfxSource.PlayOneShot(clip);
        }
    }
}