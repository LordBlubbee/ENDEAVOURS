
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableEnemySpawner", order = 1)]
public class ScriptableEnemySpawner : ScriptableObject
{
    //List of groups that can be spawned...
    public List<EnemyGroupWithWeight> SpawnEnemyGroupList;
}

[Serializable]
public class EnemyGroupWithWeight
{
    public ScriptableEnemyGroup EnemyGroup;
    public int Weight;
}