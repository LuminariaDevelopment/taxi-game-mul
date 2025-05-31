using UnityEngine;

public class PropCollide : MonoBehaviour
{
    private Rigidbody rb;
    public string colliderLayerName = "Collider";
    public float minImpactSpeed = 5f; 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} is missing a Rigidbody!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(colliderLayerName))
        {
            Rigidbody otherRb = other.attachedRigidbody;
            if (otherRb != null && otherRb.velocity.magnitude >= minImpactSpeed)
            {
                if (rb != null && rb.isKinematic)
                {
                    rb.isKinematic = false;
                }
            }
        }
    }
}
