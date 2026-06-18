using UnityEngine;
using System.Collections.Generic;

public class Wind : MonoBehaviour
{
    [Header("Wind")]
    [SerializeField] private Collider2D windArea;
    [SerializeField] private float airWindForce = 8f;
    [SerializeField] private float groundWindForce = 5f;
    [SerializeField] private float chargingWindForce = 3f;
    [SerializeField] private float windDuration = 4f;
    [SerializeField] private float noWindDuration = 4f;
    [SerializeField] private bool startBlowingRight = true;

    [Header("Optional VFX")]
    [SerializeField] private ParticleSystem windParticleSystemRight;
    [SerializeField] private ParticleSystem windParticleSystemLeft;

    private readonly HashSet<Rigidbody2D> bodiesInside = new HashSet<Rigidbody2D>();
    private float phaseTimer;
    private int phaseIndex;
    private int currentDirection = 1;

    private void Reset()
    {
        windArea = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        if (windArea == null)
        {
            windArea = GetComponent<Collider2D>();
        }

        if (windArea != null)
        {
            windArea.isTrigger = true;
        }

        currentDirection = startBlowingRight ? 1 : -1;
        ApplyPhaseState();
    }

    private void Update()
    {
        phaseTimer += Time.deltaTime;

        float activeDuration = IsWindActivePhase() ? windDuration : noWindDuration;
        if (phaseTimer < activeDuration)
        {
            return;
        }

        phaseTimer = 0f;
        phaseIndex = (phaseIndex + 1) % 4;
        ApplyPhaseState();
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody2D body in bodiesInside)
        {
            if (body == null)
            {
                continue;
            }

            float windVelocityX = GetWindForceForBody(body);

            PlayerMovement playerMovement = body.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetExternalWindX(windVelocityX);
            }

            NovoMovimento novoMovimento = body.GetComponent<NovoMovimento>();
            if (novoMovimento != null)
            {
                novoMovimento.SetExternalWindX(windVelocityX);
            }

            if (IsWindActivePhase() && playerMovement == null && novoMovimento == null)
            {
                body.AddForce(Vector2.right * windVelocityX, ForceMode2D.Force);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D body = other.attachedRigidbody;
        if (body != null)
        {
            bodiesInside.Add(body);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D body = other.attachedRigidbody;
        if (body != null)
        {
            PlayerMovement playerMovement = body.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetExternalWindX(0f);
            }

            NovoMovimento novoMovimento = body.GetComponent<NovoMovimento>();
            if (novoMovimento != null)
            {
                novoMovimento.SetExternalWindX(0f);
            }

            bodiesInside.Remove(body);
        }
    }

    private bool IsWindActivePhase()
    {
        return phaseIndex == 0 || phaseIndex == 2;
    }

    private float GetWindForceForBody(Rigidbody2D body)
    {
        if (!IsWindActivePhase())
        {
            return 0f;
        }

        float force = groundWindForce;

        PlayerMovement playerMovement = body.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            if (playerMovement.IsJumpCharging)
            {
                force = chargingWindForce;
            }
            else if (!playerMovement.IsGroundedNow)
            {
                force = airWindForce;
            }
        }
        else
        {
            NovoMovimento novoMovimento = body.GetComponent<NovoMovimento>();
            if (novoMovimento != null)
            {
                if (novoMovimento.IsChargingJump)
                {
                    force = chargingWindForce;
                }
                else if (!novoMovimento.IsSupported)
                {
                    force = airWindForce;
                }
            }
        }

        return currentDirection * force;
    }

    private void ApplyPhaseState()
    {
        if (phaseIndex == 0)
        {
            currentDirection = startBlowingRight ? 1 : -1;
        }
        else if (phaseIndex == 2)
        {
            currentDirection = -currentDirection;
        }

        ParticleSystem activeParticles = currentDirection == 1 ? windParticleSystemRight : windParticleSystemLeft;
        ParticleSystem inactiveParticles = currentDirection == 1 ? windParticleSystemLeft : windParticleSystemRight;

        if (IsWindActivePhase())
        {
            if (activeParticles != null && !activeParticles.isPlaying)
            {
                activeParticles.Play();
            }
        }
        else
        {
            if (activeParticles != null && activeParticles.isPlaying)
            {
                activeParticles.Stop();
            }
        }

        if (inactiveParticles != null && inactiveParticles.isPlaying)
        {
            inactiveParticles.Stop();
        }
    }
}
