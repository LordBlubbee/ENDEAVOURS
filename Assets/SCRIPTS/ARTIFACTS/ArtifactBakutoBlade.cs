using System.Collections;
using UnityEngine;

public class ArtifactBakutoBlade : ArtifactAbility
{
    public ArtifactBakutoBlade(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitMelee(CREW crew)
    {
        Hit(crew);
    }
    private void Hit(CREW crew)
    {
        if (Random.Range(0f, 1f) > 0.05f + User.GetATT_ALCHEMY() * 0.01f) return;

        User.AddStamina(50);
    }
}
