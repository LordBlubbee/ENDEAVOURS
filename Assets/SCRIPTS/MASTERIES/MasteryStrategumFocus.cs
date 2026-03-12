using System.Collections;
using UnityEngine;

public class MasteryStrategumFocus : ArtifactAbility
{
    public MasteryStrategumFocus(CREW crew) : base(crew)
    {
    }
    public override void PeriodicEffect()
    {
        int Allies = 0;
        foreach (CREW allies in CO.co.GetAlliedCrew(User.GetFaction()))
        {
            if (User == allies) continue;
            if ((User.transform.position - allies.transform.position).magnitude > 16f) continue;
            if (allies.isDead()) continue;
            Allies++;
            if (Allies > 3) break;
        }
        if (Allies == 0) return;
        ScriptableBuff buff = new();
        buff.name = "StrategumFocus";
        buff.MaxStacks = Allies;
        buff.Duration = 2;
        buff.ModifyMeleeDamage = 3;
        buff.ModifyRangedDamage = 3;
        buff.ModifySpellDamage = 3;
        for (int i = 0; i < Allies; i++)
        {
            User.AddBuff(buff, User);
        }
    }
}
