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
    public AudioClip exitCutsceneAudio; // optional separate audio for the cutscene

    VideoPlayer exitVideoPlayer;
    AudioSource exitAudioSource;
    GameObject exitCutsceneCanvas;
    bool exitCutscenePlayed = false;
    bool cutscenePlaying = false;

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

        BuildExitCutsceneUI();
        StartCoroutine(PlayExitCutsceneRoutine());
    }

    System.Collections.IEnumerator PlayExitCutsceneRoutine()
    {
        exitVideoPlayer.Prepare();

        float timeout = 5f;
        while (!exitVideoPlayer.isPrepared && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        cutscenePlaying = true;
        exitVideoPlayer.Play();

        if (exitAudioSource != null && exitCutsceneAudio != null)
            exitAudioSource.Play();
    }

    void BuildExitCutsceneUI()
    {
        // Video player
        exitVideoPlayer = gameObject.AddComponent<VideoPlayer>();
        exitVideoPlayer.playOnAwake = false;
        exitVideoPlayer.source = VideoSource.VideoClip;
        exitVideoPlayer.clip = exitCutsceneClip;
        exitVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        exitVideoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
        exitVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        exitVideoPlayer.isLooping = false;
        exitVideoPlayer.loopPointReached += OnExitCutsceneEnd;

        // Mute the video's own audio to avoid AudioSampleProvider buffer overflow.
        exitVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        exitVideoPlayer.SetDirectAudioMute(0, true);

        // Separate audio source for the cutscene sound.
        exitAudioSource = gameObject.AddComponent<AudioSource>();
        exitAudioSource.playOnAwake = false;
        exitAudioSource.loop = false;
        exitAudioSource.clip = exitCutsceneAudio;
        exitAudioSource.volume = 1f;

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

    void Update()
    {
        // Fallback: close the cutscene when the video ends, even if loopPointReached never fires.
        if (!cutscenePlaying || exitVideoPlayer == null || exitVideoPlayer.frame <= 0)
            return;

        bool ended = !exitVideoPlayer.isPlaying;
        if (!ended && exitVideoPlayer.frameCount > 0)
            ended = exitVideoPlayer.frame >= (long)exitVideoPlayer.frameCount - 1;

        if (ended)
            CloseExitCutscene();
    }

    void CloseExitCutscene()
    {
        cutscenePlaying = false;

        if (exitVideoPlayer != null)
        {
            exitVideoPlayer.Stop();
            if (exitVideoPlayer.targetTexture != null)
            {
                exitVideoPlayer.targetTexture.Release();
                exitVideoPlayer.targetTexture = null;
            }
        }

        if (exitAudioSource != null)
            exitAudioSource.Stop();

        if (exitCutsceneCanvas != null)
        {
            Destroy(exitCutsceneCanvas);
            exitCutsceneCanvas = null;
        }
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
