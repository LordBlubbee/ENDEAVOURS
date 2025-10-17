
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CO : NetworkBehaviour
{
    public static CO co;

    [NonSerialized] public NetworkVariable<bool> HasShipBeenLaunched = new();
    [NonSerialized] public NetworkVariable<bool> AreWeInDanger = new();
    [NonSerialized] public NetworkVariable<bool> CommunicationGamePaused = new();
    [NonSerialized] public NetworkVariable<float> EnemyBarRelative = new(-1);
    [NonSerialized] public NetworkVariable<FixedString32Bytes> EnemyBarString = new("");
    [NonSerialized] public DRIFTER PlayerMainDrifter;
    [NonSerialized] public NetworkVariable<int> PlayerMapPointID = new();

    public bool IsSafe()
    {
        return !AreWeInDanger.Value;
    }
    public MapPoint GetMapPoint(int ID)
    {
        foreach (MapPoint point in GetMapPoints())
        {
            if (point.PointID.Value == ID) return point;
        }
        return GetMapPoints()[0];
    }
    public MapPoint GetPlayerMapPoint()
    {
        return GetMapPoint(PlayerMapPointID.Value);
    }
    public SPACE GetPlayerSpace()
    {
        return PlayerMainDrifter.Space;
    }

    [NonSerialized] public NetworkVariable<int> Resource_Materials = new();
    [NonSerialized] public NetworkVariable<int> Resource_Supplies = new();
    [NonSerialized] public NetworkVariable<int> Resource_Ammo = new();
    [NonSerialized] public NetworkVariable<int> Resource_Tech = new();
    [NonSerialized] public Dictionary<Faction, int> Resource_Reputation;
    public enum Faction
    {
        LOGIPEDES_INVICTUS,
        LOGIPEDES_PRAGMATICUS,
        LOGIPEDES_STELLAE,
        EPHEMERAL_EYE,
        ORDER_OF_THE_STORM,
        DEMOCRATIC_CATALI,
        NOMADEN_COALITION,
        NOMADEN_CLANS,
        NOMADEN_INSURRECTION,
        AESPERIANS,
        PHOS,
        SEEKERS
    }
    [NonSerialized] public List<ScriptableEquippable> Drifter_Inventory = new();

    [Rpc(SendTo.ClientsAndHost)]
    public void SetDrifterInventoryForClientsRpc(FixedString64Bytes[] strings)
    {
        Drifter_Inventory = new();
        foreach (FixedString64Bytes str in strings)
        {
            if (str == "") Drifter_Inventory.Add(null);
            else Drifter_Inventory.Add(Resources.Load<ScriptableEquippable>(str.ToString()));
        }
    }

    [Rpc(SendTo.Server)]
    public void SetDrifterInventoryItemRpc(int slot, string str)
    {
        while (slot >= Drifter_Inventory.Count) Drifter_Inventory.Add(null);
        if (str == null) Drifter_Inventory[slot] = null;
        else Drifter_Inventory[slot] = Resources.Load<ScriptableEquippable>(str.ToString());
    }

    public ScriptableBiome CurrentBiome;
    private void Start()
    {
        co = this; 
        StartCoroutine(PeriodicClientUpdates());
        if (IsServer)
        {
            StartCoroutine(PeriodicUpdates());
        }
    }

    IEnumerator PeriodicClientUpdates()
    {
        while (true)
        {
            UpdateMapConnections();
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator PeriodicUpdates()
    {
        while (true)
        {
            foreach (LOCALCO local in GetLOCALCO())
            {
                if (local.GetPlayer() != null)
                {
                    local.GetPlayer().EquipArmorLocallyRpc(local.GetPlayer().EquippedArmor ? local.GetPlayer().EquippedArmor.ItemResourceID : null);
                    for (int i = 0; i < 3; i++)
                    {
                        local.GetPlayer().EquipWeaponLocallyRpc(i, local.GetPlayer().EquippedWeapons[i] ? local.GetPlayer().EquippedWeapons[i].ItemResourceID : null);
                        local.GetPlayer().EquipArtifactLocallyRpc(i, local.GetPlayer().EquippedArtifacts[i] ? local.GetPlayer().EquippedArtifacts[i].ItemResourceID : null);
                    }
                }
                SendPeriodicInventoryUpdate();
            }
            yield return new WaitForSeconds(5f);
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestPeriodicInventoryUpdateRpc()
    {
        SendPeriodicInventoryUpdate();
    }
    public void SendPeriodicInventoryUpdate()
    {
        if (PlayerMainDrifter == null) return;
        List<FixedString64Bytes> strings = new();
        for (int i = 0; i < CO.co.Drifter_Inventory.Count; i++)
        {
            if (CO.co.Drifter_Inventory[i]) strings.Add(CO.co.Drifter_Inventory[i].ItemResourceID);
            else strings.Add("");
        }
        CO.co.SetDrifterInventoryForClientsRpc(strings.ToArray());
    }

    public void StartGame()
    {
        GenerateMap(25, 20);

        //NEW GAME
        Resource_Materials.Value = 50;
        Resource_Supplies.Value = 50;
        Resource_Ammo.Value = 50;
        Resource_Tech.Value = 0;
        AddInventoryItem(Resources.Load<ScriptableEquippable>("OBJ/SCRIPTABLES/Items/Weapons/Logipedes_Crossbow"));
        AddInventoryItem(Resources.Load<ScriptableEquippable>("OBJ/SCRIPTABLES/Items/Weapons/Logipedes_Halberd"));
    }

    public void AddInventoryItem(ScriptableEquippable equip)
    {
        Drifter_Inventory.Add(equip);
    }
    public override void OnNetworkSpawn()
    {
        co = this;
    }
    /*CHOOSE MAP*/
    private void Update()
    {
        /*
         TO DO
         
            -Get players to see CHOOSE LEVEL once the seas are safe
            -Get players to see an ANIMATION when the SHIP MOVES
            -Get the map to regenerate when the ship moves
            -Switch players to the comms screen after they move with the new event ready immediately
            -Make choosing an option work
         
         */
        if (!IsServer) return;
        if (HasVoteResult() != -1)
        {
            Debug.Log("Moving to point!");
            MapPoint destination = GetPlayerMapPoint().ConnectedPoints[HasVoteResult()];
            CO_STORY.co.SetStory(destination.AssociatedPoint.InitialDialog);
            StartCoroutine(Travel(destination));
            PlayerMapPointID.Value = destination.PointID.Value;
            ResetMapVotes();
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        HandleGravity();
    }
    public Vector3 GetLookVectorWithRandomSpread(Vector3 originalLook, float maxAngle = 20f)
    {
        // Pick a random angle between -maxAngle and +maxAngle
        float randomAngle = UnityEngine.Random.Range(-maxAngle, maxAngle);

        // Rotate the vector around the Y-axis (assuming 3D horizontal look)
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);

        // Return the rotated direction
        return rotation * originalLook;
    }
    public Vector3 RotatePointWithLookVectors(Vector3 pointA, Vector3 pointB, Vector3 oldLookVector, Vector3 newLookVector)
    {
        // Get A’s position relative to B
        Vector3 relativePos = pointA - pointB;

        // Compute the rotation needed to go from old look to new look
        Quaternion rotation = Quaternion.FromToRotation(oldLookVector, newLookVector);

        // Apply that rotation to the relative position
        Vector3 rotatedRelative = rotation * relativePos;

        // Return the new world position
        return pointB + rotatedRelative;
    }
    void HandleGravity() //Gravity Relativity
    {
        if (PlayerMainDrifter == null) return;
        Vector3 CurrentCenter = PlayerMainDrifter.CurrentLocationPoint;
        Vector3 BackgroundSpeed = -PlayerMainDrifter.getLookVector() * PlayerMainDrifter.GetCurrentMovement();
        float CurrentSpeed = 99f;
        if (PlayerMainDrifter.GetRelativeSpeed() > 0.05f)
        {
            foreach (DRIFTER drift in GetAllDrifters())
            {
                CurrentSpeed = Mathf.Min(drift.GetCurrentMovement(), CurrentSpeed);
                drift.CurrentPositionTimer -= CO.co.GetWorldSpeedDelta();
                if (drift.CurrentPositionTimer < 0)
                {
                    drift.CurrentPositionTimer = UnityEngine.Random.Range(10f, 20f);
                    if (PlayerMainDrifter == drift)
                    {
                        drift.CurrentLocationPoint = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
                        Vector3 newLookVector = GetLookVectorWithRandomSpread(drift.getLookVector(), 20f * drift.GetRelativeSpeed());
                        drift.SetLookTowards(newLookVector);
                        foreach (DRIFTER drift2 in GetAllDrifters())
                        {
                            drift2.SetLookTowards(drift.getLookVector());
                            drift2.SetMoveTowards(RotatePointWithLookVectors(drift2.CurrentLocationPoint, drift.CurrentLocationPoint, drift.getLookVector(), newLookVector));
                        }
                    }
                    else drift.SetMoveTowards(CurrentCenter + (drift.CurrentLocationPoint - CurrentCenter).normalized * UnityEngine.Random.Range(60f, 180f) + new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f)));
                }
            }
        }
       
        /* OLD
         * 
         * List<Transform> Trans = new();
        foreach (CREW ob in GetAllCrews())
        {
            if (ob.Space == null) Trans.Add(ob.transform);
        }
        List<DRIFTER> Drifters = GetAllDrifters();
        foreach (DRIFTER ob in Drifters)
        {
            Trans.Add(ob.transform);
        }

        if (Trans.Count < 2) return; // not enough objects to compare

        float maxDist = 0f;
        Vector3 posA = Vector3.zero;
        Vector3 posB = Vector3.zero;

        // Find the two furthest transforms
        for (int i = 0; i < Trans.Count; i++)
        {
            for (int j = i + 1; j < Trans.Count; j++)
            {
                float dist = Vector3.Distance(Trans[i].position, Trans[j].position);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    posA = Trans[i].position;
                    posB = Trans[j].position;
                }
            }
        }
        // Calculate midpoint between them
        Vector3 midpoint = (posA + posB) * 0.5f;

        // You can now use `midpoint` for whatever you need (debug, centering camera, etc.)

        float noPullRange = 40f + Drifters.Count*10;       // distance within which no pull is applied
        float pullStrength = 0.1f;    // base multiplier for how strongly they move per frame (tune as needed)

        foreach (Transform trans in Trans)
        {
            Vector3 dir = (midpoint - trans.position);
            float dist = dir.magnitude;
            if (dist <= noPullRange) continue; // skip close ones

            dir.Normalize();

            // The pull grows stronger the further they are beyond 50 units
            float pullFactor = (dist - noPullRange) * pullStrength;

            // Optionally clamp it so it doesn’t yank things too hard
            pullFactor = Mathf.Max(pullFactor, 0f);

            // Apply pull
            trans.position += dir * pullFactor * GetWorldSpeedDelta();
        }*/
    }

    IEnumerator Travel(MapPoint destination)
    {
        Vector3 moveDirection = destination.transform.position - GetPlayerMapPoint().transform.position;
        PlayerMainDrifter.SetLookTowards(moveDirection);
        PlayerMainDrifter.SetMoveInput(moveDirection, 1f);
        PlayerMainDrifter.SetCanReceiveInput(false);
        foreach (LOCALCO local in GetLOCALCO())
        {
            local.ShipTransportFadeAwayRpc(destination.GetName());
        }
        yield return new WaitForSeconds(2.5f);
        GenerateLevel();
        //UpdatePlayerMapPointRpc(destination.transform.position);
        foreach (LOCALCO local in GetLOCALCO())
        {
            local.ShipTransportFadeInRpc();
        }
        //Generate Area here


        yield return new WaitForSeconds(1f);
        PlayerMainDrifter.SetCanReceiveInput(true);
    }

    private void GenerateLevel()
    {
        PlayerMainDrifter.transform.position = Vector3.zero;
        PlayerMainDrifter.transform.Rotate(Vector3.forward, PlayerMainDrifter.AngleToTurnTarget());
        PlayerMainDrifter.SetMoveInput(Vector3.zero, 1f);
        foreach (CREW crew in new List<CREW>(GetAllCrews()))
        {
            if (crew.Space != PlayerMainDrifter.Space || crew.isDead())
            {
                crew.DespawnAndUnregister();
            }
        }
    }
    /*
    [Rpc(SendTo.Server)]
    private void RequestPlayerMapPointRpc()
    {
        if (!GetPlayerMapPoint()) return;
        UpdatePlayerMapPointRpc(GetPlayerMapPoint().transform.position);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePlayerMapPointRpc(Vector3 vec)
    {
        MapPoint nearest = null;
        float minDist = float.MaxValue;

        foreach (MapPoint mp in GetMapPoints())
        {
            float dist = (mp.transform.position - vec).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = mp;
            }
        }

        PlayerMapPoint = nearest;
        UI.ui.MapUI.UpdateMap();
    }*/
    private int HasVoteResult()
    {
        int num = -1;
        foreach (LOCALCO local in GetLOCALCO())
        {
            if (local.CurrentMapVote.Value != -1)
            {
                if (num == -1) num = local.CurrentMapVote.Value;
                else if (num != local.CurrentMapVote.Value) return -1; //No consensus
            }
            else return -1; //Not everyone has voted
        }
        return num;
    }
    public int VoteResultAmount(int num)
    {
        int count = 0;
        foreach (LOCALCO local in GetLOCALCO())
        {
            if (local.CurrentMapVote.Value == num) count++;
        }
        return count;
    }
    /*MAP GENERATION*/

    public void ResetMapVotes()
    {
        foreach (LOCALCO local in CO.co.GetLOCALCO())
        {
            local.CurrentMapVote.Value = -1;
        }
    }
    [Rpc(SendTo.Server)]
    public void VoteForMapRpc(int localID, int ID)
    {
        LOCALCO local = GetLOCALCO(localID);
        if (local.CurrentMapVote.Value == ID) local.CurrentMapVote.Value = -1;
        else local.CurrentMapVote.Value = ID;
    }
    public void GenerateMap(float mapSize, int PointAmount)
    {
        foreach (MapPoint map in GetMapPoints())
        {
            map.NetworkObject.Despawn();
        }
        RegisteredMapPoints = new();
        MapPoint mapPoint = CO_SPAWNER.co.CreateMapPoint(new Vector3(-mapSize, UnityEngine.Random.Range(-mapSize * 0.7f, mapSize * 0.7f)));
        //StartPoint
        PlayerMapPointID.Value = 0;
        RegisterMapPoint(mapPoint);
        int Tries = 50;

        while (PointAmount > 0)
        {
            Vector3 tryPos = new Vector3(UnityEngine.Random.Range(-mapSize, mapSize), UnityEngine.Random.Range(-mapSize * 0.7f, mapSize * 0.7f));
            if (IsPointLegal(tryPos) || Tries < 1)
            {
                mapPoint = CO_SPAWNER.co.CreateMapPoint(tryPos);
                PointAmount--;
                RegisterMapPoint(mapPoint);
                Tries = 50;
            } else
            {
                Tries--;
            }
        }
        Debug.Log("Generating map");
        int ID = 0;
        foreach (MapPoint map in GetMapPoints())
        {
            map.ConnectedPoints = GetConnectedPoints(map.transform.position, map);
            map.Init(CurrentBiome.PossiblePointsRandom[UnityEngine.Random.Range(0, CurrentBiome.PossiblePointsRandom.Count)], ID);
            ID++;
        }
        foreach (MapPoint map in GetMapPoints())
        {
            foreach (MapPoint map2 in map.ConnectedPoints)
            {
                if (!map2.ConnectedPoints.Contains(map))
                {
                    map2.ConnectedPoints.Add(map);
                }
            }
        }
    }

    private void UpdateMapConnections()
    {
        foreach (MapPoint map in GetMapPoints())
        {
            map.ConnectedPoints = GetConnectedPoints(map.transform.position, map);
        }
        foreach (MapPoint map in GetMapPoints())
        {
            foreach (MapPoint map2 in map.ConnectedPoints)
            {
                if (!map2.ConnectedPoints.Contains(map))
                {
                    map2.ConnectedPoints.Add(map);
                }
            }
        }
    }
    private bool IsPointLegal(Vector3 center)
    {
        float ConnectionDist = 10f;
        List<MapPoint> list = new();
        foreach (MapPoint map in GetMapPoints())
        {
            float dist = (map.transform.position - center).magnitude;
            if (dist < ConnectionDist)
            {
                list.Add(map);
            }
            if (dist < 2f)
            {
                return false;
            }
        }
        return list.Count < 5;
    }
    private List<MapPoint> GetConnectedPoints(Vector3 center, MapPoint us = null)
    {
        MapPoint Closest1 = null;
        MapPoint Closest2 = null;
        float Closest1Dist = 999f;
        float Closest2Dist = 999f;
        float ConnectionDist = 10f;
        List<MapPoint> list = new();
        foreach (MapPoint map in GetMapPoints())
        {
            if (map == us) continue;
            if (list.Count > 4) break;
            float dist = (map.transform.position - center).magnitude;
            if (dist < Closest1Dist)
            {
                Closest1 = map;
                Closest1Dist = dist;
            } else if (dist < Closest2Dist)
            {
                Closest2 = map;
                Closest2Dist = dist;
            }
            if (dist < ConnectionDist)
            {
                list.Add(map);
            }
        }
        if (Closest1 && !list.Contains(Closest1)) list.Add(Closest1);
        if (Closest2 && !list.Contains(Closest2)) list.Add(Closest2);
        return list;
    }

    /*REGISTRIES*/
    int LOCALCO_IDCOUNT = 1;
    List<LOCALCO> RegisteredLOCALCO = new();

    public List<LOCALCO> GetLOCALCO()
    {
        return RegisteredLOCALCO;
    }

    public float GetWorldSpeedDelta()
    {
        if (CommunicationGamePaused.Value) return 0f;
        return Time.deltaTime;
    }
    public float GetWorldSpeedDeltaFixed()
    {
        if (CommunicationGamePaused.Value) return 0f;
        return Time.fixedDeltaTime;
    }

    public LOCALCO GetLOCALCO(int ID)
    {
        foreach (LOCALCO loc in GetLOCALCO())
        {
            if (loc.GetPlayerID() == ID)
            {
                return loc;
            }
        }
        return null;
    }
    public void RegisterLOCALCO(LOCALCO loc)
    {
        if (RegisteredLOCALCO.Contains(loc)) return;
        RegisteredLOCALCO.Add(loc);
        if (IsServer)
        {
            loc.SetPlayerID(LOCALCO_IDCOUNT);
            LOCALCO_IDCOUNT++;
        }
    }
    public List<CREW> GetEnemyCrew(int fac = 1)
    {
        List<CREW> list = new();
        foreach (CREW loc in GetAllCrews())
        {
            if (loc.GetFaction() > 0 && loc.GetFaction() != fac)
            {
                list.Add(loc);
            }
        }
        return list;
    }
    public List<CREW> GetAlliedCrew(int fac = 1)
    {
        List<CREW> list = new();
        foreach (CREW loc in GetAllCrews())
        {
            if (loc.GetFaction() == fac)
            {
                list.Add(loc);
            }
        }
        return list;
    }

    List<CREW> RegisteredCREW = new();
    public List<CREW> GetAllCrews()
    {
        return RegisteredCREW;
    }
    public CREW GetPlayerCharacter(int ID)
    {
        foreach (CREW loc in GetAllCrews())
        {
            if (loc.PlayerController.Value == ID)
            {
                return loc;
            }
        }
        return null;
    }
    public void RegisterCrew(CREW crew)
    {
        if (RegisteredCREW.Contains(crew)) return;
        RegisteredCREW.Add(crew);
    }
    public void UnregisterCrew(CREW crew)
    {
        RegisteredCREW.Remove(crew);
    }

    int RegisteredDrifters = 0;
    List<DRIFTER> RegisteredDRIFTER = new();
    public List<DRIFTER> GetAllDrifters()
    {
        return RegisteredDRIFTER;
    }
    public void RegisterDrifter(DRIFTER drifter)
    {
        if (RegisteredDRIFTER.Contains(drifter)) return;
        RegisteredDRIFTER.Add(drifter);
        if (drifter.IsMainDrifter.Value) PlayerMainDrifter = drifter;
    }

    public void UnregisterDrifter(DRIFTER drifter)
    {
        RegisteredDRIFTER.Remove(drifter);
    }


    List<MapPoint> RegisteredMapPoints = new();
    public List<MapPoint> GetMapPoints()
    {
        return RegisteredMapPoints;
    }
    public void RegisterMapPoint(MapPoint crew)
    {
        if (RegisteredMapPoints.Contains(crew)) return;
        RegisteredMapPoints.Add(crew);
    }
    public void UnregisterMapPoint(MapPoint crew)
    {
        RegisteredMapPoints.Remove(crew);
    }

    List<SPACE> RegisteredSPACES = new();
    int SpaceID = 0;
    public List<SPACE> GetAllSpaces()
    {
        //Server Only
        return RegisteredSPACES;
    }
    public SPACE GetSpace(int ID)
    {
        foreach (SPACE space in GetAllSpaces())
        {
            if (space.SpaceID.Value == ID) return space;
        }
        return null;
    }
    public void RegisterSpace(SPACE Space)
    {
        if (RegisteredSPACES.Contains(Space)) return;
        RegisteredSPACES.Add(Space);
        if (IsServer)
        {
            SpaceID++;
            Space.SpaceID.Value = SpaceID;
        }
    }
    public void UnregisterSpace(SPACE Space)
    {
        RegisteredSPACES.Remove(Space);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateATTUIRpc()
    {
        UI.ui.InventoryUI.SkillRefresh();
    }

    /*GAME EVENTS*/
    ScriptableEvent CurrentEvent = null;


    public void PerformEvent(ScriptableEvent even)
    {
        //LOOOOOOOOOOOOOOOOOOOOOOOOONG LIST
        CurrentEvent = even;
        string str = even.EventController;
        switch (str)
        {
            case "GenericBattle":
                StartCoroutine(Event_GenericBattle());
                break;
        }
    }
    IEnumerator Event_GenericBattle()
    {
        AreWeInDanger.Value = true;
        ResetWeights();
        int i = 0;
        List<EnemyGroupWithWeight> Groups = new();
        foreach (EnemyGroupWithWeight weighted in CurrentEvent.EnemyWave.SpawnEnemyGroupList)
        {
            Groups.Add(weighted);
            AddWeights(i, weighted.Weight);
            i++;
        }
        CO_SPAWNER.co.SpawnEnemyGroup(Groups[GetWeight()].EnemyGroup, 1f);
        float Death = 0f;
        while (Death < 1)
        {
            yield return new WaitForSeconds(0.5f);
            int DeadAmount = GroupDeathAmount(GetEnemyCrew());
            int AliveAmount = GetEnemyCrew().Count - DeadAmount;
            Death = (float)DeadAmount / (float)GetEnemyCrew().Count;
            EnemyBarRelative.Value = 1f-Death;
            EnemyBarString.Value = $"THREATS: ({AliveAmount})";
        }
        EnemyBarRelative.Value = -1;
        if (CurrentEvent.DebriefDialog) CO_STORY.co.SetStory(CurrentEvent.DebriefDialog);
        AreWeInDanger.Value = false;
        ProcessLootTable(CurrentEvent.LootTable, 1f);
    }

    private int GroupDeathAmount(List<CREW> crewList)
    {
        int Death = 0;
        foreach (CREW crew in crewList)
        {
            if (crew.isDead()) Death += 1;
        }
        return Death;
    }

    public float GetCommanderExperienceFactor()
    {
        float ExperienceMod = 1f;
        foreach (CREW crew in GetAlliedCrew())
        {
            ExperienceMod = Mathf.Max(ExperienceMod, crew.GetATT_COMMAND() * 0.05f + 1f);
        }
        return ExperienceMod;
    }
    private void ProcessLootTable(ScriptableLootTable table, float LootLevelMod)
    {
        List<LootItem> list = new List<LootItem>();
        List<Faction> Factions = new();
        List<int> FactionChanges = new();

        foreach (FactionReputation rep in table.ReputationChanges)
        {
            Resource_Reputation[rep.Fac] += rep.Amount;
            Factions.Add(rep.Fac);
            FactionChanges.Add(rep.Amount);
        }

        foreach (LootItem item in table.GuaranteedLoot) list.Add(item);
        ResetWeights();
        int i = 0;
        List<RandomLootItem> RandomLoot = new();
        foreach (RandomLootItem item in table.RandomLoot)
        {
            AddWeights(i, item.Weight);
            RandomLoot.Add(item);
        }
        for (i = 0; i < RandomLoot.Count; ++i)
        {
            list.Add(RandomLoot[GetWeight()]);
        }
        int ChangeAmmo = 0;
        int ChangeMaterials = 0;
        int ChangeSupplies = 0;
        int ChangeTech = 0;
        int ChangeXP = 0;
        List<FixedString64Bytes> ItemTranslate = new();
        foreach (LootItem item in list)
        {
            ChangeAmmo+= Mathf.RoundToInt(item.Resource_Ammunition * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeMaterials += Mathf.RoundToInt(item.Resource_Materials * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeSupplies += Mathf.RoundToInt(item.Resource_Supplies * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeTech += Mathf.RoundToInt(item.Resource_Technology * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeXP += Mathf.RoundToInt(item.Resource_XP * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod * GetCommanderExperienceFactor());
            if (item.ItemDrop)
            {
                AddInventoryItem(item.ItemDrop);
                ItemTranslate.Add(item.ItemDrop.ItemResourceID);
            }
        }
        Resource_Ammo.Value += ChangeAmmo;
        Resource_Materials.Value += ChangeMaterials;
        Resource_Supplies.Value += ChangeSupplies;
        Resource_Tech.Value += ChangeTech;
        foreach (CREW crew in GetAlliedCrew())
        {
            crew.XPPoints.Value += ChangeXP;
        }
        OpenRewardScreenRpc(ChangeMaterials, ChangeSupplies, ChangeAmmo, ChangeTech, ChangeXP, Factions.ToArray(), FactionChanges.ToArray(), ItemTranslate.ToArray());
        //Send report to clients with all faction rep changes
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void OpenRewardScreenRpc(int Materials, int Supplies, int Ammo, int Tech, int XP, Faction[] Facs, int[] FacChanges, FixedString64Bytes[] RewardItemsGained)
    {
        List<FactionReputation> list = new();
        for (int i = 0; i < Facs.Length ;i++)
        {
            FactionReputation newfac = new FactionReputation();
            newfac.Fac = Facs[i];
            newfac.Amount = FacChanges[i];
            list.Add(newfac);
        }
        UI.ui.TalkUI.OpenRewardScreen(Materials, Supplies, Ammo, Tech, XP, list.ToArray(), RewardItemsGained);
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
