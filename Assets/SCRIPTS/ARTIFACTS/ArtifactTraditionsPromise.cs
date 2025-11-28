using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ArtifactTraditionsPromise : ArtifactAbility
{
    public ArtifactTraditionsPromise(CREW crew) : base(crew)
    {
    }
    public override void PeriodicEffect()
    {
        if (CooldownTimer > 0f) return;
        foreach (CREW allies in CO.co.GetAlliedCrew(User.GetFaction()))
        {
            if (User == allies) continue;
            if ((User.transform.position - allies.transform.position).magnitude > 16f) continue;
            if (allies.GetHealthRelative() > 0.5f) continue;
            allies.Heal(20f + User.GetATT_MEDICAL() * 4f);
            User.StartCoroutine(Cooldown());
        }
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 20f / (0.9f + User.GetATT_COMMAND() * 0.1f);
        while (CooldownTimer > 0f)
        {
            yield return new WaitForSeconds(1);
            CooldownTimer -= 1;
        }
    }
}
