using System.Collections;
using UnityEngine;

public class MasteryDeterminationVictory : ArtifactAbility
{
    public MasteryDeterminationVictory(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitMelee(CREW crew, float damageDone)
    {
        Hit(crew);
    }
    private void Hit(CREW Crew)
    {
        ScriptableBuff buff = new();
        buff.name = "DeterminationVictory";
        buff.MaxStacks = 10;
        buff.BuffParticles = CO_SPAWNER.BuffParticles.VENGEANCE;
        buff.Duration = 2;
        buff.ModifyDamageResMelee = 0.1f;
        Crew.AddBuff(buff, User);
    }
}
