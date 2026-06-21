using UnityEngine;

public class MusicRegionStop : MonoBehaviour
{
    [SerializeField] private MusicRegion musicRegion;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (musicRegion != null)
            musicRegion.StartFadeOut();
    }
}