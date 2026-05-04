using UnityEngine;
using UnityEngine.UI;

public class UIButtonsManager : MonoBehaviour
{
    public static UIButtonsManager Instance;

    public Image uiImage;
    public Sprite[] buttonSprites;

    int count;

    void Awake()
    {
        Instance = this;
    }

    public void AddButton()
{
    count++;

    Debug.Log("contagem agora: " + count);

    uiImage.sprite = buttonSprites[count];

    Debug.Log("o sprite eh o numero: " + count);
}
}