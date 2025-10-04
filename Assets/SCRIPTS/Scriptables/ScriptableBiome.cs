using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableBiome", order = 1)]
public class ScriptableBiome : ScriptableObject
{
    public string BiomeName;
    public string BiomeDescription;
    public List<ScriptablePoint> PossiblePointsRandom
    { 
        get
        {
            List<ScriptablePoint> list = new();
            list.AddRange(PossiblePointsCalm);
            list.AddRange(PossiblePointsNeutral);
            list.AddRange(PossiblePointsHostile);
            return list;
        }
    }
    public List<ScriptablePoint> PossiblePointsCalm;
    public List<ScriptablePoint> PossiblePointsNeutral;
    public List<ScriptablePoint> PossiblePointsHostile;
    public List<ScriptablePoint> PossiblePointsArrival;
    public List<ScriptablePoint> PossiblePointsExit;
}
