using UnityEngine;

[RequireComponent(typeof(SCC_Drivetrain))]
[RequireComponent(typeof(SCC_InputProcessor))]
[RequireComponent(typeof(Rigidbody))]
public class ChasePlayer : MonoBehaviour {

    public Transform player;

    [Header("Chase & Steering")]
    public float predictionTime = 0.5f;
    public float fullThrottleDistance = 30f;
    public float brakeDistance = 8f;
    public float steerSmoothing = 5f;
    public float rotationSpeed = 5f;

    [Header("PIT Maneuvering")]
    public bool enablePitManeuver = true;
    public float pitRange = 8f;
    public float pitAngleThreshold = 25f;
    public float pitForce = 800f;
    public float pitApproachOffset = 2f; // offset to the side

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;
    public float detectionDistance = 8f;
    public float sideRayOffset = 1.5f;
    public float avoidSteerStrength = 0.7f;

    [Header("Stuck Handling")]
    public float stuckSpeedThreshold = 0.5f;
    public float stuckTime = 1.5f;
    public float reverseDuration = 2f;
    private float stuckTimer = 0f;
    private float reverseTimer = 0f;
    private bool isReversing = false;

    private SCC_InputProcessor inputProcessor;
    private SCC_Drivetrain drivetrain;
    private Rigidbody rb;
    private Rigidbody playerRb;

    private float currentSteerInput = 0f;
    private Vector3 debugTargetPos;
    private bool preparingPit = false;

    void Awake() {
        inputProcessor = GetComponent<SCC_InputProcessor>();
        drivetrain = GetComponent<SCC_Drivetrain>();
        rb = GetComponent<Rigidbody>();
        if (player != null)
            playerRb = player.GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        if (player == null) return;
        if (playerRb == null) playerRb = player.GetComponent<Rigidbody>();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float speed = rb.velocity.magnitude;
        float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
        float playerSpeed = playerRb.velocity.magnitude;

        Vector3 predictedTarget = PredictPlayerPosition();
        debugTargetPos = predictedTarget;

        Vector3 localTarget = transform.InverseTransformPoint(predictedTarget);
        float targetSteer = Mathf.Clamp(localTarget.x / localTarget.magnitude, -1f, 1f);

        // === Obstacle Avoidance Override ===
        float obstacleSteer = DetectObstacles();
        if (Mathf.Abs(obstacleSteer) > 0.01f) {
            targetSteer = obstacleSteer;
        }

        currentSteerInput = Mathf.Lerp(currentSteerInput, targetSteer, steerSmoothing * Time.fixedDeltaTime);

        float throttleInput = 0f;
        float brakeInput = 0f;

        // === Reverse Logic ===
        if (isReversing) {
            reverseTimer -= Time.fixedDeltaTime;
            if (reverseTimer <= 0f) {
                isReversing = false;
                stuckTimer = 0f;
            }
            throttleInput = -1f;
        } else {
            if (forwardSpeed < stuckSpeedThreshold && Mathf.Abs(inputProcessor.inputs.throttleInput) > 0.5f) {
                stuckTimer += Time.fixedDeltaTime;
                if (stuckTimer >= stuckTime) {
                    isReversing = true;
                    reverseTimer = reverseDuration;
                    stuckTimer = 0f;
                }
            } else {
                stuckTimer = 0f;
            }

            // === Speed Matching ===
            if (distanceToPlayer < pitRange) {
                float speedDelta = playerSpeed - forwardSpeed;

                if (Mathf.Abs(speedDelta) < 2f) {
                    // Close to speed match – prepare PIT
                    preparingPit = true;
                    throttleInput = Mathf.Clamp01(speedDelta + 1f); // slight push
                } else {
                    // Try to match
                    throttleInput = Mathf.Clamp01(speedDelta + 1f);
                    preparingPit = false;
                }
            } else {
                preparingPit = false;

                if (distanceToPlayer > fullThrottleDistance) {
                    throttleInput = 1f;
                } else if (distanceToPlayer < brakeDistance) {
                    brakeInput = 1f;
                } else {
                    throttleInput = Mathf.InverseLerp(brakeDistance, fullThrottleDistance, distanceToPlayer);
                }
            }
        }

        // === Rotation (if moving) ===
        if (speed > 1f && !isReversing) {
            Vector3 dir = (predictedTarget - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            float scaledRotSpeed = rotationSpeed * Mathf.Clamp01(speed / 10f);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, scaledRotSpeed * Time.fixedDeltaTime));
        }

        if (enablePitManeuver && preparingPit && !isReversing) {
            TryStrategicPIT();
        }

        inputProcessor.inputs.steerInput = currentSteerInput;
        inputProcessor.inputs.throttleInput = throttleInput;
        inputProcessor.inputs.brakeInput = brakeInput;
        inputProcessor.inputs.handbrakeInput = 0f;
    }

    Vector3 PredictPlayerPosition() {
        return player.position + playerRb.velocity * predictionTime;
    }

    void TryStrategicPIT() {
        Vector3 toPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, toPlayer);

        if (angle < pitAngleThreshold) {
            // Choose side offset
            Vector3 side = player.right * (Random.value > 0.5f ? 1 : -1);
            Vector3 quarterTarget = player.position - player.forward * 2f + side * pitApproachOffset;

            Vector3 dir = (quarterTarget - transform.position).normalized;
            rb.AddForce(dir * pitForce, ForceMode.Force);
        }
    }

    float DetectObstacles() {
        float steer = 0f;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 fwd = transform.forward;
        Vector3 left = Quaternion.AngleAxis(-30, Vector3.up) * fwd;
        Vector3 right = Quaternion.AngleAxis(30, Vector3.up) * fwd;

        RaycastHit hit;
        if (Physics.Raycast(origin, fwd, out hit, detectionDistance, obstacleMask)) steer -= avoidSteerStrength;
        if (Physics.Raycast(origin + transform.right * sideRayOffset, right, out hit, detectionDistance, obstacleMask)) steer -= avoidSteerStrength;
        if (Physics.Raycast(origin - transform.right * sideRayOffset, left, out hit, detectionDistance, obstacleMask)) steer += avoidSteerStrength;

        return Mathf.Clamp(steer, -1f, 1f);
    }

    void OnDrawGizmosSelected() {
        if (player != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, debugTargetPos);
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, transform.forward * detectionDistance);
        Gizmos.DrawRay(origin + transform.right * sideRayOffset, Quaternion.AngleAxis(30, Vector3.up) * transform.forward * detectionDistance);
        Gizmos.DrawRay(origin - transform.right * sideRayOffset, Quaternion.AngleAxis(-30, Vector3.up) * transform.forward * detectionDistance);

        if (isReversing) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position - transform.forward * 3f);
        }

        if (preparingPit) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, pitRange);
        }
    }
}
