using System.Collections;
using UnityEngine;

public class MasteryMedicineStim : ArtifactAbility
{
    public MasteryMedicineStim(CREW crew) : base(crew)
    {
    }
    public override void OnMedkit(CREW crew)
    {
        ScriptableBuff buff = new();
        buff.name = "MedicineStim";
        buff.MaxStacks = 1;
        buff.Duration = 10;
        buff.BuffParticles = CO_SPAWNER.BuffParticles.KNUCKLES_BUFF;
        buff.ModifyStaminaRegen = 0.2f + User.GetATT_MEDICAL() * 0.02f;
        buff.ModifyMovementSpeed = 0.1f + User.GetATT_MEDICAL() * 0.01f;
        buff.ModifyMeleeDamage = 3 + User.GetATT_COMMAND() * 0.5f;
        buff.ModifyRangedDamage = 3 + User.GetATT_COMMAND() * 0.5f;
        crew.AddBuff(buff, User);
    }
}
