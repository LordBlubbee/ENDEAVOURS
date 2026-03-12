using UnityEngine;

public class ArtifactKnuckles : ArtifactAbility
{
    public ArtifactKnuckles(CREW crew) : base(crew)
    {
    }

    public override void OnDamaged()
    {
        ScriptableBuff buff = new();
        buff.name = "KnucklesRage";
        buff.MaxStacks = 3;
        buff.Duration = 5;
        buff.BuffParticles = CO_SPAWNER.BuffParticles.KNUCKLES_BUFF;
        buff.ModifyAnimationSpeed = 0.1f;
        buff.ModifyMovementSpeed = 0.2f;
        buff.ModifyMeleeDamage = 3;
        User.AddBuff(buff, User);
    }
}
