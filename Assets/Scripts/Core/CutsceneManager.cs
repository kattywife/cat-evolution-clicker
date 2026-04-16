using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.IO;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("Настройки")]
    public bool enableLoadingScreen = true;
    public bool enableIntro = true;
    [HideInInspector] public bool hasWatchedIntro = false;

    [Header("Панели (Объекты)")]
    public GameObject loadingPanel;
    public GameObject introPanel;
    public GameObject endingPanel;
    public GameObject whiteFadePanel;

    [Header("Экран Загрузки")]
    public Image loadingFillImage;
    public GameObject loadingStartButton;
    public VideoPlayer loadingCatVideoPlayer;
    public GameObject loadingProgressBarObject;
    public float loadingDuration = 3.0f;
    private bool isStartButtonClicked = false;

    [Header("Интро")]
    public VideoPlayer introVideoPlayer;
    public float introFadeDuration = 1.5f;

    [Header("Концовка")]
    public float endingDelay = 3.0f;
    public float endingFadeDuration = 2.0f;
    public VideoPlayer endingVideoPlayer;
    public GameObject postVideoUI;
    public AudioClip endingMusic;

    private bool isWatchingEnding = false;

    private void Awake()
    {
        Instance = this;
        isWatchingEnding = false; // Сбрасываем флаг при создании
        ForceHideAll();
    }

    public void ForceHideAll()
    {
        if (loadingPanel) loadingPanel.SetActive(false);
        if (introPanel) introPanel.SetActive(false);
        if (endingPanel) endingPanel.SetActive(false);
        if (whiteFadePanel) whiteFadePanel.SetActive(false);
        
        if (GameManager.Instance && GameManager.Instance.mainGamePanel)
            GameManager.Instance.mainGamePanel.SetActive(false);
            
        Debug.Log("<color=yellow>[CutsceneManager]</color> Все панели принудительно скрыты.");
    }

    public void StartGameFlow()
    {
        Debug.Log("<color=cyan>[CutsceneManager]</color> Старт логики.");
        if (enableLoadingScreen && loadingPanel)
            StartCoroutine(PlayLoadingSequence("loading.mp4"));
        else if (enableIntro && !hasWatchedIntro && introPanel)
            StartCoroutine(StartIntroSequence("intro.mp4"));
        else
            StartCoroutine(FinishSequences());
    }

    #region --- ЛОГИКА ЗАГРУЗКИ ---

    public void OnLoadingStartClicked()
    {
        isStartButtonClicked = true;
        if (GameManager.Instance && GameManager.Instance.catClickSound)
            AudioManager.Instance.PlaySound(GameManager.Instance.catClickSound);
    }


    public IEnumerator PlayLoadingSequence(string videoName)
    {
        Debug.Log("<color=white>[CutsceneManager]</color> Шаг 1: Экран загрузки.");
        loadingPanel.SetActive(true);
        
        // В начале загрузки: полоска включена, кнопка выключена
        if (loadingProgressBarObject) loadingProgressBarObject.SetActive(true);
        if (loadingStartButton) loadingStartButton.SetActive(false);
        
        isStartButtonClicked = false;

        if (loadingCatVideoPlayer)
        {
            yield return StartCoroutine(PrepareVideoSafe(loadingCatVideoPlayer, videoName));
            if (loadingCatVideoPlayer.isPrepared) loadingCatVideoPlayer.Play();
        }

        float timer = 0f;
        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;
            if (loadingFillImage) loadingFillImage.fillAmount = timer / loadingDuration;
            yield return null;
        }
        
        // --- ВОТ ИЗМЕНЕНИЕ ТУТ ---
        // 1. Прячем полоску прогресса
        if (loadingProgressBarObject) loadingProgressBarObject.SetActive(false);
        
        // 2. Показываем кнопку Старт
        if (loadingStartButton) loadingStartButton.SetActive(true);

        if (YandexManager.Instance) YandexManager.Instance.ReportGameReady();

        // Ждем клика
        yield return new WaitUntil(() => isStartButtonClicked);

        // Дальше логика интро или игры...
        if (enableIntro && !hasWatchedIntro && introPanel)
        {
            yield return StartCoroutine(FadeWhite(true, 0.5f));
            loadingPanel.SetActive(false);
            if (loadingCatVideoPlayer) loadingCatVideoPlayer.Stop();
            StartCoroutine(StartIntroSequence("intro.mp4"));
        }
        else
        {
            loadingPanel.SetActive(false);
            StartCoroutine(FinishSequences());
        }
    }


    #endregion

    #region --- ИНТРО ---

    public IEnumerator StartIntroSequence(string videoName)
    {
        introPanel.SetActive(true);
        if (introVideoPlayer)
        {
            introVideoPlayer.isLooping = false;
            yield return StartCoroutine(PrepareVideoSafe(introVideoPlayer, videoName));
            if (introVideoPlayer.isPrepared) introVideoPlayer.Play();
        }

        yield return StartCoroutine(FadeWhite(false, 0.8f));

        if (introVideoPlayer && introVideoPlayer.isPrepared)
        {
            while (!introVideoPlayer.isPlaying) yield return null;
            while (introVideoPlayer.isPlaying) yield return null;
        }

        hasWatchedIntro = true;
        if (SaveManager.Instance) SaveManager.Instance.Save();

        yield return StartCoroutine(FadeWhite(true, introFadeDuration));
        introPanel.SetActive(false);
        StartCoroutine(FinishSequences());
    }

    #endregion

    #region --- ЗАВЕРШЕНИЕ И ГЕЙМПЛЕЙ ---

    private IEnumerator FinishSequences()
    {
        Debug.Log("<color=green>[CutsceneManager]</color> Включаю геймплей.");

        if (GameManager.Instance)
        {
            // Включаем панель игры ДО того, как уберем белый экран
            GameManager.Instance.mainGamePanel.SetActive(true);
            
            yield return StartCoroutine(FadeWhite(false, 1.0f));
            
            GameManager.Instance.enabled = true;

            if (ProgressionManager.Instance) 
                ProgressionManager.Instance.UpdateUI(EconomyManager.Instance.score);
        }
    }

    #endregion

    #region --- КОНЦОВКА ---

    // Главная точка входа для финала
    public void StartEndingSequence(string videoName)
    {
        Debug.Log("<color=magenta>[CutsceneManager]</color> Начинаю фоновую подготовку видео финала...");
        StartCoroutine(PrepareAndSwapToEnding(videoName));
    }

    private IEnumerator PrepareAndSwapToEnding(string videoName)
    {
        if (endingPanel != null)
        {
            // 1. ВКЛЮЧАЕМ объект (чтобы плеер мог готовиться)
            endingPanel.SetActive(true);

            // 2. Делаем его НЕВИДИМЫМ через CanvasGroup
            CanvasGroup cg = endingPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = endingPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0; 

            if (postVideoUI != null) postVideoUI.SetActive(false); 

        }

        if (endingVideoPlayer)
        {
            // Убеждаемся, что сам компонент и объект плеера активны
            endingVideoPlayer.gameObject.SetActive(true); 
            endingVideoPlayer.isLooping = false;

            // Теперь Prepare сработает мгновенно, так как объект активен
            yield return StartCoroutine(PrepareVideoSafe(endingVideoPlayer, videoName));
        }

        // 3. Видео готово! Делаем мгновенную подмену
        Debug.Log("<color=magenta>[CutsceneManager]</color> Подготовка завершена. Включаю финал.");

        // Прячем игру
        if (GameManager.Instance && GameManager.Instance.mainGamePanel)
            GameManager.Instance.mainGamePanel.SetActive(false);

        // Делаем панель финала видимой
        if (endingPanel)
        {
            CanvasGroup cg = endingPanel.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1;
        }

        if (endingVideoPlayer)
        {
            endingVideoPlayer.Play();
            isWatchingEnding = true;
        }

        if (endingMusic) AudioManager.Instance.PlayMusic(endingMusic);
    }

    private IEnumerator PrepareAndPlayEnding(VideoPlayer vp, string videoName)
    {
        yield return StartCoroutine(PrepareVideoSafe(vp, videoName));
        if (vp.isPrepared)
        {
            vp.Play();
            isWatchingEnding = true;
        }
        else if (postVideoUI) postVideoUI.SetActive(true);
    }

    private void Update()
    {
        if (isWatchingEnding && endingVideoPlayer != null && endingVideoPlayer.isPlaying)
        {
            if (endingVideoPlayer.time >= endingVideoPlayer.length - 0.2f)
            {
                endingVideoPlayer.Pause();
                isWatchingEnding = false;
                if (postVideoUI) postVideoUI.SetActive(true);
            }
        }
    }

    #endregion

    #region --- УТИЛИТЫ ---

    public IEnumerator FadeWhite(bool fadeIn, float duration)
    {
        if (!whiteFadePanel) yield break;
        
        whiteFadePanel.SetActive(true);
        CanvasGroup cg = whiteFadePanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = whiteFadePanel.AddComponent<CanvasGroup>();

        float start = fadeIn ? 0 : 1;
        float end = fadeIn ? 1 : 0;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        cg.alpha = end;
        if (!fadeIn) whiteFadePanel.SetActive(false);
    }

    private IEnumerator PrepareVideoSafe(VideoPlayer vp, string fileName)
    {
        string videoPath = Path.Combine(Application.streamingAssetsPath, fileName);
        vp.source = VideoSource.Url;
        vp.url = videoPath;
        vp.Prepare();

        float timeout = 15.0f;
        while (!vp.isPrepared && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
    }

    #endregion
}