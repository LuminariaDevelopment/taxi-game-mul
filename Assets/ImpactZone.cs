// ImpactZone.cs
using UnityEngine;

public class ImpactZone : MonoBehaviour
{
    [Tooltip("The WindowProfile that should shatter when this zone is hit")]
    public WindowProfile targetWindow;

    [Tooltip("If true, this ImpactZone GameObject will be deactivated when its window breaks.")]
    public bool deactivateOnBreak = true;
}
