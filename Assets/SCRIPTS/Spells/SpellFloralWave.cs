using UnityEngine;

public class SpellFloralWave : UniqueSpell
{
    public override void UseUniqueSpell(CREW Caster, Vector3 AimTowards)
    {
        CO_SPAWNER.co.SpawnFloralHealSpellImpactRpc(AimTowards);
        foreach (CREW allies in CO.co.GetAlliedCrew(Caster.GetFaction()))
        {
            if (Caster == allies) continue;
            if ((AimTowards - allies.transform.position).magnitude > 7f) continue;
            allies.Heal(10f + Caster.GetATT_COMMUNOPATHY() * 2f + Caster.GetATT_MEDICAL() * 2f);
        }
    }
}
