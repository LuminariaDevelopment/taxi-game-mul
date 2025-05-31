using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class SparkOnCollision : MonoBehaviour
{
    [Tooltip("Layers to ignore (no sparks when colliding with these)")]
    [SerializeField] private LayerMask ignoredLayers;

    [Tooltip("Minimum relative speed to produce sparks")]
    public float minSpeed = 5f;

    void OnCollisionEnter(Collision col)
    {
        // Ignore if the other object is in an ignored layer
        if (((1 << col.gameObject.layer) & ignoredLayers) != 0)
        {
            return;
        }

        float speed = col.relativeVelocity.magnitude;
        if (speed < minSpeed)
        {
            return;
        }

        if (SparksPool.Instance == null)
        {
            return;
        }

        foreach (var contact in col.contacts)
        {
            var ps = SparksPool.Instance.GetSpark();
            if (ps == null) continue;

            ps.transform.position = contact.point;
            ps.transform.rotation = Quaternion.LookRotation(contact.normal);
            ps.Play();

            float wait = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(ReturnAfter(ps, wait));
        }
    }

    private IEnumerator ReturnAfter(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        SparksPool.Instance.ReturnSpark(ps);
    }
}
