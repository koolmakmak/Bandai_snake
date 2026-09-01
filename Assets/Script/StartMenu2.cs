using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartMenu : MonoBehaviour
{
    [Header("How to Play panel")]
    public GameObject howToPlayPanel;

    [Header("Exit Cutscene")]
    public VideoClip exitCutsceneClip;

    VideoPlayer exitVideoPlayer;
    AudioSource exitAudioSource;
    GameObject exitCutsceneCanvas;
    bool exitCutscenePlayed = false;
    bool cutscenePlaying = false;
    bool exitCutsceneClosing = false;

    void Start()
    {
        // Hide the How to Play canvas by default.
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene("FinalScene");
    }

    public void ToggleHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(!howToPlayPanel.activeSelf);
    }

    public void CloseHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    public void Exit()
    {
        // First click: play the goodbye cutscene, then return to menu.
        // Second click: actually quit.
        if (cutscenePlaying || exitCutsceneClosing)
            return;

        if (!exitCutscenePlayed)
        {
            exitCutscenePlayed = true;
            PlayExitCutscene();
        }
        else
        {
            QuitGame();
        }
    }

    void PlayExitCutscene()
    {
        if (exitCutsceneClip == null)
        {
            QuitGame();
            return;
        }

        if (cutscenePlaying || exitCutsceneClosing)
            return;

        BuildExitCutsceneUI();
        exitVideoPlayer.prepareCompleted += OnExitVideoPrepared;
        exitVideoPlayer.Prepare();
    }

    void OnExitVideoPrepared(VideoPlayer vp)
    {
        if (exitCutsceneClosing || vp == null)
            return;

        cutscenePlaying = true;
        vp.Play();
    }

    void BuildExitCutsceneUI()
    {
        if (exitVideoPlayer != null)
        {
            exitVideoPlayer.prepareCompleted -= OnExitVideoPrepared;
            exitVideoPlayer.loopPointReached -= OnExitCutsceneEnd;
            exitVideoPlayer.Stop();
            Destroy(exitVideoPlayer);
            exitVideoPlayer = null;
        }

        if (exitAudioSource != null)
        {
            exitAudioSource.Stop();
            Destroy(exitAudioSource);
            exitAudioSource = null;
        }

        if (exitCutsceneCanvas != null)
        {
            Destroy(exitCutsceneCanvas);
            exitCutsceneCanvas = null;
        }

        // Video player
        exitVideoPlayer = gameObject.AddComponent<VideoPlayer>();
        exitVideoPlayer.playOnAwake = false;
        exitVideoPlayer.source = VideoSource.VideoClip;
        exitVideoPlayer.clip = exitCutsceneClip;
        exitVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        exitVideoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
        exitVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        exitVideoPlayer.isLooping = false;
        exitVideoPlayer.waitForFirstFrame = true;
        exitVideoPlayer.skipOnDrop = true;
        exitVideoPlayer.loopPointReached += OnExitCutsceneEnd;

        // Play the video's own audio track through the AudioSource.
        exitVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        // Audio source for the video's own sound.
        exitAudioSource = gameObject.AddComponent<AudioSource>();
        exitAudioSource.playOnAwake = false;
        exitAudioSource.loop = false;
        exitAudioSource.volume = 1f;
        exitVideoPlayer.SetTargetAudioSource(0, exitAudioSource);

        // Fullscreen overlay canvas
        GameObject canvasGO = new GameObject("ExitCutsceneCanvas");
        exitCutsceneCanvas = canvasGO;
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Black background behind the video (visible while loading)
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bg = bgGO.AddComponent<Image>();
        bg.color = Color.black;
        RectTransform bgRect = bg.rectTransform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Video display (white color so the video is not tinted)
        GameObject rawGO = new GameObject("Video");
        rawGO.transform.SetParent(canvasGO.transform, false);
        RawImage raw = rawGO.AddComponent<RawImage>();
        raw.color = Color.white;
        raw.texture = exitVideoPlayer.targetTexture;
        RectTransform r = raw.rectTransform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    void OnExitCutsceneEnd(VideoPlayer vp)
    {
        CloseExitCutscene();
    }

    void CloseExitCutscene()
    {
        if (exitCutsceneClosing)
            return;

        exitCutsceneClosing = true;
        cutscenePlaying = false;

        if (exitVideoPlayer != null)
        {
            exitVideoPlayer.prepareCompleted -= OnExitVideoPrepared;
            exitVideoPlayer.loopPointReached -= OnExitCutsceneEnd;
            exitVideoPlayer.Stop();
            if (exitVideoPlayer.targetTexture != null)
            {
                exitVideoPlayer.targetTexture.Release();
                exitVideoPlayer.targetTexture = null;
            }
            Destroy(exitVideoPlayer);
            exitVideoPlayer = null;
        }

        if (exitAudioSource != null)
        {
            exitAudioSource.Stop();
            Destroy(exitAudioSource);
            exitAudioSource = null;
        }

        if (exitCutsceneCanvas != null)
        {
            Destroy(exitCutsceneCanvas);
            exitCutsceneCanvas = null;
        }

        exitCutsceneClosing = false;
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
