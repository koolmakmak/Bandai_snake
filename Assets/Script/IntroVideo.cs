using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroVideo : MonoBehaviour
{
    [Header("Video Intro")]
    public VideoClip videoClip;

    [Header("Game Music")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    VideoPlayer videoPlayer;
    AudioSource musicSource;
    GameObject introCanvas;

    void Start()
    {
        BuildMusic();

        if (videoClip == null)
        {
            PlayMusic();
            return;
        }

        BuildIntroUI();
        if (videoPlayer != null)
            videoPlayer.Play();
    }

    void BuildMusic()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
    }

    void PlayMusic()
    {
        if (musicSource != null && musicClip != null)
            musicSource.Play();
    }

    void Update()
    {
        // Apply the inspector slider live during play.
        if (musicSource != null && !Mathf.Approximately(musicSource.volume, musicVolume))
            musicSource.volume = musicVolume;
    }

    void BuildIntroUI()
    {
        // Video player (renders to a RenderTexture shown in the UI)
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
        videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoEnd;

        // Canvas on top of the game UI
        GameObject canvasGO = new GameObject("IntroCanvas");
        introCanvas = canvasGO;
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Fullscreen black panel
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelImg = panelGO.AddComponent<Image>();
        panelImg.color = Color.black;
        RectTransform p = panelImg.rectTransform;
        p.anchorMin = Vector2.zero;
        p.anchorMax = Vector2.one;
        p.offsetMin = Vector2.zero;
        p.offsetMax = Vector2.zero;

        // Video display
        GameObject rawGO = new GameObject("Video");
        rawGO.transform.SetParent(panelGO.transform, false);
        RawImage raw = rawGO.AddComponent<RawImage>();
        raw.texture = videoPlayer.targetTexture;
        RectTransform r = raw.rectTransform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        // Skip button (upper-left)
        GameObject btnGO = new GameObject("SkipButton");
        btnGO.transform.SetParent(panelGO.transform, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0f, 0f, 0f, 0.6f);
        Button btn = btnGO.AddComponent<Button>();
        RectTransform b = btnGO.GetComponent<RectTransform>();
        b.anchorMin = new Vector2(0f, 1f);
        b.anchorMax = new Vector2(0f, 1f);
        b.pivot = new Vector2(0f, 1f);
        b.anchoredPosition = new Vector2(24f, -24f);
        b.sizeDelta = new Vector2(180f, 64f);
        btn.onClick.AddListener(SkipIntro);

        GameObject txtGO = new GameObject("Label");
        txtGO.transform.SetParent(btnGO.transform, false);
        Text label = txtGO.AddComponent<Text>();
        label.text = "Skip";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        RectTransform t = label.rectTransform;
        t.anchorMin = Vector2.zero;
        t.anchorMax = Vector2.one;
        t.offsetMin = Vector2.zero;
        t.offsetMax = Vector2.zero;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SkipIntro();
    }

    public void SkipIntro()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            if (videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.Release();
                videoPlayer.targetTexture = null;
            }
        }

        if (introCanvas != null)
        {
            Destroy(introCanvas);
            introCanvas = null;
        }

        PlayMusic();
    }
}
