// SCC_InputProcessor.cs
using UnityEngine;

/// <summary>
/// Input processor of the vehicle.
/// </summary>
[AddComponentMenu("BoneCracker Games/Simple Car Controller/SCC Input Processor")]
public class SCC_InputProcessor : MonoBehaviour
{
    public SCC_Inputs inputs = new SCC_Inputs();

    public bool receiveInputsFromInputManager = true;
    public bool smoothInputs = true;
    public float smoothingFactor = 5f;

    private void Update()
    {
        if (inputs == null) inputs = new SCC_Inputs();

        if (receiveInputsFromInputManager && SCC_InputManager.Instance != null)
        {
            var src = SCC_InputManager.Instance.inputs;
            if (smoothInputs)
            {
                inputs.throttleInput  = Mathf.MoveTowards(inputs.throttleInput,  src.throttleInput,  Time.deltaTime * smoothingFactor);
                inputs.steerInput     = Mathf.MoveTowards(inputs.steerInput,     src.steerInput,     Time.deltaTime * smoothingFactor);
                inputs.brakeInput     = Mathf.MoveTowards(inputs.brakeInput,     src.brakeInput,     Time.deltaTime * smoothingFactor);
                inputs.handbrakeInput = Mathf.MoveTowards(inputs.handbrakeInput, src.handbrakeInput, Time.deltaTime * smoothingFactor);
            }
            else
            {
                inputs = src;
            }
        }
    }

    /// <summary>
    /// Overrides inputs (e.g. via RPC). Disable receiveInputsFromInputManager first.
    /// </summary>
    public void OverrideInputs(SCC_Inputs newInputs)
    {
        if (!smoothInputs)
        {
            inputs = newInputs;
        }
        else
        {
            inputs.throttleInput  = Mathf.MoveTowards(inputs.throttleInput,  newInputs.throttleInput,  Time.deltaTime * smoothingFactor);
            inputs.steerInput     = Mathf.MoveTowards(inputs.steerInput,     newInputs.steerInput,     Time.deltaTime * smoothingFactor);
            inputs.brakeInput     = Mathf.MoveTowards(inputs.brakeInput,     newInputs.brakeInput,     Time.deltaTime * smoothingFactor);
            inputs.handbrakeInput = Mathf.MoveTowards(inputs.handbrakeInput, newInputs.handbrakeInput, Time.deltaTime * smoothingFactor);
        }
    }
}
