
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
    public List<EnemyCrewWithWeight> GuaranteedCrewList;
    public List<EnemyCrewWithWeight> SpawnCrewList;
    public int DrifterQualityLevel = 100;
    public List<ScriptableEnemyDrifter> SpawnDrifter;

    [Header("SPAWNING GROUP")]
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