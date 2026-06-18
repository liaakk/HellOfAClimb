using UnityEngine;

[System.Serializable]
public class NotebookData
{
    public string entryName;

    public Sprite image;

    [TextArea]
    public string description;
}