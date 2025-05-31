using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ApplyForce : MonoBehaviour
{
    [Tooltip("Direction of the impulse in world‐space.")]
    public Vector3 impulseDirection = Vector3.forward;
    [Tooltip("Strength of the impulse.")]
    public float impulseMagnitude = 10f;

    private Rigidbody rb;
    private bool hasImpulsed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!hasImpulsed && Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 impulse = impulseDirection.normalized * impulseMagnitude;
            rb.AddForce(impulse, ForceMode.Impulse);
            hasImpulsed = true;
        }
    }

}
