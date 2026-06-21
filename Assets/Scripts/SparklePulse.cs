using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SparklePulse : MonoBehaviour
{
    [Header("Referências")]
    public Light2D sparkleLight;
    public Animator sparkleAnimator;
    public SpriteRenderer sparkleSprite;

    [Header("Frames da animação")]
    public int totalFrames = 36;      // frames 0 até 35
    public int peakFrame = 7;         // frame em que a estrela está no pico
    public int fadeEndFrame = 15;     // frame em que ela desaparece
    public int restartDotFrame = 35;  // último frame, pontinho antes de reiniciar

    [Header("Luz")]
    public float minIntensity = 0f;
    public float maxIntensity = 1.2f;
    public float restartDotIntensity = 0.12f;

    public float minRadius = 0f;
    public float maxRadius = 1.3f;
    public float restartDotRadius = 0.35f;

    [Header("Sprite - opcional")]
    public bool pulseSpriteScale = false;
    public float minScale = 0.95f;
    public float maxScale = 1.08f;

    [Header("Variação entre sparkles")]
    public bool randomizeAnimationStart = true;

    private Vector3 originalSpriteScale;

    void Start()
    {
        if (sparkleLight == null)
            sparkleLight = GetComponentInChildren<Light2D>();

        if (sparkleAnimator == null)
            sparkleAnimator = GetComponentInChildren<Animator>();

        if (sparkleSprite == null)
            sparkleSprite = GetComponentInChildren<SpriteRenderer>();

        if (sparkleSprite != null)
            originalSpriteScale = sparkleSprite.transform.localScale;

        if (sparkleLight != null)
        {
            sparkleLight.intensity = 0f;
            sparkleLight.pointLightOuterRadius = 0f;
        }

        // Para os sparkles do mapa não piscarem todos ao mesmo tempo
        if (randomizeAnimationStart && sparkleAnimator != null)
        {
            float randomStart = Random.Range(0f, 1f);
            sparkleAnimator.Play(0, 0, randomStart);
        }
    }

    void Update()
    {
        float pulse = GetPulseFromAnimationFrame();

        if (sparkleLight != null)
        {
            int currentFrame = GetCurrentFrame();

            if (currentFrame == restartDotFrame)
            {
                sparkleLight.intensity = restartDotIntensity;
                sparkleLight.pointLightOuterRadius = restartDotRadius;
            }
            else
            {
                sparkleLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
                sparkleLight.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, pulse);
            }
        }

        if (pulseSpriteScale && sparkleSprite != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, pulse);
            sparkleSprite.transform.localScale = originalSpriteScale * scale;
        }
    }

    int GetCurrentFrame()
    {
        if (sparkleAnimator == null)
            return 0;

        AnimatorStateInfo stateInfo = sparkleAnimator.GetCurrentAnimatorStateInfo(0);

        // valor de 0 a 1 dentro do loop atual da animação
        float normalizedTime = stateInfo.normalizedTime % 1f;

        // converte para frame 0–35
        int currentFrame = Mathf.FloorToInt(normalizedTime * totalFrames);

        return Mathf.Clamp(currentFrame, 0, totalFrames - 1);
    }

    float GetPulseFromAnimationFrame()
    {
        int currentFrame = GetCurrentFrame();

        float pulse = 0f;

        // frames 0 a 7: luz aumenta
        if (currentFrame <= peakFrame)
        {
            pulse = Mathf.InverseLerp(0, peakFrame, currentFrame);
        }
        // frames 8 a 15: luz diminui
        else if (currentFrame <= fadeEndFrame)
        {
            pulse = Mathf.InverseLerp(fadeEndFrame, peakFrame, currentFrame);
        }
        // frames 16 a 34: luz apagada
        else if (currentFrame < restartDotFrame)
        {
            pulse = 0f;
        }
        // frame 35: pontinho fraquinho antes de reiniciar
        else
        {
            pulse = 0f;
        }

        // suaviza a subida/descida
        pulse = Mathf.SmoothStep(0f, 1f, pulse);

        return pulse;
    }
}