using System.Collections;
using UnityEngine;

public class MasteryBombardmentBlast : ArtifactAbility
{
    public MasteryBombardmentBlast(CREW crew) : base(crew)
    {
        Mastery = item;
    }
    public override void OnEnemyHitRanged(CREW crew, float damageDone)
    {
        Hit(crew, damageDone);
    }
    public override void OnEnemyHitSpell(CREW crew, float damageDone)
    {
        Hit(crew, damageDone);
    }
    public void Hit(CREW crew, float damageDone)
    {
        if ((crew.transform.position - User.transform.position).magnitude < 24f) return;
        foreach (Collider2D col in Physics2D.OverlapCircleAll(crew.transform.position, 5f))
        {
            CREW hit = col.GetComponent<CREW>();
            if (hit != null)
            {
                if (hit == crew) continue;
                if (!hit.CanBeTargeted(crew.Space)) continue;
                if (hit.GetFaction() == crew.GetFaction())
                {
                    User.GainCredit_CrewDamage(hit.TakeDamage(damageDone * 0.5f, hit.transform.position, iDamageable.DamageType.TRUE));
                }
            }
        }
        CO_SPAWNER.co.SpawnExplosionTinyRpc(crew.transform.position);
    }
}
