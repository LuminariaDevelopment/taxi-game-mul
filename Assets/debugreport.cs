using UnityEngine;
public class debugreport : MonoBehaviour
{
    void OnDisable()
    {
        Debug.LogWarning($"[{name}] disabled by: {new System.Diagnostics.StackTrace(1, true)}");
    }
}
