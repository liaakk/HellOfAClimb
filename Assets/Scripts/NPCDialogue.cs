using System.Collections; //necessário para typing effect
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [Header("Animações")]
    public Animator animator; //para controlar animações do NPC
    public string talkState = "SadimTalk"; // state name to play while talking
    public string idleState = "SadimIdle"; // optional state to return to when dialogue ends
    
    [Header("Falas NPC")]
    public string[] lines; //falas do NPC
    private int index = 0; //controlo da fala atual
    public string npcName; //nome do NPC para definir falas específicas

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
    
    //MEMÓRIA NA CADERNETA
    [Header("Notebook")]
    public NotebookData notebookData;
    private bool addedToNotebook = false;

    [Header("Interação com Player")]
    public Transform player;
    public float interactionDistanceX;
    public float interactionDistanceY;

    [Header("Hint Text (opcional)")]
    public GameObject hintText;
    private bool hasPressedE = false;

    void Start()
    {
        speechBubble.SetActive(false);

        if (hintText != null){
            hintText.SetActive(false);
        }
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
            if (hintText != null){
                hintText.SetActive(false);
            }
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
                if (hintText != null){
                    hintText.SetActive(false);
                }
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
        if (hintText != null){
            if (isNear && !hasPressedE)
                {
                    hintText.SetActive(true);
                }
            else
                {
                    hintText.SetActive(false);
                }
        }
    }

 public void StartDialogue()
{
    if (dialogueActive) return;

    speechBubble.SetActive(true);
    dialogueActive = true;
    index = 0;

    if (animator != null && !string.IsNullOrEmpty(talkState))
        animator.Play(talkState);

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

    if (hintText != null)
        hintText.SetActive(false);

    dialogueActive = false;
    hasPressedE = false;

    if (animator != null && !string.IsNullOrEmpty(idleState))
        animator.Play(idleState);

    if (npcName == "Sadim" && !addedToNotebook)
    {
        addedToNotebook = true;
        Notebook.Instance.UnlockNPC(notebookData);
        NotebookAnimationController.Instance.PlayAnimation();
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
        else if (npcName == "Shrew")
        {
            lines = new string[]
            {
                //o shrew não fala
            };
        }
        else if (npcName == "Glashy")
        {
            lines = new string[]
            {
                //dialogo wtv
            };
        }
        else if (npcName == "Mourchid")
        {
            lines = new string[]
            {
                "You climb still.",
                "Even now.",
                "Even after countless falls, after all the heights you have crossed.",
                "You climb as you once did in life.",
                "You remember fragments, yet not the shape they formed.",
                "Allow me to show you.",
                "There was a time when your hands knew only kindness.",
                "Thread and cloth became routine.",
                "Wood and paint became familiar.",
                "Children who possessed nothing held your creations as treasures, and through them you found purpose.",
                "It was a noble summit.",
                "Yet summits are curious places.",
                "The higher one rises, the harder it becomes to see the ground below.",
                "Your gifts became sought after.",
                "The humble table became a workshop.",
                "The workshop became a store.",
                "The store became an enterprise.",
                "And with every step upward, you assured yourself you remained where you had begun.",
                "After all, dolls were still being made.",
                "Children were still smiling.",
                "The shape had changed, but the purpose had not.",
                "Or so you believed.",
                "But a path does not remain the same merely because it carries the same name.",
                "Beyond your sight, other hands began stitching your toys.",
                "Smaller hands.",
                "Hungrier hands.",
                "Hands that should have been holding the very gifts they were forced to create.",
                "You saw enough to wonder.",
                "Enough to ask.",
                "Enough to know.",
                "Yet certainty would have demanded a choice.",
                "And so you wrapped yourself in doubt.",
                "You called it caution.",
                "You called it necessity.",
                "You called it ignorance.",
                "But ignorance is not blindness.",
                "It is the act of closing one's eyes.",
                "You climbed, and others bore the weight of your ascent.",
                "Not because you wished them harm.",
                "That would have been simpler.",
                "No.",
                "You merely wished to keep climbing.",
                "And that desire proved heavier than the suffering below.",
                "That is why you awoke in the depths.",
                "Not for the good you performed.",
                "Not for the success you achieved.",
                "But for the truth you refused to carry.",
                "In life, you turned away from it.",
                "Yet truths are patient.",
                "They do not fade.",
                "Now, you find yourself at its edge.",
                "At the threshold between descent and ascent.",
                "The burden has been returned to your hands.",
                "No doubt remains to shelter you.",
                "This is the weight you sought to leave behind.",
                "Now, it is yours to bear."
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