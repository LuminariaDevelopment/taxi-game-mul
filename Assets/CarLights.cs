using UnityEngine;

public class CarLights : MonoBehaviour
{
    public Light[] headlights;        // Combine small and big headlights
    public Light[] brakeLights;
    public Light[] reverseLights;

    public Material headlightMaterial;
    public KeyCode headlightsKey = KeyCode.L;
    public KeyCode brakeAndReverseKey = KeyCode.S;

    private Rigidbody carRigidbody;
    private bool headlightsOn = false;
    private bool isBraking = false;
    private bool isReversing = false;

    public SpriteRenderer headlightIndicator;

    public GameObject[] objectsToKeepOn;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        TurnOffAllLights();
    }

    void Update()
    {
        HandleHeadlights();
        HandleBrakeAndReverseLights();
        ManageObjectsBasedOnLights();
    }

    void HandleHeadlights()
    {
        if (Input.GetKeyDown(headlightsKey))
        {
            headlightsOn = !headlightsOn;
            SetLights(headlights, headlightsOn);
            SetHeadlightEmission(headlightsOn);

            if (headlightIndicator != null)
                headlightIndicator.enabled = headlightsOn;
        }
    }

    void HandleBrakeAndReverseLights()
    {
        if (carRigidbody.velocity.magnitude < 0.1f)
        {
            SetLights(brakeLights, true);
            SetLights(reverseLights, false);
            return;
        }

        bool isPressingSKey = Input.GetKey(brakeAndReverseKey);
        float backwardVelocity = Vector3.Dot(carRigidbody.velocity, transform.forward);
        isReversing = isPressingSKey && backwardVelocity < -0.5f;

        SetLights(reverseLights, isReversing);
        SetLights(brakeLights, isPressingSKey && !isReversing);
    }

    void TurnOffAllLights()
    {
        SetLights(headlights, false);
        SetLights(brakeLights, false);
        SetLights(reverseLights, false);
        SetHeadlightEmission(false);

        if (headlightIndicator != null)
            headlightIndicator.enabled = false;
    }

    void SetLights(Light[] lights, bool state)
    {
        foreach (Light light in lights)
            light.enabled = state;
    }

    void SetHeadlightEmission(bool state)
    {
        if (headlightMaterial != null)
        {
            if (state)
            {
                headlightMaterial.EnableKeyword("_EMISSION");
                headlightMaterial.SetColor("_EmissionColor", Color.white * 2f);
            }
            else
            {
                headlightMaterial.DisableKeyword("_EMISSION");
                headlightMaterial.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    void ManageObjectsBasedOnLights()
    {
        foreach (GameObject obj in objectsToKeepOn)
            obj.SetActive(headlightsOn);
    }
}
