using UnityEngine;

public class Rotors : MonoBehaviour
{
    public DRIFTER Drifter;
    public float PhaseSpeedMax = 32f;
    public int Phase = -1;
    private float Scale = 1;
    void Update()
    {
        float Speed = PhaseSpeedMax * Time.deltaTime * Drifter.GetRotorSpeed();
        if (Speed != 0)
        {
            Scale += Speed * Phase;
            transform.localScale = new Vector3(1, Scale, 1);
            if (Scale < -1 && Phase < 0) Phase = 1;
            else if (Scale > 1 && Phase > 0) Phase = -1;
        }
    }
}
