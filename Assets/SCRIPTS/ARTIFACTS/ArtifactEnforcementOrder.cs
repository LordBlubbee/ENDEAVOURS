using System.Collections;
using UnityEngine;

public class ArtifactEnforcementOrder : ArtifactAbility
{
    public ArtifactEnforcementOrder(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitRanged(CREW crew)
    {
        Hit(crew);
    }
    public override void OnEnemyHitMelee(CREW crew)
    {
        Hit(crew);
    }
    private void Hit(CREW crew)
    {
        if (Random.Range(0f, 1f) > 0.05f + User.GetATT_COMMAND() * 0.01f) return;

        ScriptableBuff buff = new();
        buff.name = "EnforcementOrder";
        buff.MaxStacks = 1;
        buff.Duration = 3;
        buff.ModifyMovementSlow = 1f;
        buff.ModifyAnimationSlow = 1f;
        buff.BuffParticles = CO_SPAWNER.BuffParticles.FROST;
        crew.TakeDamage(10f + User.GetATT_COMMAND() * 2f, crew.transform.position, iDamageable.DamageType.TRUE);
        crew.AddBuff(buff);
    }
}
