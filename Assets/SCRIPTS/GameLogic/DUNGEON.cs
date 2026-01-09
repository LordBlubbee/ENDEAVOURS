using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static iDamageable;

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

    [Serializable]
    public struct DungeonAlteration
    {
        public List<DungeonVariants> Variants;
    }
    [Serializable] public struct DungeonVariants
    {
        public List<WalkableTile> WalkablesRemoved;
        public List<SpriteRenderer> SpritesRemoved;
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
            foreach (SpriteRenderer tile in ChosenVar.SpritesRemoved)
            {
                if (tile == null) continue;
                tile.gameObject.SetActive(false);
            }
            i++;
        }
    }

    [Header("Spawned Object Prefabs")]
    public RespawnArea PrefabSpawningArea;

    private Vector3 GetRandomOnTile(float off = 2)
    {
        return new Vector3(UnityEngine.Random.Range(-off, off), UnityEngine.Random.Range(-off, off));
    }

    [NonSerialized] public List<RespawnArea> SpawnedRespawnAreas = new();
    public void GenerateCrates(int MaterialWorth, int SuppliesWorth, int AmmoWorth, int TechWorth, float Density)
    {
        int Crates = Mathf.CeilToInt(Density * MiscPossibilities.Count);
        for (int i = 0; i < Mathf.Min(Crates,4); i++)
        {
            Vector3 vec = GetRandomOnTile();
            ResourceCrate.ResourceTypes Typ = ResourceCrate.ResourceTypes.NONE;
            if (SuppliesWorth > 0) Typ = ResourceCrate.ResourceTypes.SUPPLIES;
            else if (MaterialWorth > 0) Typ = ResourceCrate.ResourceTypes.MATERIALS;
            else if (AmmoWorth > 0) Typ = ResourceCrate.ResourceTypes.AMMUNITION;
            else if (TechWorth > 0) Typ = ResourceCrate.ResourceTypes.TECHNOLOGY;

            for (int i2 = 0; i2 < UnityEngine.Random.Range(1, 4); i2++)
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
                            break;
                        case ResourceCrate.ResourceTypes.SUPPLIES:
                            a = CO_SPAWNER.co.SpawnSupCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num);
                            break;
                        case ResourceCrate.ResourceTypes.AMMUNITION:
                            a = CO_SPAWNER.co.SpawnAmmoCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num);
                            break;
                        case ResourceCrate.ResourceTypes.TECHNOLOGY:
                            a = CO_SPAWNER.co.SpawnTechCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num);
                            break;
                    }
                }
                a = CO_SPAWNER.co.SpawnCrate(Space, vec + GetRandomOnTile(), Mathf.RoundToInt(UnityEngine.Random.Range(30f, 70f) * CO.co.GetEncounterDifficultyModifier()), Num, Typ);
                a.NetworkObject.Spawn();
                DungeonNetworkObjects.Add(a.NetworkObject);
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
    public void Impact(PROJ fl, Vector3 ImpactArea)
    {
        float Damage = fl.AttackDamage;
        foreach (CREW mod in Space.GetCrew())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (Dist < fl.CrewDamageSplash)
            {
                mod.TakeDamage(Damage * fl.CrewDamageModifier, mod.transform.position, DamageType.TRUE);
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
            networkObject.Despawn();
        }
        Debug.Log("Despawn and unregister");
        CO.co.UnregisterSpace(Space);
        NetworkObject.Despawn();
    }
}
