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
    public ResourceCrate PrefabAmmoCrate;

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

    public GameObject[] BuffParticleList;
    public enum BuffParticles
    {
        NONE
    }

    [Header("BACKGROUND")]
    public GameObject[] NebulaSparks;
    public BackgroundRock[] BackgroundSmallRocks;
    public BackgroundRock[] BackgroundLargeRocks;
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

    public List<BackgroundRock> LandscapeObjects = new();
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
        if (gr.SpawnDrifter.Count == 0)
        {
            group = Instantiate(PrefabAIGROUP);

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
                Debug.Log("Spawning creature...");
                Vector3 tryPos = Spawn + new Vector3(UnityEngine.Random.Range(-Radius, Radius), UnityEngine.Random.Range(-Radius, Radius));
                ScriptableEnemyCrew enemyType = PossibleSpawns[GetWeight()];
                CREW enem = Instantiate(enemyType.SpawnCrew, tryPos, Quaternion.identity);
                CrewWorthPoints -= enemyType.Worth;
                enem.NetworkObject.Spawn();
                enem.Faction.Value = Faction;
                enem.transform.Rotate(Vector3.forward, Degrees + UnityEngine.Random.Range(-30f, 30f));
                SetQualityLevelOfCrew(enem, enemyType, CrewQualityPoints);
                enem.Init();
                members.Add(enem.GetComponent<AI_UNIT>());
            }

            group.SetAI(gr.AI_Type, gr.AI_Group, 2, members);
            group.SetAIHome(Spawn);
            return null;
        }
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
            SetQualityLevelOfCrew(crew, enemyType, CrewQualityPoints);
            CrewWorthPoints -= enemyType.Worth;
        }
        group = drifter.CrewGroup;
        group.SetAI(gr.AI_Type, gr.AI_Group, 2, members);
        return drifter;
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
        float Budget = drifterData.BaseWeaponBudgetMod * Quality;
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
        Budget = drifterData.BaseModuleBudgetMod * Quality;
        Tries = 10;
        while (Budget > 0)
        {
            float ChanceForNewModule = 0f;
            float Cost;
            if (UnityEngine.Random.Range(0f, 1f) < ChanceForNewModule)
            {
                /*ScriptableEquippableModule equip = drifterData.EquippableWeapons[UnityEngine.Random.Range(0, drifterData.EquippableWeapons.Count)];
                Cost = equip.PrefabModule.ModuleWorth * 1.2f;
                if (Cost > Budget && Tries > 0)
                {
                    Tries--;
                    if (Tries == 0 && drifter.Interior.WeaponModules.Count > 0) break;
                    continue;
                }
                drifter.Interior.AddModule(equip, true);
                Budget -= Cost;
                Tries = 10;*/
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
    private void SetQualityLevelOfCrew(CREW crew, ScriptableEnemyCrew crewData, float Quality)
    {
        Quality -= 100;
        Quality *= UnityEngine.Random.Range(0.6f, 1.4f);
        int Levels = 0;
        while (Quality > 100)
        {
            Quality -= 100;
            Levels++;
        }
        if (Levels > 0)
        {
            crew.ModifyHealthMax += crewData.HealthIncreasePerLevelup * Levels;
            crew.AddAttributes(
                    Mathf.FloorToInt(crewData.PointIncreasePerLevelup[0] * Levels),
                     Mathf.FloorToInt(crewData.PointIncreasePerLevelup[1] * Levels),
                      Mathf.FloorToInt(crewData.PointIncreasePerLevelup[2] * Levels),
                       Mathf.FloorToInt(crewData.PointIncreasePerLevelup[3] * Levels),
                        Mathf.FloorToInt(crewData.PointIncreasePerLevelup[4] * Levels),
                         Mathf.FloorToInt(crewData.PointIncreasePerLevelup[5] * Levels),
                          Mathf.FloorToInt(crewData.PointIncreasePerLevelup[6] * Levels),
                           Mathf.FloorToInt(crewData.PointIncreasePerLevelup[7] * Levels)
                );
        }
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
    public void SpawnArmorImpactRpc(Vector3 pos)
    {
        float Trans = UnityEngine.Random.Range(0.5f, 0.7f);
        float Fade = UnityEngine.Random.Range(0.8f, 1.2f);
        PART part = Instantiate(ArmorImpact, pos, Quaternion.identity);
        part.transform.SetParent(CO.co.GetTransformAtPoint(pos));
        part.transform.localScale = new Vector3(Trans, Trans, 1);
        part.FadeChange *= Fade;
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
