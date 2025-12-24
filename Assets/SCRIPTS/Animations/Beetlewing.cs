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
        if (Crew.isDead()) return;

        float sped = Crew.GetCurrentSpeed();
        float speedmod = Mathf.Clamp01((sped / MaximumSpeed)+0.4f);

        CurrentWingAngle = OpenWingDirection * OpenWingMaximumAngle * speedmod;
        sped = (Mathf.Max(sped, 2f)) * CO.co.GetWorldSpeedDelta() * LegDirection * LegRotSpeed;
        transform.Rotate(Vector3.forward, sped);
        LegRotCurrent += sped;
        if (LegRotCurrent > CurrentWingAngle + (MaximumOscillate * speedmod) && LegDirection > 0) LegDirection *= -1;
        else if (LegRotCurrent < CurrentWingAngle - (MaximumOscillate * speedmod) && LegDirection < 0) LegDirection *= -1;
    }
}
