using UnityEngine;
using UnityEngine.InputSystem;

public class NotebookManager : MonoBehaviour
{
    public GameObject notebookPanel;

    bool isOpen = false;

    void Update()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            notebookPanel.SetActive(isOpen);
        }
    }
}