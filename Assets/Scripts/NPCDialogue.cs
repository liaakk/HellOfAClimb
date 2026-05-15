using System.Collections; //necessário para typing effect
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    // Animation
    public Animator animator; //referência ao Animator do NPC
    // name of the trigger parameter used to start the talk animation (configurable in inspector)
    public string talkTrigger = "SadimTalk";
    //SOBRE O NPC
    public string npcName; //identificar o NPC
    public string[] lines; //falas do NPC
    private int index = 0; //controlo da fala atual

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
    private bool hasStartedDialogue = false;

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

        // começa automaticamente quando o player está perto
        if (isNear && !hasStartedDialogue)
        {
            StartDialogue();
            hasStartedDialogue = true;
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

        // hint só aparece quando está perto e em diálogo
        if (dialogueActive && isNear && !hasPressedE)
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

        // trigger animation if animator assigned and parameter exists
        if (animator != null)
        {
            bool hasParam = false;
            foreach (var p in animator.parameters)
            {
                if (p.name == talkTrigger)
                {
                    hasParam = true;
                    break;
                }
            }

            if (hasParam)
            {
                animator.SetTrigger(talkTrigger);
            }
            else
            {
                string paramList = "";
                if (animator.parameters != null && animator.parameters.Length > 0)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (var p in animator.parameters)
                    {
                        sb.AppendFormat("{0} ({1}), ", p.name, p.type);
                    }
                    paramList = sb.ToString().TrimEnd(' ', ',');
                }
                else
                {
                    paramList = "(none)";
                }

                Debug.LogWarning($"Animator on '{gameObject.name}' does not have a parameter named '{talkTrigger}'. Available parameters: {paramList}. Add a Trigger parameter with this name or change `talkTrigger` in the inspector.");
            }
        }
        else
        {
            Debug.LogWarning($"No Animator assigned on '{gameObject.name}'. Assign one in the inspector to play talk animations.");
        }

        ShowLine();
    }

    void NextLine()
    {
        index++;

        if (index >= lines.Length)
        {
            index = 0; // LOOP
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