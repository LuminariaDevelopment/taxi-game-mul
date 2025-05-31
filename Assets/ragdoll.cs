using UnityEngine;

public class ragdoll : MonoBehaviour
{
    [Header("Ragdoll Settings")]
    public float impactForceThreshold = 5f;  // Only ragdoll if hit is harder than this

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Animator animator;

    // Reference to the Passenger script from passenger.cs
    private Passenger passengerScript;

    // Ragdoll state flag (private but accessible via public property)
    private bool isRagdoll = false;
    public bool IsRagdolled { get { return isRagdoll; } }

    void Start()
    {
        animator = GetComponent<Animator>();

        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);
        
        // Get the passenger script attached to this GameObject
        passengerScript = GetComponent<Passenger>();

        EnableRagdoll(false);
    }

    void EnableRagdoll(bool enabled)
    {
        // Toggle physics for ragdoll rigidbodies and colliders
        foreach (var rb in ragdollBodies)
        {
            if (rb.gameObject != gameObject)
                rb.isKinematic = !enabled;
        }

        foreach (var col in ragdollColliders)
        {
            if (col.gameObject != gameObject)
                col.isTrigger = !enabled;
        }

        // Enable/Disable the animator
        if (animator)
            animator.enabled = !enabled;

        // Disable the passenger script when ragdolled, re-enable otherwise
        if (passengerScript != null)
            passengerScript.enabled = !enabled;

        // Change tag if ragdolled
        if (enabled)
            gameObject.tag = "Untagged";

        isRagdoll = enabled;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (!isRagdoll && collision.relativeVelocity.magnitude >= impactForceThreshold)
        {
            EnableRagdoll(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isRagdoll)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null && rb.velocity.magnitude >= impactForceThreshold)
            {
                EnableRagdoll(true);
            }
        }
    }
}
