using UnityEngine;

public class SpellFloralWave : UniqueSpell
{
    public override void UseUniqueSpell(CREW Caster, Vector3 AimTowards)
    {
        CO_SPAWNER.co.SpawnFloralImpactRpc(AimTowards);
        foreach (CREW allies in CO.co.GetAlliedCrew(Caster.GetFaction()))
        {
            if (Caster == allies) continue;
            if ((AimTowards - allies.transform.position).magnitude > 7f) continue;
            allies.Heal(20f + Caster.GetATT_COMMUNOPATHY() * 4f + Caster.GetATT_MEDICAL() * 4f);
        }
    }
}
