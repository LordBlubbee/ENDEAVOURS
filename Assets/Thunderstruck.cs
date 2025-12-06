using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Thunderstruck : MonoBehaviour
{
    public Light2D Light;
    public float LoseLightSpeed = 20f;
    public float MinIntensity = 20f;
    public float MaxIntensity = 30f;
    public float MinDelayThunder = 0f;
    public float MaxDelayThunder = 1.5f;
    public AudioClip[] ThunderSFX;
    private float ThunderDelay = 0f;

    private void Start()
    {
        ThunderDelay = Random.Range(MinDelayThunder, MaxDelayThunder);
        Light.intensity = Random.Range(MinIntensity, MaxIntensity);
    }
    void Update()
    {
        ThunderDelay -= CO.co.GetWorldSpeedDelta();
        if (Light.intensity < 5f)
        {
            Light.intensity -= CO.co.GetWorldSpeedDelta() * LoseLightSpeed * 0.5f;
        } else
        {
            Light.intensity -= CO.co.GetWorldSpeedDelta() * LoseLightSpeed;
        }
        if (ThunderDelay < 0f && ThunderDelay > -50)
        {
            ThunderDelay = -99;
            AUDCO.aud.PlaySFX(ThunderSFX, 0.4f);
        }
        if (Light.intensity <= 0f && ThunderDelay < -50)
        {
            Destroy(gameObject);
        }
    }
}
