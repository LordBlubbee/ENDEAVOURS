using UnityEngine;
using UnityEngine.UI;

public class ParticleEffector : MonoBehaviour
{
    public ModuleEffector module;
    public ParticleSystem part;
    void Update()
    {
        if (module.IsEffectActive())
        {
            if (!part.isPlaying) part.Play();
        } else if (part.isPlaying)
        {
            part.Stop();
        }
    }
}
