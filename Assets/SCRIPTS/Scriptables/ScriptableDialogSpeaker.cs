using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableDialogSpeaker", order = 1)]
public class ScriptableDialogSpeaker : ScriptableObject
{
    //This is a SPEAKER, an animated/speaking entity that does the animation for a piece of text tied to it.
    public Color NameColor;
    public Sprite Portrait;
    public AudioClip[] Voice;
}
