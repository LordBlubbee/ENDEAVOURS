using UnityEngine;

public class Legs : MonoBehaviour
{
    public CREW Crew;
    public float MaximumLegRot = 15f;
    public float LegRotSpeed = 25f;
    public float LegRotCurrent = 0f;
    public int LegDirection = 1;

    private void Start()
    {
        transform.Rotate(Vector3.forward, LegRotCurrent);
    }
    void Update()
    {
        float sped = Crew.GetCurrentSpeed();
        if (sped > 0)
        {
            sped *= CO.co.GetWorldSpeedDelta() * LegDirection;
            transform.Rotate(Vector3.forward, sped);
            LegRotCurrent += sped;
            if (LegRotCurrent > MaximumLegRot && LegDirection > 0) LegDirection *= -1;
            else if (LegRotCurrent < -MaximumLegRot && LegDirection < 0) LegDirection *= -1;
        }
    }
}
