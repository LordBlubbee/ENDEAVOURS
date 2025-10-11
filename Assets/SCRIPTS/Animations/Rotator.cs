using UnityEngine;
using UnityEngine.UI;

public class Rotator : MonoBehaviour
{
    public float RotationSpeed = 180f;
    void Update()
    {
        transform.Rotate(Vector3.forward, RotationSpeed * CO.co.GetWorldSpeedDelta());
    }
}
