using UnityEngine;

public class ProximitySound : MonoBehaviour
{
    public Transform player;
    public AudioSource audioSource;
    public float maxVolume = 1f;

    public float maxDistance = 10f;

    void Update()
    {
        float distance = Vector2.Distance(
            player.position,
            transform.position
        );

        if (distance > maxDistance)
        {
            audioSource.volume = 0f;

            if (audioSource.isPlaying)
                audioSource.Pause();

            return;
        }

        if (!audioSource.isPlaying)
            audioSource.UnPause();

        float volume = 1f - (distance / maxDistance);

        audioSource.volume = Mathf.Clamp01(volume * maxVolume);
    }
}