using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance;

    [Range(0f, 1f)]
    public float musicVolume = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}