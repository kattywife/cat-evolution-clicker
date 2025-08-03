using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("��������� �����")]
    [Tooltip("�������� ����� ��� ��������� �������� (UI, ����� � �.�.)")]
    public AudioSource sfxSource;

    // --- ���������: ��������� �������� ��� ������� ������ ---
    [Tooltip("�������� ����� ��� ������� ������")]
    public AudioSource musicSource;

    private bool isMuted = false;
    private const string MutePrefKey = "IsMuted";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            isMuted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
            ApplyMuteState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ����� ��� �������� ������ ������� ��� ���������
    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // --- ���������: ����� ����� ��� ������������ ������� ������ ---
    /// <summary>
    /// ����������� ������� ������. ������������� ����������, ���� ��� ������.
    /// </summary>
    /// <param name="musicClip">��������� ��� ������������.</param>
    public void PlayMusic(AudioClip musicClip)
    {
        // ���������, ��� ���� � ��������, � ����
        if (musicClip != null && musicSource != null)
        {
            // ���� ��� ������ �� �� ������, ������ �� ������
            if (musicSource.clip == musicClip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.Stop(); // ������������� ������� ������
            musicSource.clip = musicClip; // ��������� ����� ����
            musicSource.loop = true; // ������ ������ �����������
            musicSource.Play(); // ��������!
        }
    }


    // --- ���������� ����� ������ (��� ���������) ---

    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyMuteState();

        PlayerPrefs.SetInt(MutePrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyMuteState()
    {
        // AudioListener.volume - ��� ���������� ���������, ��� �������� �� ��� ��������� �����
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    public bool IsMuted()
    {
        return isMuted;
    }
}