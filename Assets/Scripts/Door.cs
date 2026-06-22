using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Door : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float pullTime = 0.6f;

    private bool isTransitioning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;

        if (other.transform.root.CompareTag("Player"))
        {
            StartCoroutine(Transition(other.transform.root.gameObject));
        }
    }

    private IEnumerator Transition(GameObject player)
    {
        isTransitioning = true;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        // trava movimento
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        Vector3 startPos = player.transform.position;
        Vector3 endPos = transform.position;

        float t = 0;

        // puxar o player para a porta
        while (t < pullTime)
        {
            t += Time.deltaTime;
            player.transform.position = Vector3.Lerp(startPos, endPos, t / pullTime);
            yield return null;
        }

       
         
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }
}