using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VoiceHandler : NetworkBehaviour
{
    private CREW Crew;
    public Color SpeechColor
    {
        get
        {
            if (Crew == null) return Color.white;
            if (Crew.CharacterBackground == null) return Color.white;
            return Crew.CharacterBackground.BackgroundColor;
        }
    }
    public AudioClip SpeechVoice;
    public ScriptableVoicelist VoiceListEnemy;
    public ScriptableVoicelist VoiceListAlly;

    public ScriptableVoicelist GetVoicelist()
    {
        return Crew.GetFaction() == 1 ? VoiceListAlly : VoiceListEnemy;
    }

    private float Cooldown;

    public enum PriorityTypes
    {
        SILENCE,
        IDLE,
        NORMAL,
        PRIORITY,
        GUARANTEE
    }

    public float GetSilenceTime(PriorityTypes typ)
    {
        switch (typ)
        {
            case PriorityTypes.SILENCE:
                return 4f;
            case PriorityTypes.IDLE:
                return 2f;
            case PriorityTypes.NORMAL:
                return 0f;
            case PriorityTypes.PRIORITY:
                return -1f;
            case PriorityTypes.GUARANTEE:
                return -10f;
        }
        return 0f;
    }

    public bool HasPriority(PriorityTypes typ)
    {
        return TimeOfSilence() > GetSilenceTime(typ);
    }
    
    public float TimeOfSilence()
    {
        return 0f - Cooldown;
    }
    public void SetCooldown(float fl)
    {
        Cooldown = Mathf.Max(fl, Cooldown);
    }
    public void PlayVCX(Voiceline Voice, float Cooldown)
    {
        CO_SPAWNER.co.SpawnVoice(Voice.VoiceTex, Crew, Voice.Style);
        SetCooldown(Cooldown * 1.2f + 1f);
        foreach (CREW crew in CO.co.GetAlliedCrew(Crew.GetFaction()))
        {
            if (crew.GetVoiceHandler() == null) continue;
            if ((crew.transform.position - transform.position).magnitude > 40) continue;
            crew.GetVoiceHandler().SetCooldown(Cooldown);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayVCXRpc(int VoiceEnum, int VoiceID, float Cooldown)
    {
        List<Voiceline> Voices = GetVoicelist().GetVoicelines((ScriptableVoicelist.VoicelineTypes)VoiceEnum);
        Voiceline Voice = Voices[VoiceID];
        PlayVCX(Voice, Cooldown);
    }
    private void Start()
    {
        Crew = GetComponent<CREW>();
    }
    private void Update()
    {
        if (Cooldown > -20f)
        {
            Cooldown -= CO.co.GetWorldSpeedDelta();
        }
    }
}