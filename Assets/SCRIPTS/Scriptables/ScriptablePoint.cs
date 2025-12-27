using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    public CO.MapWeatherSelectors WeatherSelectors = CO.MapWeatherSelectors.RANDOM_MILD;
    public ScriptableDialog GetInitialDialog()
    {
        foreach (AlternativeInitialDialog alternativeDialog in AlternativeDialogs)
        {
            if (alternativeDialog.ArePrerequisitesMet()) return alternativeDialog.ReplaceDialog;
        }
        return InitialDialog;
    }

    [Header("Optional")]
    public ScriptableBiome GateToBiome;
    public List<AlternativeInitialDialog> AlternativeDialogs = new();
}

[Serializable]
public class AlternativeInitialDialog //This dialog will be played only if its prerequisites are met.
{
    public ScriptableDialog ReplaceDialog;
    public ScriptablePrerequisite[] Prerequisites;
    public bool ArePrerequisitesMet()
    {
        foreach (var prerequisite in Prerequisites)
        {
            if (!prerequisite.IsTrue())
            {
                return false;
            }
        }
        return true;
    }
}