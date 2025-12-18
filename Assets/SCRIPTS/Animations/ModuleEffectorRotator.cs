using UnityEngine;
using UnityEngine.UI;

public class ModuleEffectorRotator : MonoBehaviour
{
    public ModuleEffector module;
    public float RotationSpeedIdle = 45f;
    public float RotationSpeed = 360f;
    void Update()
    {
        if (!module.IsDisabled())
        {
            if (module.IsEffectActive()) transform.Rotate(Vector3.forward, RotationSpeed * CO.co.GetWorldSpeedDelta());
            else transform.Rotate(Vector3.forward, RotationSpeedIdle * CO.co.GetWorldSpeedDelta());
        }
    }
}
