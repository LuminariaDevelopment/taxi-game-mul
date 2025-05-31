using UnityEngine;

public class CarCam : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target;

    [Header("Isometric View Settings")]
    public float height = 15f;
    public float distance = 15f;
    public float verticalAngle = 45f;
    public float sideTiltAngle = 15f;
    public float followSpeed = 5f;

    [Header("Rotation Snap Settings")]
    public float snapAngle = 90f;
    public float rotationLerpSpeed = 5f;
    public float angleTolerance = 5f;

    [Header("Collision Settings")]
    public float minDistance = 5f;
    public float maxDistance = 30f;
    public float zoomSpeed = 10f;
    public LayerMask collisionLayers;
    public float collisionOffset = 0.3f;

    private float currentDistance;
    private float targetDistance;
    private Vector3 velocity = Vector3.zero;
    private float lastSnapYaw = 0f;
    private Quaternion currentCamRotation;

    void Start()
    {
        currentDistance = distance;
        targetDistance = distance;
        lastSnapYaw = Mathf.Round(target.eulerAngles.y / snapAngle) * snapAngle;
        currentCamRotation = Quaternion.Euler(verticalAngle, lastSnapYaw + sideTiltAngle, 0f);
    }

    void LateUpdate()
    {
        if (!target) return;

        // Scroll wheel zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            targetDistance -= scrollInput * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        float currentYaw = target.eulerAngles.y;
        float deltaYaw = Mathf.DeltaAngle(lastSnapYaw, currentYaw);

        if (Mathf.Abs(deltaYaw) >= snapAngle - angleTolerance)
        {
            lastSnapYaw = Mathf.Round(currentYaw / snapAngle) * snapAngle;
        }

        Quaternion desiredRotation = Quaternion.Euler(verticalAngle, lastSnapYaw + sideTiltAngle, 0f);
        currentCamRotation = Quaternion.Slerp(currentCamRotation, desiredRotation, Time.deltaTime * rotationLerpSpeed);

        Vector3 direction = currentCamRotation * Vector3.back;
        Vector3 targetPos = target.position + Vector3.up * height;
        Vector3 desiredCamPos = targetPos + direction * targetDistance;

        // Collision detection
        RaycastHit hit;
        if (Physics.Raycast(targetPos, direction, out hit, targetDistance, collisionLayers))
        {
            float hitDist = Vector3.Distance(targetPos, hit.point) - collisionOffset;
            currentDistance = Mathf.Lerp(currentDistance, Mathf.Clamp(hitDist, minDistance, targetDistance), Time.deltaTime * zoomSpeed);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
        }

        Vector3 finalCamPos = targetPos + direction.normalized * currentDistance;
        transform.position = Vector3.SmoothDamp(transform.position, finalCamPos, ref velocity, 1f / followSpeed);

        transform.LookAt(target.position + Vector3.up * 2f);
    }
}
