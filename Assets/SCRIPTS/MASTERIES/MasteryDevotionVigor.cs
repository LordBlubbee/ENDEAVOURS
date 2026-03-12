using System.Collections;
using UnityEngine;

public class MasteryDevotionVigor : ArtifactAbility
{
    public MasteryDevotionVigor(CREW crew) : base(crew)
    {
    }

    int Energy = 0;
    public override void OnEnemyHitMelee(CREW crew, float damageDone)
    {
        Energy++;
        Hit(crew);
    }
    public override void OnEnemyHitRanged(CREW crew, float damageDone)
    {
        Energy++;
        Hit(crew);
    }
    public override void OnEnemyHitSpell(CREW crew, float damageDone)
    {
        Energy++;
        Hit(crew);
    }

    void Hit(CREW crew)
    {
        if (Energy >= 20)
        {
            Energy--;
            crew.Push(8f, 0.25f, (crew.transform.position - User.transform.position).normalized);
        }
    }
}
