using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SoftPermanentDeformation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshFilter[] meshFilters = default;
    [SerializeField] private MeshCollider[] colliders = default;

    [Header("Impact Settings")]
    [SerializeField] private float impactDamage = 1f;
    [SerializeField] private float deformationRadius = 0.5f;
    [SerializeField] private float maxDeformation = 0.5f;
    [SerializeField] private float minVelocity = 2f;
    [SerializeField] private float delayTimeDeform = 0.1f;

    [Header("Spring & Permanence")]
    [SerializeField] [Range(0, 1)] private float permanence = 0.5f;
    [SerializeField] private float springStrength = 20f;
    [SerializeField] private float damping = 4f;
    [SerializeField] private float baselineRestoreSpeed = 0.2f;

    [Header("Layers to Ignore")]
    [SerializeField] private LayerMask ignoredLayers;

    // Cached for speed
    private Mesh[]        meshes;
    private Transform[]   transforms;
    private Vector3[][]   originalVerts;
    private Vector3[][]   baselineVerts;
    private Vector3[][]   velocities;
    private bool[]        dirty;

    private float nextTimeDeform;
    private float rSq, dmg;

    private void Start()
    {
        int count = meshFilters.Length;
        meshes     = new Mesh[count];
        transforms = new Transform[count];
        originalVerts = new Vector3[count][];
        baselineVerts = new Vector3[count][];
        velocities    = new Vector3[count][];
        dirty         = new bool[count];

        // Precompute constants
        rSq = deformationRadius * deformationRadius;
        dmg = impactDamage;

        for (int i = 0; i < count; i++)
        {
            var mf = meshFilters[i];
            meshes[i]     = mf.mesh;
            transforms[i] = mf.transform;

            var verts = meshes[i].vertices;
            originalVerts[i] = (Vector3[])verts.Clone();
            baselineVerts[i] = (Vector3[])verts.Clone();
            velocities[i]    = new Vector3[verts.Length];
            meshes[i].MarkDynamic();
            dirty[i] = false;
        }
    }

    private void Update()
    {
        SimulateSprings();
        RestoreBaseline();
        CommitMeshUpdates();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < nextTimeDeform) return;
        if (collision.relativeVelocity.sqrMagnitude < minVelocity * minVelocity) return;
        if (((1 << collision.gameObject.layer) & ignoredLayers) != 0) return;

        nextTimeDeform = Time.time + delayTimeDeform;

        Vector3 worldP = collision.contacts[0].point;
        Vector3 worldF = collision.relativeVelocity * 0.02f;

        for (int i = 0; i < meshes.Length; i++)
            ApplyImpact(i, worldP, worldF);
    }

    private void ApplyImpact(int i, Vector3 worldP, Vector3 worldF)
    {
        var mesh = meshes[i];
        Vector3[] verts = mesh.vertices;

        // Transform once
        Vector3 localP = transforms[i].InverseTransformPoint(worldP);
        Vector3 localF = transforms[i].InverseTransformDirection(worldF);

        for (int j = 0, len = verts.Length; j < len; j++)
        {
            Vector3 delta = verts[j] - localP;
            float d2 = delta.sqrMagnitude;
            if (d2 > rSq) continue;

            // squared falloff: falloff ~= (1 - (d/r)^2)^2
            float norm = 1f - (d2 * (1f / rSq));
            float falloff = norm * norm;

            // apply velocity
            Vector3 impulse = localF * (falloff * dmg);
            velocities[i][j] += impulse;

            // permanent baseline shift
            Vector3 def = impulse * (permanence * Time.deltaTime);
            baselineVerts[i][j] += def;

            // clamp max deformation
            Vector3 offset = baselineVerts[i][j] - originalVerts[i][j];
            if (offset.sqrMagnitude > maxDeformation * maxDeformation)
            {
                baselineVerts[i][j] = originalVerts[i][j] + offset.normalized * maxDeformation;
            }

            dirty[i] = true;
        }
    }

    private void SimulateSprings()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < meshes.Length; i++)
        {
            Vector3[] verts = meshes[i].vertices;
            bool changed = false;

            for (int j = 0, len = verts.Length; j < len; j++)
            {
                Vector3 rest = baselineVerts[i][j];
                Vector3 pos  = verts[j];
                Vector3 vel  = velocities[i][j];

                // spring + damping
                Vector3 accel = (rest - pos) * springStrength - vel * damping;
                vel += accel * dt;
                pos += vel * dt;

                if (vel.sqrMagnitude > 1e-6f)
                {
                    verts[j] = pos;
                    velocities[i][j] = vel;
                    changed = true;
                }
            }

            if (changed)
            {
                meshes[i].vertices = verts;
                dirty[i] = true;
            }
        }
    }

    private void RestoreBaseline()
    {
        if (baselineRestoreSpeed <= 0f) return;
        float rate = baselineRestoreSpeed * Time.deltaTime;

        for (int i = 0; i < baselineVerts.Length; i++)
        {
            var baseArr = baselineVerts[i];
            var origArr = originalVerts[i];

            for (int j = 0; j < baseArr.Length; j++)
            {
                baseArr[j] = Vector3.MoveTowards(baseArr[j], origArr[j], rate);
            }
        }
    }

    private void CommitMeshUpdates()
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            if (!dirty[i]) continue;
            dirty[i] = false;

            var mesh = meshes[i];
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (i < colliders.Length && colliders[i] != null)
                colliders[i].sharedMesh = mesh;
        }
    }
}