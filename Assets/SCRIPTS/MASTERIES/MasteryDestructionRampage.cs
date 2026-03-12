using System.Collections;
using UnityEngine;

public class MasteryDestructionRampage : ArtifactAbility
{
    public MasteryDestructionRampage(CREW crew) : base(crew)
    {
    }
    public override void OnMelee()
    {
        int Enemies = 0;
        foreach (CREW allies in CO.co.GetAllCrews())
        {
            if (User == allies) continue;
            if ((User.transform.position - allies.transform.position).magnitude > 16f) continue;
            if (allies.isDead()) continue;
            if (allies.GetFaction() == User.GetFaction()) Enemies--;
            else Enemies++;
        }
        if (Enemies < 4) return;
        ScriptableBuff buff = new();
        buff.name = "DestructionRampage";
        buff.MaxStacks = 1;
        buff.Duration = 2;
        buff.ModifyAnimationSpeed = 0.3f;
        buff.ModifyMovementSpeed = 0.2f;
        buff.ModifyDamageResMelee = 0.4f;
        buff.ModifyDamageResRanged = 0.4f;
        User.AddBuff(buff, User);
    }
}
