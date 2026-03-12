using System.Collections;
using UnityEngine;

public class MasteryDestructionShockwave : ArtifactAbility
{
    public MasteryDestructionShockwave(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyKill(CREW crew)
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(crew.transform.position, 4f))
        {
            CREW hit = col.GetComponent<CREW>();
            if (hit != null)
            {
                if (!hit.CanBeTargeted(crew.Space)) continue;
                if (hit.GetFaction() == crew.GetFaction())
                {
                    User.GainCredit_CrewDamage(hit.TakeDamage(10f * (1f + User.GetATT_ALCHEMY() * 0.2f), hit.transform.position, iDamageable.DamageType.SPELL_CRIT));
                }
            }
        }
        CO_SPAWNER.co.SpawnShockwaveImpactRpc(crew.transform.position);
    }
}
