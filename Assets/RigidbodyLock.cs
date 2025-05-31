using UnityEngine;

public class RigidbodyLocker : MonoBehaviour
{
    private Rigidbody rb;
    private RigidbodyConstraints originalConstraints;

    void Awake()
    {
        // Cache the Rigidbody component.
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("RigidbodyLocker: No Rigidbody component found on this GameObject.");
            return;
        }
        
        // Store the Rigidbody's original constraints.
        originalConstraints = rb.constraints;
    }

    /// <summary>
    /// Lock the Rigidbody by freezing all movement and rotation.
    /// </summary>
    public void LockRigidbody()
    {
        if (rb == null)
            return;

        // Freeze all position and rotation.
        rb.constraints = RigidbodyConstraints.FreezeAll;
        // Optionally, reset the velocity to zero.
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log("Rigidbody has been locked.");
    }

    /// <summary>
    /// Unlock the Rigidbody by restoring its original constraints.
    /// </summary>
    public void UnlockRigidbody()
    {
        if (rb == null)
            return;

        // Restore the original Rigidbody constraints.
        rb.constraints = originalConstraints;

        Debug.Log("Rigidbody has been unlocked.");
    }
}
