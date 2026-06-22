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

        if (entryImage != null)
        {
            entryImage.sprite = data.image;
            entryImage.enabled = true;
        }

        if (entryName != null)
        {
            entryName.text = data.entryName;
            entryName.enabled = true;
        }

        if (entryDescription != null)
        {
            entryDescription.text = data.description;
            entryDescription.enabled = true;
        }
    }

    public void Clear()
    {
         if (entryImage != null)
        {
            entryImage.enabled = false;
        }

        if (entryName != null)
            entryName.enabled = false;

        if (entryDescription != null)
            entryDescription.enabled = false;
    }
}