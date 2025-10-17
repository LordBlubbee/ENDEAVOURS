using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.PlayerSettings;

public class CO_SPAWNER : NetworkBehaviour
{
    public GamerTag PrefabGamerTag;
    public Sprite DefaultInventorySprite;
    public TOOL PrefabWrenchLogipedes;
    public TOOL PrefabMedkitLogipedes;
    public TOOL PrefabGrappleLogipedes;
    public TOOL PrefabGrappleSilent;
    public ResourceCrate PrefabAmmoCrate;
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

        foreach (CREW Prefab in drifter.StartingCrew)
        {
            SpawnUnitOnShip(Prefab, drifter);
        }

        CO.co.StartGame();
    }

    public void SpawnUnitOnShip(CREW Prefab, DRIFTER drift)
    {
        CREW enem = Instantiate(Prefab, drift.Space.Bridge, Quaternion.identity);
        enem.NetworkObject.Spawn();
        enem.CharacterName.Value = Prefab.CharacterBackground.GetRandomName();
        Color col = Prefab.CharacterBackground.BackgroundColor;
        enem.CharacterNameColor.Value = new Vector3(col.r,col.g,col.b);
        enem.Init();
        //enem.EquipWeaponPrefab(0);
        drift.Interior.AddCrew(enem);
        drift.CrewGroup.Add(enem.GetComponent<AI_UNIT>());
    }
    public void SpawnEnemyGroup(ScriptableEnemyGroup gr, float PowerLevelFactor)
    {
        float Degrees = UnityEngine.Random.Range(0, 360);
        float Radius = gr.SpawnGroupRange;
        float Dist = UnityEngine.Random.Range(gr.SpawnDistanceMin, gr.SpawnDistanceMax);
        AI_GROUP group = Instantiate(PrefabAIGROUP);
        List<AI_UNIT> members = new();

        float WorthPoints = PowerLevelFactor * gr.CrewPowerLevel;
        List<EnemyCrewWithWeight> PossibleSpawns = new();
        ResetWeights();
        int i = 0;
        foreach (EnemyCrewWithWeight weight in gr.SpawnCrewList)
        {
            AddWeights(i, weight.Weight);
            PossibleSpawns.Add(weight);
        }
        Vector3 Offset = UnityEngine.Random.insideUnitCircle.normalized * Dist;
        Vector3 Spawn = CO.co.PlayerMainDrifter.transform.position + Offset;
        while (WorthPoints > 0)
        {
            Vector3 tryPos = Spawn + new Vector3(UnityEngine.Random.Range(-Radius, Radius), UnityEngine.Random.Range(-Radius, Radius));
            EnemyCrewWithWeight enemyType = PossibleSpawns[GetWeight()];
            CREW enem = Instantiate(enemyType.SpawnCrew, tryPos, Quaternion.identity);
            WorthPoints -= enemyType.Worth;
            enem.NetworkObject.Spawn();
            enem.transform.Rotate(Vector3.forward, Degrees + UnityEngine.Random.Range(-30f, 30f));
            enem.Init();
            members.Add(enem.GetComponent<AI_UNIT>());
        }

        group.SetAI(gr.AI_Type, gr.AI_Group, 2, members);
        group.SetAIHome(Spawn);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnWordsRpc(string dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.InitWords(dm, 1f, Color.red);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnDMGRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.InitNumber(dm, 1f, Color.red);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnArmorDMGRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.InitNumber(dm, 1f, Color.yellow);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnHealRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.InitNumber(dm, 1f, Color.green);
    }

    public MapPoint CreateMapPoint(Vector3 pos)
    {
        MapPoint mapPoint = Instantiate(PrefabMapPoint, pos, Quaternion.identity);
        mapPoint.NetworkObject.Spawn();
        return mapPoint;
    }
    /// <summary>
    /// Clears all weights.
    /// </summary>
    /// 
    private Dictionary<int, int> weights = new();
    private int totalWeight = 0;
    public void ResetWeights()
    {
        weights.Clear();
        totalWeight = 0;
    }

    /// <summary>
    /// Adds weight for a given ID. If the ID already exists, its weight is increased.
    /// </summary>
    public void AddWeights(int id, int weight)
    {
        if (weight <= 0) return;
        if (weights.ContainsKey(id))
            weights[id] += weight;
        else
            weights[id] = weight;
        totalWeight += weight;
    }

    /// <summary>
    /// Returns a random ID based on the weights, or -1 if empty.
    /// </summary>
    public int GetWeight()
    {
        if (totalWeight == 0) return -1;
        int rand = UnityEngine.Random.Range(1, totalWeight + 1);
        int cumulative = 0;
        foreach (var pair in weights)
        {
            cumulative += pair.Value;
            if (rand <= cumulative)
                return pair.Key;
        }
        return -1;
    }

    /**/
}
