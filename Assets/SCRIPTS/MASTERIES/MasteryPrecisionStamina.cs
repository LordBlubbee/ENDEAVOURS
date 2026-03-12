using System.Collections;
using UnityEngine;

public class MasteryPrecisionStamina : ArtifactAbility
{
    public MasteryPrecisionStamina(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitRanged(CREW crew)
    {
        Hit(crew);
    }
    public override void OnEnemyHitSpell(CREW crew)
    {
        Hit(crew);
    }
    private void Hit(CREW Crew)
    {
        if (User.GetStaminaRelative() >= 0.7f) Crew.TakeDamage(18, Crew.transform.position, iDamageable.DamageType.RANGED_CRIT);
    }
}
