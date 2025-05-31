using UnityEngine;

public class PassengerManager : MonoBehaviour
{
    private Passenger currentPassenger;
    private bool deliveryActivated = false; // New flag to ensure delivery is only activated once
    private float chaseTimer = 0f; // Timer to track how long the passenger is chasing the taxi

    [Header("Marker Settings")]
    public GameObject destinationArrowPrefab;
    public GameObject destinationSpotPrefab;
    public Vector3 arrowForwardAxis = Vector3.forward;

    private GameObject destinationArrow;
    private GameObject destinationSpot;

    [Header("Pickup Settings")]
    public string passengerTag = "Passenger";
    public float pickupRange = 3f;

    // New variable to check taxi speed before picking up a passenger.
    public float pickupSpeedThreshold = 0.1f; // Adjust this value as needed.

    private Vector3 pickupLocation;

    [Header("Delivery Timer")]
    public float speedFactor = 10f;
    public float currentDeliveryTime = 0f;
    private float maxDeliveryTime = 0f;
    private bool isDelivering = false;
    public bool IsDelivering()
    {
        return isDelivering;
    }

    // Cache the taxi's Rigidbody for performance.
    private Rigidbody taxiRigidbody;

    void Start()
    {
        taxiRigidbody = GetComponent<Rigidbody>();
        if (taxiRigidbody == null)
        {
            Debug.LogWarning("PassengerManager: No Rigidbody found on taxi. Passenger pickup speed check will not work properly.");
        }
    }

    void Update()
    {
        
        // If there is no current passenger, try to trigger a passenger run.
        if (currentPassenger == null)
        {
            TryInitiatePassengerRun();
        }
        else
        {
            // If passenger hasn't been picked up, increment the chase timer.
            if (!currentPassenger.IsPickedUp())
            {
                chaseTimer += Time.deltaTime;
                if (chaseTimer >= 5f)
                {
                    // Passenger loses interest after 5 seconds.
                    Debug.Log("Passenger lost interest and stops chasing the taxi.");
                    // Call a method on the passenger to handle stopping (adjust method name as needed).
                    currentPassenger.StopChasing();
                    currentPassenger = null;
                    chaseTimer = 0f;
                    return; // Exit Update to prevent further processing
                }
            }
            else
            {
                // Reset the chase timer if the passenger is picked up
                chaseTimer = 0f;
            }

            // New: Check if the passenger has just been picked up to activate delivery.
            if (!deliveryActivated && currentPassenger.IsPickedUp())
            {
                ActivateDelivery();
            }

            // Continue to update the delivery markers if a delivery is in progress.
            CheckDropoff();
            UpdateArrowDirection();
        }

        if (isDelivering)
        {
            currentDeliveryTime -= Time.deltaTime;
            if (currentDeliveryTime <= 0f)
            {
                currentDeliveryTime = 0f;
                HandleFailedDelivery();
            }
            GameManager.Instance.UpdateDeliveryTimer(currentDeliveryTime, maxDeliveryTime);
        }
        else
        {
            GameManager.Instance.UpdateDeliveryTimer(0f, 1f); // Placeholder to avoid divide-by-zero
        }
    }

    /// <summary>
    /// Initiates the passenger run behavior if the taxi is almost stopped.
    /// </summary>
    void TryInitiatePassengerRun()
    {
        if (taxiRigidbody != null && taxiRigidbody.velocity.magnitude > pickupSpeedThreshold)
        {
            return;
        }

        // Use a larger radius for the run trigger.
        float runTriggerRange = pickupRange * 3f;
        Collider[] hits = Physics.OverlapSphere(transform.position, runTriggerRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(passengerTag))
            {
                Passenger passenger = hit.GetComponent<Passenger>();
                if (passenger != null)
                {
                    if (passenger.IsPickedUp())
                        continue;

                    // Trigger the passenger to run toward the taxi.
                    passenger.RunToTaxi();

                    // Set the current passenger and wait for them to call Pickup().
                    currentPassenger = passenger;
                    // Reset the chase timer each time a new passenger is triggered.
                    chaseTimer = 0f;

                    Debug.Log("Passenger is running towards the taxi!");

                    // Reset delivery activation flag.
                    deliveryActivated = false;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Activates delivery once the passenger has been picked up.
    /// </summary>
    void ActivateDelivery()
    {
        // Let the passenger pick a random destination.
        currentPassenger.AssignRandomDestination();
        // Record the pickup location (if needed before the passenger deactivates).
        pickupLocation = currentPassenger.transform.position;

        // Calculate the distance between pickup location and drop-off destination.
        float distance = Vector3.Distance(pickupLocation, currentPassenger.assignedDestination.position);
        maxDeliveryTime = distance / speedFactor;
        currentDeliveryTime = maxDeliveryTime;
        isDelivering = true;
        deliveryActivated = true;

        // Create markers for the destination.
        CreateArrowMarker(currentPassenger.assignedDestination);
        CreateDestinationSpot(currentPassenger.assignedDestination);

        Debug.Log("Passenger picked up and delivery activated!");
    }

    void CheckDropoff()
    {
        if (currentPassenger == null || currentPassenger.assignedDestination == null)
            return;

        float distanceToDestination = Vector3.Distance(transform.position, currentPassenger.assignedDestination.position);

        // Require player to be close enough AND stopped
        if (distanceToDestination < 5f && taxiRigidbody.velocity.magnitude < pickupSpeedThreshold)
        {
            Debug.Log("Passenger dropped off!");
            float deliveryDistance = Vector3.Distance(pickupLocation, currentPassenger.assignedDestination.position);
            float timeRemaining = Mathf.Max(0f, currentDeliveryTime);
            GameManager.Instance.AddScoreFromDelivery(deliveryDistance, timeRemaining);
            CleanupPassengerDelivery();
        }
    }


    void HandleFailedDelivery()
    {
        Debug.Log("Delivery failed! Time ran out.");
        CleanupPassengerDelivery();
    }

    void CleanupPassengerDelivery()
    {
        if (destinationArrow)
            Destroy(destinationArrow);
        if (destinationSpot)
            Destroy(destinationSpot);

        destinationArrow = null;
        destinationSpot = null;
        currentPassenger = null;
        isDelivering = false;
        currentDeliveryTime = 0f;
    }

    void CreateArrowMarker(Transform destination)
    {
        if (destinationArrowPrefab != null)
        {
            destinationArrow = Instantiate(destinationArrowPrefab, transform.position + Vector3.up * 3, Quaternion.identity);
        }
    }

    void CreateDestinationSpot(Transform destination)
    {
        if (destinationSpotPrefab != null)
        {
            destinationSpot = Instantiate(destinationSpotPrefab, destination.position, Quaternion.identity);
        }
    }

    void UpdateArrowDirection()
    {
        if (destinationArrow && currentPassenger != null)
        {
            destinationArrow.transform.position = transform.position + Vector3.up * 3f;
            Vector3 direction = currentPassenger.assignedDestination.position - transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                destinationArrow.transform.rotation = rotation * Quaternion.FromToRotation(arrowForwardAxis, Vector3.forward);
            }
        }
    }

    // Draw a gizmo to represent the pickup radius.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
