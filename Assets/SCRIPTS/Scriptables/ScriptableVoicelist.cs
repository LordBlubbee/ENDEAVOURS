using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Voicelist", order = 1)]
public class ScriptableVoicelist : ScriptableObject
{
    public enum VoicelineTypes
    {
        FIRST_ENGAGE,
        IN_COMBAT,
        SKIRMISHING,
        RETREATING,
        REPAIRING,
        HEALING,
        BOARDING,
        READYING,
        DOWNED,
        SURRENDERING,
        FORMATION,
        PREPARE_CHARGE,
        CHARGE,
        SALUTE,
        SALUTE_EASYVICTORY,
        SALUTE_HARDVICTORY,
        SALUTE_WORRIED,
        SALUTE_LOWAMMO,
        SALUTE_LOWSUPPLIES,
        SALUTE_PROMOTION
    }
    public List<Voiceline> GetVoicelines(VoicelineTypes typ)
    {
        switch (typ)
        {
            case VoicelineTypes.FIRST_ENGAGE:
                return VCX_FirstEngage;

            case VoicelineTypes.IN_COMBAT:
                return VCX_InCombat;

            case VoicelineTypes.SKIRMISHING:
                return VCX_Skirmishing;

            case VoicelineTypes.RETREATING:
                return VCX_Retreating;

            case VoicelineTypes.REPAIRING:
                return VCX_Repairing;

            case VoicelineTypes.HEALING:
                return VCX_Healing;

            case VoicelineTypes.BOARDING:
                return VCX_Boarding;

            case VoicelineTypes.READYING:
                return VCX_ReadyingForBattle;

            case VoicelineTypes.DOWNED:
                return VCX_Downed;

            case VoicelineTypes.SURRENDERING:
                return VCX_Surrendering;

            case VoicelineTypes.FORMATION:
                return VCX_Formation;

            case VoicelineTypes.PREPARE_CHARGE:
                return VCX_PrepareCharge;

            case VoicelineTypes.CHARGE:
                return VCX_Charge;

            case VoicelineTypes.SALUTE:
                return VCX_SalutePlayer;

            case VoicelineTypes.SALUTE_EASYVICTORY:
                return VCX_SalutePlayer_EasyVictory;

            case VoicelineTypes.SALUTE_HARDVICTORY:
                return VCX_SalutePlayer_HardVictory;

            case VoicelineTypes.SALUTE_WORRIED:
                return VCX_SalutePlayer_Worried;

            case VoicelineTypes.SALUTE_LOWAMMO:
                return VCX_SalutePlayer_LowAmmo;

            case VoicelineTypes.SALUTE_LOWSUPPLIES:
                return VCX_SalutePlayer_LowSupplies;

            case VoicelineTypes.SALUTE_PROMOTION:
                return VCX_SalutePlayer_Promotion;

            default:
                return new(); // or new List<Voiceline>();
        }
    }

    [Header("In Combat")]
    public List<Voiceline> VCX_FirstEngage;
    public List<Voiceline> VCX_InCombat;
    public List<Voiceline> VCX_Skirmishing;
    public List<Voiceline> VCX_Retreating;
    public List<Voiceline> VCX_Repairing;
    public List<Voiceline> VCX_Healing;
    public List<Voiceline> VCX_Boarding;
    public List<Voiceline> VCX_ReadyingForBattle;
    public List<Voiceline> VCX_Downed;

    public List<Voiceline> VCX_Surrendering;
    public List<Voiceline> VCX_Formation;
    public List<Voiceline> VCX_PrepareCharge;
    public List<Voiceline> VCX_Charge;

    [Header("Peaceful Voicelines")]
    public List<Voiceline> VCX_SalutePlayer;
    public List<Voiceline> VCX_SalutePlayer_EasyVictory;
    public List<Voiceline> VCX_SalutePlayer_HardVictory;
    public List<Voiceline> VCX_SalutePlayer_Worried;
    public List<Voiceline> VCX_SalutePlayer_LowAmmo;
    public List<Voiceline> VCX_SalutePlayer_LowSupplies;
    public List<Voiceline> VCX_SalutePlayer_Promotion;

    public List<Conversation> TargetedConversations;
    public List<Conversation> GenericConversations;
}


[Serializable]
public class Voiceline
{
    [TextArea(2, 5)]
    public string VoiceTex;
    public VCX.VoiceStyles Style;
}

[Serializable]
public class Conversation
{
    public List<ScriptableVoicelist> Target;
    public List<ConversationPart> Parts;
}
[Serializable]
public class ConversationPart
{
    public Voiceline Voice;
    public bool IsPersonA;
}