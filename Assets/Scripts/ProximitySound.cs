using UnityEngine;

public class ProximitySound : MonoBehaviour
{
    public Transform player;
    public AudioSource audioSource;

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

        float volume = 1 - (distance / maxDistance);

        audioSource.volume = volume;
    }
}