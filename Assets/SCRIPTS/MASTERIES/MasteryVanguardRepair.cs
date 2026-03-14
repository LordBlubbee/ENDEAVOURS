using System.Collections;
using UnityEngine;

public class MasteryVanguardRepair : ArtifactAbility
{
    public MasteryVanguardRepair(CREW crew) : base(crew)
    {
    }
    public override void OnSpell()
    {
        Chance(0.3f);
    }
    public void Chance(float chance)
    {
        if (Random.Range(0f, 1f) > chance) return;
        foreach (Collider2D col in Physics2D.OverlapCircleAll(User.transform.position, 12f))
        {
            Module hit = col.GetComponent<Module>();
            if (hit != null)
            {
                if (hit.GetFaction() == User.GetFaction())
                {
                    User.GainCredit_Repairs(hit.Heal(10 + User.GetATT_ENGINEERING() * 2 + User.GetATT_COMMAND() * 2));
                }
            }
        }
    }
}
