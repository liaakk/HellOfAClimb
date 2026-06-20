using UnityEngine;

public class Falling : MonoBehaviour
{
    public float velocidade;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         print("Falling script started on " + gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 posicao = this.transform.localPosition;
        posicao.y += velocidade * Time.deltaTime;
        this.transform.localPosition = posicao;

            if (this.transform.localPosition.y < -30)
            {
                posicao.y = 40;
                this.transform.localPosition = posicao;
                print("Looped");
            }
    }
}