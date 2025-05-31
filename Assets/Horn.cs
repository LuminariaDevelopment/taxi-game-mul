using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Horn : MonoBehaviour
{
    [Header("Horn Audio Clip")]
    public AudioClip hornClip;    // Single horn sound

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;
    private bool isHornActive = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartHorn();
        }
        else if (Input.GetKeyUp(KeyCode.H))
        {
            StopHorn();
        }
    }

    private void StartHorn()
    {
        if (isHornActive || hornClip == null) return;

        audioSource.clip = hornClip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
        isHornActive = true;
    }

    private void StopHorn()
    {
        if (!isHornActive) return;

        audioSource.Stop();
        audioSource.loop = false;
        isHornActive = false;
    }
}
