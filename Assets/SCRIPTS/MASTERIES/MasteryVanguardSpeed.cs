using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MasteryVanguardSpeed : ArtifactAbility
{
    public MasteryVanguardSpeed(CREW crew) : base(crew)
    {
    }
    public override void PeriodicEffect()
    {
        foreach (CREW allies in CO.co.GetAlliedCrew(User.GetFaction()))
        {
            if (User == allies) continue;
            if ((User.transform.position - allies.transform.position).magnitude > 16f) continue;
            if (allies.isDead()) continue;
            ScriptableBuff buff = new();
            buff.name = "VanguardSpeed";
            buff.MaxStacks = 1;
            buff.Duration = 2;
            buff.BuffParticles = CO_SPAWNER.BuffParticles.DEFAULT_SPEED;
            buff.ModifyAnimationSpeed = 0.2f;
            buff.ModifyMovementSpeed = 0.2f;
        }
    }
}
