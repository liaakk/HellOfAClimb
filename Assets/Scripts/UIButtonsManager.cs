using UnityEngine;
using UnityEngine.UI;

public class UIButtonsManager : MonoBehaviour
{
    public static UIButtonsManager Instance;

    [Header("UI")]
    public Image uiImage;
    public Sprite[] buttonSprites;

    private int count = 0;

// nisso aqui você pode definir quantos botões existem no jogo, e o script vai dividir os sprites igualmente entre eles
    private int totalButtons = 4;

    void Awake()
    {
        Instance = this;
    }

    public void AddButton()
    {
        count++;

        
        count = Mathf.Clamp(count, 0, totalButtons);

        Debug.Log("botões coletados: " + count);

        if (buttonSprites == null || buttonSprites.Length == 0) return;
        if (uiImage == null) return;

        // quantos sprites por etapa
        int spritesPerStep = buttonSprites.Length / totalButtons;

        // calcula o índice
        int idx = count * spritesPerStep;

        // evita ultrapassar o array
        idx = Mathf.Clamp(idx, 0, buttonSprites.Length - 1);

        uiImage.sprite = buttonSprites[idx];

        Debug.Log("sprite atual: " + idx);
    }
}