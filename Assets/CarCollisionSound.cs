using UnityEngine;
using System.Collections;

[System.Serializable]
public class LayerSound
{
    [Tooltip("Select one or more layers via the dropdown")] public LayerMask layers;
    [Tooltip("Audio clip to play when colliding with any of these layers")] public AudioClip clip;
    [Tooltip("Minimum duration (in seconds) to play this clip")] public float minPlayDuration = 1f;
}

public class CarCollisionSound : MonoBehaviour
{
    [Header("Layer-to-Sound Mappings")]
    [Tooltip("Define which audio clip and duration to play for each set of layers")] public LayerSound[] layerSounds;

    [Header("Default Settings")]
    [Tooltip("Audio clip to play when no layer mapping is found")] public AudioClip defaultClip;
    [Tooltip("Minimum duration (in seconds) to play default clip")] public float defaultMinPlayDuration = 1f;
    [Tooltip("Volume for collision sounds (0.0 to 1.0)")] [Range(0f, 1f)] public float collisionVolume = 1f;

    [Header("Audio Source")]
    [Tooltip("Assign the AudioSource component used to play collision sounds")] public AudioSource audioSource;

    private Coroutine playCoroutine;

    void Awake()
    {
        if (audioSource == null)
        {
            Debug.LogError("CarCollisionSound: No AudioSource assigned in inspector.");
            enabled = false;
            return;
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        TryPlaySound(collision.gameObject.layer);
    }

    private void TryPlaySound(int otherLayer)
    {
        // If a sound is already playing its minimum duration, don't restart
        if (playCoroutine != null)
            return;

        AudioClip clipToPlay = defaultClip;
        float playDuration = defaultMinPlayDuration;

        // Find matching LayerSound and its duration
        foreach (LayerSound ls in layerSounds)
        {
            if (ls.clip == null) continue;
            if ((ls.layers.value & (1 << otherLayer)) != 0)
            {
                clipToPlay = ls.clip;
                playDuration = ls.minPlayDuration;
                break;
            }
        }

        if (clipToPlay != null)
        {
            playCoroutine = StartCoroutine(PlayOneShotRoutine(clipToPlay, playDuration));
        }
    }

    private IEnumerator PlayOneShotRoutine(AudioClip clip, float duration)
    {
        audioSource.PlayOneShot(clip, collisionVolume);
        yield return new WaitForSeconds(duration);
        playCoroutine = null;
    }
}
