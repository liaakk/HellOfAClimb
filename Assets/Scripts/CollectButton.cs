using UnityEngine;

public class CollectButton : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("BOTÃO COLETADO!");

            UIButtonsManager.Instance.AddButton();

            Destroy(gameObject);
        }
    }
}