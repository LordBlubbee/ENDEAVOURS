
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableEnemyGroup", order = 1)]
public class ScriptableEnemyGroup : ScriptableObject
{
    //This spawns a group of enemies
    public List<EnemyCrewWithWeight> SpawnCrewList;
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
}
[Serializable]
public class EnemyDrifterWithWeight
{
    public DRIFTER SpawnDrifter;
    public int Weight;
}