
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableEnemyGroup", order = 1)]
public class ScriptableEnemyGroup : ScriptableObject
{
    //This spawns a group of enemies
    public List<CREW> SpawnCrew;
    public DRIFTER SpawnDrifter;
    public float SpawnGroupRange = 10f; //Area in which the group is spawned
    public float SpawnDistanceMin = 100f; //Distance away from players
    public float SpawnDistanceMax = 100f;
}
