using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableDialogSpeaker", order = 1)]
public class ScriptableDialogSpeaker : ScriptableObject
{
    public string Name;
    public Color NameColor;
    public Sprite Portrait;
    public AudioClip Voice;
}
