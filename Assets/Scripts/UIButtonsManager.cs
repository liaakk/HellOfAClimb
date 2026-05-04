using UnityEngine;
using UnityEngine.UI;

public class UIButtonsManager : MonoBehaviour
{
    public static UIButtonsManager Instance;

    public Image uiImage;
    public Sprite[] buttonSprites;
    public Animator uiAnimator; // optional animator for UI animations
    [SerializeField] private string firstCollectAnimationState = "1";

    int count;

    void Awake()
    {
        Instance = this;
    }

    public void AddButton()
{
    count++;

    Debug.Log("contagem agora: " + count);

    // Safely set sprite if available
    if (buttonSprites != null && buttonSprites.Length > 0)
    {
        int idx = Mathf.Clamp(count, 0, buttonSprites.Length - 1);
        if (uiImage != null)
            uiImage.sprite = buttonSprites[idx];
        Debug.Log("o sprite eh o numero: " + idx);
    }

    // Play animation state "1" when the first button is collected
    if (count == 1 && uiAnimator != null && !string.IsNullOrEmpty(firstCollectAnimationState))
    {
        uiAnimator.Play(firstCollectAnimationState, 0, 0f);
    }
}
}