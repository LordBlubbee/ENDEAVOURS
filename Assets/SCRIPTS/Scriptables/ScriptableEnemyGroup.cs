
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableEnemyGroup", order = 1)]
public class ScriptableEnemyGroup : ScriptableObject
{
    //This spawns a group of enemies
    public AI_GROUP.AI_TYPES AI_Type;
    public AI_GROUP.AI_OBJECTIVES AI_Group;

    [Header("SPAWN TARGETS")]
    public int CrewAmountLevel = 100;
    public int CrewQualityLevel = 100;
    public List<ScriptableEnemyCrew> GuaranteedCrewList;
    public List<EnemyCrewWithWeight> SpawnCrewList;

    [Header("DRIFTER")]
    public int DrifterQualityLevel = 100;
    public List<ScriptableEnemyDrifter> SpawnDrifter;

    [Header("DUNGEON")]
    public int DungeonQualityLevel = 100;
    public DUNGEON SpawnDungeon;

    [Header("SPAWNING GROUP")]
 
    public int SpawnGroupAmountMin = 1;
    public int SpawnGroupAmountMax = 1; 
    public int GetSpawnGroupAmount()
    {
        return UnityEngine.Random.Range(SpawnGroupAmountMin, SpawnGroupAmountMax);
    }
    public float SpawnGroupRange = 10f; //Area in which the group is spawned
    public float SpawnDistanceMin = 100f; //Distance away from players
    public float SpawnDistanceMax = 100f;
}
[Serializable]
public class EnemyCrewWithWeight
{

    public int Weight = 20;
    public int WeightIncreaseOverQualityGradient;
    public int MinimumQuality;
    public int MaximumQuality;

    public int GetWeight(float Quality)
    {
        float we = Weight;
        if (Quality > MaximumQuality)
        {
            we += WeightIncreaseOverQualityGradient;
        }
        else if (Quality > MinimumQuality)
        {
            we += WeightIncreaseOverQualityGradient * (Quality - (float)MinimumQuality) / ((float)MaximumQuality - (float)MinimumQuality);
        }
        return Mathf.RoundToInt(we);
    }
    public ScriptableEnemyCrew Crew;
}