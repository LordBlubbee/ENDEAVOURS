
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
    public int CrewPowerLevel = 100;
    public List<EnemyCrewWithWeight> SpawnCrewList;
    public int DrifterPowerLevel = 0;
    public List<EnemyDrifterWithWeight> SpawnDrifter;
    public float SpawnGroupRange = 10f; //Area in which the group is spawned
    public float SpawnDistanceMin = 100f; //Distance away from players
    public float SpawnDistanceMax = 100f;
}

[Serializable]
public class EnemyCrewWithWeight
{
    public CREW SpawnCrew;
    public int Weight;
    public int Worth;
}
[Serializable]
public class EnemyDrifterWithWeight
{
    public DRIFTER SpawnDrifter;
    public int Weight;
}