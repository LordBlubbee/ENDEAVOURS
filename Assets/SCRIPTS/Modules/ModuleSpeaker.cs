using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModuleSpeaker : ModuleEffector
{
    public GameObject ActivateVFXPrefab;
    public AudioClip ActivateSFX;

    [Rpc(SendTo.ClientsAndHost)]
    private void ActivationRpc()
    {
        AUDCO.aud.PlaySFXLoud(ActivateSFX, transform.position);
        StartCoroutine(SpeakerWaves());
    }
    IEnumerator SpeakerWaves()
    {
        for (int i = 0; i < 3; i++)
        {
            Instantiate(ActivateVFXPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }
    }
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
        ActivationRpc();
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
                    crew.AddBuff(buff, null);
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
                    crew.AddBuff(buff, null);
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
                    crew.AddBuff(buff, null);
                }
                break;
        }
    }
}
