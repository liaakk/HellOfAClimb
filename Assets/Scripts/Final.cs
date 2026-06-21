using UnityEngine;
using UnityEngine.SceneManagement;

public class Final : MonoBehaviour
{
    [SerializeField] private Collider2D BeginFinal;
    [SerializeField] private Collider2D LoopEnding;
    [SerializeField] private Collider2D GoodEnding;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BeginFinal.enabled = true;
        LoopEnding.enabled = false;
        GoodEnding.enabled = false;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (collision.IsTouching(BeginFinal))
        {
            BeginFinal.enabled = false;
            LoopEnding.enabled = true;
            GoodEnding.enabled = true;
        }

        if (collision.IsTouching(LoopEnding))
        {
            Loop();
        }

        if (collision.IsTouching(GoodEnding))
        {
            Ending();
        }
    }
    void Loop() 
    {
        print("LoopEnding");
        SceneManager.LoadScene("MainMenu");
    }

    void Ending() 
    {
        print("Ending");
        SceneManager.LoadScene("MainMenu");
    }
}
