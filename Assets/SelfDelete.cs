using UnityEngine;

public class SelfDelete : MonoBehaviour
{
    public float delayBeforeDestroy = 1.0f;
    private bool isScheduledForDestruction = false;
    private ragdoll ragdollScript;
    private Camera mainCamera;
    private Collider objectCollider;

    void Start()
    {
        ragdollScript = GetComponent<ragdoll>();
        mainCamera = Camera.main;
        objectCollider = GetComponent<Collider>();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            Debug.LogWarning("[DestroyWhenNotInCameraView] No Renderer found on " + gameObject.name);
        else
            Debug.Log("[DestroyWhenNotInCameraView] Found " + renderers.Length + " Renderer(s) on " + gameObject.name);
    }

    void Update()
    {
        if (ragdollScript != null && ragdollScript.IsRagdolled)
        {
            if (!IsInView())
            {
                if (!isScheduledForDestruction)
                {
                    Debug.Log("[DestroyWhenNotInCameraView] Object " + gameObject.name + " is not in view. Scheduling destruction in " + delayBeforeDestroy + " seconds.");
                    isScheduledForDestruction = true;
                    Invoke("DeleteSelf", delayBeforeDestroy);
                }
            }
            else
            {
                if (isScheduledForDestruction)
                {
                    Debug.Log("[DestroyWhenNotInCameraView] Object " + gameObject.name + " is in view. Cancelling scheduled destruction.");
                    CancelInvoke("DeleteSelf");
                    isScheduledForDestruction = false;
                }
            }
        }
    }

    bool IsInView()
    {
        if (mainCamera == null)
            return true;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        if (objectCollider != null)
            return GeometryUtility.TestPlanesAABB(planes, objectCollider.bounds);
        else
            return true; // Default to true if no collider is found.
    }

    void DeleteSelf()
    {
        if (ragdollScript != null && ragdollScript.IsRagdolled)
        {
            Debug.Log("[DestroyWhenNotInCameraView] Destroying object " + gameObject.name + " as it remains out of view.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("[DestroyWhenNotInCameraView] Object " + gameObject.name + " is no longer ragdolled. Cancelling destruction.");
            isScheduledForDestruction = false;
        }
    }
}
