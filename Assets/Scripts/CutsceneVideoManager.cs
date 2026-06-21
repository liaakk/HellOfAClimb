using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutsceneVideoManager : MonoBehaviour
{
    public static CutsceneVideoManager Instance;

    [Header("UI")]
    public GameObject cutscenePanel;
    public Image blackBackground;
    public RawImage videoImage;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RenderTexture videoRenderTexture;

    [Header("Tempos")]
    public float delayBeforeVideo = 1f;
    public float blackFadeDuration = 0.8f;
    public float videoFadeDuration = 0.8f;

    private bool isPlaying = false;
    private bool isEnding = false;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        SetBlackAlpha(0f);

        if (videoImage != null)
        {
            videoImage.enabled = false;
            SetVideoAlpha(0f);
        }

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
        isEnding = false;

        // Espera antes da cutscene começar
        yield return new WaitForSecondsRealtime(delayBeforeVideo);

        // Pausa o jogo
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        SetBlackAlpha(0f);

        if (videoImage != null)
        {
            videoImage.enabled = false;
            SetVideoAlpha(0f);
        }

        // Fade in do preto
        yield return FadeBlack(0f, 1f);

        // Prepara o vídeo
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // Mostra o vídeo invisível primeiro
        if (videoImage != null)
        {
            videoImage.enabled = true;
            SetVideoAlpha(0f);
        }

        videoPlayer.Play();

        // Fade in do vídeo por cima do preto
        yield return FadeVideo(0f, 1f);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (isEnding) return;

        isEnding = true;
        StartCoroutine(EndCutsceneRoutine());
    }

    private IEnumerator EndCutsceneRoutine()
    {
        // Fade out do vídeo
        if (videoImage != null)
            yield return FadeVideo(1f, 0f);

        if (videoPlayer != null)
            videoPlayer.Stop();

        if (videoImage != null)
            videoImage.enabled = false;

        // Fade out do fundo preto
        yield return FadeBlack(1f, 0f);

        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        Time.timeScale = previousTimeScale;

        isPlaying = false;
        isEnding = false;
    }

    private IEnumerator FadeBlack(float from, float to)
    {
        if (blackBackground == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < blackFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / blackFadeDuration;
            float alpha = Mathf.Lerp(from, to, t);

            SetBlackAlpha(alpha);

            yield return null;
        }

        SetBlackAlpha(to);
    }

    private IEnumerator FadeVideo(float from, float to)
    {
        if (videoImage == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < videoFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / videoFadeDuration;
            float alpha = Mathf.Lerp(from, to, t);

            SetVideoAlpha(alpha);

            yield return null;
        }

        SetVideoAlpha(to);
    }

    private void SetBlackAlpha(float alpha)
    {
        if (blackBackground == null) return;

        Color color = blackBackground.color;
        color.a = alpha;
        blackBackground.color = color;
    }

    private void SetVideoAlpha(float alpha)
    {
        if (videoImage == null) return;

        Color color = videoImage.color;
        color.a = alpha;
        videoImage.color = color;
    }
}