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
    private Coroutine introCoroutine;

    [Header("Концовка")]
    public float endingDelay = 3.0f;
    public float endingFadeDuration = 2.0f;
    public VideoPlayer endingVideoPlayer;
    public GameObject postVideoUI;
    public AudioClip endingMusic;

    private bool isWatchingEnding = false;

    private bool endingPreloaded = false;

    // Этот метод мы вызовем на 7 уровне
    public void PreloadEndingVideo(string fileName)
    {
        if (endingPreloaded || endingVideoPlayer == null) return;

        Debug.Log("<color=magenta>[CutsceneManager]</color> Начинаю фоновую предзагрузку финала...");
        
        string videoPath = Path.Combine(Application.streamingAssetsPath, fileName);
        endingVideoPlayer.source = VideoSource.Url;
        endingVideoPlayer.url = videoPath;
        
        // Подготавливаем, но НЕ играем
        endingVideoPlayer.Prepare(); 
        endingPreloaded = true;
    }

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
            StartCoroutine(PlayLoadingSequence("loading.webm"));
        else if (enableIntro && !hasWatchedIntro && introPanel)
            introCoroutine = StartCoroutine(StartIntroSequence("intro.webm"));
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
        Debug.Log("<color=white>[CutsceneManager]</color> Шаг 1: Инициализация данных Яндекса.");
        
        // Включаем панель загрузки
        loadingPanel.SetActive(true);
        
        if (loadingProgressBarObject) loadingProgressBarObject.SetActive(false); 
        if (loadingStartButton) loadingStartButton.SetActive(false);
        
        isStartButtonClicked = false;

        // --- ВОЗВРАЩАЕМ ВИДЕО В UNITY ---
        // Это запустит воспроизведение котика внутри игры, убирая черный квадрат
        if (loadingCatVideoPlayer)
        {
            yield return StartCoroutine(PrepareVideoSafe(loadingCatVideoPlayer, videoName));
            if (loadingCatVideoPlayer.isPrepared) loadingCatVideoPlayer.Play();
        }

        // Ждем готовности Яндекс SDK
        if (YandexManager.Instance)
        {
            yield return new WaitUntil(() => YandexManager.Instance.isSdkReady);
        }

        // Ждем, пока SaveManager загрузит данные игрока из облака Яндекса
        if (SaveManager.Instance)
        {
            yield return new WaitUntil(() => SaveManager.Instance.isDataLoaded);
        }

        // Как только всё готово — сообщаем Яндексу, что игра полностью готова к показу
        if (YandexManager.Instance) 
        {
            YandexManager.Instance.ReportGameReady();
        }

        // Показываем кнопку «СТАРТ»
        if (loadingStartButton) loadingStartButton.SetActive(true);

        // Ждем клика по кнопке Старт от игрока
        yield return new WaitUntil(() => isStartButtonClicked);

        // Переходим к интро или геймплею
        if (enableIntro && !hasWatchedIntro && introPanel)
        {
            yield return StartCoroutine(FadeWhite(true, 0.5f));
            loadingPanel.SetActive(false);
            if (loadingCatVideoPlayer) loadingCatVideoPlayer.Stop();
            introCoroutine = StartCoroutine(StartIntroSequence("intro.webm"));
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

    public void SkipIntro()
    {
        // Если интро сейчас не идет, ничего не делаем
        if (introCoroutine == null) return;

        Debug.Log("<color=yellow>[CutsceneManager]</color> Игрок пропустил интро.");

        // 1. Останавливаем корутину ожидания видео
        StopCoroutine(introCoroutine);
        introCoroutine = null;

        // 2. Останавливаем сам плеер
        if (introVideoPlayer) 
            introVideoPlayer.Stop();

        // 3. Записываем, что интро просмотрено, и сохраняем игру
        hasWatchedIntro = true;
        if (SaveManager.Instance) 
            SaveManager.Instance.Save();

        // 4. Скрываем панель интро и запускаем игру
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
            // 1. Сначала включаем панель игры. 
            // Все звуки/эффекты ТЕПЕРЬ МОЛЧАТ, потому что мы сняли Play On Awake.
            GameManager.Instance.mainGamePanel.SetActive(true);
            
            // 2. Плавно убираем белый экран (белый становится прозрачным)
            yield return StartCoroutine(FadeWhite(false, 1.0f));
            
            // 3. ВОТ ТЕПЕРЬ, когда экран прозрачный, запускаем мяуканье и эффекты!
            GameManager.Instance.PlayStartEffects();

            // 4. Включаем остальную логику игры
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
            // Если видео еще не готово (например, игрок ОЧЕНЬ быстро прошел с 8 по 10 уровень)
            if (!endingVideoPlayer.isPrepared)
            {
                yield return StartCoroutine(PrepareVideoSafe(endingVideoPlayer, videoName));
            }
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

    /* private IEnumerator PrepareVideoSafe(VideoPlayer vp, string fileName)
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
    } */

    private IEnumerator PrepareVideoSafe(VideoPlayer vp, string fileName)
    {
        string videoPath = Path.Combine(Application.streamingAssetsPath, fileName);
        vp.source = VideoSource.Url;
        vp.url = videoPath;
        
        // Вместо долгого ожидания Prepare, мы просто даем команду Play.
        // В WebGL VideoPlayer сам начнет буферизацию.
        vp.Prepare();

        float timeout = 10.0f; // Уменьшим таймаут
        while (!vp.isPrepared && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
        
        // Если за 10 секунд не подготовилось (плохой интернет), 
        // всё равно пробуем играть, иначе игрок зависнет на черном экране.
        if (!vp.isPrepared) Debug.LogWarning("Video prep timed out, trying to play anyway...");
    }

    #endregion
}