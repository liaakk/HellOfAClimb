using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SquareAspect : MonoBehaviour
{
    void Start()
    {
        ApplyAspect();
    }

    void ApplyAspect()
    {
        Camera cam = GetComponent<Camera>();

        float targetAspect = 1f; // 1600x1600
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect > targetAspect)
        {
            float width = targetAspect / screenAspect;

            cam.rect = new Rect(
                (1f - width) / 2f,
                0f,
                width,
                1f
            );
        }
    }
}