
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableShopItemList", order = 1)]
public class ScriptableShopItemList : ScriptableObject
{
    public List<ScriptableShopitem> GetPossibleDrops()
    {
        switch (CO.co.GetBiomeProgress())
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
    public List<ScriptableShopitem> CommonDrops = new();
    [Header("Region 2-3")]
    public List<ScriptableShopitem> UncommonDrops = new();
    [Header("Region 4-7")]
    public List<ScriptableShopitem> RareDrops = new();
}