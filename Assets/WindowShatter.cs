 using UnityEngine; 

public class WindowShatter : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        int ignoredLayer = LayerMask.NameToLayer("People");
        if (collision.gameObject.layer == ignoredLayer)
            return;

        float impactSpeed = collision.relativeVelocity.magnitude;

        foreach (var contact in collision.contacts)
        {
            // Find the window that should break
            WindowProfile profile = contact.thisCollider.GetComponentInParent<WindowProfile>();
            if (profile == null)
            {
                var zone = contact.thisCollider.GetComponentInParent<ImpactZone>();
                if (zone != null)
                    profile = zone.targetWindow;
            }

            if (profile == null || profile.isBroken)
                continue;

            if (impactSpeed < profile.breakSpeedThreshold)
                return;

            Debug.Log($"Breaking window: {profile.gameObject.name}");

            // Disable the window's BoxCollider that was hit
            BoxCollider glassCollider = contact.thisCollider as BoxCollider;
            if (glassCollider != null)
            {
                glassCollider.enabled = false;
            }

            ShatterWindow(profile, contact.point);
            return;
        }
    }



    void ShatterWindow(WindowProfile profile, Vector3 impactPoint)
    {
        profile.isBroken = true;
        profile.gameObject.SetActive(false);

        // Disable the ImpactZone's GameObject if it exists *and* is flagged to deactivate
        ImpactZone zone = profile.GetComponentInParent<ImpactZone>();
        if (zone != null && zone.deactivateOnBreak)
        {
            zone.gameObject.SetActive(false);
        }

        var brokenGroup = profile.brokenWindowParent;
        brokenGroup.SetActive(true);
        brokenGroup.transform.SetParent(null);

        foreach (var rb in brokenGroup.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 dir = (rb.position - impactPoint).normalized
                        + Random.insideUnitSphere * 0.25f;
            rb.AddForce(dir * 50f);
        }

        Destroy(brokenGroup, 5f);
    }

}