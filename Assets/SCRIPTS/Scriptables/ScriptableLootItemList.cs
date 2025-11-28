
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableLootItemList", order = 1)]
public class ScriptableLootItemList : ScriptableObject
{
    public List<WeightedLootItem> GetPossibleDrops()
    {
        switch (CO.co.BiomeProgress)
        {
            case > 4:
                return RareDrops;
            case > 2:
                return UncommonDrops;
            default:
                return CommonDrops;
        }
    }
    [Header("Region 0-1")]
    public List<WeightedLootItem> CommonDrops = new();
    [Header("Region 2-3")]
    public List<WeightedLootItem> UncommonDrops = new();
    [Header("Region 4-7")]
    public List<WeightedLootItem> RareDrops = new();
}

[Serializable]
public class WeightedLootItem
{
    public ScriptableEquippable Item;
    public int Weight;
}