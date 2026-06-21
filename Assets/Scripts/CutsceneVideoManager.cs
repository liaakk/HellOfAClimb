using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutsceneVideoManager : MonoBehaviour
{
    public static CutsceneVideoManager Instance;

    [Header("UI")]
    public GameObject cutscenePanel;
    public CanvasGroup fadeCanvasGroup;
    public RawImage videoImage;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RenderTexture videoRenderTexture;

    [Header("Tempos")]
    public float delayBeforeVideo = 1f;
    public float fadeDuration = 0.5f;

    private bool isPlaying = false;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        if (videoImage != null)
            videoImage.enabled = false;

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        if (videoImage != null && videoRenderTexture != null)
            videoImage.texture = videoRenderTexture;

        if (videoPlayer != null && videoRenderTexture != null)
            videoPlayer.targetTexture = videoRenderTexture;
    }

    public void PlayCutscene(VideoClip clip)
    {
        if (isPlaying) return;

        StartCoroutine(PlayCutsceneRoutine(clip));
    }

    private IEnumerator PlayCutsceneRoutine(VideoClip clip)
    {
        isPlaying = true;

        // Espera 1 segundo depois de apanhar o botão
        yield return new WaitForSecondsRealtime(delayBeforeVideo);

        // Pausa o jogo
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        if (videoImage != null)
            videoImage.enabled = false;

        // Fade para preto
        yield return Fade(0f, 1f);

        // Prepara e toca o vídeo
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        if (videoImage != null)
            videoImage.enabled = true;

        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(EndCutsceneRoutine());
    }

    private IEnumerator EndCutsceneRoutine()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (videoImage != null)
            videoImage.enabled = false;

        // Fecha instantaneamente, como pediste
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        Time.timeScale = previousTimeScale;

        isPlaying = false;

        yield return null;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / fadeDuration;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        fadeCanvasGroup.alpha = to;
    }
}