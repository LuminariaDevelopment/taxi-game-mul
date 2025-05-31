using UnityEngine;

public class BrakeSound : MonoBehaviour
{
    public AudioSource brakeSqueakAudioSource; // Reference to the audio source for the brake squeak
    public AudioClip brakeSqueakClip; // Brake squeak sound clip
    public Rigidbody carRigidbody; // Reference to the car's rigidbody

    [Header("Brake Squeak Settings")]
    public float minBrakeSqueakPitch = 0.7f; // Minimum pitch of the brake squeak
    public float maxBrakeSqueakPitch = 1.5f; // Maximum pitch of the brake squeak (higher = more intense)
    public float brakeSqueakVolume = 0.5f; // Volume of the brake squeak sound
    public float brakeSqueakDuration = 0.5f; // Duration of the brake squeak sound
    public float reverseSpeedThreshold = 2f; // Threshold for reverse speed to consider it as reversing

    private float brakeStartTime; // Time when the brake key was pressed
    private bool isBraking = false; // To track if the brake is being applied
    private bool isReversing = false; // To track if the car is reversing

    void Start()
    {
        // Initialize the AudioSource with the brake squeak clip
        brakeSqueakAudioSource.clip = brakeSqueakClip;
        brakeSqueakAudioSource.loop = true; // Set the audio to loop while braking
    }

    void Update()
    {
        // Check if the car is reversing and the 'S' key is pressed
        isReversing = Input.GetKey("s") && Vector3.Dot(carRigidbody.velocity, transform.forward) < -reverseSpeedThreshold;


        // If not reversing and the car is moving forward or stopped
        if (!isReversing && carRigidbody.velocity.magnitude > 0 && Input.GetKey("s"))
        {
            if (!isBraking)
            {
                // Start playing the brake squeak sound
                brakeSqueakAudioSource.Play();
                brakeStartTime = Time.time; // Record when the brake key was first pressed
                isBraking = true;
            }

            // Adjust pitch based on how long the 'S' key is held down
            float brakeDuration = Time.time - brakeStartTime;
            float pitch = Mathf.Lerp(minBrakeSqueakPitch, maxBrakeSqueakPitch, brakeDuration);
            brakeSqueakAudioSource.pitch = pitch;

            // Adjust the volume depending on the car speed (higher speed = louder squeak)
            float volume = Mathf.Clamp01(carRigidbody.velocity.magnitude / 10f); // You can tweak the divisor
            brakeSqueakAudioSource.volume = volume * brakeSqueakVolume;
        }
        else
        {
            if (isBraking)
            {
                // Stop playing the brake squeak sound when the 'S' key is released or speed becomes 0
                brakeSqueakAudioSource.Stop();
                isBraking = false;
            }
        }
    }
}
