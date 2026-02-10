using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Voicelist", order = 1)]
public class ScriptableVoicelist : ScriptableObject
{
    [Header("In Combat")]
    public List<Voiceline> VCX_FirstEngage;
    public List<Voiceline> VCX_InCombat;
    public List<Voiceline> VCX_Skirmishing;
    public List<Voiceline> VCX_Retreating;
    public List<Voiceline> VCX_Repairing;
    public List<Voiceline> VCX_Formation;
    public List<Voiceline> VCX_Charge;
    public List<Voiceline> VCX_Healing;
    public List<Voiceline> VCX_Boarding;
    public List<Voiceline> VCX_ReadyingForBattle;
    public List<Voiceline> VCX_Surrendering;

    [Header("Peaceful Voicelines")]
    public List<Voiceline> VCX_SalutePlayer;
    public List<Voiceline> VCX_SalutePlayer_EasyVictory;
    public List<Voiceline> VCX_SalutePlayer_HardVictory;
    public List<Voiceline> VCX_SalutePlayer_Worried;
    public List<Voiceline> VCX_SalutePlayer_LowAmmo;
    public List<Voiceline> VCX_SalutePlayer_LowSupplies;

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
    public ScriptableVoicelist Target;
    public List<ConversationPart> Parts;
}
[Serializable]
public class ConversationPart
{
    public Voiceline Voice;
    public bool IsPersonA;
}