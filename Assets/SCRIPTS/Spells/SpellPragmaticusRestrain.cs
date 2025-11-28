using UnityEngine;

public class SpellPragmaticusRestrain : UniqueSpell
{
    public override void UseUniqueSpell(CREW Caster, Vector3 AimTowards)
    {
        CO_SPAWNER.co.SpawnPragmaticusRestrainImpactRpc(AimTowards);
        float MaxDis = 4f;
        CREW Target = null;
        foreach (CREW enemy in CO.co.GetEnemyCrew(Caster.GetFaction()))
        {
            if (Caster == enemy) continue;
            if (enemy.isDead()) continue;
            float Dis = (AimTowards - enemy.transform.position).magnitude;
            if (Dis > MaxDis) continue;
            MaxDis = Dis;
            Target = enemy;
        }
        if (Target)
        {
            ScriptableBuff buff = new();
            buff.name = "PragmaticusRestrainSpell";
            buff.MaxStacks = 1;
            buff.Duration = 3f + Caster.GetATT_COMMUNOPATHY() * 0.4f;
            buff.ModifyMeleeDamage -= 10;
            buff.ModifyMovementSlow = 99;
            Target.AddBuff(buff);
        }
    }
}
