using UnityEngine;

public class CollectButton : MonoBehaviour
{
    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            UIButtonsManager.Instance.AddButton();

            Destroy(gameObject);
        }
    }
}