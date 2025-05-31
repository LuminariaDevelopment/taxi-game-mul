using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class SoftbodyMeshDeformer : MonoBehaviour
{
    [Header("Deformation Settings")]
    [Tooltip("Radius around collision point to affect vertices.")]
    public float deformationRadius = 0.5f;
    [Tooltip("Multiplier to scale deformation force.")]
    public float forceMultiplier = 1f;

    [Header("Spring Settings")]
    [Tooltip("How strong the mesh springs back to original shape.")]
    public float springForce = 20f;
    [Tooltip("Damping for the spring motion.")]
    public float damping = 5f;

    [Header("Rigidbody Override")]
    [Tooltip("Optionally specify a Rigidbody manually if it's not on this object or its parent.")]
    public Rigidbody externalRigidbodyOverride;

    private Mesh deformingMesh;
    private Vector3[] originalVertices, displacedVertices, vertexVelocities;
    private MeshCollider meshCollider;
    private Rigidbody rb;

    void Start()
    {
        // Resolve Rigidbody: override > self > parent > root
        rb = externalRigidbodyOverride
             ?? GetComponent<Rigidbody>()
             ?? GetComponentInParent<Rigidbody>()
             ?? transform.root.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError($"[{nameof(SoftbodyMeshDeformer)}] No Rigidbody found on '{name}', its parents, or root. Please assign one in 'externalRigidbodyOverride'.");
        }
        else
        {
            Debug.Log($"[{nameof(SoftbodyMeshDeformer)}] Using Rigidbody from '{rb.gameObject.name}' for deformation.");
        }

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.convex = true;

        // Clone the mesh at runtime so we don't modify the shared asset
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh sharedMesh = mf.sharedMesh;
        deformingMesh = Instantiate(sharedMesh);
        deformingMesh.name = sharedMesh.name + " (deforming)";
        mf.mesh = deformingMesh;
        meshCollider.sharedMesh = deformingMesh;

        // Cache original and displaced vertices
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        vertexVelocities = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
            vertexVelocities[i] = Vector3.zero;
        }
    }

    void Update()
    {
        // Spring back each vertex
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            Vector3 offset = displacedVertices[i] - originalVertices[i];
            Vector3 springAcc = -springForce * offset;
            vertexVelocities[i] += springAcc * Time.deltaTime;
            vertexVelocities[i] *= (1f - damping * Time.deltaTime);
            displacedVertices[i] += vertexVelocities[i] * Time.deltaTime;
        }

        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
        meshCollider.sharedMesh = deformingMesh;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb == null) return;

        Vector3 collisionForce = collision.relativeVelocity * rb.mass;
        foreach (ContactPoint contact in collision.contacts)
        {
            ApplyDeformation(contact.point, collisionForce);
        }
    }

    void ApplyDeformation(Vector3 point, Vector3 force)
    {
        Vector3 localPoint = transform.InverseTransformPoint(point);
        float sqrRad = deformationRadius * deformationRadius;

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            Vector3 toVert = displacedVertices[i] - localPoint;
            float sqDist = toVert.sqrMagnitude;
            if (sqDist > sqrRad) continue;

            float dist = Mathf.Sqrt(sqDist);
            float falloff = 1f - (dist / deformationRadius);

            Vector3 dir = toVert.normalized;
            Vector3 vel = dir * force.magnitude * forceMultiplier * falloff;
            vertexVelocities[i] += vel * Time.deltaTime;
        }
    }
}
