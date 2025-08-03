// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Tooltip("�������� ����� ��� ��������� �������� (UI, ����� � �.�.)")]
    public AudioSource sfxSource;

    // --- ���� ��������� ---
    private bool isMuted = false;
    private const string MutePrefKey = "IsMuted"; // ���� ��� ���������� ��������� �����

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ��������� ����������� ��������� ����� ��� ������� ����
            isMuted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
            ApplyMuteState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ����� ��� ������������ ����� (��� ��������� � ������ ������)
    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // --- ����� ������ ��� ���������� ������ ---

    /// <summary>
    /// ����������� ��������� ����� (���/����).
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyMuteState();

        // ��������� ����� ������������ ����� ��������
        PlayerPrefs.SetInt(MutePrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ��������� ������� ��������� (isMuted) � ����������� �����.
    /// </summary>
    private void ApplyMuteState()
    {
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    /// <summary>
    /// ���������� ������� ��������� �����.
    /// </summary>
    public bool IsMuted()
    {
        return isMuted;
    }
}