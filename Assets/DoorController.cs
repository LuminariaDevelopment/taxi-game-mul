using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [System.Serializable]
    public class Door
    {
        [Tooltip("The door Transform (pivoted correctly at its hinge)")]
        public Transform transform;
        [Tooltip("Local Euler angles when this door is fully open (e.g. (0, 90, 0))")]
        public Vector3 openEuler;

        [HideInInspector] public Quaternion closedRotation;
        [HideInInspector] public Quaternion openRotation;
    }

    [Header("Doors to toggle")]
    public Door[] doors;

    [Header("Taxi & Speed Settings")]
    [Tooltip("The Rigidbody on your taxi/car")]
    public Rigidbody taxiRigidbody;
    [Tooltip("Speed (in units/sec) below which doors will open")]
    public float openSpeedThreshold = 0.5f;
    [Tooltip("Speed above which doors will close")]
    public float closeSpeedThreshold = 1.0f;

    [Header("Animation Settings")]
    [Tooltip("Time in seconds for open/close swing")]
    public float swingDuration = 0.5f;

    private bool isOpen = false;
    private Coroutine swingCoroutine;
    private float doorOpenedTime = -Mathf.Infinity; // Time when doors were last opened

    void Awake()
    {
        // Cache each door's closed & open rotations
        foreach (var d in doors)
        {
            d.closedRotation = d.transform.localRotation;
            d.openRotation = d.closedRotation * Quaternion.Euler(d.openEuler);
        }

        if (taxiRigidbody == null)
            Debug.LogError("[TaxiDoorAutoToggle] No Rigidbody assigned for taxi detection!");
    }

    void Update()
    {
        if (taxiRigidbody == null) return;

        float speed = taxiRigidbody.velocity.magnitude;

        // Open doors if taxi is stopped and doors are closed
        if (!isOpen && speed <= openSpeedThreshold)
        {
            TriggerSwing(open: true);
        }
        // Close doors if taxi is moving and doors have been open for at least 2 seconds
        else if (isOpen && speed >= closeSpeedThreshold && Time.time - doorOpenedTime >= 2f)
        {
            TriggerSwing(open: false);
        }
    }

    private void TriggerSwing(bool open)
    {
        if (swingCoroutine != null)
            StopCoroutine(swingCoroutine);

        swingCoroutine = StartCoroutine(SwingAllDoors(currentlyOpen: !open));
        isOpen = open;

        if (open)
            doorOpenedTime = Time.time; // Record the time doors were opened
    }

    private IEnumerator SwingAllDoors(bool currentlyOpen)
    {
        float elapsed = 0f;

        var starts = new Quaternion[doors.Length];
        var ends = new Quaternion[doors.Length];
        for (int i = 0; i < doors.Length; i++)
        {
            starts[i] = doors[i].transform.localRotation;
            ends[i] = currentlyOpen ? doors[i].closedRotation : doors[i].openRotation;
        }

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / swingDuration);
            for (int i = 0; i < doors.Length; i++)
                doors[i].transform.localRotation = Quaternion.Slerp(starts[i], ends[i], t);
            yield return null;
        }

        for (int i = 0; i < doors.Length; i++)
            doors[i].transform.localRotation = ends[i];

        swingCoroutine = null;
    }
}
