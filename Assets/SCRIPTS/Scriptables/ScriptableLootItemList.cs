
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableLootItemList", order = 1)]
public class ScriptableLootItemList : ScriptableObject
{
    public List<WeightedLootItem> PossibleDrops = new();
}

[Serializable]
public class WeightedLootItem
{
    public ScriptableEquippable Item;
    public int Weight;
}