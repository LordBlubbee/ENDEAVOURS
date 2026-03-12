using System.Collections;
using UnityEngine;

public class ArtifactRedPowder : ArtifactAbility
{
    private float CooldownTimer = 0f;

    public ArtifactRedPowder(CREW crew) : base(crew)
    {
    }

    IEnumerator Cooldown()
    {
        CooldownTimer = 10f / (0.9f + User.GetATT_ALCHEMY() * 0.1f);
        while (CooldownTimer > 0f)
        {
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
    }
    public override void OnEnemyHitRanged(CREW crew)
    {
        if (CooldownTimer > 0) return;
        foreach (Collider2D col in Physics2D.OverlapCircleAll(crew.transform.position, 4f))
        {
            CREW hit = col.GetComponent<CREW>();
            if (hit != null)
            {
                if (!hit.CanBeTargeted(crew.Space)) continue;
                if (hit.GetFaction() == crew.GetFaction())
                {
                    hit.TakeDamage(20f * (1f + User.GetATT_ALCHEMY() * 0.2f), hit.transform.position, iDamageable.DamageType.SPELL_CRIT);
                }
            }
        }
        CO_SPAWNER.co.SpawnExplosionTinyRpc(crew.transform.position);
        User.StartCoroutine(Cooldown());
    }
}
