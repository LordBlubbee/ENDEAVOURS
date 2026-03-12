using System.Collections;
using UnityEngine;

public class MasteryDeterminationSecondWind : ArtifactAbility
{
    public MasteryDeterminationSecondWind(CREW crew) : base(crew)
    {
    }

    public override void OnDamaged()
    {
        if (CooldownTimer > 0) return;
        if (User.GetHealthRelative() > 0.3f) return;
        ScriptableBuff buff = new();
        buff.name = "DeterminationSecondWind";
        buff.BuffParticles = CO_SPAWNER.BuffParticles.PRAGMATICUS_SHIELD;
        buff.MaxStacks = 1;
        buff.Duration = 4;
        buff.HealthChangePerSecond = 8f + User.GetATT_PHYSIQUE() * 2f;
        buff.ModifyDamageResEnv = 1f;
        buff.ModifyDamageResMelee = 1f;
        buff.ModifyDamageResRanged = 1f;
        buff.ModifyDamageResSpell = 1f;
        User.AddBuff(buff, User);

        User.StartCoroutine(Cooldown());
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 90f / (0.9f + User.GetATT_MEDICAL() * 0.1f);
        while (CooldownTimer > 0f)
        {
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
    }
}
