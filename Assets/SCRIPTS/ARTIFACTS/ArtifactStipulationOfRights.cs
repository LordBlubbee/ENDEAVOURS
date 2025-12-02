using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

public class ArtifactStipulationOfRights : ArtifactAbility
{
    public ArtifactStipulationOfRights(CREW crew) : base(crew)
    {
    }

    public override void OnDamaged()
    {
        if (CooldownTimer > 0) return;
        if (User.GetHealthRelative() > 0.5f) return;
        ScriptableBuff buff = new();
        buff.name = "StipulationOfRights";
        buff.BuffParticles = CO_SPAWNER.BuffParticles.PRAGMATICUS_SHIELD;
        buff.MaxStacks = 1;
        buff.Duration = 10;
        buff.TemporaryHitpoints = 30f + User.GetATT_COMMAND()*10f;
        User.AddBuff(buff);

        User.StartCoroutine(Cooldown());
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 60f / (0.9f + User.GetATT_COMMAND() * 0.1f);
        while (CooldownTimer > 0f)
        {
            yield return new WaitForSeconds(1);
            CooldownTimer -= 1;
        }
    }
}
