using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private NovoMovimento player;
    [SerializeField] private CinemachineCamera cineCam;

    [SerializeField] private float normalZoom = 8f;
    [SerializeField] private float dashZoom = 4f;

    [SerializeField] private float zoomSpeed = 5f;

    private void Update()
    {
        float targetZoom = normalZoom;

        if (player.IsChargingDash)
        {
            targetZoom = dashZoom;
        }

        cineCam.Lens.OrthographicSize = Mathf.Lerp(
            cineCam.Lens.OrthographicSize,
            targetZoom,
            zoomSpeed * Time.deltaTime
        );
    }
}