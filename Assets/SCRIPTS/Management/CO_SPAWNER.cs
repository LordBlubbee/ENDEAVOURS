using System;
using System.Collections;
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
    public Sprite GrappleCursor; 
  
    public ShopItem PrefabShopItem;
    public Sprite ShopItemMaterialDeal;
    public Sprite ShopItemSupplyDeal;
    public Sprite ShopItemAmmoDeal;
    public Sprite ShopItemTechnologyDeal;
    public Vault VaultObjectiveObject;
    public Vault RepulsorObjectiveObject;
    public Vault RespawnObjectiveObject;

    [Header("VFX")]
    public PART ArmorImpact;
    public PART ExplosionSmall;
    public PART ExplosionMedium;
    public PART ExplosionLarge;
    public GameObject EmberSmall;
    public GameObject EmberLarge;
    public GameObject SparkSmall;
    public GameObject SparkMedium;
    public GameObject ImpactSparks;
    public GameObject Soundwave;
    public GameObject Thunderstruck;

    public ParticleBuff[] BuffParticleList;
    public enum BuffParticles
    {
        NONE,
        FROST,
        VENGEANCE,
        MIST,
        PRAGMATICUS_SHIELD,
        COMMAND,
        KNUCKLES_BUFF,
        SKIRMISH_BUFF,
        SPEAKER_PRAGMATICUS,
        SPEAKER_INVICTUS,
        SPEAKER_STELLAE
    }

    [Header("BACKGROUND")]
    public GameObject[] NebulaSparks;
    public BackgroundRock[] BackgroundSmallRocks;
    public BackgroundRock[] BackgroundLargeRocks;

    [Header("DUNGEON")]
    public ResourceCrate PrefabCrate;
    public ResourceCrate PrefabMatCrate;
    public ResourceCrate PrefabSupCrate;
    public ResourceCrate PrefabAmmoCrate;
    public ResourceCrate PrefabTechCrate;
    public DamagingZone LeakingMist;
    public DamagingZone[] DifficultTerrain;
    public ResourceCrate PrefabExplosiveCrate;
    public LooncrabEgg PrefabLooncrabEgg;
    public enum BackgroundType
    {
        EMPTY,
        RANDOM_ROCK,
        BARREN,
        MOUNTAINOUS
    }
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

    [Header("MISC")]
    public DMG PrefabDMG;
    public VCX PrefabVCX;
    public ConvoHandler PrefabConvo;
    public MapPoint PrefabMapPoint;
    public AI_GROUP PrefabAIGROUP;
    public CREW PlayerPrefab;
    //This one saves all the prefabs!
    public static CO_SPAWNER co;

    private void Start()
    {
        co = this;
        StartCoroutine(BackgroundSparkles());
    }
    private void Awake()
    {
        co = this;
    }
    IEnumerator BackgroundSparkles()
    {
        while (CO.co == null) yield return null;
        while (CO.co.PlayerMainDrifter == null) yield return null;
        while (true)
        {
            Instantiate(NebulaSparks[UnityEngine.Random.Range(0,NebulaSparks.Length)], GetParticleBackgroundPoint(), Quaternion.identity);
            yield return new WaitForSeconds(0.4f);
        }
    }
    private Vector3 GetParticleBackgroundPoint()
    {
        float radius = BackgroundTransform.MapSize() * 0.6f;
        return CO.co.PlayerMainDrifter.transform.position + new Vector3(UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius));
    }
    private Vector3 GetBackgroundPoint(BackgroundRock rock, float scale)
    {
        float radius = BackgroundTransform.MapSize();
       
        while (true)
        {
            bool tryAgain = false;
            Vector3 tryPos = CO.co.PlayerMainDrifter.transform.position + new Vector3(UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius));
            foreach (BackgroundRock others in LandscapeObjects)
            {
                if (Vector3.Distance(others.transform.position,tryPos) < rock.Radius*scale+others.Radius*others.transform.localScale.x)
                {
                    tryAgain = true;
                    break;
                }
            }
            if (!tryAgain) return tryPos;
        }
    }

    private List<BackgroundRock> LandscapeObjects = new();
    public void CreateLandscape(BackgroundType LandscapeType)
    {
        foreach (BackgroundRock ob in LandscapeObjects)
        {
            ob.NetworkObject.Despawn();
        }
        LandscapeObjects = new();
        switch (LandscapeType)
        {
            case BackgroundType.RANDOM_ROCK:
                switch (UnityEngine.Random.Range(0,3))
                {
                    case 0:
                        LandscapeType = BackgroundType.EMPTY;
                        break;
                    case 1:
                        LandscapeType = BackgroundType.BARREN;
                        break;
                    case 2:
                        LandscapeType = BackgroundType.MOUNTAINOUS;
                        break;
                }
                break;
        }
        switch (LandscapeType)
        {
            case BackgroundType.EMPTY:
                break;
            case BackgroundType.BARREN:
                for (int i = 0; i < UnityEngine.Random.Range(30, 45); i++)
                {
                    SpawnSmallRock();
                }
                break;
            case BackgroundType.MOUNTAINOUS:
                for (int i = 0; i < UnityEngine.Random.Range(8,14); i++)
                {
                    SpawnLargeRock();
                }
                for (int i = 0; i < UnityEngine.Random.Range(70, 95); i++)
                {
                    SpawnSmallRock();
                }
                break;
        }
    }

    private void SpawnSmallRock()
    {
        BackgroundRock spawn = BackgroundSmallRocks[UnityEngine.Random.Range(0, BackgroundSmallRocks.Length)];
        float Scale = UnityEngine.Random.Range(0.7f, 1.3f);
        BackgroundRock ob = Instantiate(spawn, GetBackgroundPoint(spawn, Scale), Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
        
        ob.transform.localScale = new Vector3(Scale, Scale, 1);
        LandscapeObjects.Add(ob);
        ob.NetworkObject.Spawn();
    }
    private void SpawnLargeRock()
    {
        BackgroundRock spawn = BackgroundSmallRocks[UnityEngine.Random.Range(0, BackgroundSmallRocks.Length)];
        float Scale = UnityEngine.Random.Range(0.7f, 1.3f);
        BackgroundRock ob = Instantiate(spawn, GetBackgroundPoint(spawn, Scale), Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
     
        ob.transform.localScale = new Vector3(Scale, Scale, 1);
        LandscapeObjects.Add(ob);
        ob.NetworkObject.Spawn();
    }

    [Rpc(SendTo.Server)]
    public void SpawnPlayerShipRpc(int ID)
    {
        if (CO.co.HasShipBeenLaunched.Value) return;
        CO.co.HasShipBeenLaunched.Value = true;

        DRIFTER driftPrefab = UI.ui.ShipSelectionUI.SpawnableShips[ID].Prefab;
        CO.co.PlayerMainDrifterTypeID = driftPrefab.name;
        DRIFTER drifter = Instantiate(driftPrefab,Vector3.zero,Quaternion.identity);
        drifter.IsMainDrifter.Value = true;
        drifter.NetworkObject.Spawn();
        drifter.Faction.Value = 1;
        drifter.Init();
        drifter.SetHealth(drifter.GetMaxHealth() * 0.75f);
        CO.co.PlayerMainDrifter = drifter;

        foreach (CREW Prefab in drifter.StartingCrew)
        {
            SpawnUnitOnShip(Prefab, drifter);
        }

        CO.co.StartGame();
    }

    public void SpawnLoadedPlayerShip(string link, float Health, List<dataModule> Modules)
    {
        if (CO.co.HasShipBeenLaunched.Value) return;
        CO.co.HasShipBeenLaunched.Value = true;

        DRIFTER driftPrefab = Resources.Load<DRIFTER>($"OBJ/DRIFTERS/Player/{link}");

        CO.co.PlayerMainDrifterTypeID = driftPrefab.name;
        DRIFTER drifter = Instantiate(driftPrefab, Vector3.zero, Quaternion.identity);
        drifter.StartingModules = new();
        drifter.IsMainDrifter.Value = true;
        drifter.NetworkObject.Spawn();
        drifter.Faction.Value = 1;
        drifter.Init();
        drifter.SetHealth(Health);
        CO.co.PlayerMainDrifter = drifter;
        foreach (dataModule mod in Modules)
        {
            ScriptableEquippableModule scr = Resources.Load<ScriptableEquippableModule>($"{mod.ModuleLink}");
            if (scr == null)
            {
                Debug.LogError($"Error: Failed to load module {mod.ModuleLink}");
                return;
            }
            Module module = drifter.Space.AddModule(scr, true);

            Debug.Log($"Loaded module {mod.ModuleLink} of level {mod.ModuleLevel}");
            for (int i = 0; i < mod.ModuleLevel; i++)
            {
                module.UpgradeLevel();
            }
        }

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
        enem.CharacterName.Value = drift.GetFaction() == 1 ? Prefab.CharacterBackground.GetRandomName() : Prefab.CharacterBackground.GetRandomNameEnemy();
        Color col = Prefab.CharacterBackground.BackgroundColor;
        enem.CharacterNameColor.Value = new Vector3(col.r,col.g,col.b);
        enem.EquipWeapon1Rpc();
        enem.Init();
        enem.SetHomeDrifter(drift);
        drift.Interior.AddCrew(enem);
        drift.CrewGroup.Add(enem.GetComponent<AI_UNIT>());
        return enem;
    }
    public CREW SpawnUnitInDungeon(CREW Prefab, DUNGEON drift, int Faction = 2)
    {
        return SpawnUnitInDungeon(Prefab, drift,drift.Space.GetRandomUnboardableGrid().transform.position + GetRandomOnTile(), Faction);
    }
    public CREW SpawnUnitInDungeon(CREW Prefab, DUNGEON drift, Vector3 pos, int Faction = 2)
    {
        CREW enem = Instantiate(Prefab, pos, Quaternion.identity);
        enem.NetworkObject.Spawn();
        enem.Faction.Value = Faction;
        if (Prefab.CharacterBackground)
        {
            enem.CharacterName.Value = Faction == 1 ? Prefab.CharacterBackground.GetRandomName() : Prefab.CharacterBackground.GetRandomNameEnemy();
            Color col = Prefab.CharacterBackground.BackgroundColor;
            enem.CharacterNameColor.Value = new Vector3(col.r, col.g, col.b);
        }
        enem.EquipWeapon1Rpc();
        enem.Init();
        drift.Space.AddCrew(enem);
        drift.AI.Add(enem.GetComponent<AI_UNIT>());
        return enem;
    }
    public DRIFTER SpawnEnemyGroup(ScriptableEnemyGroup gr, int Faction = 2)
    {
        float Degrees = UnityEngine.Random.Range(0, 360);
        float Radius = gr.SpawnGroupRange;
        float Dist = UnityEngine.Random.Range(gr.SpawnDistanceMin, gr.SpawnDistanceMax);
        int i;
        Vector3 Offset = UnityEngine.Random.insideUnitCircle.normalized * Dist;
        Vector3 Spawn = CO.co.PlayerMainDrifter.transform.position + Offset;
        List<ScriptableEnemyCrew> PossibleSpawns = new();
        float CrewWorthPoints = CO.co.GetEncounterSizeModifier() * gr.CrewAmountLevel;
        float CrewQualityPoints = CO.co.GetEncounterDifficultyModifier() * gr.CrewQualityLevel;
        float DrifterQualityPoints = CO.co.GetDrifterDifficultyModifier() * gr.DrifterQualityLevel;

        Debug.Log($"Spawning enemy group with crew amount {CrewWorthPoints} and crew quality {CrewQualityPoints} and Drfiter quality {DrifterQualityPoints}");
        AI_GROUP group;
        List<AI_UNIT> members = new();
        if (gr.SpawnDungeon)
        {
            /*
             * SPAWN GROUP IN DUNGEON
             */

            DUNGEON dun = Instantiate(gr.SpawnDungeon, Vector3.zero, Quaternion.identity);
            dun.SetDungeonVariant();
            dun.NetworkObject.Spawn();
            dun.Init();
            CO.co.SetCurrentDungeon(dun);

            foreach (ScriptableEnemyCrew enemyType in gr.GuaranteedCrewList)
            {
                CREW crew = SpawnUnitInDungeon(enemyType.SpawnCrew, dun);
                SetQualityLevelOfCrew(crew, CrewQualityPoints);
                CrewWorthPoints -= enemyType.Worth;
            }

            ResetWeights();
            i = 0;
            foreach (EnemyCrewWithWeight weight in gr.SpawnCrewList)
            {
                AddWeights(i, weight.GetWeight(CrewQualityPoints));
                PossibleSpawns.Add(weight.Crew);
                i++;
            }
            while (CrewWorthPoints > 0)
            {
                Vector3 tryPos = dun.Space.GetRandomUnboardableGrid().transform.position + GetRandomOnTile();
                ScriptableEnemyCrew enemyType = PossibleSpawns[GetWeight()];
                CREW crew = SpawnUnitInDungeon(enemyType.SpawnCrew, dun);
                SetQualityLevelOfCrew(crew, CrewQualityPoints);
                CrewWorthPoints -= enemyType.Worth;
            }
            group = dun.AI;
            group.SetAI(gr.AI_Type, gr.AI_Group, 2, members);
            return null;
        }

        if (gr.SpawnDrifter.Count == 0)
        {
            /*
             * SPAWN GROUP IN NEBULA
             */
            group = Instantiate(PrefabAIGROUP);

            ResetWeights();
            i = 0;
            foreach (EnemyCrewWithWeight weight in gr.SpawnCrewList)
            {
                AddWeights(i, weight.GetWeight(CrewQualityPoints));
                PossibleSpawns.Add(weight.Crew);
                i++;
            }

            int SpawnAmount = gr.GetSpawnGroupAmount();
            for (int sp = 0; sp < SpawnAmount; sp++)
            {
                Offset = UnityEngine.Random.insideUnitCircle.normalized * Dist;
                Spawn = CO.co.PlayerMainDrifter.transform.position + Offset;
                CrewWorthPoints = CO.co.GetEncounterSizeModifier() * gr.CrewAmountLevel / SpawnAmount;
                while (CrewWorthPoints > 0)
                {
                    Vector3 tryPos = Spawn + new Vector3(UnityEngine.Random.Range(-Radius, Radius), UnityEngine.Random.Range(-Radius, Radius));
                    ScriptableEnemyCrew enemyType = PossibleSpawns[GetWeight()];
                    Debug.Log($"Spawning creature {enemyType} with point worth {enemyType.Worth}");
                    CREW enem = Instantiate(enemyType.SpawnCrew, tryPos, Quaternion.identity);
                    CrewWorthPoints -= enemyType.Worth;
                    enem.NetworkObject.Spawn();
                    enem.Faction.Value = Faction;
                    enem.transform.Rotate(Vector3.forward, Degrees + UnityEngine.Random.Range(-30f, 30f));
                    SetQualityLevelOfCrew(enem, CrewQualityPoints);
                    enem.Init();
                    members.Add(enem.GetComponent<AI_UNIT>());
                }
            }
            group.SetAI(gr.AI_Type, gr.AI_Group, 2, members);
            group.SetAIHome(Spawn);
            return null;
        }
        

        /*
         * SPAWN GROUP IN DRIFTER
         */
        ResetWeights();
        i = 0;
        List<ScriptableEnemyDrifter> PossibleDrifterSpawns = new();
        foreach (ScriptableEnemyDrifter weight in gr.SpawnDrifter)
        {
            AddWeights(i, weight.Weight);
            PossibleDrifterSpawns.Add(weight);
            i++;
        }
        ScriptableEnemyDrifter drifterType = PossibleDrifterSpawns[GetWeight()];
        Offset = UnityEngine.Random.insideUnitCircle.normalized * Dist * 2.5f;
        Spawn = CO.co.PlayerMainDrifter.transform.position + Offset;
        DRIFTER drifter = SpawnOtherShip(drifterType.SpawnDrifter, Spawn, Faction);
        
        SetQualityLevelOfDrifter(drifter, drifterType, DrifterQualityPoints);
        Offset = UnityEngine.Random.insideUnitCircle.normalized * Dist;
        Spawn = CO.co.PlayerMainDrifter.transform.position + Offset;
        drifter.CurrentLocationPoint = Spawn;
        ResetWeights();
        i = 0;
        foreach (EnemyCrewWithWeight weight in gr.SpawnCrewList)
        {
            AddWeights(i, weight.GetWeight(CrewQualityPoints));
            PossibleSpawns.Add(weight.Crew);
            i++;
        }
        while (CrewWorthPoints > 0)
        {
            Vector3 tryPos = drifter.Interior.GetRandomGrid().transform.position;
            ScriptableEnemyCrew enemyType = PossibleSpawns[GetWeight()];
            CREW crew = SpawnUnitOnShip(enemyType.SpawnCrew, drifter);
            SetQualityLevelOfCrew(crew, CrewQualityPoints);
            CrewWorthPoints -= enemyType.Worth;
        }
        group = drifter.CrewGroup;
        group.SetAI(gr.AI_Type, gr.AI_Group, 2, members);
        return drifter;
    }
    private Vector3 GetRandomOnTile(float off = 2)
    {
        return new Vector3(UnityEngine.Random.Range(-off, off), UnityEngine.Random.Range(-off, off));
    }
    public List<Vault> SpawnVaultObjectives(DUNGEON Dungeon)
    {
        List<Vault> List = new();
        List<WalkableTile> ObjectivePositions = new(Dungeon.MainObjectivePossibilities);
        foreach (WalkableTile tile in new List<WalkableTile>(ObjectivePositions))
        {
            if (!tile.gameObject.activeSelf)
            {
                ObjectivePositions.Remove(tile);
            }
        }
       
        int Vaults = UnityEngine.Random.Range(2, 5);
        for (int i = 0; i < Vaults; i++)
        {
            WalkableTile SelectedTile = ObjectivePositions[UnityEngine.Random.Range(0, ObjectivePositions.Count)];
            ObjectivePositions.Remove(SelectedTile);
            Vault vault = Instantiate(VaultObjectiveObject, SelectedTile.transform.position+ GetRandomOnTile(), SelectedTile.transform.rotation);
            vault.ExtraMaxHealth.Value = ((50 + CO.co.GetLOCALCO().Count * 100) / Vaults) -50;
            vault.NetworkObject.Spawn();
            vault.transform.SetParent(Dungeon.Space.transform);
            vault.SpaceID.Value = Dungeon.Space.SpaceID.Value;
            vault.Faction = 0;
            vault.Init();
            vault.TakeDamage(9999, vault.transform.position, iDamageable.DamageType.TRUE);
            Dungeon.DungeonNetworkObjects.Add(vault.NetworkObject);
            List.Add(vault);
        }
        return List;
    }
    public List<Vault> SpawnRepulsorObjective(DUNGEON Dungeon)
    {
        List<Vault> List = new();
        List<WalkableTile> ObjectivePositions = new(Dungeon.MainObjectivePossibilities);
        foreach (WalkableTile tile in new List<WalkableTile>(ObjectivePositions))
        {
            if (!tile.gameObject.activeSelf)
            {
                ObjectivePositions.Remove(tile);
            }
        }
        WalkableTile SelectedTile = ObjectivePositions[UnityEngine.Random.Range(0, ObjectivePositions.Count)];
        ObjectivePositions.Remove(SelectedTile);

        List<WalkableTile> NearbyTiles = new(Dungeon.Space.RoomTiles);
        foreach (WalkableTile tile in new List<WalkableTile>(NearbyTiles))
        {
            if (Vector3.Distance(tile.transform.position, SelectedTile.transform.position) > 30f)
            {
                NearbyTiles.Remove(tile);
            }
        }

        int Vaults = UnityEngine.Random.Range(3, 6);
        for (int i = 0; i < Vaults; i++)
        {
            if (NearbyTiles.Count == 0) break;
            WalkableTile SelectedNearbyTile = NearbyTiles[UnityEngine.Random.Range(0, NearbyTiles.Count)];
            NearbyTiles.Remove(SelectedNearbyTile);
            Vault vault = Instantiate(RepulsorObjectiveObject, SelectedNearbyTile.transform.position + GetRandomOnTile(), Quaternion.identity);
            vault.transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0f, 360f));
            vault.ExtraMaxHealth.Value = 50f * CO.co.GetEncounterDifficultyModifier() - 50;
            vault.NetworkObject.Spawn();
            vault.transform.SetParent(Dungeon.Space.transform);
            vault.SpaceID.Value = Dungeon.Space.SpaceID.Value;
            vault.Faction = 0;
            vault.Init();
            vault.TakeDamage(vault.GetMaxHealth() * UnityEngine.Random.Range(0.5f, 0.6f), vault.transform.position, iDamageable.DamageType.TRUE);
            Dungeon.DungeonNetworkObjects.Add(vault.NetworkObject);
            List.Add(vault);
        } 

        return List;
    }
    public List<Vault> SpawnRallyObjective(DUNGEON Dungeon)
    {
        List<Vault> List = new();
        List<WalkableTile> ObjectivePositions = new(Dungeon.MainObjectivePossibilities);
        foreach (WalkableTile tile in new List<WalkableTile>(ObjectivePositions))
        {
            if (!tile.gameObject.activeSelf)
            {
                ObjectivePositions.Remove(tile);
            }
        }

        int Vaults = 3;
        for (int i = 0; i < Vaults; i++)
        {
            WalkableTile SelectedTile = ObjectivePositions[UnityEngine.Random.Range(0, ObjectivePositions.Count)];
            ObjectivePositions.Remove(SelectedTile);
            Vault vault = Instantiate(RespawnObjectiveObject, SelectedTile.transform.position + GetRandomOnTile(), SelectedTile.transform.rotation);
            vault.ExtraMaxHealth.Value = 100f * CO.co.GetEncounterDifficultyModifier();
            vault.NetworkObject.Spawn();
            vault.transform.SetParent(Dungeon.Space.transform);
            vault.SpaceID.Value = Dungeon.Space.SpaceID.Value;
            vault.Faction = 2;
            vault.GetComponent<RespawnArea>().SetSpace(Dungeon.Space);
            vault.Init();
            vault.Heal(vault.GetMaxHealth());
            Dungeon.DungeonNetworkObjects.Add(vault.NetworkObject);
            List.Add(vault);
        }
        return List;
    }
    private void SetQualityLevelOfDrifter(DRIFTER drifter, ScriptableEnemyDrifter drifterData, float Quality)
    {
        float Levelup = Quality-100;
        Levelup *= UnityEngine.Random.Range(0.6f, 1.4f);
        while (Levelup > 100)
        {
            Levelup -= 100;
            drifter.MaxHealth += drifterData.HullIncreasePerLevelup;
        }
        float Budget = drifterData.BaseWeaponBudgetMod * Quality + 40;
        int Tries = 10;
        while (Budget > 0)
        {
            float ChanceForNewWeapon = 0f;
            if (drifter.Interior.WeaponModules.Count < drifter.Interior.WeaponModuleLocations.Count)
            {
                switch (drifter.Interior.WeaponModules.Count)
                {
                    case 0:
                        ChanceForNewWeapon = 1f;
                        break;
                    case 1:
                        ChanceForNewWeapon = 0.8f;
                        break;
                    case 2:
                        ChanceForNewWeapon = 0.5f;
                        break;
                    default:
                        ChanceForNewWeapon = 0.3f;
                        break;
                }
            }
            float Cost;
            if (UnityEngine.Random.Range(0f, 1f) < ChanceForNewWeapon)
            {
                ScriptableEquippableModule equip = drifterData.EquippableWeapons[UnityEngine.Random.Range(0, drifterData.EquippableWeapons.Count)];
                Cost = equip.PrefabModule.ModuleWorth;
                if (Cost > Budget && Tries > 0)
                {
                    Tries--;
                    if (Tries == 0 && drifter.Interior.WeaponModules.Count > 0) break;
                    continue;
                }
                drifter.Interior.AddModule(equip, true);
                Budget -= Cost;
                Tries = 10;
            }
            else
            {
                Module upgrade = drifter.Interior.WeaponModules[UnityEngine.Random.Range(0, drifter.Interior.WeaponModules.Count)];
                if (upgrade.ModuleLevel.Value >= upgrade.ModuleUpgradeMaterials.Length - 1)
                {
                    if (Tries > 0)
                    {
                        Tries--;
                        if (Tries == 0) break;
                    }
                    continue;
                }
                Cost = 0.9f * (upgrade.ModuleUpgradeMaterials[upgrade.ModuleLevel.Value] + upgrade.ModuleUpgradeTechs[upgrade.ModuleLevel.Value] * 2);
                if (Cost > Budget && Tries > 0)
                {
                    Tries--;
                    if (Tries == 0) break;
                    continue;
                }
                Budget -= Cost;
                upgrade.UpgradeLevel();
                Tries = 10;
            }
        }
        Budget = drifterData.BaseModuleBudgetMod * 1.2f * Quality + 40;
        float TotalBudget = Budget;
        Tries = 10;
        while (Budget > 0)
        {
            float ChanceForNewModule = 0f;
            int AmountOfNonArmorModules = 0;
            foreach (Module mod in drifter.Interior.SystemModules)
            {
                if (!(mod is ModuleArmor)) AmountOfNonArmorModules++;
            }
            switch (AmountOfNonArmorModules)
            {
                case 0:
                    ChanceForNewModule = 0.4f;
                    break;
                case 1:
                    ChanceForNewModule = 0.3f;
                    break;
                case 2:
                    ChanceForNewModule = 0.3f;
                    break;
                default:
                    ChanceForNewModule = 0f;
                    break;
            }
            float Cost;
            if (UnityEngine.Random.Range(0f, 1f) < ChanceForNewModule)
            {
                ScriptableEquippableModule equip = drifterData.EquippableModules[UnityEngine.Random.Range(0, drifterData.EquippableModules.Count)];
                Cost = equip.PrefabModule.ModuleWorth;
                if (Cost > Budget && Tries > 0)
                {
                    Tries--;
                    if (Tries == 0) break;
                    continue;
                }
                drifter.Interior.AddModule(equip, true);
                Budget -= Cost;
                Tries = 10;
            }
            else
            {
                Module upgrade = drifter.Interior.GetNonWeaponModules()[UnityEngine.Random.Range(0, drifter.Interior.GetNonWeaponModules().Count)];
                Cost = 0.9f * (upgrade.ModuleUpgradeMaterials[upgrade.ModuleLevel.Value] + upgrade.ModuleUpgradeTechs[upgrade.ModuleLevel.Value] * 2);
                if (Cost > Budget && Tries > 0)
                {
                    Tries--;
                    if (Tries == 0) break;
                    continue;
                }
                Budget -= Cost;
                upgrade.UpgradeLevel();
                Tries = 10;
            }
        }
    }
    public void SetQualityLevelOfCrew(CREW crew, float Quality)
    {
        Quality -= 100;
        Quality *= UnityEngine.Random.Range(0.6f, 1.4f);
        int Levels = 0;
        if (Quality < 50) Levels--;
        while (Quality > 100)
        {
            Quality -= 100;
            Levels++;
        }
        crew.AddUpgradeLevel(Levels);
    }
    public ResourceCrate SpawnCrate(SPACE space, Vector3 vec, int health, int resources, ResourceCrate.ResourceTypes type)
    {
        ResourceCrate ob = Instantiate(CO_SPAWNER.co.PrefabCrate, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.SetSpace(space);
        ob.InitCrate(health, resources, type);
        return ob;
    }
   
    public ResourceCrate SpawnMatCrate(SPACE space, Vector3 vec, int health, int resources)
    {
        ResourceCrate ob = Instantiate(CO_SPAWNER.co.PrefabMatCrate, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.SetSpace(space);
        ob.InitCrate(health, resources, ResourceCrate.ResourceTypes.MATERIALS);
        return ob;
    }
    public ResourceCrate SpawnSupCrate(SPACE space, Vector3 vec, int health, int resources)
    {
        ResourceCrate ob = Instantiate(CO_SPAWNER.co.PrefabSupCrate, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.SetSpace(space);
        ob.InitCrate(health, resources, ResourceCrate.ResourceTypes.SUPPLIES);
        return ob;
    }
    public ResourceCrate SpawnAmmoCrate(SPACE space, Vector3 vec, int health, int resources)
    {
        ResourceCrate ob = Instantiate(CO_SPAWNER.co.PrefabAmmoCrate, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.SetSpace(space);
        ob.InitCrate(health, resources, ResourceCrate.ResourceTypes.AMMUNITION);
        return ob;
    }
    public ResourceCrate SpawnTechCrate(SPACE space, Vector3 vec, int health, int resources)
    {
        ResourceCrate ob = Instantiate(CO_SPAWNER.co.PrefabTechCrate, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.SetSpace(space);
        ob.InitCrate(health, resources, ResourceCrate.ResourceTypes.TECHNOLOGY);
        return ob;
    }
    public ResourceCrate SpawnExplosiveCrate(SPACE space, Vector3 vec, int health)
    {
        ResourceCrate ob = Instantiate(CO_SPAWNER.co.PrefabExplosiveCrate, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.SetSpace(space);
        ob.InitCrate(health, 0, ResourceCrate.ResourceTypes.NONE);
        return ob;
    }
    public LooncrabEgg SpawnLooncrabEgg(SPACE space, DUNGEON dun, Vector3 vec, int health, int quality)
    {
        LooncrabEgg ob = Instantiate(CO_SPAWNER.co.PrefabLooncrabEgg, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.InitializeEgg(space, dun, quality);
        ob.InitEgg(health);
        return ob;
    }
    public DamagingZone SpawnDifficultTerrain(SPACE space, Vector3 vec)
    {
        DamagingZone ob = Instantiate(DifficultTerrain[UnityEngine.Random.Range(0, DifficultTerrain.Length)], vec, Quaternion.identity);
        ob.transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0, 4)*90);
        ob.NetworkObject.Spawn();
        ob.transform.SetParent(space.transform);
        ob.Init();
        return ob;
    }
    public DamagingZone SpawnLeakingMist(SPACE space, Vector3 vec)
    {
        DamagingZone ob = Instantiate(LeakingMist, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.transform.SetParent(space.transform);
        ob.Init();
        return ob;
    }

    /*VFX*/
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnExplosionTinyRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.5f, 0.7f);
        float Fade = UnityEngine.Random.Range(0.8f, 1.2f);
        PART part = Instantiate(ExplosionSmall, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
        part.FadeChange *= Fade;
    }

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
        if (ExplosionPower > 2) CAM.cam.ShakeCamera(ExplosionPower * 0.6f);
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
    public void SpawnArmorImpactRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.5f, 0.7f);
        float Fade = UnityEngine.Random.Range(0.8f, 1.2f);
        PART part = Instantiate(ArmorImpact, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
        part.FadeChange *= Fade;
    }
    public void SpawnSoundwave(Vector3 pos, float Trans)
    {
        GameObject part = Instantiate(Soundwave, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
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
        dmg.InitWords(dm, 1.2f, Color.red);
    }
    public void SpawnVoice(string dm, CREW tr, VCX.VoiceStyles style)
    {
        VCX dmg = Instantiate(PrefabVCX, tr.transform.position + new Vector3(0, 3.5f), Quaternion.identity);
        dmg.InitVoice(dm, tr.transform, style, tr.GetVoiceHandler().SpeechVoice, tr.GetVoiceHandler().SpeechColor);
    }
    public void SpawnVoice(string dm, Transform tr, VCX.VoiceStyles style, AudioClip voice, Color col)
    {
        VCX dmg = Instantiate(PrefabVCX, tr.transform.position + new Vector3(0,3.5f), Quaternion.identity);
        dmg.InitVoice(dm, tr, style, voice, col);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnDMGRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        dmg.InitNumber(dm, 0.5f, Color.red);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnArmorDMGRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        dmg.InitNumber(dm, 0.5f, Color.yellow);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnHealRpc(float dm, Vector3 pos)
    {
        DMG dmg = Instantiate(PrefabDMG, pos, Quaternion.identity);
        dmg.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        dmg.InitNumber(dm, 0.5f, Color.green);
    }
    public void SpawnConvo(CREW us, CREW them, Conversation convo)
    {
        ConvoHandler han = Instantiate(PrefabConvo);
        han.InitConvo(us, them, convo);
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
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnThunderRpc(Vector3 pos)
    {
        GameObject ob = Instantiate(Thunderstruck, pos, Quaternion.identity);
    }

    /**/
    public GameObject SleepVFX;
    public GameObject WarningPointerVFX;
    public GameObject WarningIconVFX;

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnSleepRpc(Vector3 pos)
    {
        GameObject ob = Instantiate(SleepVFX, pos, Quaternion.identity);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnWarningPointerRpc(Vector3 towards)
    {
        Transform tr = CAM.cam.transform;
        GameObject ob = Instantiate(WarningIconVFX, new Vector3(tr.position.x, tr.position.y,0), Quaternion.identity);
        ob.transform.SetParent(tr);
        ob = Instantiate(WarningPointerVFX, new Vector3(tr.position.x, tr.position.y, 0), Quaternion.identity);
        ob.transform.Rotate(Vector3.forward, Vector2.SignedAngle(new Vector3(1,0), towards - tr.position));
        ob.transform.SetParent(tr);
    }
    public GameObject TeleportRespawnVFX; //

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnTeleportRespawnVFXRpc(Vector3 pos)
    {
        GameObject ob = Instantiate(TeleportRespawnVFX, pos, Quaternion.identity);
        Transform trans = CO.co.GetTransformAtPoint(pos);
        if (trans) ob.transform.SetParent(trans); ;
    }


    [Header("SPELLS")]
    public GameObject CommandVFX;
    public GameObject FloralImpactVFX;
    public GameObject FloralHealSpellImpactVFX;
    public GameObject WaywardConsumptionVFX;
    public GameObject PragmaticusRestrainImpactVFX;
    public GameObject PragmaticusShieldImpactVFX;
    public void SpawnCommandVFX(CREW crew)
    {
        GameObject ob = Instantiate(CommandVFX, crew.transform.position, Quaternion.identity);
        Transform trans = crew.transform;
        if (trans) ob.transform.SetParent(trans);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnFloralImpactRpc(Vector3 pos)
    {
        GameObject ob = Instantiate(FloralImpactVFX, pos, Quaternion.identity);
        Transform trans = CO.co.GetTransformAtPoint(pos);
        if (trans) ob.transform.SetParent(trans);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnFloralHealSpellImpactRpc(Vector3 pos)
    {
        GameObject ob = Instantiate(FloralHealSpellImpactVFX, pos, Quaternion.identity);
        Transform trans = CO.co.GetTransformAtPoint(pos);
        if (trans) ob.transform.SetParent(trans);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnWaywardConsumptionRpc(Vector3 pos)
    {
        GameObject ob = Instantiate(WaywardConsumptionVFX, pos, Quaternion.identity);
        Transform trans = CO.co.GetTransformAtPoint(pos);
        if (trans) ob.transform.SetParent(trans);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnPragmaticusRestrainImpactRpc(Vector3 pos)
    {
        GameObject ob = Instantiate(PragmaticusRestrainImpactVFX, pos, Quaternion.identity);
        Transform trans = CO.co.GetTransformAtPoint(pos);
        if (trans) ob.transform.SetParent(trans);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnPragmaticusShieldImpactRpc(Vector3 pos)
    {
        GameObject ob = Instantiate(PragmaticusShieldImpactVFX, pos, Quaternion.identity);
        Transform trans = CO.co.GetTransformAtPoint(pos);
        if (trans) ob.transform.SetParent(trans);
    }

}
