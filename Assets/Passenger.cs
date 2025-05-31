using UnityEngine;
using UnityEngine.AI;

public class Passenger : MonoBehaviour
{
    [Header("Destination Options")]
    public Transform[] possibleDestinations;

    [HideInInspector] public Transform assignedDestination;
    private bool isPickedUp = false;
    private bool isRunningToTaxi = false;

    // Reference to the NavMeshAgent component.
    private NavMeshAgent agent;

    // Reference to the Animator component for handling animations.
    private Animator animator;

    // Distance threshold to decide when the passenger has reached the taxi.
    public float runPickupThreshold = 3f;

    void Awake()
    {
        // Cache references for NavMeshAgent and Animator.
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Passenger: No NavMeshAgent found on the passenger.");
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Passenger: No Animator component found. Please add one to enable animations.");
        }
    }

    void Update()
    {
        // If the passenger is running towards the taxi, update the destination.
        if (isRunningToTaxi && agent != null && TaxiReference.Instance != null)
        {
            Transform taxiTransform = TaxiReference.Instance.transform;
            agent.SetDestination(taxiTransform.position);

            // Check if the passenger is close enough to the taxi.
            float distanceToTaxi = Vector3.Distance(transform.position, taxiTransform.position);
            if (distanceToTaxi < runPickupThreshold)
            {
                Pickup();
            }
        }
    }

    /// <summary>
    /// Triggers the passenger to run towards the taxi.
    /// </summary>
    public void RunToTaxi()
    {
        if (agent == null)
            return;

        TaxiReference taxi = TaxiReference.Instance; // Ensure there is a global TaxiReference.
        if (taxi != null)
        {
            isRunningToTaxi = true;
            agent.SetDestination(taxi.transform.position);

            // Trigger the running animation if available.
            if (animator != null)
            {
                animator.SetBool("isRunning", true);
            }
        }
    }

    /// <summary>
    /// Stops the passenger's chase behavior and transitions back to idle.
    /// </summary>
    public void StopChasing()
    {
        isRunningToTaxi = false;
        
        // Stop the NavMeshAgent from moving.
        if (agent != null)
        {
            agent.ResetPath();
        }

        // Cancel the running animation and optionally trigger the idle animation.
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetTrigger("Idle"); // Ensure your Animator Controller has an "Idle" trigger.
        }

        Debug.Log("Passenger has stopped chasing and is now idle.");
    }

    /// <summary>
    /// Assigns a random destination from the list of available possibleDestinations.
    /// </summary>
    public void AssignRandomDestination()
    {
        if (possibleDestinations.Length == 0)
        {
            Debug.LogWarning("Passenger: No destinations available.");
            return;
        }

        int index = Random.Range(0, possibleDestinations.Length);
        assignedDestination = possibleDestinations[index];
    }

    /// <summary>
    /// Called when the passenger successfully reaches the taxi and is picked up.
    /// </summary>
    public void Pickup()
    {
        isPickedUp = true;
        isRunningToTaxi = false;  // Stop chasing behavior.

        // Stop the agent from moving.
        if (agent != null)
        {
            agent.ResetPath();
        }

        // Update animations: stop the running animation and trigger the pickup animation.
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetTrigger("Pickup"); // Make sure your Animator Controller defines a "Pickup" trigger.
        }

        // Optionally hide the passenger after pickup.
        gameObject.SetActive(false);
        Debug.Log("Passenger has been picked up!");
    }

    /// <summary>
    /// Returns true if the passenger has been picked up.
    /// </summary>
    public bool IsPickedUp() => isPickedUp;
}
