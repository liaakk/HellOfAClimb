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
    public Vector2 hiddenPosition;
    public Vector2 visiblePosition;

    [Header("Velocidade")]
    public float moveSpeed = 500f;
    public float frameSpeed = 0.05f;

    [Header("Glow")]
    public Image glowImage;

    bool isPlaying = false;

    private void Awake()
    {
        Instance = this;
    }

void Start()
{
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

        yield return MoveBook(hiddenPosition, visiblePosition);

        // abrir até frame 18
        for (int i = 0; i <= 18; i++)
        {
            notebookImage.sprite = bookFrames[i];
            yield return new WaitForSeconds(frameSpeed);
        }

        // glow
        if (glowImage != null)
        {
            glowImage.gameObject.SetActive(true);

            yield return new WaitForSeconds(1f);

            glowImage.gameObject.SetActive(false);
        }

        // fechar até frame 23
        for (int i = 18; i <= 23; i++)
        {
            notebookImage.sprite = bookFrames[i];
            yield return new WaitForSeconds(frameSpeed);
        }

        yield return MoveBook(visiblePosition, hiddenPosition);

        isPlaying = false;
    }

    IEnumerator MoveBook(Vector2 start, Vector2 end)
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 3f;

            bookTransform.anchoredPosition =
                Vector2.Lerp(start, end, t);

            yield return null;
        }
    }
}