using UnityEngine;

[AddComponentMenu("BoneCracker Games/Simple Car Controller/SCC Audio")]
public class SCC_Audio : MonoBehaviour
{
    private SCC_Drivetrain drivetrain;
    public SCC_Drivetrain Drivetrain
    {
        get
        {
            if (drivetrain == null)
                drivetrain = GetComponent<SCC_Drivetrain>();
            return drivetrain;
        }
    }

    private SCC_InputProcessor inputProcessor;
    public SCC_InputProcessor InputProcessor
    {
        get
        {
            if (inputProcessor == null)
                inputProcessor = GetComponent<SCC_InputProcessor>();
            return inputProcessor;
        }
    }

    public AudioSource engineOnSource;
    public AudioSource engineOffSource;

    public float minimumVolume = 0.1f;
    public float maximumVolume = 1f;
    public float minimumPitch = 0.75f;
    public float maximumPitch = 1.25f;

    private void Start()
    {
        engineOnSource.loop = true;
        engineOffSource.loop = true;
        engineOnSource.Play();
        engineOffSource.Play();
    }

    private void Update()
    {
        if (Drivetrain == null || InputProcessor == null)
        {
            enabled = false;
            return;
        }

        float inputValue = Drivetrain.direction == 1
            ? InputProcessor.inputs.throttleInput
            : InputProcessor.inputs.brakeInput;

        engineOnSource.volume = Mathf.Max(minimumVolume, Mathf.Lerp(minimumVolume, maximumVolume, inputValue));
        engineOffSource.volume = Mathf.Max(minimumVolume, Mathf.Lerp(maximumVolume, 0f, inputValue));

        float pitch = Mathf.Lerp(minimumPitch, maximumPitch, Drivetrain.currentEngineRPM / Drivetrain.maximumEngineRPM);
        engineOnSource.pitch = pitch;
        engineOffSource.pitch = pitch;
    }
}
