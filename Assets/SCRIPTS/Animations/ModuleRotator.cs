using UnityEngine;
using UnityEngine.UI;

public class ModuleRotator : MonoBehaviour
{
    public Module module;
    public float RotationSpeed = 180f;
    void Update()
    {
        if (!module.IsDisabled()) transform.Rotate(Vector3.forward, RotationSpeed * CO.co.GetWorldSpeedDelta());
    }
}
