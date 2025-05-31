using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum LightState { Red, Yellow, Green }

/// <summary>
/// Four‑way intersection controller.
/// • Drag each signal‑head parent into the North/South or East/West list.
/// • Children must be named "Green Light", "Yellow Light", and "Red Light".
/// • If a pole's Rigidbody becomes non‑kinematic, **only that pole** blacks out; the rest of the junction continues cycling.
/// </summary>
public class IntersectionController : MonoBehaviour
{
    #region Inspector
    [Header("North / South (same phase)")]
    public List<GameObject> northSouthRoots;

    [Header("East / West (same phase)")]
    public List<GameObject> eastWestRoots;

    [Header("Timings (seconds)")]
    public float greenDuration  = 10f;
    public float yellowDuration = 3f;
    public float allRedBuffer   = 1f;
    #endregion

    // ──────────────────────────────────────────────────────────────────────────────
    private class LightGroup
    {
        private readonly GameObject red, yellow, green;
        private readonly Rigidbody rb;

        public LightGroup(Transform root)
        {
            red    = FindChild(root, "Red Light");
            yellow = FindChild(root, "Yellow Light");
            green  = FindChild(root, "Green Light");
            rb     = root.GetComponent<Rigidbody>();
        }

        public void ApplyState(LightState desired)
        {
            if (IsBlackout)
            {
                ToggleAll(false);
                return;
            }
            ToggleAll(false);
            switch (desired)
            {
                case LightState.Red:    red  ?.SetActive(true); break;
                case LightState.Yellow: yellow?.SetActive(true); break;
                case LightState.Green:  green ?.SetActive(true); break;
            }
        }

        public bool IsBlackout => rb != null && !rb.isKinematic;

        private void ToggleAll(bool on)
        {
            red  ?.SetActive(on);
            yellow?.SetActive(on);
            green ?.SetActive(on);
        }

        private static GameObject FindChild(Transform root, string name)
        {
            var t = root.Find(name);
            if (t == null) Debug.LogError($"[{root.name}] Child '{name}' not found");
            return t?.gameObject;
        }
    }

    private List<LightGroup> nsLights;
    private List<LightGroup> ewLights;

    private void Awake()
    {
        nsLights = northSouthRoots.Select(go => new LightGroup(go.transform)).ToList();
        ewLights = eastWestRoots .Select(go => new LightGroup(go.transform)).ToList();
    }

    private void Start() => StartCoroutine(Cycle());

    private IEnumerator Cycle()
    {
        while (true)
        {
            // North/South phase
            SetGroupStates(nsLights, LightState.Green);
            SetGroupStates(ewLights, LightState.Red);
            yield return HoldPhase(greenDuration, nsLights, LightState.Green, ewLights, LightState.Red);

            SetGroupStates(nsLights, LightState.Yellow);
            yield return HoldPhase(yellowDuration, nsLights, LightState.Yellow, ewLights, LightState.Red);

            SetGroupStates(nsLights, LightState.Red);
            yield return HoldPhase(allRedBuffer, nsLights, LightState.Red, ewLights, LightState.Red);

            // East/West phase
            SetGroupStates(ewLights, LightState.Green);
            SetGroupStates(nsLights, LightState.Red);
            yield return HoldPhase(greenDuration, ewLights, LightState.Green, nsLights, LightState.Red);

            SetGroupStates(ewLights, LightState.Yellow);
            yield return HoldPhase(yellowDuration, ewLights, LightState.Yellow, nsLights, LightState.Red);

            SetGroupStates(ewLights, LightState.Red);
            yield return HoldPhase(allRedBuffer, ewLights, LightState.Red, nsLights, LightState.Red);
        }
    }

    /// <summary>
    /// Keeps the desired states applied for <param name="seconds"/> while continuously refreshing
    /// each light so newly blacked‑out poles turn off immediately.
    /// </summary>
    private IEnumerator HoldPhase(float seconds,
                                  List<LightGroup> grpA, LightState stateA,
                                  List<LightGroup> grpB, LightState stateB)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            foreach (var lg in grpA) lg.ApplyState(stateA);
            foreach (var lg in grpB) lg.ApplyState(stateB);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private static void SetGroupStates(IEnumerable<LightGroup> group, LightState state)
    {
        foreach (var lg in group) lg.ApplyState(state);
    }
}
