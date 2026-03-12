using System.Collections;
using UnityEngine;

public class MasteryMedicineAura : ArtifactAbility
{
    public MasteryMedicineAura(CREW crew) : base(crew)
    {
    }
    public override void PeriodicEffect()
    {
        if (CooldownTimer > 0f) return;
        foreach (CREW allies in CO.co.GetAlliedCrew(User.GetFaction()))
        {
            if (User == allies) continue;
            if ((User.transform.position - allies.transform.position).magnitude > 8f) continue;
            if (allies.isDead() || allies.GetHealthRelative() >= 1f) continue;
            allies.Heal(3f + User.GetATT_MEDICAL() * 0.5f);
        }
        User.StartCoroutine(Cooldown());
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 4f;
        while (CooldownTimer > 0f)
        {
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
    }
}
