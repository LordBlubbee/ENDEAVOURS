using UnityEngine;

public class SpellWaywardConsumption : UniqueSpell
{
    public override void UseUniqueSpell(CREW Caster, Vector3 AimTowards)
    {
        CO_SPAWNER.co.SpawnWaywardConsumptionRpc(Caster.transform.position);
        foreach (CREW enemy in CO.co.GetEnemyCrew(Caster.GetFaction()))
        {
            if (Caster == enemy) continue;
            if ((Caster.transform.position - enemy.transform.position).magnitude > 12f) continue;
            enemy.TakeDamage(20f + Caster.GetATT_COMMUNOPATHY() * 4f,enemy.transform.position,iDamageable.DamageType.SPELL);
            enemy.Push(9f + Caster.GetATT_COMMUNOPATHY() * 0.5f, 0.5f, (enemy.transform.position - Caster.transform.position).normalized);
        }
    }
}
