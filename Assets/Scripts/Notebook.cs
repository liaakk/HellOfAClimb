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
    public GameObject objectsPage1;
    public GameObject objectsPage2;
    public GameObject cutscenesPage;

    [Header("Slots")]
    public NotebookSlot[] npcSlots;
    public NotebookSlot[] objectSlots1;
    public NotebookSlot[] objectSlots2;
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
        notebookPanel.SetActive(false);

        ClearSlots(npcSlots);
        ClearSlots(objectSlots1);
        ClearSlots(objectSlots2);
        ClearSlots(memorySlots);

        ShowObjects1();
    }

    void Update()
    {
        if (isOpen)
        {
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame)
            {
                if (currentPage < 3)
                {
                    currentPage++;
                    UpdatePage();
                }
                    
            }

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.aKey.wasPressedThisFrame)
            {
                if (currentPage > 0)
                {
                    currentPage--;
                    UpdatePage();
                }
            }
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
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
        for(int i = 0; i < objectSlots1.Length; i++)
        {
            if(i < unlockedObjects.Count)
                objectSlots1[i].SetData(unlockedObjects[i]);
            else
                objectSlots1[i].Clear();
        }
        for(int i = 0; i < objectSlots2.Length; i++)
        {
            int objectIndex = i + objectSlots1.Length;
            if(objectIndex < unlockedObjects.Count)
                objectSlots2[i].SetData(unlockedObjects[objectIndex]);
            else
                objectSlots2[i].Clear();
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
    public void ShowObjects1()
    {
        currentPage = 0;
        SetPages(true, false, false, false);
    }

    public void ShowObjects2()
    {
        currentPage = 1;
        SetPages(false, true, false, false);
    }

    public void ShowCharacters()
    {
        currentPage = 2;
        SetPages(false, false, true, false);
    }

    public void ShowCutscenes()
    {
        currentPage = 3;
        SetPages(false, false, false, true);
    }

    void SetPages(bool obj1, bool obj2, bool cha, bool cut)
    {
        objectsPage1.SetActive(obj1);
        objectsPage2.SetActive(obj2);
        charactersPage.SetActive(cha);
        cutscenesPage.SetActive(cut);
    }

    void UpdatePage()
    {
        switch (currentPage)
        {
            case 0:
                ShowObjects1();
                break;

            case 1:
                ShowObjects2();
                break;

            case 2:
                ShowCharacters();
                break;

            case 3:
                ShowCutscenes();
                break;
        }
    }
}