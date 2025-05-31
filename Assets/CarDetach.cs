using UnityEngine;
using System.Collections.Generic;

public class CarDetach : MonoBehaviour
{
    public float detachForceThreshold = 10f;

    [Header("Optional: Only detach these specific parts")]
    public List<Transform> detachableParts;

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        Debug.Log($"Car collided with {collision.gameObject.name}, force: {impactForce}");

        if (impactForce >= detachForceThreshold)
        {
            DetachParts();
        }
    }

    void DetachParts()
    {
        if (detachableParts.Count > 0)
        {
            foreach (Transform part in detachableParts)
            {
                TryDetach(part);
            }
        }
        else
        {
            // If no specific list, go through all children
            foreach (Transform child in transform)
            {
                TryDetach(child);
            }
        }
    }

    void TryDetach(Transform part)
    {
        // Unparent the part first
        part.parent = null;

        Rigidbody rb = part.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = part.gameObject.AddComponent<Rigidbody>();
            rb.mass = 5f;
        }

        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        Debug.Log($"Detached and activated physics on: {part.name}");
    }
}
