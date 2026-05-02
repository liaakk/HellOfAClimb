using System.Collections; //necessário para typing effect
using UnityEngine;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    //SOBRE O NPC
    public string npcName; //identificar o NPC
    public string[] lines; // lista de falas do NPC
    private int index = 0; // fala que está a ser mostrada atualmente
    
    //SOBRE A VISUALIZAÇÃO DO TEXTO
    public GameObject speechBubble; // o canvas
    public TMP_Text speechText; // o texto dentro do canvas
    public float typingSpeed = 0.05f; // velocidade do efeito typing
    //EFEITO TYPING
    private bool isTyping = false; // verificar se está a aparecer uma fala atualmente:
    private Coroutine typingCoroutine; // guarda o efeito typing (para poder parar se necessário):
    //"coroutine" é uma animação ao longo do tempo

    //SOBRE AS INSTRUÇÕES DA TECLA E
    private bool dialogueActive = false; // para aparecerem as instruções de clicar na tecla E:
    public GameObject hintText;
    private bool hintShown = false; 

    //SOBRE A VISIBILIDADE DO NPC NA CAMARA
    public SpriteRenderer npcRenderer;
    private bool isVisible;



    void Start()
    {
        speechBubble.SetActive(false);
        //garante que o alerta começa escondido
        hintText.SetActive(false);
        //para o texto começar escondido
        SetupDialogueByName();
        if (npcRenderer == null)
        {
            npcRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        // garantir renderer automático / autodeteção do NPC
    }

    void Update()
    {
        // verificar se NPC está visível na câmara
        isVisible = IsInCameraView();
        //se não estiver visível, não faz nada
        if (!isVisible){
            return;
        }
            
        // tecla E para avançar
        if (dialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                // se ainda está a escrever, mostra a frase inteira
                StopCoroutine(typingCoroutine);
                speechText.text = lines[index];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue() //inicia o diálogo
    {
        speechBubble.SetActive(true);
        dialogueActive = true;
        index = 0;
        ShowLine();
    }

    void NextLine()
    {
        index++;

        if (index >= lines.Length)
        {
            index = 0; // LOOP no fim
        }

        ShowLine();
    }

    void ShowLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        //efeito de mostrar a fala letra a letra
        typingCoroutine = StartCoroutine(TypeLine(lines[index]));

        //alerta da tecla E só no 1o diálogo
         if (npcName == "Sadim" && index == 0 && !hintShown && dialogueActive)
        {
            hintShown = true; // impede repetir
            StartCoroutine(ShowHintOnce());
        }
    }


    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        speechText.text = ""; //começa vazio

        foreach (char letter in line)
        {
            speechText.text += letter; //adiciona uma letra de cada vez
            yield return new WaitForSeconds(typingSpeed); //tempo até adicionar a próxima letra
        }

        isTyping = false;
    }

    //só para o alerta da tecla E
    IEnumerator ShowHintOnce()
    {
        hintText.SetActive(true);
        yield return new WaitForSeconds(5f);
        hintText.SetActive(false);
    }

    void SetupDialogueByName() //configuração de falas para diferentes NPCs
    {
        if (npcName == "Sadim")
        {
            lines = new string[]
            {
                "Look at me...",
                "I need to look at myself.",

                "Are you still here?",
                "I recognize your footsteps...",
                "Did greed also bind you in here?",

                "I want to look at myself one last time",
                "But I cannot see...",
                "I cannot see, as Greed has blinded me"
            };
        }
        else if (npcName == "")
        {
            lines = new string[]
            {
            };
        }
    }

    // DETEÇÃO DE CÂMARA
    bool IsInCameraView()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, npcRenderer.bounds);
    }
}