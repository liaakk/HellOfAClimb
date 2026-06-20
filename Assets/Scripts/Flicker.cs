using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    private Light2D lightComponent;

    [Header("Sound")]
    [SerializeField] private AudioSource flickerAudio;

    [Header("Flicker Count")]
    [SerializeField] private int minFlickers = 2;
    [SerializeField] private int maxFlickers = 5;

    [Header("Cooldown")]
    [SerializeField] private float minCooldown = 1f;
    [SerializeField] private float maxCooldown = 20f;

    [Header("Flicker Timing")]
    [SerializeField] private float minFlickerTime = 0.05f;
    [SerializeField] private float maxFlickerTime = 0.2f;

    private void Start()
    {
        lightComponent = GetComponent<Light2D>();

        if (lightComponent == null)
        {
            Debug.LogError("No Light2D component found on " + gameObject.name);
            return;
        }

        StartCoroutine(FlickerLoop());
    }

    private IEnumerator FlickerLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minCooldown, maxCooldown));

            int flickers = Random.Range(minFlickers, maxFlickers + 1);

            for (int i = 0; i < flickers; i++)
            {
                lightComponent.enabled = false;

                if (flickerAudio != null)
                    flickerAudio.Play();

                yield return new WaitForSeconds(Random.Range(minFlickerTime, maxFlickerTime));

                lightComponent.enabled = true;

                yield return new WaitForSeconds(Random.Range(minFlickerTime, maxFlickerTime));
            }
        }
    }
}