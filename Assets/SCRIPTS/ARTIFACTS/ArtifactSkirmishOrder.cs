using System.Collections;
using UnityEngine;

public class ArtifactSkirmishOrder : ArtifactAbility
{
    public ArtifactSkirmishOrder(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitMelee(CREW crew)
    {
        Hit(crew);
    }
    private void Hit(CREW crew)
    {
        foreach (CREW allies in CO.co.GetAlliedCrew(User.GetFaction()))
        {
            if (User == allies) continue;
            if ((User.transform.position - allies.transform.position).magnitude > 16f) continue;
            ScriptableBuff buff = new();
            buff.name = "SkirmishOrder";
            buff.MaxStacks = 1;
            buff.Duration = 5;
            buff.ModifyMovementSpeed = 0.2f + User.GetATT_COMMAND() * 0.03f;
            buff.ModifyMeleeDamage = 0.2f + User.GetATT_COMMAND() * 0.03f;
            allies.AddBuff(buff);
        }
       
    }
}
