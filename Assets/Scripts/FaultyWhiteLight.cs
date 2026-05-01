using UnityEngine;

public class FaultyWhiteLight : MonoBehaviour
{
    [Header("Light References")]
    [SerializeField] private Light[] targetLights;

    [Header("Flicker")]
    [SerializeField] private float minFlickerIntensity = 0f;
    [SerializeField] private float maxFlickerIntensity = 6f;
    [SerializeField] private float minFlickerDelay = 0.03f;
    [SerializeField] private float maxFlickerDelay = 0.18f;
    [SerializeField] private float blackoutChance = 0.25f;

    [Header("Power Restored")]
    [SerializeField] private float steadyIntensity = 4f;

    private bool isSteady;
    private float nextFlickerTime;

    private void Awake()
    {
        if (targetLights == null || targetLights.Length == 0)
            targetLights = GetComponentsInChildren<Light>(true);

        SetLightColor(Color.white);
    }

    private void OnEnable()
    {
        isSteady = false;
        nextFlickerTime = Time.time;
    }

    private void Update()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.PowerRoomComplete)
        {
            if (!isSteady)
                SetSteadyLight();

            return;
        }

        FlickerLight();
    }

    private void FlickerLight()
    {
        if (Time.time < nextFlickerTime) return;

        bool blackout = Random.value < blackoutChance;
        float intensity = blackout ? 0f : Random.Range(minFlickerIntensity, maxFlickerIntensity);

        SetLightIntensity(intensity);
        nextFlickerTime = Time.time + Random.Range(minFlickerDelay, maxFlickerDelay);
    }

    private void SetSteadyLight()
    {
        isSteady = true;
        SetLightColor(Color.white);
        SetLightIntensity(steadyIntensity);
    }

    private void SetLightColor(Color color)
    {
        foreach (var lightComp in targetLights)
        {
            if (lightComp == null) continue;
            lightComp.color = color;
        }
    }

    private void SetLightIntensity(float intensity)
    {
        foreach (var lightComp in targetLights)
        {
            if (lightComp == null) continue;
            lightComp.enabled = intensity > 0f;
            lightComp.intensity = intensity;
        }
    }
}
