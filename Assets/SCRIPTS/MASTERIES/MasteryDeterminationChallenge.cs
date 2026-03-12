using System.Collections;
using UnityEngine;

public class MasteryDeterminationChallenge : ArtifactAbility
{
    public MasteryDeterminationChallenge(CREW crew) : base(crew)
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
        if ((User.transform.position-Crew.transform.position).magnitude < 8f) User.GainCredit_CrewDamage(Crew.TakeDamage(12, Crew.transform.position, iDamageable.DamageType.RANGED_CRIT));
    }
}
