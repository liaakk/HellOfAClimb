using UnityEngine;

public class NotebookTabs : MonoBehaviour
{
    public GameObject objectsPage;
    public GameObject charactersPage;
    public GameObject cutscenesPage;

    public void ShowObjects()
    {
        objectsPage.SetActive(true);
        charactersPage.SetActive(false);
        cutscenesPage.SetActive(false);
    }

    public void ShowCharacters()
    {
        objectsPage.SetActive(false);
        charactersPage.SetActive(true);
        cutscenesPage.SetActive(false);
    }

    public void ShowCutscenes()
    {
        objectsPage.SetActive(false);
        charactersPage.SetActive(false);
        cutscenesPage.SetActive(true);
    }
}