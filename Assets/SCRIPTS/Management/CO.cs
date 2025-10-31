
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
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

    private bool ShouldDriftersMove = false;
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
                    local.GetPlayer().EquipArmorLocallyRpc(local.GetPlayer().EquippedArmor ? local.GetPlayer().EquippedArmor.GetItemResourceIDShort() : null);
                    for (int i = 0; i < 3; i++)
                    {
                        local.GetPlayer().EquipWeaponLocallyRpc(i, local.GetPlayer().EquippedWeapons[i] ? local.GetPlayer().EquippedWeapons[i].GetItemResourceIDShort() : null);
                        local.GetPlayer().EquipArtifactLocallyRpc(i, local.GetPlayer().EquippedArtifacts[i] ? local.GetPlayer().EquippedArtifacts[i].GetItemResourceIDShort() : null);
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
            if (CO.co.Drifter_Inventory[i]) strings.Add(CO.co.Drifter_Inventory[i].GetItemResourceIDFull());
            else strings.Add("");
        }
        CO.co.SetDrifterInventoryForClientsRpc(strings.ToArray());
    }
    [Rpc(SendTo.Server)]
    public void RequestModuleUpdateRpc()
    {
        UI.ui.InventoryUI.RefreshDrifterSubscreen();
        UI.ui.InventoryUI.RefreshDrifterWeaponSubscreen();
    }
    public void StartGame()
    {
        GenerateMap(15);

        //NEW GAME
        Resource_Materials.Value = 50;
        Resource_Supplies.Value = 50;
        Resource_Ammo.Value = 50;
        Resource_Tech.Value = 0;
    }
    [Rpc(SendTo.Server)]
    public void AddInventoryItemRpc(FixedString64Bytes moduleLink)
    {
        //Server only
        Drifter_Inventory.Add(Resources.Load<ScriptableEquippable>(moduleLink.ToString()));
    }
    public void AddInventoryItem(ScriptableEquippable equip)
    {
        //Server only
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
        HandleGravity();
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

    [Rpc(SendTo.Server)]
    public void BoardingManeuverRpc()
    {
        foreach (DRIFTER drift in GetAllDrifters())
        {
            drift.CurrentPositionTimer = UnityEngine.Random.Range(15f, 20f);
            GravityResposition(drift, 40f, 50f);
        }
    }
    [Rpc(SendTo.Server)]
    public void EvasiveManeuverRpc()
    {
        foreach (DRIFTER drift in GetAllDrifters())
        {
            drift.CurrentPositionTimer = UnityEngine.Random.Range(15f, 20f);
            GravityResposition(drift, 150f, 200f);
        }
    }
    void GravityResposition(DRIFTER drift, float mindis, float maxdis)
    {
        Vector3 CurrentCenter = PlayerMainDrifter.CurrentLocationPoint;
        if (PlayerMainDrifter == drift)
        {
            drift.SetMoveTowards(Vector3.zero);
        }
        else
        {
            Vector3 ToLocation = PlayerMainDrifter.getLookVector();
            Vector3 LeftDir = new Vector3(-ToLocation.y, ToLocation.x);
            Vector3 RightDir = new Vector3(ToLocation.y, -ToLocation.x);
            float distLeft = Vector3.Distance(drift.transform.position, CurrentCenter + LeftDir);
            float distRight = Vector3.Distance(drift.transform.position, CurrentCenter + RightDir);
            Vector3 FlankDir = distLeft < distRight ? LeftDir : RightDir;
            Vector3 Deviate = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
            drift.SetMoveTowards(CurrentCenter + FlankDir.normalized * UnityEngine.Random.Range(mindis, maxdis) + Deviate);
            /*Vector3 Deviation = new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), UnityEngine.Random.Range(-0.4f, 0.4f));
            Vector3 TowardsDrifter = (drift.CurrentLocationPoint - CurrentCenter).normalized;
            Vector3 AroundPoint = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
            if (boardingAction)
            {
                Deviation = new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f));
                Vector3 ToLocation = (drift.CurrentLocationPoint - CurrentCenter).normalized;
                Vector3 LeftDir = new Vector3(-ToLocation.y, ToLocation.x);
                Vector3 RightDir = new Vector3(ToLocation.y, -ToLocation.x);
                float distLeft = Vector3.Distance(drift.transform.position, CurrentCenter + LeftDir);
                float distRight = Vector3.Distance(drift.transform.position, CurrentCenter + RightDir);
                Vector3 FlankDir = distLeft < distRight ? LeftDir : RightDir;
                drift.SetMoveTowards(CurrentCenter + (FlankDir + Deviation).normalized * UnityEngine.Random.Range(mindis, maxdis) + AroundPoint);
            }
            else drift.SetMoveTowards(CurrentCenter + (TowardsDrifter + Deviation).normalized * UnityEngine.Random.Range(mindis, maxdis) + AroundPoint);*/
        }
    }
    void HandleGravity() //Gravity Relativity
    {
        if (PlayerMainDrifter == null) return;
        if (!ShouldDriftersMove) return;
        Vector3 BackgroundSpeed = -PlayerMainDrifter.getLookVector();
        float CurrentSpeed = 99f;
        foreach (DRIFTER drift in GetAllDrifters())
        {
            CurrentSpeed = Mathf.Min(drift.GetCurrentMovement(), CurrentSpeed);
            drift.CurrentPositionTimer -= CO.co.GetWorldSpeedDelta();
            if (drift.CurrentPositionTimer < 0)
            {
                drift.CurrentPositionTimer = UnityEngine.Random.Range(10f, 15f);
                if (UnityEngine.Random.Range(0f,1f) < 0.4f || IsSafe())
                {
                    GravityResposition(drift, 40f, 50f);
                } else
                {
                    GravityResposition(drift, 80f, 160f);
                }
            }
        }
        BackgroundSpeed *= Mathf.Max(0, CurrentSpeed);
        BackgroundTransform.back.AddPosition(BackgroundSpeed * CO.co.GetWorldSpeedDelta() * 5f);
    }

    IEnumerator Travel(MapPoint destination)
    {
        Vector3 moveDirection = destination.transform.position - GetPlayerMapPoint().transform.position;
        PlayerMainDrifter.SetLookTowards(moveDirection);
        PlayerMainDrifter.SetMoveInput(moveDirection);
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
        PlayerMainDrifter.SetMoveInput(Vector3.zero);

        ClearMap();
    }

    private void ClearMap()
    {
        //CLEAR MAP
        BackgroundTransform.back.ResetPosition();
        foreach (CREW crew in new List<CREW>(GetAllCrews()))
        {
            if (crew.isDeadForever())
            {
                crew.DespawnAndUnregister();
            }
            else if (crew.Space != PlayerMainDrifter.Space || crew.isDead())
            {
                if (crew.GetFaction() == 1) transform.position = PlayerMainDrifter.Interior.Bridge;
                else crew.DespawnAndUnregister();
            }
        }
        foreach (DRIFTER drift in new List<DRIFTER>(GetAllDrifters()))
        {
            if (drift != PlayerMainDrifter)
            {
                drift.DespawnAndUnregister();
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

    private int GetMapWidth() { return 20; }
    private int GetPointStep() { return 5; }
    public void GenerateMap(float mapSize)
    {
        foreach (MapPoint map in GetMapPoints())
        {
            map.NetworkObject.Despawn();
        }
        RegisteredMapPoints = new();
        //StartPoint
        PlayerMapPointID.Value = 0;
        float MapWidth = GetMapWidth();
        MapPoint mapPoint = CO_SPAWNER.co.CreateMapPoint(new Vector3(-GetPointStep(), MapWidth * 0.5f));
        RegisterMapPoint(mapPoint);
       // mapPoint.Init(CurrentBiome.PossiblePointsRandom[UnityEngine.Random.Range(0,CurrentBiome.PossiblePointsRandom.Count)], 0);
        for (int i = 0; i < mapSize; i++)
        {
            int max = UnityEngine.Random.Range(2, 5);
            for (int amn = 0; amn < max; amn++)
            {
                // 5
                // 2.5 7.5
                // 3.33 6.67
                Vector3 tryPos = new Vector3(i* GetPointStep(), (amn + 1) * (MapWidth / max) - (MapWidth / max));
                mapPoint = CO_SPAWNER.co.CreateMapPoint(tryPos);
                RegisterMapPoint(mapPoint);
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
    }

    private void UpdateMapConnections()
    {
        foreach (MapPoint map in GetMapPoints())
        {
            map.ConnectedPoints = GetConnectedPoints(map.transform.position, map);
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
        /*MapPoint Closest1 = null;
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
        if (Closest2 && !list.Contains(Closest2)) list.Add(Closest2);*/
        List<MapPoint> list = new();
        foreach (MapPoint map in GetMapPoints())
        {
            if (map == us) continue;
            if (map.transform.position.x > center.x - 0.5f + GetPointStep() && map.transform.position.x < center.x + 0.5f + GetPointStep()) list.Add(map);
        }
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
        Debug.Log("Register drifter?");
        if (RegisteredDRIFTER.Contains(drifter)) return;
        RegisteredDRIFTER.Add(drifter);
        Debug.Log("Main drifter?");
        if (drifter.IsMainDrifter.Value)
        {
            PlayerMainDrifter = drifter;
            Debug.Log("Main drifter detected and set!");
        }
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
            case "GenericLoot":
                StartCoroutine(Event_GenericLoot());
                break;
            case "GenericBattle":
                StartCoroutine(Event_GenericBattle());
                break;
            case "GenericSurvival":
                StartCoroutine(Event_GenericSurvival());
                break;
        }
    }
    IEnumerator Event_GenericLoot()
    {
        ShouldDriftersMove = true;
        ProcessLootTable(CurrentEvent.LootTable, 1f);
        yield break;
    }
    IEnumerator Event_GenericBattle()
    {
        ShouldDriftersMove = true;
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
        ScriptableEnemyGroup EnemyGroup = Groups[GetWeight()].EnemyGroup;
        DRIFTER enemyDrifter = CO_SPAWNER.co.SpawnEnemyGroup(EnemyGroup, 1f);
        if (enemyDrifter == null)
        {
            float Death = 0f;
            while (Death < 1)
            {
                yield return new WaitForSeconds(0.5f);
                int DeadAmount = GroupDeathAmount(GetEnemyCrew());
                int AliveAmount = GetEnemyCrew().Count - DeadAmount;
                Death = (float)DeadAmount / (float)GetEnemyCrew().Count;
                EnemyBarRelative.Value = 1f - Death;
                EnemyBarString.Value = $"THREATS: {AliveAmount}";
            }
        } else
        {
            float Death = 0f;
            while (Death < 1)
            {
                yield return new WaitForSeconds(0.5f);
                int DeadAmount = GroupDeathAmount(GetEnemyCrew());
                int AliveAmount = GetEnemyCrew().Count - DeadAmount;
                Death = (float)DeadAmount / (float)GetEnemyCrew().Count;
                EnemyBarRelative.Value = enemyDrifter.GetHealthRelative();
                if (enemyDrifter.isDead()) EnemyBarString.Value = $"DRIFTER DISABLED";
                else EnemyBarString.Value = $"INTEGRITY: {enemyDrifter.GetHealth().ToString("0")}";
            }
        }

        EnemyBarRelative.Value = -1;

        if (enemyDrifter)
        {
            enemyDrifter.Disable();
        }

        yield return new WaitForSeconds(5f);
        LOCALCO.local.CinematicTexRpc("THREATS ELIMINATED");

        yield return new WaitForSeconds(3f);
        if (CurrentEvent.DebriefDialog) CO_STORY.co.SetStory(CurrentEvent.DebriefDialog);
        AreWeInDanger.Value = false;
        ProcessLootTable(CurrentEvent.LootTable, 1f);
    }
    IEnumerator Event_GenericSurvival()
    {
        ShouldDriftersMove = true;
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
        ScriptableEnemyGroup EnemyGroup = Groups[GetWeight()].EnemyGroup;
        DRIFTER enemyDrifter = CO_SPAWNER.co.SpawnEnemyGroup(EnemyGroup, 1f);
        float Death = 0f;
        float MaxTimer = 60f;
        float Timer = MaxTimer;
        while (Death < 1 && Timer > 0)
        {
            yield return new WaitForSeconds(0.5f);
            Timer -= 0.5f;
            int DeadAmount = GroupDeathAmount(GetEnemyCrew());
            int AliveAmount = GetEnemyCrew().Count - DeadAmount;
            Death = (float)DeadAmount / (float)GetEnemyCrew().Count;
            EnemyBarRelative.Value = Timer/MaxTimer;
            EnemyBarString.Value = $"SURVIVE: {Timer.ToString("0")}";
        }

        EnemyBarRelative.Value = -1;

        if (enemyDrifter)
        {
            enemyDrifter.Disable();
        }

        yield return new WaitForSeconds(5f);
        LOCALCO.local.CinematicTexRpc("THREATS ELIMINATED");

        yield return new WaitForSeconds(3f);
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

    public Transform GetTransformAtPoint(Vector3 vec)
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(vec, 0.1f))
        {
            if (col.GetComponent<SPACE>() != null)
            {
                return col.transform;
            }
        }
        return null;
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
            i++;
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
                ResetWeights();
                i = 0;
                foreach (WeightedLootItem weighted in item.ItemDrop.PossibleDrops)
                {
                    AddWeights(i, weighted.Weight);
                    i++;
                }
                ScriptableEquippable newItem = item.ItemDrop.PossibleDrops[GetWeight()].Item;
                AddInventoryItem(newItem);
                ItemTranslate.Add(newItem.GetItemResourceIDFull());
            }
        }
        Resource_Ammo.Value += ChangeAmmo;
        Resource_Materials.Value += ChangeMaterials;
        Resource_Supplies.Value += ChangeSupplies;
        Resource_Tech.Value += ChangeTech;
        foreach (CREW crew in GetAlliedCrew())
        {
            crew.AddXP(ChangeXP);
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
