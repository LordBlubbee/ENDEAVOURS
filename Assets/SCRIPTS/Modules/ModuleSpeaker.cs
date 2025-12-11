using UnityEngine;

public class ModuleSpeaker : ModuleEffector
{
    public float GetBuffEffectPower()
    {
        switch (speakerType)
        {
            case SpeakerTypes.INVICTUS:
                return 8f + ModuleLevel.Value * 4f;
            case SpeakerTypes.PRAGMATICUS:
                return 1f + ModuleLevel.Value*0.5f;
            case SpeakerTypes.STELLAE:
                return 3f + ModuleLevel.Value * 2f;
        }
        return 0f;
    }
    public enum SpeakerTypes
    {
        INVICTUS,
        PRAGMATICUS,
        STELLAE
    }
    public SpeakerTypes speakerType;
    protected override void Activation()
    {
        switch (speakerType)
        {
            case SpeakerTypes.INVICTUS:
                foreach (CREW crew in CO.co.GetAlliedCrew(GetFaction()))
                {
                    if (crew.isDead()) continue;
                    ScriptableBuff buff = new();
                    buff.name = "SpeakersInvictus";
                    buff.MaxStacks = 3;
                    buff.Duration = GetEffectDuration();
                    buff.BuffParticles = CO_SPAWNER.BuffParticles.SPEAKER_INVICTUS;
                    buff.ModifyMeleeDamage = GetBuffEffectPower();
                    buff.ModifyRangedDamage = GetBuffEffectPower();
                    crew.AddBuff(buff);
                }
                break;
            case SpeakerTypes.PRAGMATICUS:
                foreach (CREW crew in CO.co.GetEnemyCrew(GetFaction()))
                {
                    if (crew.isDead()) continue;
                    ScriptableBuff buff = new();
                    buff.name = "SpeakersPragmaticus";
                    buff.MaxStacks = 3;
                    buff.Duration = GetEffectDuration();
                    buff.BuffParticles = CO_SPAWNER.BuffParticles.SPEAKER_PRAGMATICUS;
                    buff.ModifyAnimationSlow = GetBuffEffectPower() * 0.3f;
                    buff.ModifyMovementSlow = GetBuffEffectPower();
                    crew.AddBuff(buff);
                }
                break;
            case SpeakerTypes.STELLAE:
                foreach (CREW crew in CO.co.GetAlliedCrew(GetFaction()))
                {
                    if (crew.isDead()) continue;
                    ScriptableBuff buff = new();
                    buff.name = "SpeakersStellae";
                    buff.MaxStacks = 3;
                    buff.Duration = GetEffectDuration();
                    buff.BuffParticles = CO_SPAWNER.BuffParticles.SPEAKER_STELLAE;
                    buff.HealthChangePerSecond = GetBuffEffectPower();
                    crew.AddBuff(buff);
                }
                break;
        }
    }
}
