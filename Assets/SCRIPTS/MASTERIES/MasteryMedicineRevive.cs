using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MasteryMedicineRevive : ArtifactAbility
{
    public MasteryMedicineRevive(CREW crew) : base(crew)
    {
    }
    public override void PeriodicEffect()
    {
        if (CooldownTimer > 0f) return;
        foreach (CREW allies in CO.co.GetAlliedCrew(User.GetFaction()))
        {
            if (User == allies) continue;
            if ((User.transform.position - allies.transform.position).magnitude > 8f) continue;
            if (!allies.isDead() || allies.isDeadForever()) continue;
            CO_SPAWNER.co.SpawnFloralImpactRpc(allies.transform.position);
            allies.Heal(50f);
            User.StartCoroutine(Cooldown());
            break;
        }
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 70f / (0.9f + User.GetATT_ALCHEMY() * 0.1f + User.GetATT_MEDICAL() * 0.1f);
        while (CooldownTimer > 0f)
        {
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
    }
}
