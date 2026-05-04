using UnityEngine;
using System.Collections.Generic;

public class TImeMaster : MonoBehaviour
{
    public static TImeMaster Instance { get; private set; }

    private float normalFixedDeltaTime;
    private float currentTimeScale = 1f;
    private List<Animator> trackedAnimators = new List<Animator>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        normalFixedDeltaTime = Time.fixedDeltaTime;
    }

    /// <summary>
    /// Sets the global time scale and scales all tracked animators accordingly.
    /// </summary>
    public void SetTimeScale(float timeScale)
    {
        currentTimeScale = Mathf.Clamp01(timeScale);
        Time.timeScale = currentTimeScale;
        Time.fixedDeltaTime = normalFixedDeltaTime * currentTimeScale;
        
        // Scale all tracked animator speeds
        foreach (var animator in trackedAnimators)
        {
            if (animator != null)
            {
                animator.speed = currentTimeScale;
            }
        }
    }

    /// <summary>
    /// Restores time to normal (1.0).
    /// </summary>
    public void RestoreTimeScale()
    {
        SetTimeScale(1f);
    }

    /// <summary>
    /// Registers an animator to be scaled with global time changes.
    /// </summary>
    public void RegisterAnimator(Animator animator)
    {
        if (animator != null && !trackedAnimators.Contains(animator))
        {
            trackedAnimators.Add(animator);
        }
    }

    /// <summary>
    /// Unregisters an animator from time scale tracking.
    /// </summary>
    public void UnregisterAnimator(Animator animator)
    {
        if (animator != null)
        {
            trackedAnimators.Remove(animator);
        }
    }

    /// <summary>
    /// Gets the current time scale.
    /// </summary>
    public float GetCurrentTimeScale()
    {
        return currentTimeScale;
    }

    /// <summary>
    /// Emergency reset: restores all time values to normal.
    /// </summary>
    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
        
        foreach (var animator in trackedAnimators)
        {
            if (animator != null)
            {
                animator.speed = 1f;
            }
        }
    }
}
