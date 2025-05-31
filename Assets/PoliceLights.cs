using UnityEngine;
using System.Collections.Generic;

public class PoliceLights : MonoBehaviour
{
    [Header("Point Lights")]
    public Light redLight;
    public Light blueLight;

    [Header("Emission Objects")]
    public List<Renderer> redEmissiveObjects;
    public List<Renderer> blueEmissiveObjects;

    [Header("Flashing Settings")]
    public float flashInterval = 0.5f;
    public Color redEmissionColor = Color.red;
    public Color blueEmissionColor = Color.blue;

    private float timer;
    private bool isRedOn = true;

    void Start()
    {
        timer = flashInterval;
        SetState(true);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isRedOn = !isRedOn;
            SetState(isRedOn);
            timer = flashInterval;
        }
    }

    private void SetState(bool redOn)
    {
        if (redLight) redLight.enabled = redOn;
        if (blueLight) blueLight.enabled = !redOn;

        SetEmission(redEmissiveObjects, redOn, redEmissionColor);
        SetEmission(blueEmissiveObjects, !redOn, blueEmissionColor);
    }

    private void SetEmission(List<Renderer> objects, bool enable, Color emissionColor)
    {
        foreach (var renderer in objects)
        {
            if (renderer == null) continue;

            foreach (Material mat in renderer.materials)
            {
                if (enable)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emissionColor);
                }
                else
                {
                    mat.DisableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }
}
