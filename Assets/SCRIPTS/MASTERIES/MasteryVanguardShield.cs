using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MasteryVanguardShield : ArtifactAbility
{
    public MasteryVanguardShield(CREW crew) : base(crew)
    {
    }

    public override void OnDamaged()
    {
        if (CooldownTimer > 0)
        {
            CooldownTimer = 10f;
            return;
        }
        User.StartCoroutine(Cooldown());
    }
    public override void PeriodicEffect()
    {
        if (CooldownTimer > 0) return;
        if (User.GetHealthRelative() < 1f) return;
        ScriptableBuff buff = new();
        buff.name = "VanguardShield";
        buff.BuffParticles = CO_SPAWNER.BuffParticles.PRAGMATICUS_SHIELD;
        buff.MaxStacks = 1;
        buff.Duration = 20f;
        buff.TemporaryHitpoints = 20f + User.GetATT_COMMAND() * 2f + User.GetATT_MEDICAL() * 2f + User.GetATT_COMMUNOPATHY() * 2f;
        User.AddBuff(buff, User);
        User.StartCoroutine(Cooldown());
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 10f;
        while (CooldownTimer > 0f)
        {
            yield return null;
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
        }
    }
}
