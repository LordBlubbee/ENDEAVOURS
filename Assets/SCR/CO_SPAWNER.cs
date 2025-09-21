using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CO_SPAWNER : NetworkBehaviour
{
    [SerializeField] private List<TOOL> ToolPrefabInstances;
    public Dictionary<ToolType, TOOL> ToolPrefabs = new();
    public GamerTag PrefabGamerTag;
    public enum ToolType
    {
        NONE,
        SPEAR
    }

    public CREW PlayerPrefab;
    //This one saves all the prefabs!
    public static CO_SPAWNER co;

    private void Awake()
    {
        co = this;

        int i = 0;
        foreach (ToolType type in Enum.GetValues(typeof(ToolType)))
        {
            ToolPrefabs.Add(type, ToolPrefabInstances[i]);
            i++;
        }
    }

    [Rpc(SendTo.Server)]
    public void SpawnPlayerShipRpc(int ID)
    {
        if (CO.co.HasShipBeenLaunched.Value) return;
        DRIFTER driftPrefab = UI.ui.ShipSelectionUI.SpawnableShips[ID].Prefab;
        CO.co.HasShipBeenLaunched.Value = true;
        DRIFTER drifter = Instantiate(driftPrefab,Vector3.zero,Quaternion.identity);
        drifter.NetworkObject.Spawn();
        drifter.Init();
        CO.co.PlayerMainDrifter = drifter;
    }
}
