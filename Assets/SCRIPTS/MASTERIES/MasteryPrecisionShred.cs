using System.Collections;
using UnityEngine;

public class MasteryPrecisionShred : ArtifactAbility
{
    public MasteryPrecisionShred(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitMelee(CREW crew)
    {
        Hit(crew);
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
        ScriptableBuff buff = new();
        buff.name = "PrecisionShred";
        buff.MaxStacks = 1;
        buff.BuffParticles = CO_SPAWNER.BuffParticles.VENGEANCE;
        buff.Duration = 7;
        buff.ModifyDamageResRanged -= 0.25f + User.GetATT_ALCHEMY() * 0.02f;
        Crew.AddBuff(buff, User);
    }
}
