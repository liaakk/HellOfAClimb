using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NotebookAnimationController : MonoBehaviour
{
    public static NotebookAnimationController Instance;

    [Header("Livro")]
    public Image notebookImage;

    [Header("Sprites")]
    public Sprite[] bookFrames;

    [Header("Posições")]
    public RectTransform bookTransform;

    [Tooltip("Posição escondida em cima da tela")]
    public Vector2 hiddenPosition;

    [Tooltip("Posição visível onde o livro fica parado")]
    public Vector2 visiblePosition;

    [Header("Velocidade")]
    public float moveDuration = 0.4f;
    public float frameSpeed = 0.05f;
    public float openHoldTime = 0.4f;
    public float beforeGoUpTime = 0.2f;

    [Header("Glow")]
    public Image glowImage;

    private bool isPlaying = false;
    private float fixedX;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (bookTransform != null)
        {
            // guarda o X original do livro
            fixedX = bookTransform.anchoredPosition.x;

            // começa escondido em cima
            bookTransform.anchoredPosition = new Vector2(fixedX, hiddenPosition.y);
        }

        if (notebookImage != null && bookFrames.Length > 0)
            notebookImage.sprite = bookFrames[0];

        if (glowImage != null)
            glowImage.gameObject.SetActive(false);
    }

    public void PlayAnimation()
    {
        if (!isPlaying)
            StartCoroutine(AnimationRoutine());
    }

    IEnumerator AnimationRoutine()
    {
        isPlaying = true;

        // começa escondido em cima
        bookTransform.anchoredPosition = new Vector2(fixedX, hiddenPosition.y);

        // 1. Desce até à posição visível
        yield return MoveBookToY(visiblePosition.y);

        // 2. Abre até ao frame 18
        for (int i = 0; i <= 18; i++)
        {
            notebookImage.sprite = bookFrames[i];
            yield return new WaitForSeconds(frameSpeed);
        }

        // 3. Fica aberto um pouquinho
        yield return new WaitForSeconds(openHoldTime);

        // 4. Glow
        if (glowImage != null)
        {
            glowImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            glowImage.gameObject.SetActive(false);
        }

        // 5. Fecha até ao frame 23
        for (int i = 18; i <= 23; i++)
        {
            notebookImage.sprite = bookFrames[i];
            yield return new WaitForSeconds(frameSpeed);
        }

        // 6. Fica fechado só um instante
        yield return new WaitForSeconds(beforeGoUpTime);

        // 7. Sobe outra vez para a posição escondida
        yield return MoveBookToY(hiddenPosition.y);

        isPlaying = false;
    }

    IEnumerator MoveBookToY(float targetY)
    {
        Vector2 startPosition = bookTransform.anchoredPosition;
        Vector2 endPosition = new Vector2(fixedX, targetY);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / moveDuration;

            // deixa o movimento mais suave
            t = Mathf.SmoothStep(0f, 1f, t);

            bookTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        bookTransform.anchoredPosition = endPosition;
    }
}