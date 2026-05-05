using UnityEngine;

public class LineController : MonoBehaviour
{
    [Header("Rope Physics")]
    [SerializeField] private int ropeSegments = 10; // Number of segments in the rope
    [SerializeField] private float segmentLength = 0.5f; // Length of each segment
    [SerializeField] private float ropeStiffness = 50f; // Spring force (constraint strength)
    [SerializeField] private float ropeDamping = 0.95f; // Velocity damping (0-1)
    [SerializeField] private Vector2 gravity = new Vector2(0, -9.81f); // Custom gravity
    [SerializeField] private float pullForce = 5f; // Force applied to the player

    private LineRenderer lineRenderer;
    private Transform anchorPoint; // Fixed point where rope is attached
    private Transform playerTransform; // Player being dragged
    private Rigidbody2D playerRigidbody;

    // Rope segments - each stores position and old position for Verlet integration
    private Vector3[] segmentPositions;
    private Vector3[] segmentOldPositions;
    private float ropeLength;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetPoints(Transform pointA, Transform pointB, Rigidbody2D playerBody = null)
    {
        anchorPoint = pointA;
        playerTransform = pointB;
        playerRigidbody = playerBody;

        // Initialize rope segments
        InitializeRope();
    }

    private void InitializeRope()
    {
        if (anchorPoint == null || playerTransform == null)
            return;

        segmentPositions = new Vector3[ropeSegments + 1];
        segmentOldPositions = new Vector3[ropeSegments + 1];

        // Set initial positions along the line between anchor and player
        Vector3 direction = (playerTransform.position - anchorPoint.position).normalized;
        ropeLength = Vector3.Distance(anchorPoint.position, playerTransform.position);

        for (int i = 0; i <= ropeSegments; i++)
        {
            float t = (float)i / ropeSegments;
            Vector3 pos = anchorPoint.position + direction * ropeLength * t;
            segmentPositions[i] = pos;
            segmentOldPositions[i] = pos;
        }
    }

    private void Update()
    {
        if (anchorPoint == null || playerTransform == null)
            return;

        // Update rope physics
        SimulateRope();

        // Apply rope tension force to player
        ApplyRopeForceToPlayer();

        // Update the line renderer
        UpdateLineRenderer();
    }

    private void SimulateRope()
    {
        // First segment is anchored
        segmentPositions[0] = anchorPoint.position;
        segmentOldPositions[0] = anchorPoint.position;

        // Simulate each segment using Verlet integration
        for (int i = 1; i <= ropeSegments; i++)
        {
            Vector3 pos = segmentPositions[i];
            Vector3 oldPos = segmentOldPositions[i];
            Vector3 vel = (pos - oldPos) * ropeDamping;
            Vector3 newPos = pos + vel + (Vector3)gravity * Time.deltaTime * Time.deltaTime;

            segmentOldPositions[i] = pos;
            segmentPositions[i] = newPos;
        }

        // Constraint solving - maintain rope length
        float stiffnessCoefficient = ropeStiffness / 1000f; // Normalize stiffness for reasonable values
        for (int iter = 0; iter < 3; iter++) // Multiple iterations for stability
        {
            for (int i = 0; i < ropeSegments; i++)
            {
                Vector3 pos1 = segmentPositions[i];
                Vector3 pos2 = segmentPositions[i + 1];
                float distance = Vector3.Distance(pos1, pos2);
                float targetDistance = segmentLength;

                if (distance > 0.001f)
                {
                    Vector3 direction = (pos2 - pos1) / distance;
                    float difference = (distance - targetDistance) / 2f;
                    Vector3 offset = direction * difference * stiffnessCoefficient;

                    // Don't move the anchor point (first segment)
                    if (i > 0)
                        segmentPositions[i] -= offset;
                    segmentPositions[i + 1] += offset;
                }
            }
        }

        // Keep the last segment close to the player
        segmentPositions[ropeSegments] = Vector3.Lerp(
            segmentPositions[ropeSegments],
            playerTransform.position,
            0.3f
        );
    }

    private void ApplyRopeForceToPlayer()
    {
        if (playerRigidbody == null)
            return;

        // Get the position of the rope's last segment (closest to player)
        Vector3 ropeEnd = segmentPositions[ropeSegments];
        Vector3 directionToRope = (ropeEnd - playerTransform.position).normalized;
        float distanceToRope = Vector3.Distance(playerTransform.position, ropeEnd);

        // Apply pulling force toward the rope
        if (distanceToRope > 0.1f)
        {
            Vector3 force = directionToRope * pullForce;
            playerRigidbody.AddForce(force, ForceMode2D.Force);
        }
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = ropeSegments + 1;
        for (int i = 0; i <= ropeSegments; i++)
        {
            lineRenderer.SetPosition(i, segmentPositions[i]);
        }
    }

    // Optional: Get rope tension at any point for visual effects
    public float GetRopeTension()
    {
        Vector3 pos1 = segmentPositions[0];
        Vector3 pos2 = segmentPositions[ropeSegments];
        return Vector3.Distance(pos1, pos2);
    }
}