using UnityEngine;

public sealed class SharedCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Renderer floorRenderer;

    [SerializeField, Min(0.01f)] private float followSmoothTime = 0.15f;
    [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.2f;
    [SerializeField, Range(0f, 0.35f)] private float viewportPadding = 0.12f;
    [SerializeField, Min(0f)] private float playerPadding = 0.8f;
    [SerializeField, Min(1f)] private float maxZoomMultiplier = 2f;
    [SerializeField, Min(0f)] private float cameraBoundsPadding = 0.5f;

    private Vector3 initialDirection;
    private Quaternion initialRotation;
    private float baseDistance;
    private float currentDistance;
    private float distanceVelocity;
    private Vector3 positionVelocity;
    private bool initialized;

    private void Awake()
    {
        targetCamera = targetCamera != null ? targetCamera : GetComponent<Camera>();
        if (targetCamera == null)
        {
            Debug.LogWarning(name + " SharedCameraFollow requires a Camera reference.");
            enabled = false;
            return;
        }

        initialRotation = targetCamera.transform.rotation;
        var midpoint = GetMidpoint();
        initialDirection = (targetCamera.transform.position - midpoint).normalized;
        if (initialDirection.sqrMagnitude < 0.001f)
            initialDirection = -targetCamera.transform.forward;

        baseDistance = Mathf.Max(0.01f, Vector3.Distance(targetCamera.transform.position, midpoint));
        currentDistance = baseDistance;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized || player1 == null || player2 == null) return;

        var midpoint = ClampToFloorBounds(GetMidpoint());
        var desiredDistance = CalculateRequiredDistance(midpoint);
        desiredDistance = Mathf.Clamp(desiredDistance, baseDistance, baseDistance * maxZoomMultiplier);

        currentDistance = Mathf.SmoothDamp(
            currentDistance,
            desiredDistance,
            ref distanceVelocity,
            Mathf.Max(0.01f, zoomSmoothTime));

        var desiredPosition = midpoint + initialDirection * currentDistance;
        targetCamera.transform.position = Vector3.SmoothDamp(
            targetCamera.transform.position,
            desiredPosition,
            ref positionVelocity,
            Mathf.Max(0.01f, followSmoothTime));

        targetCamera.transform.rotation = initialRotation;
    }

    private Vector3 GetMidpoint()
    {
        return (player1.position + player2.position) * 0.5f;
    }

    private Vector3 ClampToFloorBounds(Vector3 center)
    {
        if (floorRenderer == null)
        {
            return center;
        }

        var bounds = floorRenderer.bounds;
        var minX = bounds.min.x + cameraBoundsPadding;
        var maxX = bounds.max.x - cameraBoundsPadding;
        var minZ = bounds.min.z + cameraBoundsPadding;
        var maxZ = bounds.max.z - cameraBoundsPadding;

        if (minX > maxX) minX = maxX = bounds.center.x;
        if (minZ > maxZ) minZ = maxZ = bounds.center.z;

        center.x = Mathf.Clamp(center.x, minX, maxX);
        center.z = Mathf.Clamp(center.z, minZ, maxZ);
        return center;
    }

    private float CalculateRequiredDistance(Vector3 midpoint)
    {
        if (targetCamera == null || player1 == null || player2 == null) return baseDistance;

        var halfVerticalFov = targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        var verticalTangent = Mathf.Max(0.001f, Mathf.Tan(halfVerticalFov));
        var horizontalTangent = Mathf.Max(0.001f, verticalTangent * targetCamera.aspect);
        var safeWidth = Mathf.Max(0.05f, 1f - viewportPadding * 2f);

        var relative1 = player1.position - midpoint;
        var relative2 = player2.position - midpoint;
        var horizontalExtent = Mathf.Max(
            Mathf.Abs(Vector3.Dot(relative1, targetCamera.transform.right)),
            Mathf.Abs(Vector3.Dot(relative2, targetCamera.transform.right))) + playerPadding;
        var verticalExtent = Mathf.Max(
            Mathf.Abs(Vector3.Dot(relative1, targetCamera.transform.up)),
            Mathf.Abs(Vector3.Dot(relative2, targetCamera.transform.up))) + playerPadding;

        var horizontalDistance = horizontalExtent / (horizontalTangent * safeWidth);
        var verticalDistance = verticalExtent / (verticalTangent * safeWidth);
        return Mathf.Max(baseDistance, horizontalDistance, verticalDistance);
    }

    private void OnValidate()
    {
        followSmoothTime = Mathf.Max(0.01f, followSmoothTime);
        zoomSmoothTime = Mathf.Max(0.01f, zoomSmoothTime);
        maxZoomMultiplier = Mathf.Max(1f, maxZoomMultiplier);
        playerPadding = Mathf.Max(0f, playerPadding);
        cameraBoundsPadding = Mathf.Max(0f, cameraBoundsPadding);
    }
}