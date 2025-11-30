using Unity.VisualScripting;
using UnityEngine;

public class ParticleBuff : MonoBehaviour
{
    public CO_SPAWNER.BuffParticles ParticleType;

    public float ExpectedDuration = 0f;
    public Sprite BuffIcon;
    [TextArea(3, 8)]
    public string BuffDesc;
    public Color BuffColor;
}
