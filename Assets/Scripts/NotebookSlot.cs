using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotebookSlot : MonoBehaviour
{
    public Image entryImage;
    public TMP_Text entryName;
    public TMP_Text entryDescription;

    public void SetData(NotebookData data)
    {
        gameObject.SetActive(true);

        entryImage.sprite = data.image;
        entryName.text = data.entryName;
        entryDescription.text = data.description;

        entryImage.enabled = true;
        entryName.enabled = true;
        entryDescription.enabled = true;
    }

    public void Clear()
    {
        entryImage.enabled = false;
        entryName.enabled = false;
        entryDescription.enabled = false;
    }
}