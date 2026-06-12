using UnityEngine;

public class parallax : MonoBehaviour
{
    public Transform cam;
    public float relativeMovement = .3f;

    public bool lockY = false;
    public bool lockX = false;

    // Update is called once per frame
    void Update()
    {
        Vector2 newPosition = transform.position;
        if (!lockX)
            newPosition.x = cam.position.x * relativeMovement;
        if (!lockY)
            newPosition.y = cam.position.y * relativeMovement;
        transform.position = newPosition;
    }
}
