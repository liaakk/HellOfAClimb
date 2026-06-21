using UnityEngine;

public class CloudFlight : MonoBehaviour
{
    public float velocidade;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 posicao = this.transform.position;
        posicao.x += velocidade * Time.deltaTime;
        this.transform.position = posicao;
            if (this.transform.position.x < -48.5)
            {
                posicao.x = 0;
                this.transform.position = posicao;
            }
    }
}