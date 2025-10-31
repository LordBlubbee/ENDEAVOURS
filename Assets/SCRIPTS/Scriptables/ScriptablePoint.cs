using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptablePoint", order = 1)]
public class ScriptablePoint : ScriptableObject
{
    public string GetResourceLink()
    {
        return ResourceLink;
    }
    public string UniqueName = "";
    public string ResourceLink = "";
    public AudioClip InitialSoundtrack;
    public AudioClip CombatSoundtrack;
    [TextArea(3, 10)]
    public string InitialMapData = "";
    public ScriptableDialog InitialDialog;

    [Header("Optional")]
    public ScriptableBiome GateToBiome;
}
