using System.Collections;
using UnityEngine;

public class MasteryPrecisionStamina : ArtifactAbility
{
    public MasteryPrecisionStamina(CREW crew) : base(crew)
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
    private void Hit(CREW Crew)
    {
        if (User.GetStaminaRelative() >= 0.7f) User.GainCredit_CrewDamage(Crew.TakeDamage(18, Crew.transform.position, iDamageable.DamageType.RANGED_CRIT));
    }
}
