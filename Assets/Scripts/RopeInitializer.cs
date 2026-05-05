using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RopeInitializer : MonoBehaviour
{
    [Tooltip("LineController that renders/simulates the rope. If null, the script will try to find one in the scene.")]
    public LineController lineController;

    [Tooltip("Attach point transform (child of the player). If null the script will look for a child named 'RopeAnchor'.")]
    public Transform anchor;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (lineController == null)
            lineController = FindObjectOfType<LineController>();

        if (anchor == null)
        {
            var child = transform.Find("RopeAnchor");
            if (child != null)
                anchor = child;
        }

        if (lineController == null)
        {
            Debug.LogWarning("RopeInitializer: No LineController found in scene.");
            return;
        }

        if (anchor == null)
        {
            Debug.LogWarning("RopeInitializer: No anchor assigned or child named 'RopeAnchor' found on player.");
            return;
        }

        // Pass anchor, player transform, and the player's Rigidbody2D so rope can pull the player
        lineController.SetPoints(anchor, transform, rb);
    }
}
