using System.Collections;
using UnityEngine;

public class MasteryStrategumShield : ArtifactAbility
{
    public MasteryStrategumShield(CREW crew) : base(crew)
    {
    }
    public override void OnMelee()
    {
        Chance(0.05f);
    }
    public override void OnRanged()
    {
        Chance(0.06f);
    }
    public void Chance(float chance)
    {
        if (Random.Range(0f, 1f) > chance) return;
        CREW Target = null;
        foreach (CREW allies in CO.co.GetAlliedCrew(User.GetFaction()))
        {
            if (User == allies) continue;
            if ((User.transform.position - allies.transform.position).magnitude > 8f) continue;
            if (allies.isDead()) continue;
            Target = allies;
        }
        if (!Target) return;
        ScriptableBuff buff = new();
        buff.name = "StrategumShield";
        buff.BuffParticles = CO_SPAWNER.BuffParticles.PRAGMATICUS_SHIELD;
        buff.MaxStacks = 1;
        buff.Duration = 4;
        buff.TemporaryHitpoints = 20f + User.GetATT_COMMAND() * 2f + User.GetATT_ALCHEMY() * 2f;
        Target.AddBuff(buff, User);
    }
}
