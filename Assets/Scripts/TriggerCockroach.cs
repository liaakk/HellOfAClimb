using UnityEngine;

public class TriggerCockroach : MonoBehaviour
{
    public ControlCockroaches barata;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            barata.ComecarMovimento();
        }
    }
}