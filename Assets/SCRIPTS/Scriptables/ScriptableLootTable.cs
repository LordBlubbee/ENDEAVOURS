
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableLootTable", order = 1)]
public class ScriptableLootTable : ScriptableObject
{
    public List<LootItem> GuaranteedLoot = new();
    public List<FactionReputation> ReputationChanges = new();
    public List<CREW> RecruitCrewmembers = new();

    public CREW GetNewCrew()
    {
        if (RecruitCrewmembers.Count == 0) return null;
        return RecruitCrewmembers[UnityEngine.Random.Range(0,RecruitCrewmembers.Count)];
    }

    [Header("RANDOM LOOT")]
    public int TotalLootDrops = 1;
    public List<RandomLootItem> RandomLoot = new();

    [Header("SHOP DROPS")]
    public int MinimumShopDrops = 0;
    public int MaximumShopDrops = 0;
    public List<WeightedShopItem> PossibleShopDrops = new();
}
[Serializable]
public class LootItem
{
    public float Randomness = 0;
    public int Resource_Materials; //Normal factor: 1:4
    public int Resource_Supplies; //Normal factor: 1:4
    public int Resource_Ammunition; //Normal factor: 1:4
    public int Resource_Technology; //Normal factor: 1:1
    public int Resource_Repairs;
    public int Resource_XP; //Normal factor: 1:1
    public ScriptableLootItemList ItemDrop;
}

[Serializable]
public class RandomLootItem : LootItem
{
    public int Weight = 10;
}

[Serializable]
public class WeightedShopItem
{
    public ScriptableShopitem Item;
    public int Weight;
}