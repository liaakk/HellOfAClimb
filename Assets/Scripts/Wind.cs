using UnityEngine;
using System.Collections.Generic;

public class Wind : MonoBehaviour
{
    [Header("Wind")]
    [SerializeField] private Collider2D windArea;
    [SerializeField] private float airWindForce = 8f;
    [SerializeField] private float groundWindForce = 5f;
    [SerializeField] private float chargingWindForce = 3f;
    [SerializeField] private float chargingDashWindForce = 2f;
    [SerializeField] private float dashWindForce = 1f;
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

            NovoMovimento novoMovimento = body.GetComponent<NovoMovimento>();
            if (novoMovimento != null)
            {
                novoMovimento.SetExternalWindX(windVelocityX);
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

        NovoMovimento novoMovimento = body.GetComponent<NovoMovimento>();
        if (novoMovimento != null)
        {
            if (!novoMovimento.IsGrounded)
            {
                print("airborne WIND");
                force = airWindForce;
            }
            else if (novoMovimento.IsChargingJump)
            {
                print("charging jump WIND");
                force = chargingWindForce;
            }
            else if (novoMovimento.IsChargingDash)
            {
                //funciona
                print("charging dash WIND");
                force = chargingDashWindForce;
            }
            else if (novoMovimento.DashesRemaining == 0)
            {
                print("dash boost WIND");
                force = dashWindForce;
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
