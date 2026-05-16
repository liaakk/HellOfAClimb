using System.Collections; //necessário para typing effect
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    public Animator animator; //para controlar animações do NPC
    public string talkState = "SadimTalk"; // state name to play while talking
    public string idleState = "SadimIdle"; // optional state to return to when dialogue ends
    //SOBRE O NPC
    public string[] lines; //falas do NPC
    private int index = 0; //controlo da fala atual
    public string npcName; //nome do NPC para definir falas específicas

    //SOBRE O TEXTO
    public GameObject speechBubble;
    public TMP_Text speechText;
    public float typingSpeed = 0.05f; //velocidade do efeito typing

    //EFEITO TYPING
    private bool isTyping = false; //verificar se está a aparecer alguma fala no momento
    private Coroutine typingCoroutine; //guarda o efeito typing
    //coroutine é uma animação ao longo do tempo

    //ESTADO DO DIÁLOGO
    private bool dialogueActive = false;
    private bool hasAutoStartedOnce = false;

    //HINT (TECLA E)
    public GameObject hintText;
    private bool hasPressedE = false;

    //DISTÂNCIA DO PLAYER AO NPC
    public Transform player;
    public float interactionDistanceX;
    public float interactionDistanceY;

    void Start()
    {
        speechBubble.SetActive(false);
        hintText.SetActive(false);
        //o botão começa invisível

        SetupDialogueByName();
    }

    void Update()
    {
        bool isNear = IsPlayerNear();

        // first encounter starts automatically once
        if (isNear && !dialogueActive && !hasAutoStartedOnce)
        {
            StartDialogue();
            hasAutoStartedOnce = true;
        }

        // press E to start the dialogue when close and idle
        if (!dialogueActive && isNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            hasPressedE = true;
            hintText.SetActive(false);
            StartDialogue();
        }

        // if player leaves while dialogue active, end the dialogue (and stop talk animation)
        if (dialogueActive && !isNear)
        {
            EndDialogue();
        }

        // tecla E (para avançar no diálogo)
        if (dialogueActive && Keyboard.current.eKey.wasPressedThisFrame)
        {
           if (!hasPressedE)
            {
                hasPressedE = true;
                hintText.SetActive(false);
            }

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

        // hint aparece quando está perto e ainda não pressionou E neste ciclo
        if (isNear && !hasPressedE)
            {
                hintText.SetActive(true);
            }
        else
            {
                hintText.SetActive(false);
            }
    }

    public void StartDialogue() //INICIA DIÁLOGO
    {
        if (dialogueActive) return;

        speechBubble.SetActive(true);
        dialogueActive = true;
        index = 0;

        // play talk animation while dialogue is active
        if (animator != null && !string.IsNullOrEmpty(talkState))
        {
            animator.Play(talkState);
        }

        ShowLine();
    }

    public void EndDialogue()
    {
        if (!dialogueActive) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        speechBubble.SetActive(false);
        hintText.SetActive(false);
        dialogueActive = false;
        hasPressedE = false;

        if (animator != null && !string.IsNullOrEmpty(idleState))
        {
            animator.Play(idleState);
        }
    }

    void NextLine()
    {
        index++;

        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    void ShowLine()
    {
        // proteção contra erro
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("NPC sem falas!");
            return;
        }

        if (index < 0 || index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        //efeito de mostrar a fala letra a letra
        typingCoroutine = StartCoroutine(TypeLine(lines[index]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        speechText.text = "";

        foreach (char letter in line)
        {
            speechText.text += letter; //adiciona uma letra de cada vez
            yield return new WaitForSeconds(typingSpeed); //tempo para a próxima letra aparecer
        }

        isTyping = false;
    }


    void SetupDialogueByName() //falas para diferentes NPCs
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
    }

    //DISTÂNCIA DO NPC AO PLAYER
    bool IsPlayerNear()
    {
        if (player == null) return false;

        float dx = Mathf.Abs(transform.position.x - player.position.x);
        float dy = Mathf.Abs(transform.position.y - player.position.y);
        return dx <= interactionDistanceX && dy <= interactionDistanceY;
    }
}