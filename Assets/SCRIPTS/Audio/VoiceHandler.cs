using System;
using System.Collections.Generic;
using UnityEngine;

public class VoiceHandler : MonoBehaviour
{
    private CREW Crew;
    public Color SpeechColor = Color.white;
    public AudioClip SpeechVoice;
    public ScriptableVoicelist VoiceList;

    private float Cooldown;
    
    public float TimeOfSilence()
    {
        return 0f - Cooldown;
    }
    public void SetCooldown(float fl)
    {
        Cooldown = Mathf.Max(fl, Cooldown);
    }
    public void PlayVCX(List<Voiceline> Voices, float Cooldown)
    {
        Voiceline Voice = Voices[UnityEngine.Random.Range(0, Voices.Count)];
        CO_SPAWNER.co.SpawnVoice(Voice.VoiceTex, Crew, Voice.Style);
        SetCooldown(Cooldown + 1f);
        foreach (CREW crew in CO.co.GetAlliedCrew(Crew.GetFaction()))
        {
            crew.GetVoiceHandler().SetCooldown(Cooldown);
        }
    }
    private void Start()
    {
        Crew = GetComponent<CREW>();
    }
    private void Update()
    {
        if (Cooldown > -10f)
        {
            Cooldown -= CO.co.GetWorldSpeedDelta();
        }
    }
}