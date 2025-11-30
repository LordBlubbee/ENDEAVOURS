using UnityEngine;

public class SpellPragmaticusShield : UniqueSpell
{
    public override void UseUniqueSpell(CREW Caster, Vector3 AimTowards)
    {
        float MaxDis = 4f;
        CREW Target = null;
        foreach (CREW allies in CO.co.GetAlliedCrew(Caster.GetFaction()))
        {
            if (Caster == allies) continue;
            if (allies.isDead()) continue;
            float Dis = (AimTowards - allies.transform.position).magnitude;
            if (Dis > MaxDis) continue;
            MaxDis = Dis;
            Target = allies;
        }
        if (Target)
        {
            CO_SPAWNER.co.SpawnPragmaticusShieldImpactRpc(Target.transform.position);
            ScriptableBuff buff = new();
            buff.name = "PragmaticusShieldSpell";
            buff.MaxStacks = 1;
            buff.BuffParticles = CO_SPAWNER.BuffParticles.PRAGMATICUS_SHIELD;
            buff.Duration = 10f;
            buff.TemporaryHitpoints = 20f + Caster.GetATT_COMMUNOPATHY() * 5f;
            Target.AddBuff(buff);
        }
    }
}
