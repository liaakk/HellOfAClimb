using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class TitleCardController : MonoBehaviour
{
    [Header("Title Cards")]
    [Tooltip("Fade this card in immediately when the player clicks.")]
    public CanvasGroup immediateTitleGroup;

    [Tooltip("Fade this card in after the delay when the player clicks.")]
    public CanvasGroup delayedTitleGroup;

    [Header("Timing")]
    public float delayedFadeDelay = 1f;
    public float holdTime = 1.5f;

    [Header("Fade")]
    public float fadeInTime = 1f;
    public float fadeOutTime = 1f;

    private bool triggered = false;
    private Image subtitleImage;

    private void Start()
    {
        if (immediateTitleGroup == null || delayedTitleGroup == null)
        {
            Debug.LogError("TitleCardController: Assign both title CanvasGroup references in the Inspector.");
            enabled = false;
            return;
        }

        subtitleImage = delayedTitleGroup.GetComponent<Image>();

        if (subtitleImage == null)
        {
            Debug.LogError("TitleCardController: The delayed title group must have an Image component for the subtitle.");
            enabled = false;
            return;
        }

        immediateTitleGroup.alpha = 0f;
        delayedTitleGroup.alpha = 0f;
    }

    private void Update()
    {
        if (triggered)
            return;

        if (AnyInputPressed())
        {
            triggered = true;
            StartCoroutine(PlayTitleSequence());
        }
    }

    private bool AnyInputPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is UnityEngine.InputSystem.Controls.ButtonControl button && button.wasPressedThisFrame)
                    return true;
            }
        }

        return false;
    }

    public void ShowTitleWithSubtitle(Sprite newSubtitle)
{
    if (newSubtitle == null)
        return;

    subtitleImage.sprite = newSubtitle;

    StopAllCoroutines();

    immediateTitleGroup.alpha = 0f;
    delayedTitleGroup.alpha = 0f;

    StartCoroutine(PlayTitleSequence());
}

    private IEnumerator PlayTitleSequence()
    {
        StartCoroutine(FadeInCanvasGroup(immediateTitleGroup));

        yield return new WaitForSeconds(delayedFadeDelay);

        yield return StartCoroutine(FadeInCanvasGroup(delayedTitleGroup));

        yield return new WaitForSeconds(holdTime);

        yield return StartCoroutine(FadeOutBothCanvasGroups());
    }

    private IEnumerator FadeInCanvasGroup(CanvasGroup group)
    {
        float t = 0f;

        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            group.alpha = t / fadeInTime;
            yield return null;
        }

        group.alpha = 1f;
    }

    private IEnumerator FadeOutBothCanvasGroups()
    {
        float t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / fadeOutTime);
            immediateTitleGroup.alpha = alpha;
            delayedTitleGroup.alpha = alpha;
            yield return null;
        }

        immediateTitleGroup.alpha = 0f;
        delayedTitleGroup.alpha = 0f;
    }
}