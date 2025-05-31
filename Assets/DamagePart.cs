using UnityEngine;
using System.Collections;

public class DamagePart : MonoBehaviour
{
    private HingeJoint hingeJoint;
    private JointLimits limits;

    [Header("Damage Thresholds")]
    public float impactForceThreshold = 5f;
    public float breakOffForceThreshold = 15f;
    public float maxOpenAngle = 120f;

    [Header("Colliders to Enable on Break")]
    public Collider[] activateOnBreakColliders;
    public float colliderActivationDelay = 2f;

    [Header("Layers to Ignore")]
    [SerializeField] private LayerMask ignoredLayers;

    private bool isDamaged = false;

    void Start()
    {
        hingeJoint = GetComponent<HingeJoint>();

        if (hingeJoint != null)
        {
            limits = hingeJoint.limits;
            // Lock the hinge joint at start
            limits.min = 0;
            limits.max = 0;
            hingeJoint.limits = limits;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignore specified layers
        if (((1 << collision.gameObject.layer) & ignoredLayers) != 0)
        {
            return;
        }

        float impactForce = collision.relativeVelocity.magnitude;

        if (!isDamaged && impactForce > impactForceThreshold)
        {
            OpenPart();
        }

        if (impactForce > breakOffForceThreshold)
        {
            BreakOffPart();
        }
    }

    void OpenPart()
    {
        if (hingeJoint != null)
        {
            isDamaged = true;
            limits.max = maxOpenAngle;
            hingeJoint.limits = limits;
        }
    }

    void BreakOffPart()
    {
        if (hingeJoint != null)
        {
            hingeJoint.connectedBody = null;
            Destroy(hingeJoint);
        }

        transform.parent = null;

        Rigidbody partRigidbody = GetComponent<Rigidbody>();
        if (partRigidbody == null)
        {
            partRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        partRigidbody.isKinematic = false;
        partRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        StartCoroutine(ActivateCollidersAfterDelay());
    }

    private IEnumerator ActivateCollidersAfterDelay()
    {
        yield return new WaitForSeconds(colliderActivationDelay);

        foreach (var col in activateOnBreakColliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }
    }
}
