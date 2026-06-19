using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private NovoMovimento player;
    [SerializeField] private CinemachineCamera cineCam;

    [SerializeField] private float normalZoom = 8f;
    [SerializeField] private float dashZoom = 4f;
    [SerializeField] private float zoomSpeed = 5f;

    [SerializeField] private float transitionSpeed = 5f;

    private CinemachineFollow followComponent;

    [SerializeField] private Vector3 normalOffset = new Vector3(0, 5, -10);
    [SerializeField] private Vector3 targetOffsetLimbo = new Vector3(0, 9, -10);

    private void Awake()
    {
        followComponent = cineCam.GetComponent<CinemachineFollow>();

        if (followComponent == null)
        {
            Debug.LogError("No CinemachineFollow component found on the Cinemachine Camera!");
        }
    }

    private void Update()
    {
        if (player == null || followComponent == null)
            return;

        if (player.IsInGunk)
            return;

        float targetZoom = player.IsChargingDash ? dashZoom : normalZoom;

        cineCam.Lens.OrthographicSize = Mathf.Lerp(
            cineCam.Lens.OrthographicSize,
            targetZoom,
            zoomSpeed * Time.deltaTime
        );

        Vector3 targetOffset =
            player.transform.position.y > 293.5f
            ? targetOffsetLimbo
            : normalOffset;

        followComponent.FollowOffset = Vector3.Lerp(
            followComponent.FollowOffset,
            targetOffset,
            transitionSpeed * Time.deltaTime
        );
    }
}