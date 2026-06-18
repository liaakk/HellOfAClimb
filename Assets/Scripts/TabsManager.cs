using UnityEngine;
using UnityEngine.UI;

public class NotebookTabs : MonoBehaviour
{
    [Header("Pages")]
    public GameObject objectsPage;
    public GameObject charactersPage;
    public GameObject cutscenesPage;

    [Header("Buttons")]
    public Image objectsButton;
    public Image charactersButton;
    public Image cutscenesButton;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.gray;

    void OnEnable()
    {
        ShowObjects(); // sempre abre na página objects
    }

    public void ShowObjects()
    {

{
    Debug.Log("CLICK OBJECTS");
    SetPages(true, false, false);
    SetButtons(objectsButton);
}
        SetPages(true, false, false);
        SetButtons(objectsButton);
    }

    public void ShowCharacters()
    {
        Debug.Log("CLICK CHARACTERS FUNCIONA");
        SetPages(false, true, false);
        SetButtons(charactersButton);
    }

    public void ShowCutscenes()
    {
        SetPages(false, false, true);
        SetButtons(cutscenesButton);
    }

    void SetPages(bool obj, bool cha, bool cut)
    {
        objectsPage.SetActive(obj);
        charactersPage.SetActive(cha);
        cutscenesPage.SetActive(cut);
    }

    void SetButtons(Image activeButton)
    {
        objectsButton.color = normalColor;
        charactersButton.color = normalColor;
        cutscenesButton.color = normalColor;

        activeButton.color = selectedColor;
    }
}