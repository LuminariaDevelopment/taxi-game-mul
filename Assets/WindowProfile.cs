// WindowProfile.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WindowProfile : MonoBehaviour
{
    [Tooltip("How hard you have to hit this window (m/s) to break it")]
    public float breakSpeedThreshold = 5f;

    [Tooltip("The disabled parent that contains all the broken‐shard pieces")]
    public GameObject brokenWindowParent;

    [HideInInspector] public bool isBroken;
}
