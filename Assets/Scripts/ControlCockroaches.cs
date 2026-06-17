using UnityEngine;

public class ControlCockroaches : MonoBehaviour
{
    public Transform destino;
    public float velocidade = 3f;
    private bool mover = false;

    void Update()
    {
        if (mover)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                destino.position,
                velocidade * Time.deltaTime
            );

            // Quando chegar ao destino, para
            if (Vector2.Distance(transform.position, destino.position) < 0.05f)
            {
                mover = false;
            }
        }
    }

    public void ComecarMovimento()
    {
        mover = true;
    }
}
