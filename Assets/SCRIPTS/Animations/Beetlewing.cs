using UnityEngine;

public class Beetlewing : MonoBehaviour
{
    public CREW Crew;

    public float OpenWingMaximumAngle = 120f;
    public float MaximumSpeed = 10f;
    public int OpenWingDirection = 1;
    float CurrentWingAngle = 0f;

    public float MaximumOscillate = 15f;
    public float LegRotCurrent = 0f;
    public float LegRotSpeed = 25f;
    public int LegDirection = 1;
    void Update()
    {
        float sped = Crew.GetCurrentSpeed();
        if (sped > 0)
        {
            CurrentWingAngle = OpenWingDirection* OpenWingMaximumAngle * Mathf.Clamp01(sped / MaximumSpeed);
            sped *= CO.co.GetWorldSpeedDelta() * LegDirection * LegRotSpeed;
            transform.Rotate(Vector3.forward, sped);
            LegRotCurrent += sped;
            if (LegRotCurrent > CurrentWingAngle + MaximumOscillate && LegDirection > 0) LegDirection *= -1;
            else if (LegRotCurrent < CurrentWingAngle - MaximumOscillate && LegDirection < 0) LegDirection *= -1;
        }
    }
}
