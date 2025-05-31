using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("BoneCracker Games/Simple Car Controller/SCC Particles")]
public class SCC_Particles : MonoBehaviour {
    private SCC_InputProcessor inputProcessor;
    public SCC_InputProcessor InputProcessor {
        get {
            if (inputProcessor == null)
                inputProcessor = GetComponent<SCC_InputProcessor>();
            return inputProcessor;
        }
    }

    private SCC_Wheel[] wheels;

    [Header("Wheel Particles")]
    public ParticleSystem wheelParticlePrefab;
    private List<ParticleSystem> createdParticles = new List<ParticleSystem>();
    private ParticleSystem.EmissionModule[] wheelEmissions;

    [Header("Exhaust Particles")]
    public ParticleSystem[] exhaustParticles;
    private ParticleSystem.EmissionModule[] exhaustEmissions;

    [Header("Skid Trails")]
    public float slip = 0.25f;
    public float minTrailDistance = 0.5f;
    public TrailRenderer[] skidTrails;

    private float[] forwardSlips;
    private float[] sidewaysSlips;
    private bool[] isSkiddingNow;
    private float[] accumulatedDistances;
    private Vector3[] previousHitPoints;

    [Header("Skid Audio Settings")]
    public AudioSource skidAudioSource;
    public AudioClip skidScreechClip;
    public float audioSlipThreshold = 0.3f;
    public float audioFadeSpeed = 2f;
    [Range(0f, 1f)]
    public float maxSkidVolume = 1f;  // ← new field

    private void Awake() {
        // Wheels setup
        wheels = GetComponentsInChildren<SCC_Wheel>();
        forwardSlips = new float[wheels.Length];
        sidewaysSlips = new float[wheels.Length];
        isSkiddingNow = new bool[wheels.Length];
        accumulatedDistances = new float[wheels.Length];
        previousHitPoints = new Vector3[wheels.Length];

        // Instantiate wheel skid particle systems
        if (wheelParticlePrefab) {
            for (int i = 0; i < wheels.Length; i++) {
                var p = Instantiate(
                    wheelParticlePrefab,
                    wheels[i].transform.position,
                    wheels[i].transform.rotation,
                    wheels[i].transform
                );
                createdParticles.Add(p);
            }
            wheelEmissions = new ParticleSystem.EmissionModule[createdParticles.Count];
            for (int i = 0; i < createdParticles.Count; i++)
                wheelEmissions[i] = createdParticles[i].emission;
        }

        // Cache exhaust emission modules
        if (exhaustParticles != null && exhaustParticles.Length > 0) {
            exhaustEmissions = new ParticleSystem.EmissionModule[exhaustParticles.Length];
            for (int i = 0; i < exhaustParticles.Length; i++)
                exhaustEmissions[i] = exhaustParticles[i].emission;
        }

        // Setup skid audio source
        if (skidAudioSource != null && skidScreechClip != null) {
            skidAudioSource.clip = skidScreechClip;
            skidAudioSource.loop = true;
            skidAudioSource.playOnAwake = false;
            skidAudioSource.spatialBlend = 1f;
            skidAudioSource.volume = 0f;
        }
    }

    private void Update() {
        UpdateWheelSlips();
        WheelParticles();
        ExhaustParticles();
        UpdateSkidTrails();
    }

    private void UpdateWheelSlips() {
        for (int i = 0; i < wheels.Length; i++) {
            WheelHit hit;
            if (wheels[i].WheelCollider.GetGroundHit(out hit)) {
                forwardSlips[i] = Mathf.Abs(hit.forwardSlip);
                sidewaysSlips[i] = Mathf.Abs(hit.sidewaysSlip);
            } else {
                forwardSlips[i] = 0f;
                sidewaysSlips[i] = 0f;
            }
        }
    }

    private void WheelParticles() {
        if (wheelParticlePrefab == null || createdParticles.Count == 0) return;

        for (int i = 0; i < wheels.Length; i++) {
            WheelHit hit;
            wheels[i].WheelCollider.GetGroundHit(out hit);
            bool emit = Mathf.Abs(hit.forwardSlip) >= slip || Mathf.Abs(hit.sidewaysSlip) >= slip;
            wheelEmissions[i].enabled = emit;
        }
    }

    private void ExhaustParticles() {
        if (exhaustParticles == null || exhaustParticles.Length == 0) return;

        for (int i = 0; i < exhaustEmissions.Length; i++) {
            // adjust rate over time; use .rateOverTime if using newer Unity
#if UNITY_2017_1_OR_NEWER
            exhaustEmissions[i].rateOverTime = Mathf.Lerp(1f, 20f, InputProcessor.inputs.throttleInput);
#else
            exhaustEmissions[i].rate = Mathf.Lerp(1f, 20f, InputProcessor.inputs.throttleInput);
#endif
        }
    }

    private void UpdateSkidTrails() {
        if (skidTrails == null || skidTrails.Length < wheels.Length) return;

        // Manage individual skid trails
        for (int i = 0; i < wheels.Length; i++) {
            WheelHit hit;
            bool hasHit = wheels[i].WheelCollider.GetGroundHit(out hit);
            bool skidding = hasHit && (Mathf.Abs(hit.forwardSlip) >= slip || Mathf.Abs(hit.sidewaysSlip) >= slip);

            if (skidding) {
                if (!isSkiddingNow[i]) {
                    isSkiddingNow[i] = true;
                    accumulatedDistances[i] = 0f;
                    previousHitPoints[i] = hit.point;
                } else {
                    float d = Vector3.Distance(hit.point, previousHitPoints[i]);
                    accumulatedDistances[i] += d;
                    previousHitPoints[i] = hit.point;
                }
                skidTrails[i].emitting = accumulatedDistances[i] >= minTrailDistance;
            } else {
                skidTrails[i].emitting = false;
                isSkiddingNow[i] = false;
                accumulatedDistances[i] = 0f;
            }
        }

        // Determine if any wheel is slipping enough to play audio
        bool playAudio = false;
        for (int i = 0; i < wheels.Length; i++) {
            if (forwardSlips[i] >= audioSlipThreshold || sidewaysSlips[i] >= audioSlipThreshold) {
                playAudio = true;
                break;
            }
        }

        // Fade audio in/out up to maxSkidVolume
        if (skidAudioSource != null) {
            if (playAudio) {
                skidAudioSource.volume = Mathf.MoveTowards(
                    skidAudioSource.volume,
                    maxSkidVolume,
                    Time.deltaTime * audioFadeSpeed
                );
                if (!skidAudioSource.isPlaying) skidAudioSource.Play();
            } else {
                skidAudioSource.volume = Mathf.MoveTowards(
                    skidAudioSource.volume,
                    0f,
                    Time.deltaTime * audioFadeSpeed
                );
                if (skidAudioSource.volume <= 0.01f && skidAudioSource.isPlaying)
                    skidAudioSource.Stop();
            }
        }
    }
}
