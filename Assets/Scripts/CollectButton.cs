using UnityEngine;
using UnityEngine.Video;

public class CollectButton : MonoBehaviour
{
    [Header("Notebook")]
    public NotebookData notebookData;

    [Header("Cutscene")]
    public VideoClip memoryVideo;

    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            UIButtonsManager.Instance.AddButton();
            Notebook.Instance.UnlockMemory(notebookData);

            if (CutsceneVideoManager.Instance != null && memoryVideo != null)
            {
                CutsceneVideoManager.Instance.PlayCutscene(memoryVideo);
            }
            else
            {
                Debug.LogWarning("CutsceneVideoManager ou memoryVideo em falta!");
            }

            Destroy(gameObject);
        }
    }
}