using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightExtensions : MonoBehaviour
{
    private Light lightComp;
    private float intensity;

    private void Awake()
    {
        lightComp = GetComponent<Light>();
        intensity = lightComp.intensity;
    }

    public void SetIntensityPercent(float percent)
    {
        lightComp.intensity = intensity * percent;
    }
}
