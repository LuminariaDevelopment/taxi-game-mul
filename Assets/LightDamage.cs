using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A serializable container for grouping multiple Light components
/// into a single “headlight unit” (e.g. low-beam + high-beam + fog lamp).
/// </summary>
[System.Serializable]
public class HeadlightGroup
{
    [Tooltip("All the Light components (GameObjects) that belong to this one headlight unit.")]
    public List<Light> lights = new List<Light>();
}

/// <summary>
/// On each high-force collision, destroys one HeadlightGroup’s GameObjects.
/// First hit destroys group[0], second hit destroys group[1], etc.
/// </summary>
public class LightDamage : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Minimum collision impulse magnitude to count as a damaging hit.")]
    public float damageThreshold = 50f;

    [Header("Headlight Groups")]
    [Tooltip("Each entry represents a headlight ‘unit’ (bundle of Lights).")]
    public List<HeadlightGroup> headlightGroups = new List<HeadlightGroup>();

    // How many big hits taken so far
    private int hitCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        float impulseMag = collision.impulse.magnitude;
        if (impulseMag >= damageThreshold)
        {
            DisableAndDestroyGroup(hitCount);
            hitCount++;
        }
    }

    /// <summary>
    /// Destroys all Light GameObjects in the specified headlight group index.
    /// </summary>
    /// <param name="groupIndex">Zero-based index into headlightGroups.</param>
    private void DisableAndDestroyGroup(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= headlightGroups.Count)
        {
            Debug.Log("No more headlight groups to destroy.");
            return;
        }

        var group = headlightGroups[groupIndex];
        foreach (var lt in group.lights)
        {
            if (lt != null && lt.gameObject != null)
            {
                Destroy(lt.gameObject);
            }
        }

        Debug.Log($"Headlight group #{groupIndex + 1} destroyed.");
    }
}
