using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CO_SPAWNER : NetworkBehaviour
{
    public GamerTag PrefabGamerTag;
    public Sprite DefaultInventorySprite;
    public TOOL PrefabWrenchLogipedes;
    public TOOL PrefabMedkitLogipedes;
    public TOOL PrefabGrappleLogipedes;
    public TOOL PrefabGrappleSilent;
    public enum DefaultEquipmentSet
    {
        NONE,
        LOGIPEDES
    }

    public TOOL GetPrefabWrench(DefaultEquipmentSet set)
    {
        switch (set)
        {
            default:
                return PrefabWrenchLogipedes;
        }
    }
    public TOOL GetPrefabMedkit(DefaultEquipmentSet set)
    {
        switch (set)
        {
            default:
                return PrefabMedkitLogipedes;
        }
    }
    public TOOL GetPrefabGrapple(DefaultEquipmentSet set)
    {
        switch (set)
        {
            case DefaultEquipmentSet.NONE:
                return PrefabGrappleSilent;
            default:
                return PrefabGrappleLogipedes;
        }
    }


    public DMG PrefabDMG;
    public MapPoint PrefabMapPoint;
    public CREW PrefabLooncrab;
    public AI_GROUP PrefabAIGROUP;

    public CREW PlayerPrefab;
    //This one saves all the prefabs!
    public static CO_SPAWNER co;

    private void Start()
    {
        co = this;

    }

    private void Awake()
    {
        co = this;

        int i = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SpawnLooncrabsAggressive(new Vector3(0, 150), 3);
        }
    }

    [Rpc(SendTo.Server)]
    public void SpawnPlayerShipRpc(int ID)
    {
        if (CO.co.HasShipBeenLaunched.Value) return;
        CO.co.HasShipBeenLaunched.Value = true;

        DRIFTER driftPrefab = UI.ui.ShipSelectionUI.SpawnableShips[ID].Prefab;
        DRIFTER drifter = Instantiate(driftPrefab,Vector3.zero,Quaternion.identity);
        drifter.NetworkObject.Spawn();
        drifter.Init();
        CO.co.PlayerMainDrifter = drifter;

        CO.co.StartGame();
    }
    public void SpawnLooncrabsAggressive(Vector3 pos, int Amount)
    {
        float Degrees = UnityEngine.Random.Range(0, 360);   
        float Radius = 5f;
        AI_GROUP group = Instantiate(PrefabAIGROUP);
        List<AI_UNIT> members = new();
        for (int i = 0; i < Amount; i++)
        {
            Vector3 tryPos = pos + new Vector3(UnityEngine.Random.Range(-Radius, Radius), UnityEngine.Random.Range(-Radius, Radius));
            CREW looncrab = Instantiate(PrefabLooncrab, tryPos, Quaternion.identity);
            looncrab.NetworkObject.Spawn();
            looncrab.transform.Rotate(Vector3.forward, Degrees + UnityEngine.Random.Range(-30f, 30f));
            looncrab.Init();
            members.Add(looncrab.GetComponent<AI_UNIT>());
        }
        group.SetAI(AI_GROUP.AI_TYPES.LOONCRAB_SWARM,AI_GROUP.AI_OBJECTIVES.BOARD, members);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnDMGRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.InitDamage(dm, 1f);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnHealRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.InitHeal(dm, 1f);
    }

    public MapPoint CreateMapPoint(Vector3 pos)
    {
        MapPoint mapPoint = Instantiate(PrefabMapPoint, pos, Quaternion.identity);
        mapPoint.NetworkObject.Spawn();
        return mapPoint;
    }
}
