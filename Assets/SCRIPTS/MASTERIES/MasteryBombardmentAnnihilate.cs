using System.Collections;
using UnityEngine;

public class MasteryBombardmentAnnihilation : ArtifactAbility
{
    public MasteryBombardmentAnnihilation(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitRanged(CREW crew, float damageDone)
    {
        Hit(crew);
    }
    public override void OnEnemyHitSpell(CREW crew, float damageDone)
    {
        Hit(crew);
    }
    public override void OnEnemyHitModule(Module mod, float damageDone)
    {
        Hit(mod);
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 20f / (0.9f + User.GetATT_ALCHEMY() * 0.1f);
        while (CooldownTimer > 0f)
        {
            yield return null;
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
        }
    }
    public void Hit(CREW crew)
    {
        if (CooldownTimer > 0f) return;
        User.StartCoroutine(Cooldown());
        foreach (Collider2D col in Physics2D.OverlapCircleAll(crew.transform.position, 8f))
        {
            CREW hit = col.GetComponent<CREW>();
            if (hit != null)
            {
                if (!hit.CanBeTargeted(crew.Space)) continue;
                if (hit.GetFaction() == crew.GetFaction())
                {
                    User.GainCredit_CrewDamage(hit.TakeDamage(12 + User.GetATT_ARMS() * 2 + User.GetATT_COMMUNOPATHY() * 2, hit.transform.position, iDamageable.DamageType.SPELL_CRIT));
                    hit.Push(4f, 0.15f, (hit.transform.position - crew.transform.position).normalized);
                }
            }
            Module hit2 = col.GetComponent<Module>();
            if (hit2 != null)
            {
                if (!hit2.CanBeTargeted(crew.Space)) continue;
                if (hit2.GetFaction() == crew.GetFaction())
                {
                    User.GainCredit_ModuleDamage(hit2.TakeDamage(12 + User.GetATT_ARMS() * 2 + User.GetATT_COMMUNOPATHY() * 2, hit.transform.position, iDamageable.DamageType.SPELL_CRIT));
                }
            }
        }
        CO_SPAWNER.co.SpawnExplosionMediumRpc(crew.transform.position);
    }
    public void Hit(Module crew)
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(crew.transform.position, 8f))
        {
            CREW hit = col.GetComponent<CREW>();
            if (hit != null)
            {
                if (!hit.CanBeTargeted(crew.Space)) continue;
                if (hit.GetFaction() == crew.GetFaction())
                {
                    User.GainCredit_CrewDamage(hit.TakeDamage(12 + User.GetATT_ARMS() * 2 + User.GetATT_COMMUNOPATHY() * 2, hit.transform.position, iDamageable.DamageType.SPELL_CRIT));
                    hit.Push(4f, 0.15f, (hit.transform.position - crew.transform.position).normalized);
                }
            }
            Module hit2 = col.GetComponent<Module>();
            if (hit2 != null)
            {
                if (!hit2.CanBeTargeted(crew.Space)) continue;
                if (hit2.GetFaction() == crew.GetFaction())
                {
                    User.GainCredit_ModuleDamage(hit2.TakeDamage(12 + User.GetATT_ARMS() * 2 + User.GetATT_COMMUNOPATHY() * 2, hit.transform.position, iDamageable.DamageType.SPELL_CRIT));
                }
            }
        }
        CO_SPAWNER.co.SpawnExplosionMediumRpc(crew.transform.position);
    }
}
