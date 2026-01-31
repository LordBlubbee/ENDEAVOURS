
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class DUNGEON : NetworkBehaviour
{
    public SPACE Space;
    public AI_GROUP AI;
    public LayerMask DrifterLayer;
    private void Start()
    {
        Init();
    }
    bool hasInitialized = false;
    private NetworkVariable<FixedString32Bytes> DungeonVariantData = new();
    public List<DungeonAlteration> Alterations = new List<DungeonAlteration>();
    public List<WalkableTile> MainObjectivePossibilities;
    public List<WalkableTile> MiscPossibilities;
    public List<NetworkObject> BackgroundObjectPossibilities;
    public float TrapSpawnDefault = 600;
    public List<ObstacleTypes> ObstaclePossibilityList;

    public enum ObstacleTypes
    {
        NONE,
        EXPLOSIVE_CRATE,
        DIFFICULT_TERRAIN,
        LOONCRAB_EGG,
        LEAKING_MIST
    }

    [Serializable]
    public struct DungeonAlteration
    {
        [TextArea(2,5)]
        public string Title;
        public List<DungeonVariants> Variants;
    }
    [Serializable] public struct DungeonVariants
    {
        public List<WalkableTile> WalkablesRemoved;
    }
    public void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        StartCoroutine(CreateSoundwaves());
        if (IsServer)
        {
            AI.SetAIHome(this);
            float Dis = 40f;
            bool Distancing = true;
            while (Distancing)
            {
                Dis += 2f;
                if (Dis > 100) break;
                transform.position = CO.co.PlayerMainDrifter.transform.position + (CO.co.PlayerMainDrifter.getLeftVector()).normalized * Dis;
                Distancing = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position,16f, DrifterLayer))
                {
                    if (col.GetComponent<DRIFTER>() != null)
                    {
                        Distancing = true;
                        break;
                    }
                }
            }
            float ang = Vector2.SignedAngle(new Vector3(1,0), CO.co.PlayerMainDrifter.transform.position - transform.position);
            transform.Rotate(Vector3.forward, ang);
        }
        GenerateDungeon();
        Space.Init();
    }
    IEnumerator CreateSoundwaves() //Runs on client
    {
        float Timer = 0f;
        while (true)
        {
            Timer -= CO.co.GetWorldSpeedDelta();
            if (Timer < 0f)
            {
                foreach (CREW Crew in Space.GetCrew())
                {
                    float Dis = (LOCALCO.local.GetPlayer().transform.position - Crew.transform.position).magnitude;
                    if (UnityEngine.Random.Range(0f, 1f) < 0.7f && Dis < 45 && Crew.GetCurrentSpeed() > 0.1f)
                    {
                        CO_SPAWNER.co.SpawnSoundwave(Crew.transform.position, 1.5f - Dis/45f);
                    }
                }
                Timer = UnityEngine.Random.Range(1f, 4f);
            }
            yield return null;
        }
    }
    private void GenerateDungeon()
    {
        //Dungeon is preset, but has different components that can be taken out/removed
        string Data = DungeonVariantData.Value.ToString();
        Debug.Log($"Receiving dungeon data {Data}");
        int i = 0;
        foreach (DungeonAlteration alt in Alterations)
        {
            char c = Data[i];
            int ID = c - '0';
            Debug.Log($"Digit {i} equals {ID}");
            if (ID == 0)
            {
                Debug.Log("Error: Variant 0");
                return;
            }
            //Remove different extra rooms
            DungeonVariants ChosenVar = alt.Variants[ID-1];
            foreach (WalkableTile tile in ChosenVar.WalkablesRemoved)
            {
                if (tile == null) continue;
                Space.RoomTiles.Remove(tile);
                MainObjectivePossibilities.Remove(tile);
                MiscPossibilities.Remove(tile);
                tile.gameObject.SetActive(false);
            }
            i++;
        }
        foreach (WalkableTile tile in Space.RoomTiles)
        {
            for (int i2 = 0; i2 < UnityEngine.Random.Range(0,3);i2++)
            {
                NetworkObject obj = Instantiate(BackgroundObjectPossibilities[UnityEngine.Random.Range(0, BackgroundObjectPossibilities.Count)], tile.transform.position + GetRandomOnTile(), Quaternion.identity);
                obj.transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0, 4)*90);
                obj.Spawn();
                obj.transform.SetParent(Space.transform);
                DungeonNetworkObjects.Add(obj);
            }
        }
    }

    [Header("Spawned Object Prefabs")]
    public RespawnArea PrefabSpawningArea;

    private Vector3 GetRandomOnTile(float off = 2)
    {
        return new Vector3(UnityEngine.Random.Range(-off, off), UnityEngine.Random.Range(-off, off));
    }

    [NonSerialized] public List<RespawnArea> SpawnedRespawnAreas = new();

    public int AverageMaterialCrateWorth = 20;
    public int AverageSuppliesCrateWorth = 40;
    public int AverageAmmoCrateWorth;
    public int AverageTechCrateWorth;
    public float CrateDensity = 0.1f;
    public void GenerateCrates(float LootLevel = 1f, float DensityLevel = 1f)
    {
        int MaterialWorth = Mathf.RoundToInt(AverageMaterialCrateWorth * LootLevel * CO.co.GetEncounterLootModifier() * UnityEngine.Random.Range(0.8f,1.2f));
        int SuppliesWorth = Mathf.RoundToInt(AverageSuppliesCrateWorth * LootLevel * CO.co.GetEncounterLootModifier() * UnityEngine.Random.Range(0.8f, 1.2f));
        int AmmoWorth = Mathf.RoundToInt(AverageAmmoCrateWorth * LootLevel * CO.co.GetEncounterLootModifier() * UnityEngine.Random.Range(0.8f, 1.2f));
        int TechWorth = Mathf.RoundToInt(AverageTechCrateWorth * LootLevel * CO.co.GetEncounterLootModifier() * UnityEngine.Random.Range(0.8f, 1.2f));
        float Density = CrateDensity * UnityEngine.Random.Range(0.8f, 1.2f) * DensityLevel;
        int Crates = Mathf.CeilToInt(Density * MiscPossibilities.Count);
        for (int i = 0; i < Mathf.Max(Crates,4); i++)
        {
            ResourceCrate.ResourceTypes Typ = ResourceCrate.ResourceTypes.NONE;
            if (SuppliesWorth > 0) Typ = ResourceCrate.ResourceTypes.SUPPLIES;
            else if (MaterialWorth > 0) Typ = ResourceCrate.ResourceTypes.MATERIALS;
            else if (AmmoWorth > 0) Typ = ResourceCrate.ResourceTypes.AMMUNITION;
            else if (TechWorth > 0) Typ = ResourceCrate.ResourceTypes.TECHNOLOGY;

            Vector3 vec = GetUtilitySpawn();
            for (int i2 = 0; i2 < UnityEngine.Random.Range(1, 6); i2++)
            {
                int Num = 0;
                switch (Typ)
                {
                    case ResourceCrate.ResourceTypes.MATERIALS:
                        Num = Mathf.Min(MaterialWorth, UnityEngine.Random.Range(3, 7));
                        MaterialWorth -= Num;
                        break;
                    case ResourceCrate.ResourceTypes.SUPPLIES:
                        Num = Mathf.Min(SuppliesWorth, UnityEngine.Random.Range(3, 7));
                        SuppliesWorth -= Num;
                        break;
                    case ResourceCrate.ResourceTypes.AMMUNITION:
                        Num = Mathf.Min(AmmoWorth, UnityEngine.Random.Range(3, 7));
                        AmmoWorth -= Num;
                        break;
                    case ResourceCrate.ResourceTypes.TECHNOLOGY:
                        Num = Mathf.Min(TechWorth, UnityEngine.Random.Range(2, 5));
                        TechWorth -= Num;
                        break;
                }
                ResourceCrate a;
                if (UnityEngine.Random.value < 0.5f)
                {
                    switch (Typ)
                    {
                        case ResourceCrate.ResourceTypes.MATERIALS:
                            a = CO_SPAWNER.co.SpawnMatCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num);
                            DungeonNetworkObjects.Add(a.NetworkObject);
                            continue;
                        case ResourceCrate.ResourceTypes.SUPPLIES:
                            a = CO_SPAWNER.co.SpawnSupCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num);
                            DungeonNetworkObjects.Add(a.NetworkObject);
                            continue;
                        case ResourceCrate.ResourceTypes.AMMUNITION:
                            a = CO_SPAWNER.co.SpawnAmmoCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num);
                            DungeonNetworkObjects.Add(a.NetworkObject);
                            continue;
                        case ResourceCrate.ResourceTypes.TECHNOLOGY:
                            a = CO_SPAWNER.co.SpawnTechCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num);
                            DungeonNetworkObjects.Add(a.NetworkObject);
                            continue;
                    }
                }
                a = CO_SPAWNER.co.SpawnCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num, Typ);
                DungeonNetworkObjects.Add(a.NetworkObject);
            }
        }
    }

    public void GenerateTraps(int TrapPoints)
    {
        while (TrapPoints > 0)
        {
            ObstacleTypes Typ = ObstaclePossibilityList[UnityEngine.Random.Range(0,ObstaclePossibilityList.Count)];
            Vector3 Spawn = GetRandomSpawn();
            switch (Typ)
            {
                case ObstacleTypes.EXPLOSIVE_CRATE:
                    {
                        TrapPoints -= 20;
                        for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++)
                        {
                            TrapPoints -= 15;
                            ResourceCrate a = CO_SPAWNER.co.SpawnExplosiveCrate(Space, Spawn + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(10f, 30f) * CO.co.GetEncounterDifficultyModifier()));
                            DungeonNetworkObjects.Add(a.NetworkObject);
                        }
                        for (int i = 0; i < UnityEngine.Random.Range(0,3); i++)
                        {
                            ResourceCrate a = CO_SPAWNER.co.SpawnCrate(Space, Spawn + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 60f) * CO.co.GetEncounterDifficultyModifier()), 0, ResourceCrate.ResourceTypes.NONE);
                            DungeonNetworkObjects.Add(a.NetworkObject);
                        }
                        break;
                    }
                case ObstacleTypes.LOONCRAB_EGG:
                    {
                        for (int i = 0; i < UnityEngine.Random.Range(2, 6); i++)
                        {
                            TrapPoints -= 10;
                            LooncrabEgg a = CO_SPAWNER.co.SpawnLooncrabEgg(Space, this, Spawn + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(40f, 70f) * CO.co.GetEncounterDifficultyModifier()), Mathf.RoundToInt(100f * CO.co.GetEncounterDifficultyModifier()));
                            DungeonNetworkObjects.Add(a.NetworkObject);
                        }
                        break;
                    }
                case ObstacleTypes.DIFFICULT_TERRAIN:
                    {
                        DamagingZone zon = CO_SPAWNER.co.SpawnDifficultTerrain(Space, Spawn + GetRandomOnTile());
                        DungeonNetworkObjects.Add(zon.NetworkObject);
                        TrapPoints -= 25;
                        break;
                    }
                case ObstacleTypes.LEAKING_MIST:
                    {
                        DamagingZone zon = CO_SPAWNER.co.SpawnLeakingMist(Space, Spawn + GetRandomOnTile());
                        DungeonNetworkObjects.Add(zon.NetworkObject);
                        TrapPoints -= 35;
                        break;
                    }
            }
        }
    }
    public void GenerateSpawningMounds(int RespawnMounds)
    {
        RespawnArea a = Instantiate(PrefabSpawningArea, GetUtilitySpawn(), Quaternion.identity);
        a.NetworkObject.Spawn();
        a.SetSpace(Space);
        DungeonNetworkObjects.Add(a.NetworkObject);
        SpawnedRespawnAreas.Add(a);
    }

    public Vector3 GetUtilitySpawn()
    {
        WalkableTile tile = MiscPossibilities[UnityEngine.Random.Range(0, MiscPossibilities.Count)];
        MiscPossibilities.Remove(tile);
        return tile.transform.position;
    }
    public Vector3 GetRandomSpawn()
    {
        WalkableTile tile = Space.RoomTiles[UnityEngine.Random.Range(0, Space.RoomTiles.Count)];
        return tile.transform.position;
    }
    public void Impact(PROJ fl, Vector3 ImpactArea)
    {
        float Damage = fl.AttackDamage;
        foreach (CREW mod in Space.GetCrew())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (Dist < fl.CrewDamageSplash)
            {
                mod.TakeDamage(Damage * fl.CrewDamageModifier, mod.transform.position, iDamageable.DamageType.TRUE);
            }
        }
    }
    public void SetDungeonVariant()
    {
        string str = "";
        foreach (DungeonAlteration alter in Alterations)
        {
            int ID = UnityEngine.Random.Range(0, alter.Variants.Count) + 1;
            str += ID.ToString();
        }
        DungeonVariantData.Value = str;
        Debug.Log($"Setting dungeon data {DungeonVariantData.Value}");
    }

    [NonSerialized] public List<NetworkObject> DungeonNetworkObjects = new();
    public void DespawnAndUnregister()
    {
        foreach (NetworkObject networkObject in DungeonNetworkObjects)
        {
            if (networkObject == null) continue;
            if (networkObject.IsSpawned) networkObject.Despawn();
            else
            {
                Debug.Log($"Error: Object {networkObject.gameObject.name} was not despawned");
                Destroy(networkObject.gameObject);
            }
        }
        Debug.Log("Despawn and unregister");
        CO.co.UnregisterSpace(Space);
        NetworkObject.Despawn();
    }
}