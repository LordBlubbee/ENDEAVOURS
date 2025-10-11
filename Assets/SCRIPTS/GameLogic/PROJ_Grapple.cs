using UnityEngine;
using static CO;

public class PROJ_Grapple : PROJ
{
    protected override void PotentialHitTarget(GameObject collision)
    {
        WalkableTile crew = collision.GetComponent<WalkableTile>();
        if (crew != null)
        {
            if (crew.Space == Space) return;
            if (!crew.canBeBoarded) return;
            BulletImpact();
            CrewOwner.UseGrapple(crew);
            return;
        }
    }
}
