using UnityEngine;

public class Blinkers : MonoBehaviour
{
    public Light frontLeftBlinker;
    public Light frontRightBlinker;
    public Light rearLeftBlinker;
    public Light rearRightBlinker;



    public float blinkInterval = 0.5f; // Adjusts light blinking speed
    private float blinkTimer;
    private bool isBlinkerActive;

    private bool isLeftBlinkerOn;
    private bool isRightBlinkerOn;
    private bool hazardsActive;



    void Update()
    {
        HandleInput();
        UpdateBlinkers();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            hazardsActive = false;
            isLeftBlinkerOn = !isLeftBlinkerOn;
            isRightBlinkerOn = false;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            hazardsActive = false;
            isRightBlinkerOn = !isRightBlinkerOn;
            isLeftBlinkerOn = false;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            hazardsActive = !hazardsActive;
            isLeftBlinkerOn = false;
            isRightBlinkerOn = false;
        }
    }

    private void UpdateBlinkers()
    {
        blinkTimer += Time.deltaTime;

        if (blinkTimer >= blinkInterval)
        {
            blinkTimer = 0;
            isBlinkerActive = !isBlinkerActive;
        }

        if (hazardsActive)
        {
            SetBlinkerState(frontLeftBlinker, isBlinkerActive);
            SetBlinkerState(rearLeftBlinker, isBlinkerActive);
            SetBlinkerState(frontRightBlinker, isBlinkerActive);
            SetBlinkerState(rearRightBlinker, isBlinkerActive);
        }
        else
        {
            SetBlinkerState(frontLeftBlinker, isLeftBlinkerOn && isBlinkerActive);
            SetBlinkerState(rearLeftBlinker, isLeftBlinkerOn && isBlinkerActive);

            SetBlinkerState(frontRightBlinker, isRightBlinkerOn && isBlinkerActive);
            SetBlinkerState(rearRightBlinker, isRightBlinkerOn && isBlinkerActive);

        }

        if (!hazardsActive && !isLeftBlinkerOn)
        {
            SetBlinkerState(frontLeftBlinker, false);
            SetBlinkerState(rearLeftBlinker, false);
        }

        if (!hazardsActive && !isRightBlinkerOn)
        {
            SetBlinkerState(frontRightBlinker, false);
            SetBlinkerState(rearRightBlinker, false);
        }
    }



    private void SetBlinkerState(Light light, bool state)
    {
        light.enabled = state;
    }


}
