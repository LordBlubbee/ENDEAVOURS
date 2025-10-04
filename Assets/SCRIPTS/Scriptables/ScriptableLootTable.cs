
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableLootTable", order = 1)]
public class ScriptableLootTable : ScriptableObject
{
    public List<LootItem> GuaranteedLoot = new();

    [Header("RANDOM LOOT")]
    public int TotalLootDrops = 1;
    public List<RandomLootItem> RandomLoot = new();
}
[Serializable]
public class LootItem
{
    public float Exactness = 1;
    public int Resource_Materials; //Normal factor: 1:4
    public int Resource_Supplies; //Normal factor: 1:4
    public int Resource_Ammunition; //Normal factor: 1:4
    public int Resource_Technology; //Normal factor: 1:1
}

[Serializable]
public class RandomLootItem : LootItem
{
    public int Weight = 10;
}

