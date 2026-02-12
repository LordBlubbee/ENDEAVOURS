using UnityEngine;

public class CrewAura : MonoBehaviour
{
    CREW Crew;
    SpriteRenderer spr;
    Color col;
    public void SetCrew(CREW cr)
    {
        Crew = cr;
        float Scale = Crew.Radius + 0.2f;
        transform.localScale = new Vector3(Scale, Scale, 1);
        spr = GetComponent<SpriteRenderer>();
        col = Crew.GetFaction() == 1 ? new Color(0,1,0,0.5f) : new Color(1, 0, 0, 0.5f);
        spr.color = col;
    }

    float TargetAlpha = 0.5f;
    void Update()
    {
        if (Crew == null)
        {
            Destroy(gameObject);
            return;
        }
        if (Crew.isDead())
        {
            if (spr.color.a > 0.45f || Crew.isDeadForever()) TargetAlpha = 0.05f;
            else if (spr.color.a < 0.1f) TargetAlpha = 0.5f;
        }
        else
        {
            TargetAlpha = 0.5f;
        }
        if (spr.color.a > TargetAlpha)
        {
            float newAlpha = Mathf.Max(spr.color.a - CO.co.GetWorldSpeedDelta(), TargetAlpha);
            spr.color = new Color(col.r, col.g, col.b, newAlpha);
        }
        else if (spr.color.a < TargetAlpha)
        {
            float newAlpha = Mathf.Min(spr.color.a + CO.co.GetWorldSpeedDelta(), TargetAlpha);
            spr.color = new Color(col.r, col.g, col.b, newAlpha);
        }
        transform.position = Crew.transform.position + new Vector3(0, 0, 0.005f);
    }
}
