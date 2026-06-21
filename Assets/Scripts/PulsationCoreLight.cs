using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CoreHeartbeatLight : MonoBehaviour
{
    [Header("Luz do Core")]
    public Light2D coreLight;

    [Header("Cor")]
    public Color coreColor = new Color(0.525f, 0.165f, 0.051f); // #862A0D

    [Header("Estado Base")]
    public float baseIntensity = 3.85f;
    public float baseInnerRadius = 1.90f;
    public float baseOuterRadius = 3.00f;

    [Header("Primeiro Tum")]
    public float firstBeatIntensity = 4.15f;
    public float firstBeatInnerRadius = 2.00f;
    public float firstBeatOuterRadius = 3.15f;

    [Header("Segundo TUM")]
    public float secondBeatIntensity = 4.38f;
    public float secondBeatInnerRadius = 2.10f;
    public float secondBeatOuterRadius = 3.25f;

    [Header("Timing")]
    public float beatUpTime = 0.08f;
    public float beatDownTime = 0.18f;

    public float timeBetweenTumTum = 0.13f;
    public float pauseAfterTumTum = 0.75f;

    private void Start()
    {
        if (coreLight == null)
            coreLight = GetComponent<Light2D>();

        if (coreLight == null)
            coreLight = GetComponentInChildren<Light2D>();

        if (coreLight != null)
        {
            coreLight.color = coreColor;
            SetLight(baseIntensity, baseInnerRadius, baseOuterRadius);
            StartCoroutine(HeartbeatRoutine());
        }
    }

    IEnumerator HeartbeatRoutine()
    {
        while (true)
        {
            // tum
            yield return PulseOnce(
                firstBeatIntensity,
                firstBeatInnerRadius,
                firstBeatOuterRadius
            );

            // pausa curtinha entre tum e TUM
            yield return new WaitForSeconds(timeBetweenTumTum);

            // TUM
            yield return PulseOnce(
                secondBeatIntensity,
                secondBeatInnerRadius,
                secondBeatOuterRadius
            );

            // pausa maior antes do próximo batimento
            yield return new WaitForSeconds(pauseAfterTumTum);
        }
    }

    IEnumerator PulseOnce(float targetIntensity, float targetInnerRadius, float targetOuterRadius)
    {
        // sobe rápido
        yield return LerpLight(
            baseIntensity,
            targetIntensity,
            baseInnerRadius,
            targetInnerRadius,
            baseOuterRadius,
            targetOuterRadius,
            beatUpTime
        );

        // desce mais suave
        yield return LerpLight(
            targetIntensity,
            baseIntensity,
            targetInnerRadius,
            baseInnerRadius,
            targetOuterRadius,
            baseOuterRadius,
            beatDownTime
        );
    }

    IEnumerator LerpLight(
        float startIntensity,
        float endIntensity,
        float startInner,
        float endInner,
        float startOuter,
        float endOuter,
        float duration
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            float intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            float inner = Mathf.Lerp(startInner, endInner, t);
            float outer = Mathf.Lerp(startOuter, endOuter, t);

            SetLight(intensity, inner, outer);

            yield return null;
        }

        SetLight(endIntensity, endInner, endOuter);
    }

    void SetLight(float intensity, float innerRadius, float outerRadius)
    {
        if (coreLight == null) return;

        coreLight.intensity = intensity;
        coreLight.pointLightInnerRadius = innerRadius;
        coreLight.pointLightOuterRadius = outerRadius;
    }
}