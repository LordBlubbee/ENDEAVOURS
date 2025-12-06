using UnityEngine;
using static AUDCO;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptablePoint", order = 1)]
public class ScriptablePoint : ScriptableObject
{
    public string GetResourceLink()
    {
        return $"{ResourceLink}/{name}";
    }
    public string UniqueName = "";
    public string ResourceLink = "";
    public Soundtrack InitialSoundtrack = Soundtrack.WASTES;
    public Soundtrack CombatSoundtrack = Soundtrack.WASTES;
    [TextArea(3, 10)]
    public string InitialMapData = "";
    public ScriptableDialog InitialDialog;
    public CO_SPAWNER.BackgroundType BackgroundType = CO_SPAWNER.BackgroundType.RANDOM_ROCK;

    [Header("Optional")]
    public ScriptableBiome GateToBiome;
}
