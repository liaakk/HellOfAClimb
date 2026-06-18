using UnityEngine;
using UnityEngine.InputSystem;

public class CollectObject : MonoBehaviour
{
    [Header("Notebook")]
    public NotebookData notebookData;

    [Header("Interação")]
    public Transform player;
    public float interactionDistanceX = 2f;
    public float interactionDistanceY = 2f;

    [Header("Hint Text")]
    public GameObject hintText;

    private bool collected = false;

    void Start()
    {
        if (hintText != null)
            hintText.SetActive(false);
    }

    void Update()
    {
        if (collected) return;

        bool isNear = IsPlayerNear();

        // Mostrar o "E"
        if (hintText != null)
            hintText.SetActive(isNear);

        // Apanhar objeto
        if (isNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Collect();
        }
    }

    void Collect()
    {
        collected = true;

        if (hintText != null)
            hintText.SetActive(false);

        Notebook.Instance.UnlockObject(notebookData);

        Destroy(gameObject);
    }

    bool IsPlayerNear()
    {
        if (player == null) return false;

        float dx = Mathf.Abs(transform.position.x - player.position.x);
        float dy = Mathf.Abs(transform.position.y - player.position.y);

        return dx <= interactionDistanceX && dy <= interactionDistanceY;
    }
}