using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
    public Sprite GrappleCursor;
    public ResourceCrate PrefabAmmoCrate;

    [Header("VFX")]
    public PART ExplosionSmall;
    public PART ExplosionMedium;
    public PART ExplosionLarge;
    public GameObject EmberSmall;
    public GameObject EmberLarge;
    public GameObject SparkSmall;
    public GameObject SparkMedium;
    public GameObject ImpactSparks;
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
        drifter.IsMainDrifter.Value = true;
        drifter.NetworkObject.Spawn();
        drifter.Faction.Value = 1;
        drifter.Init();
        CO.co.PlayerMainDrifter = drifter;

        foreach (CREW Prefab in drifter.StartingCrew)
        {
            SpawnUnitOnShip(Prefab, drifter);
        }

        CO.co.StartGame();
    }
    public DRIFTER SpawnOtherShip(DRIFTER driftPrefab, Vector3 pos, int Faction = 2)
    {
        DRIFTER drifter = Instantiate(driftPrefab, pos, Quaternion.identity);
        drifter.NetworkObject.Spawn();
        drifter.Faction.Value = Faction;
        drifter.Init();
        drifter.transform.rotation = CO.co.PlayerMainDrifter.transform.rotation;
        drifter.SetMoveInput(drifter.getLookVector());
        drifter.SetLookDirection(CO.co.PlayerMainDrifter.getLookVector());
        foreach (CREW Prefab in drifter.StartingCrew)
        {
            SpawnUnitOnShip(Prefab, drifter);
        }
        return drifter;
    }
    public CREW SpawnUnitOnShip(CREW Prefab, DRIFTER drift)
    {
        CREW enem = Instantiate(Prefab, drift.Space.transform.TransformPoint(drift.Space.Bridge), Quaternion.identity);
        enem.NetworkObject.Spawn();
        enem.Faction.Value = drift.GetFaction();
        enem.CharacterName.Value = Prefab.CharacterBackground.GetRandomName();
        Color col = Prefab.CharacterBackground.BackgroundColor;
        enem.CharacterNameColor.Value = new Vector3(col.r,col.g,col.b);
        enem.EquipWeapon1Rpc();
        enem.Init();
        drift.Interior.AddCrew(enem);
        drift.CrewGroup.Add(enem.GetComponent<AI_UNIT>());
        return enem;
    }
    public DRIFTER SpawnEnemyGroup(ScriptableEnemyGroup gr, float PowerLevelFactor, int Faction = 2)
    {
        float Degrees = UnityEngine.Random.Range(0, 360);
        float Radius = gr.SpawnGroupRange;
        float Dist = UnityEngine.Random.Range(gr.SpawnDistanceMin, gr.SpawnDistanceMax);
        int i;
        Vector3 Offset = UnityEngine.Random.insideUnitCircle.normalized * Dist;
        Vector3 Spawn = CO.co.PlayerMainDrifter.transform.position + Offset;
        List<EnemyCrewWithWeight> PossibleSpawns = new();
        float WorthPoints = PowerLevelFactor * gr.CrewPowerLevel;
        AI_GROUP group;
        List<AI_UNIT> members = new();
        if (gr.SpawnDrifter == null)
        {
            group = Instantiate(PrefabAIGROUP);

            ResetWeights();
            i = 0;
            foreach (EnemyCrewWithWeight weight in gr.SpawnCrewList)
            {
                AddWeights(i, weight.Weight);
                PossibleSpawns.Add(weight);
                i++;
            }
          
            while (WorthPoints > 0)
            {
                Vector3 tryPos = Spawn + new Vector3(UnityEngine.Random.Range(-Radius, Radius), UnityEngine.Random.Range(-Radius, Radius));
                EnemyCrewWithWeight enemyType = PossibleSpawns[GetWeight()];
                CREW enem = Instantiate(enemyType.SpawnCrew, tryPos, Quaternion.identity);
                WorthPoints -= enemyType.Worth;
                enem.NetworkObject.Spawn();
                enem.Faction.Value = Faction;
                enem.transform.Rotate(Vector3.forward, Degrees + UnityEngine.Random.Range(-30f, 30f));
                enem.Init();
                members.Add(enem.GetComponent<AI_UNIT>());
            }

            group.SetAI(gr.AI_Type, gr.AI_Group, 2, members);
            group.SetAIHome(Spawn);
            return null;
        }
        ResetWeights();
        i = 0;
        List<EnemyDrifterWithWeight> PossibleDrifterSpawns = new();
        foreach (EnemyDrifterWithWeight weight in gr.SpawnDrifter)
        {
            AddWeights(i, weight.Weight);
            PossibleDrifterSpawns.Add(weight);
            i++;
        }
        EnemyDrifterWithWeight drifterType = PossibleDrifterSpawns[GetWeight()];
        Offset = UnityEngine.Random.insideUnitCircle.normalized * Dist * 2.5f;
        Spawn = CO.co.PlayerMainDrifter.transform.position + Offset;
        DRIFTER drifter = SpawnOtherShip(drifterType.SpawnDrifter, Spawn, Faction);
        Offset = UnityEngine.Random.insideUnitCircle.normalized * Dist;
        Spawn = CO.co.PlayerMainDrifter.transform.position + Offset;
        drifter.CurrentLocationPoint = Spawn;
        ResetWeights();
        i = 0;
        foreach (EnemyCrewWithWeight weight in gr.SpawnCrewList)
        {
            AddWeights(i, weight.Weight);
            PossibleSpawns.Add(weight);
            i++;
        }
        while (WorthPoints > 0)
        {
            Vector3 tryPos = drifter.Interior.GetRandomGrid().transform.position;
            EnemyCrewWithWeight enemyType = PossibleSpawns[GetWeight()];
            CREW crew = SpawnUnitOnShip(enemyType.SpawnCrew, drifter);
            WorthPoints -= enemyType.Worth;
        }
        return drifter;
    }

    /*VFX*/

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnExplosionSmallRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.8f, 1.2f);
        float Fade = UnityEngine.Random.Range(0.8f, 1.2f);
        PART part = Instantiate(ExplosionSmall, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
        part.FadeChange *= Fade;

        float ExplosionPower = 10f - CAM.cam.Dis(pos) * 0.15f;
        if (ExplosionPower > 2) CAM.cam.ShakeCamera(ExplosionPower * 0.7f);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnExplosionMediumRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.8f, 1.2f);
        float Fade = UnityEngine.Random.Range(0.8f, 1.2f);
        PART part = Instantiate(ExplosionMedium, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
        part.FadeChange *= Fade;

        float ExplosionPower = 11f - CAM.cam.Dis(pos) * 0.14f;
        if (ExplosionPower > 2) CAM.cam.ShakeCamera(ExplosionPower* 0.8f);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnExplosionLargeRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.8f, 1.2f);
        float Fade = UnityEngine.Random.Range(0.8f, 1.2f);
        PART part = Instantiate(ExplosionLarge, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
        part.FadeChange *= Fade;

        float ExplosionPower = 12f - CAM.cam.Dis(pos)*0.12f;
        if (ExplosionPower > 2) CAM.cam.ShakeCamera(ExplosionPower);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnImpactRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.8f, 1.2f);
        GameObject part = Instantiate(ImpactSparks, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnEmberSmallRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.8f, 1.2f);
        GameObject part = Instantiate(EmberSmall, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnEmberLargeRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.8f, 1.2f);
        GameObject part = Instantiate(EmberLarge, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnWordsRpc(string dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        dmg.InitWords(dm, 0.7f, Color.red);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnDMGRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        dmg.InitNumber(dm, 1f, Color.red);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnArmorDMGRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        dmg.InitNumber(dm, 1f, Color.yellow);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnHealRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.transform.SetParent(CO.co.GetTransformAtPoint(pos));
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
