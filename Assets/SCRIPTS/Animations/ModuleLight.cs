using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ModuleLight : MonoBehaviour
{
    public Module module;
    public Light2D MainLight;
    void Update()
    {
        float WantLight = module.GetHealthRelative() > 0.5f ? 8f : 0f;
        if (WantLight > MainLight.intensity)
        {
            MainLight.intensity = Mathf.Min(MainLight.intensity + Time.deltaTime * 4f, WantLight);
        } else if (WantLight < MainLight.intensity)
        {
            MainLight.intensity = Mathf.Max(MainLight.intensity - Time.deltaTime * 4f, WantLight);
        }
    }
}
