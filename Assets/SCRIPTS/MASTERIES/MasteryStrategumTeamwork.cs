using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MasteryStrategumTeamwork : ArtifactAbility
{
    public MasteryStrategumTeamwork(CREW crew) : base(crew)
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
        buff.name = "StrategumTeamwork";
        buff.BuffParticles = CO_SPAWNER.BuffParticles.DEFAULT_HEALING;
        buff.MaxStacks = Allies;
        buff.Duration = 2;
        buff.ModifyRepairDone = 0.15f;
        buff.ModifyHealingDone = 0.15f;
        for (int i = 0; i < Allies; i++)
        {
            User.AddBuff(buff, User);
        }
    }
}
