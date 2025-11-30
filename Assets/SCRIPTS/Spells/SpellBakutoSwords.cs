using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class SpellBakutoSwords : UniqueSpell
{
    public PROJ BakutoSwordPrefab;
    public Transform[] BakutoStrikePoints;

    private Vector3 GetBakutoStrikePoint()
    {
        return BakutoStrikePoints[Random.Range(0, BakutoStrikePoints.Length)].position;
    }
    public override void UseUniqueSpell(CREW Caster, Vector3 AimTowards)
    {
        float dmg = Mathf.Min(Caster.GetHealth() - 1f, 10f);
        if (dmg > 0) Caster.TakeDamage(dmg, Caster.transform.position, iDamageable.DamageType.TRUE);
        StartCoroutine(Storm(Caster));
    }

    IEnumerator Storm(CREW Caster)
    {
        int Swords = Mathf.FloorToInt(0.3f * (Caster.GetATT_COMMUNOPATHY() + Caster.GetATT_PHYSIQUE() + Caster.GetATT_ALCHEMY()));
        Swords = Mathf.Min(1, Swords);
        float TimePerSword = 0.9f / (float)Swords;
        float Timer = 0f;
        for (int i = 0; i < Swords; i++)
        {
            Timer -= CO.co.GetWorldSpeedDelta();
            if (Timer < 0f)
            {
                Timer += TimePerSword;
                PROJ proj = Instantiate(BakutoSwordPrefab, GetBakutoStrikePoint(), Caster.transform.rotation);
                float dmg = 20f;
                dmg += Caster.ModifySpellDamage;
                dmg *= 0.7f + 0.1f * Caster.GetATT_COMMUNOPATHY() + 0.02f * Caster.GetCurrentCommanderLevel();
                if (Caster.GetATT_COMMUNOPATHY() < 4) dmg *= 0.25f * Caster.GetATT_COMMUNOPATHY();
                proj.NetworkObject.Spawn();
                proj.Init(dmg, Caster.GetFaction(), Caster.Space, Vector3.zero);
                proj.CrewOwner = Caster;
            }
            yield return null;
        }
    }
}
