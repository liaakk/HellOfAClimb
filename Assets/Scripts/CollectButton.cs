using UnityEngine;

public class CollectButton : MonoBehaviour
{
    public GameObject ButtonUI;
    [SerializeField] private float collectEnableDelay = 0.1f;

    private bool canCollect;

    private void Start()
    {
        Invoke(nameof(EnableCollect), collectEnableDelay);
    }

    private void EnableCollect()
    {
        canCollect = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canCollect)
            return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("BOTÃO COLETADO!");

            UIButtonsManager.Instance.AddButton();

            Destroy(gameObject);

            ButtonUI.GetComponent<Animator>().Play("OneButton");
        }
    }
}