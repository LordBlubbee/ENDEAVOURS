using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ModuleLight : MonoBehaviour
{
    public Module module;
    public Light2D MainLight;
    void Update()
    {
        MainLight.enabled = !module.IsDisabled();
    }
}
