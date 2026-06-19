using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Notebook : MonoBehaviour
{
    public static Notebook Instance;

    [Header("Notebook")]
    public GameObject notebookPanel;

    [Header("Pages")]
    public GameObject charactersPage;
    public GameObject objectsPage;
    public GameObject cutscenesPage;

    [Header("Buttons")]
    public Image objectsButton;
    public Image charactersButton;
    public Image cutscenesButton;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.gray;

    [Header("Slots")]
    public NotebookSlot[] npcSlots;
    public NotebookSlot[] objectSlots;
    public NotebookSlot[] memorySlots;

    [Header("Player")]
    public NovoMovimento playerMovement;

    private bool isOpen = false;
    private int currentPage = 0;

    private List<NotebookData> unlockedNPCs = new();
    private List<NotebookData> unlockedObjects = new();
    private List<NotebookData> unlockedMemories = new();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("objectsButton = " + objectsButton);
        Debug.Log("charactersButton = " + charactersButton);
        Debug.Log("cutscenesButton = " + cutscenesButton);

        notebookPanel.SetActive(false);

        ClearSlots(npcSlots);
        ClearSlots(objectSlots);
        ClearSlots(memorySlots);

        ShowObjects();
    }

    void Update()
    {
        if (isOpen)
        {
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame)
            {
                currentPage++;

                if (currentPage > 2)
                    currentPage = 0;

                UpdatePage();
            }

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.aKey.wasPressedThisFrame)
            {
                currentPage--;

                if (currentPage < 0)
                    currentPage = 2;

                UpdatePage();
            }
        }

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            notebookPanel.SetActive(isOpen);
            if (playerMovement != null)
                playerMovement.enabled = !isOpen;
        }
    }

    // DESBLOQUEAR
    public void UnlockNPC(NotebookData data)
    {
        if (unlockedNPCs.Contains(data))
            return;

        unlockedNPCs.Add(data);

        RefreshNPCs();
    }

    public void UnlockObject(NotebookData data)
    {
        if (unlockedObjects.Contains(data))
            return;

        unlockedObjects.Add(data);

        RefreshObjects();
    }

    public void UnlockMemory(NotebookData data)
    {
        if (unlockedMemories.Contains(data))
            return;

        unlockedMemories.Add(data);

        RefreshMemories();
    }


    // REFRESH
    void RefreshNPCs()
    {
        for(int i = 0; i < npcSlots.Length; i++)
        {
            if(i < unlockedNPCs.Count)
                npcSlots[i].SetData(unlockedNPCs[i]);
            else
                npcSlots[i].Clear();
        }
    }

    void RefreshObjects()
    {
        for(int i = 0; i < objectSlots.Length; i++)
        {
            if(i < unlockedObjects.Count)
                objectSlots[i].SetData(unlockedObjects[i]);
            else
                objectSlots[i].Clear();
        }
    }

    void RefreshMemories()
    {
        for(int i = 0; i < memorySlots.Length; i++)
        {
            if(i < unlockedMemories.Count)
                memorySlots[i].SetData(unlockedMemories[i]);
            else
                memorySlots[i].Clear();
        }
    }

    void ClearSlots(NotebookSlot[] slots)
    {
        foreach(var slot in slots)
            slot.Clear();
    }


    // BOTÕES
    public void ShowObjects()
    {
        currentPage = 0;
        SetPages(true, false, false);
        SetButtons(objectsButton);
    }

    public void ShowCharacters()
    {
        currentPage = 1;
        SetPages(false, true, false);
        SetButtons(charactersButton);
    }

    public void ShowCutscenes()
    {
        currentPage = 2;
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

    void UpdatePage()
    {
        switch (currentPage)
        {
            case 0:
                ShowObjects();
                break;

            case 1:
                ShowCharacters();
                break;

            case 2:
                ShowCutscenes();
                break;
        }
    }
}