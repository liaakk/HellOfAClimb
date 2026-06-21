using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MusicRegion : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("Fade")]
    [SerializeField] private float fadeOutTime = 1.5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        musicSource.volume = AudioSettings.Instance != null
            ? AudioSettings.Instance.musicVolume
            : 1f;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Default behavior if no stop zone exists.
        if (GetComponentsInChildren<MusicRegionStop>().Length == 0)
        {
            StartFadeOut();
        }
    }

    public void StartFadeOut()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float baseVolume = AudioSettings.Instance != null
            ? AudioSettings.Instance.musicVolume
            : 1f;

        float t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            musicSource.volume = baseVolume * (1f - t / fadeOutTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = baseVolume;
        fadeRoutine = null;
    }
}