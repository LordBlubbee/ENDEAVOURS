using System;
using UnityEngine;

public class VoiceHandler : MonoBehaviour
{
    private CREW Crew;
    public Color SpeechColor = Color.white;
    public AudioClip SpeechVoice;
    private void Start()
    {
        Crew = GetComponent<CREW>();
    }
}

[Serializable]
public class Voiceline
{
    [TextArea(2, 5)]
    public string VoiceTex;
    public VCX.VoiceStyles Style;
}