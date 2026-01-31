using UnityEngine;
using UnityEngine.UI;

public class ModuleRotator : MonoBehaviour
{
    public Module module;
    public float RotationSpeed = 180f;
    public bool RotateBasedOnFullHealth = false;
    void Update()
    {
        if (RotateBasedOnFullHealth)
        {
            float RotSpeedFactor = module.GetHealthRelative();
            if (RotSpeedFactor >= 1f) RotSpeedFactor = 2f;
            if (RotSpeedFactor > 0f) transform.Rotate(Vector3.forward, RotationSpeed * RotSpeedFactor * CO.co.GetWorldSpeedDelta());
        } else
        {
            if (!module.IsDisabled()) transform.Rotate(Vector3.forward, RotationSpeed * CO.co.GetWorldSpeedDelta());
        }
    }
}
