using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableBiome", order = 1)]
public class ScriptableBiome : ScriptableObject
{
    public string BiomeName;
    public string BiomeDescription;

    public float BiomeBaseDifficulty = 1f;
    public float BiomeCalmRatio = 0.2f;
    public float BiomeHostileRatio = 0.4f;
    public int BiomeSize;

    public int GetBiomeSize()
    {
        return BiomeSize;
    }
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

    public List<ScriptablePoint> PossiblePointsRest;
    public List<ScriptablePoint> PossiblePointsArrival;
}
