using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AmbienceRegion : MonoBehaviour
{
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private float fadeOutTime = 1.5f;
    [SerializeField] private float maxVolume = 1f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;
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

        ambienceSource.volume = maxVolume;

        if (!ambienceSource.isPlaying)
            ambienceSource.Play();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        StartFadeOut();
    }

    private void StartFadeOut()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float startVolume = ambienceSource.volume;
        float t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            ambienceSource.volume =
                Mathf.Lerp(startVolume, 0f, t / fadeOutTime);

            yield return null;
        }

        ambienceSource.Stop();
        ambienceSource.volume = maxVolume;
    }

}