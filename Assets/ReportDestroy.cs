using UnityEngine;

/// <summary>
/// Static helper that logs what object is being destroyed and which script initiated the destruction.
/// </summary>
public static class ReportDestroy
{
    /// <summary>
    /// Destroys the target object and logs the initiator.
    /// </summary>
    /// <param name="target">The object to destroy.</param>
    /// <param name="initiator">The MonoBehaviour that called this method.</param>
    public static void DestroyObject(Object target, MonoBehaviour initiator)
    {
        if (target != null && initiator != null)
        {
            Debug.LogFormat("[{0}] {1}.{2} is destroying {3}",
                System.DateTime.Now.ToString("HH:mm:ss"),
                initiator.gameObject.name,
                initiator.GetType().Name,
                target.name
            );
        }
        else
        {
            Debug.LogWarning("DestroyReporter: target or initiator was null");
        }
        Object.Destroy(target);
    }
}