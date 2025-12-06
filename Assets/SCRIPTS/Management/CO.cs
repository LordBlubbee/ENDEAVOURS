
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using static AUDCO;

public class CO : NetworkBehaviour
{
    public static CO co;

    [NonSerialized] public NetworkVariable<bool> HasShipBeenLaunched = new();
    [NonSerialized] public NetworkVariable<bool> AreWeInDanger = new();
    [NonSerialized] public NetworkVariable<bool> AreWeResting = new();
    [NonSerialized] public NetworkVariable<bool> AreWeCrafting = new();
    [NonSerialized] public NetworkVariable<bool> CommunicationGamePaused = new();
    [NonSerialized] public NetworkVariable<float> EnemyBarRelative = new(-1);
    [NonSerialized] public NetworkVariable<FixedString32Bytes> EnemyBarString = new("");
    [NonSerialized] public DRIFTER PlayerMainDrifter;
    [NonSerialized] public NetworkVariable<int> PlayerMapPointID = new();

    private bool ShouldDriftersMove = false;
    public bool AreDriftersMoving()
    {
        return ShouldDriftersMove;
    }
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

    public int GetDrifterRepairCost()
    {
        return 4;
    }

    [Rpc(SendTo.Server)]
    public void RepairOurDrifterRpc()
    {
        if (PlayerMainDrifter.GetHealthRelative() < 1)
        {
            if (Resource_Materials.Value > GetDrifterRepairCost())
            {
                Resource_Materials.Value -= GetDrifterRepairCost();
                PlayerMainDrifter.Heal(100);
            }
        }
    }

    private List<ShopItem> ShopItems = new();
    public List<ShopItem> GetShopItems() { return ShopItems; }
    public void AddShopItem(ShopItem item) { ShopItems.Add(item); }
    public void RemoveShopItem(ShopItem item) { ShopItems.Remove(item); }
    public void CreateShopItem(ScriptableShopitem item)
    {
        ShopItem shop = Instantiate(CO_SPAWNER.co.PrefabShopItem,Vector3.zero,Quaternion.identity);
        shop.Init(item); //This also adds to list

        float Factor = 1f;
        if (item.IsCrafted)
        {
            Factor = GetAlchemyCraftingModifier();
        }
        shop.MaterialCost.Value = Mathf.RoundToInt(UnityEngine.Random.Range(0.8f,1.2f) * item.DealMaterialsCost * Factor);
        shop.SupplyCost.Value = Mathf.RoundToInt(UnityEngine.Random.Range(0.8f, 1.2f) * item.DealSuppliesCost * Factor);
        shop.AmmoCost.Value = Mathf.RoundToInt(UnityEngine.Random.Range(0.8f, 1.2f) * item.DealAmmoCost * Factor);
        shop.TechCost.Value = Mathf.RoundToInt(UnityEngine.Random.Range(0.8f, 1.2f) * item.DealTechCost * Factor);
        shop.NetworkObject.Spawn();
    }
    public float GetAlchemyCraftingModifier()
    {
        int AlchemySkill = 0;
        foreach (CREW crew in GetAlliedCrew())
        {
            AlchemySkill = Mathf.Max(AlchemySkill, crew.GetATT_ALCHEMY());
        }
        float Factor;
        switch (AlchemySkill)
        {
            case < 1:
                Factor = 1f;
                break;
            case 1:
                Factor = 0.99f;
                break;
            case 2:
                Factor = 0.98f;
                break;
            default:
                Factor = 1.08f - AlchemySkill * 0.04f;
                break;
        }
        return Mathf.Clamp(Factor,0.25f,1f);
    }

    public void ResetShopList()
    {
        foreach (ShopItem shop in new List<ShopItem>(GetShopItems()))
        {
            shop.NetworkObject.Despawn();
        }
        ShopItems = new();
    }

    [NonSerialized] public NetworkVariable<int> Resource_Materials = new();
    [NonSerialized] public NetworkVariable<int> Resource_Supplies = new();
    [NonSerialized] public NetworkVariable<int> Resource_Ammo = new();
    [NonSerialized] public NetworkVariable<int> Resource_Tech = new();
    [NonSerialized] public Dictionary<Faction, int> Resource_Reputation = new();
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
    public int BiomeProgress;
    public int TotalZoneProgress;
    private float BaseDifficulty = 1f;
    public List<ScriptablePoint> NextBiomePoints = new();

    public List<ScriptablePoint> GetPossibleDestinations()
    {
        return NextBiomePoints;
    }

    public float GetEncounterDifficultyModifier()
    {
        float ProgressDiff = 1f;
        switch (BiomeProgress)
        {
            case 0:
                ProgressDiff = 1f;
                break;
            case 1:
                ProgressDiff = 2f;
                break;
            case 2:
                ProgressDiff = 2.5f;
                break;
            case 3:
                ProgressDiff = 3.8f;
                break;
            case 4:
                ProgressDiff = 4.5f;
                break;
            case 5:
                ProgressDiff = 6f;
                break;
            case 6:
                ProgressDiff = 6.5f;
                break;
        }
        float PlayerDiff = 1f;
        switch (GetLOCALCO().Count)
        {
            case 1:
                PlayerDiff = 0.8f;
                break;
            case 2:
                PlayerDiff = 1f;
                break;
            case 3:
                PlayerDiff = 1.15f;
                break;
            case 4:
                PlayerDiff = 1.3f;
                break;
            case 5:
                PlayerDiff = 1.4f;
                break;
            case 6:
                PlayerDiff = 1.5f;
                break;
            case 7:
                PlayerDiff = 1.6f;
                break;
            case 8:
                PlayerDiff = 1.7f;
                break;
            case 9:
                PlayerDiff = 1.9f;
                break;
            case 10:
                PlayerDiff = 2f;
                break;
        }
        return 1f * ProgressDiff * PlayerDiff * BaseDifficulty * CurrentBiome.BiomeBaseDifficulty;
    }
    public float GetNewFriendlyCrewModifier()
    {
        float ProgressDiff = 1f;
        switch (BiomeProgress)
        {
            case 0:
                ProgressDiff = 1f;
                break;
            case 1:
                ProgressDiff = 2f;
                break;
            case 2:
                ProgressDiff = 3f;
                break;
            case 3:
                ProgressDiff = 3.8f;
                break;
            case 4:
                ProgressDiff = 4.5f;
                break;
            case 5:
                ProgressDiff = 6f;
                break;
            case 6:
                ProgressDiff = 6.5f;
                break;
        }
        float PlayerDiff = 1f;
        switch (GetLOCALCO().Count)
        {
            case 1:
                PlayerDiff = 1f;
                break;
            case 2:
                PlayerDiff = 1f;
                break;
            case 3:
                PlayerDiff = 1.1f;
                break;
            case 4:
                PlayerDiff = 1.2f;
                break;
            case 5:
                PlayerDiff = 1.25f;
                break;
            case 6:
                PlayerDiff = 1.3f;
                break;
            case 7:
                PlayerDiff = 1.35f;
                break;
            case 8:
                PlayerDiff = 1.4f;
                break;
            case 9:
                PlayerDiff = 1.45f;
                break;
            case 10:
                PlayerDiff = 1.5f;
                break;
        }
        return 1f * ProgressDiff * PlayerDiff * BaseDifficulty * CurrentBiome.BiomeBaseDifficulty;
    }
    public float GetDrifterDifficultyModifier()
    {
        float ProgressDiff = 1f;
        switch (BiomeProgress)
        {
            case 0:
                ProgressDiff = 1f;
                break;
            case 1:
                ProgressDiff = 1.5f;
                break;
            case 2:
                ProgressDiff = 2f;
                break;
            case 3:
                ProgressDiff = 2.8f;
                break;
            case 4:
                ProgressDiff = 3.5f;
                break;
            case 5:
                ProgressDiff = 5.5f;
                break;
            case 6:
                ProgressDiff = 6f;
                break;
        }
        float PlayerDiff = 1f;
        switch (GetLOCALCO().Count)
        {
            case 1:
                PlayerDiff = 0.9f;
                break;
            case 2:
                PlayerDiff = 1f;
                break;
            case 3:
                PlayerDiff = 1.05f;
                break;
            case 4:
                PlayerDiff = 1.1f;
                break;
            case 5:
                PlayerDiff = 1.15f;
                break;
            case 6:
                PlayerDiff = 1.2f;
                break;
            case 7:
                PlayerDiff = 1.25f;
                break;
            case 8:
                PlayerDiff = 1.3f;
                break;
            case 9:
                PlayerDiff = 1.35f;
                break;
            case 10:
                PlayerDiff = 1.4f;
                break;
        }
        return 1f * ProgressDiff * PlayerDiff * BaseDifficulty * CurrentBiome.BiomeBaseDifficulty;
    }
    public float GetEncounterSizeModifier()
    {
        float ProgressDiff = 1f;
        switch (BiomeProgress)
        {
            case 0:
                ProgressDiff = 0.85f;
                break;
            case 1:
                ProgressDiff = 1f;
                break;
            case 2:
                ProgressDiff = 1.2f;
                break;
            case 3:
                ProgressDiff = 1.5f;
                break;
            case 4:
                ProgressDiff = 1.8f;
                break;
            case 5:
                ProgressDiff = 2.0f;
                break;
            case 6:
                ProgressDiff = 2.2f;
                break;
        }
        float PlayerDiff = 1f;
        switch (GetLOCALCO().Count)
        {
            case 1:
                PlayerDiff = 0.6f;
                break;
            case 2:
                PlayerDiff = 0.9f;
                break;
            case 3:
                PlayerDiff = 1.2f;
                break;
            case 4:
                PlayerDiff = 1.4f;
                break;
            case 5:
                PlayerDiff = 1.7f;
                break;
            case 6:
                PlayerDiff = 1.9f;
                break;
            case 7:
                PlayerDiff = 2.1f;
                break;
            case 8:
                PlayerDiff = 2.25f;
                break;
            case 9:
                PlayerDiff = 2.4f;
                break;
            case 10:
                PlayerDiff = 2.5f;
                break;
        }
        return 1f * ProgressDiff * PlayerDiff * BaseDifficulty * CurrentBiome.BiomeBaseDifficulty;
    }
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

    public ScriptableLootItemList Test_StartingLoot;
    public void StartGame()
    {
        GenerateMap();
        CO_SPAWNER.co.CreateLandscape(CO_SPAWNER.BackgroundType.BARREN);

        //NEW GAME
        Resource_Materials.Value = 50;
        Resource_Supplies.Value = 50;
        Resource_Ammo.Value = 50;
        Resource_Tech.Value = 0;

        if (Test_StartingLoot != null)
        {
            int test = 0;
            foreach (WeightedLootItem lootItem in Test_StartingLoot.CommonDrops)
            {
                test++;
                if (test < 15) continue;
                AddInventoryItem(lootItem.Item);
            }
        }

        CO_STORY.co.SetStory(GetPlayerMapPoint().AssociatedPoint.InitialDialog);
        SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.InitialSoundtrack);
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
        if (!IsServer)
        {
            RefreshWeather(TerrainDaytime.Value, TerrainWeather.Value);
        }
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

    [NonSerialized] public NetworkVariable<int> TerrainDaytime = new();
    [NonSerialized] public NetworkVariable<int> TerrainWeather = new();

    public enum DayTimes
    {
        DAY,
        DUSK,
        NIGHT
    }
    public enum WeatherTypes
    {
        CLEAR,
        SOME_CLOUDS,
        CLOUDY,
        DRIZZLE,
        RAIN,
        THUNDERSTORM
    }

    public enum MapWeatherSelectors
    {
        RANDOM_MILD,
        RANDOM_CLEAR,
        RANDOM_HEAVY,
        ALWAYS_DUSK,
        ALWAYS_THUNDER,
        RANDOM_MILD_NIGHT,
        RANDOM_CLEAR_NIGHT,
        RANDOM_HEAVY_NIGHT,
    }
    private void RefreshWeather(int Daytime, int Weather)
    {
        CAM.cam.SetTimeOfDay((DayTimes)Daytime, (WeatherTypes)Weather);
        if ((WeatherTypes)Weather == WeatherTypes.THUNDERSTORM && !IsManagingThunder)
        {
            StartCoroutine(ThunderManager());
        }
    }

    bool IsManagingThunder = false;
    IEnumerator ThunderManager()
    {
        //Runs on server
        IsManagingThunder = true;
        float Timer = UnityEngine.Random.Range(5f,30f);
        while (TerrainWeather.Value == (int)WeatherTypes.THUNDERSTORM)
        {
            Timer -= CO.co.GetWorldSpeedDelta();
            if (Timer < 0f)
            {
                Timer = UnityEngine.Random.Range(2f, 60f);
                CO_SPAWNER.co.SpawnThunderRpc(new Vector3(UnityEngine.Random.Range(-200f, 200f), UnityEngine.Random.Range(-200f, 200f)));
            }
            yield return null;
        }
        IsManagingThunder = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RefreshWeatherRpc(int Daytime, int Weather)
    {
        RefreshWeather(Daytime,Weather);
    }
    IEnumerator Travel(MapPoint destination)
    {
        ShouldDriftersMove = true;
        AreWeResting.Value = false;
        AreWeCrafting.Value = false;
        ResetShopList();
        Vector3 moveDirection = destination.transform.position - GetPlayerMapPoint().transform.position;
        PlayerMainDrifter.SetLookTowards(moveDirection);
        PlayerMainDrifter.SetMoveInput(moveDirection);
        PlayerMainDrifter.SetCanReceiveInput(false);
        foreach (LOCALCO local in GetLOCALCO())
        {
            local.ShipTransportFadeAwayRpc(destination.GetNameOnly());
        }
        yield return new WaitForSeconds(2.5f);
        GenerateLevel();

        ResetWeights();
        switch (destination.AssociatedPoint.WeatherSelectors)
        {
            default:
                AddWeights((int)DayTimes.DAY, 60);
                AddWeights((int)DayTimes.NIGHT, 30);
                AddWeights((int)DayTimes.DUSK, 10);
                break;
            case MapWeatherSelectors.ALWAYS_DUSK:
                AddWeights((int)DayTimes.DUSK, 100);
                break;
            case MapWeatherSelectors.ALWAYS_THUNDER:
                AddWeights((int)DayTimes.DAY, 20);
                AddWeights((int)DayTimes.NIGHT, 80);
                break;
            case MapWeatherSelectors.RANDOM_MILD_NIGHT:
                AddWeights((int)DayTimes.NIGHT, 100);
                break;
            case MapWeatherSelectors.RANDOM_CLEAR_NIGHT:
                AddWeights((int)DayTimes.NIGHT, 100);
                break;
            case MapWeatherSelectors.RANDOM_HEAVY_NIGHT:
                AddWeights((int)DayTimes.NIGHT, 100);
                break;
        }
        TerrainDaytime.Value = GetWeight();
        ResetWeights();
        switch (destination.AssociatedPoint.WeatherSelectors)
        {
            case MapWeatherSelectors.RANDOM_MILD:
                AddWeights((int)WeatherTypes.CLEAR, 40);
                AddWeights((int)WeatherTypes.SOME_CLOUDS, 25);
                AddWeights((int)WeatherTypes.CLOUDY, 15);
                AddWeights((int)WeatherTypes.DRIZZLE, 10);
                AddWeights((int)WeatherTypes.RAIN, 10);
                break;
            case MapWeatherSelectors.RANDOM_CLEAR:
                AddWeights((int)WeatherTypes.CLEAR, 70);
                AddWeights((int)WeatherTypes.SOME_CLOUDS, 30);
                break;
            case MapWeatherSelectors.RANDOM_HEAVY:
                AddWeights((int)WeatherTypes.CLOUDY, 30);
                AddWeights((int)WeatherTypes.RAIN, 50);
                AddWeights((int)WeatherTypes.THUNDERSTORM, 20);
                break;
            case MapWeatherSelectors.RANDOM_MILD_NIGHT:
                AddWeights((int)WeatherTypes.CLEAR, 35);
                AddWeights((int)WeatherTypes.SOME_CLOUDS, 25);
                AddWeights((int)WeatherTypes.CLOUDY, 15);
                AddWeights((int)WeatherTypes.DRIZZLE, 10);
                AddWeights((int)WeatherTypes.RAIN, 10);
                break;
            case MapWeatherSelectors.RANDOM_CLEAR_NIGHT:
                AddWeights((int)WeatherTypes.CLEAR, 70);
                AddWeights((int)WeatherTypes.SOME_CLOUDS, 30);
                break;
            case MapWeatherSelectors.RANDOM_HEAVY_NIGHT:
                AddWeights((int)WeatherTypes.CLOUDY, 30);
                AddWeights((int)WeatherTypes.RAIN, 50);
                AddWeights((int)WeatherTypes.THUNDERSTORM, 20);
                break;
            case MapWeatherSelectors.ALWAYS_DUSK:
                AddWeights((int)WeatherTypes.CLEAR, 40);
                AddWeights((int)WeatherTypes.SOME_CLOUDS, 15);
                AddWeights((int)WeatherTypes.CLOUDY, 15);
                AddWeights((int)WeatherTypes.DRIZZLE, 25);
                AddWeights((int)WeatherTypes.RAIN, 5);
                break;
            case MapWeatherSelectors.ALWAYS_THUNDER:
                AddWeights((int)WeatherTypes.THUNDERSTORM, 100);
                break;
        }
        TerrainWeather.Value = GetWeight();
        RefreshWeatherRpc(TerrainDaytime.Value, TerrainWeather.Value);

        if (destination.AssociatedPoint.GateToBiome)
        {
            CurrentBiome = destination.AssociatedPoint.GateToBiome;
            BiomeProgress++;
            GenerateMap();
            destination = RegisteredMapPoints[0];
        }
        CO_SPAWNER.co.CreateLandscape(destination.AssociatedPoint.BackgroundType);
        if (destination.AssociatedPoint.InitialDialog) CO_STORY.co.SetStory(destination.AssociatedPoint.InitialDialog);
        SetCurrentSoundtrack(destination.AssociatedPoint.InitialSoundtrack);

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
                if (crew.GetFaction() == 1) crew.TeleportCrewMember(PlayerMainDrifter.Interior.transform.TransformPoint( PlayerMainDrifter.Interior.Bridge), PlayerMainDrifter.Interior);
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
        foreach (SPACE space in new List<SPACE>(GetAllSpaces()))
        {
            if (space == null)
            {
                GetAllSpaces().Remove(space);
                continue;
            }
            DUNGEON drift = space.GetComponent<DUNGEON>();
            if (drift)
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
        if (PlayerMainDrifter == null) return -1;
        if (GetAlliedAICrew().Count > PlayerMainDrifter.MaximumCrew) return -1;
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

    public int VoteAmount()
    {
        int num = 0;
        foreach (LOCALCO local in GetLOCALCO())
        {
            if (local.CurrentMapVote.Value != -1 || local.CurrentDialogVote.Value != -1)
            {
                num++;
            }
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
    public void GenerateMap()
    {
        foreach (MapPoint map in GetMapPoints())
        {
            map.NetworkObject.Despawn();
        }
        float mapSize = CurrentBiome.GetBiomeSize();
        RegisteredMapPoints = new();
        List<MapPoint> MustBeInitialized = new();
        //StartPoint
        PlayerMapPointID.Value = 0;
        float MapWidth = GetMapWidth();
        MapPoint mapPoint = CO_SPAWNER.co.CreateMapPoint(new Vector3(-GetPointStep(), MapWidth * 0.5f));
        MustBeInitialized.Add(mapPoint);
        RegisterMapPoint(mapPoint);
        mapPoint.Init(CurrentBiome.PossiblePointsArrival[UnityEngine.Random.Range(0, CurrentBiome.PossiblePointsArrival.Count)]);
        int xSteps = 0;
        for (xSteps = 0; xSteps < mapSize; xSteps++)
        {
            int max = UnityEngine.Random.Range(2, 5);
            for (int amn = 0; amn < max; amn++)
            {
                // Reversed Y-direction (top to bottom)
                float step = MapWidth / (max + 1);
                float yPos = MapWidth - step * (amn + 1);
                Vector3 tryPos = new Vector3(xSteps * GetPointStep(), yPos);

                mapPoint = CO_SPAWNER.co.CreateMapPoint(tryPos);
                MustBeInitialized.Add(mapPoint);
                RegisterMapPoint(mapPoint);
            }
        }

        mapPoint = CO_SPAWNER.co.CreateMapPoint(new Vector3(xSteps * GetPointStep(), MapWidth * 0.5f));
        RegisterMapPoint(mapPoint);
        MapPoint LastRestPoint = mapPoint;
        mapPoint.Init(CurrentBiome.PossiblePointsRest[UnityEngine.Random.Range(0, CurrentBiome.PossiblePointsRest.Count)]);

        xSteps++;
        List<ScriptablePoint> Destinations = new List<ScriptablePoint> (GetPossibleDestinations());

        mapPoint = CO_SPAWNER.co.CreateMapPoint(new Vector3((xSteps+1) * GetPointStep(), MapWidth * 1f));
        RegisterMapPoint(mapPoint);
        mapPoint.Init(Destinations[UnityEngine.Random.Range(0, Destinations.Count)]);

        mapPoint = CO_SPAWNER.co.CreateMapPoint(new Vector3((xSteps + 2) * GetPointStep(), MapWidth * 0.5f));
        RegisterMapPoint(mapPoint);
        mapPoint.Init(Destinations[UnityEngine.Random.Range(0, Destinations.Count)]);

        mapPoint = CO_SPAWNER.co.CreateMapPoint(new Vector3((xSteps + 1) * GetPointStep(), MapWidth * 0f));
        RegisterMapPoint(mapPoint);
        mapPoint.Init(Destinations[UnityEngine.Random.Range(0, Destinations.Count)]);

        float TotalPointsLeft = MustBeInitialized.Count;
        float Points = TotalPointsLeft * CurrentBiome.BiomeHostileRatio;
        while (Points > 0)
        {
            MapPoint map = MustBeInitialized[UnityEngine.Random.Range(0, MustBeInitialized.Count)];
            map.Init(CurrentBiome.PossiblePointsHostile[UnityEngine.Random.Range(0, CurrentBiome.PossiblePointsHostile.Count)]);
            MustBeInitialized.Remove(map);
            Points--;
        }
        Points = TotalPointsLeft * CurrentBiome.BiomeCalmRatio;
        while (Points > 0)
        {
            MapPoint map = MustBeInitialized[UnityEngine.Random.Range(0, MustBeInitialized.Count)];
            map.Init(CurrentBiome.PossiblePointsCalm[UnityEngine.Random.Range(0, CurrentBiome.PossiblePointsCalm.Count)]);
            MustBeInitialized.Remove(map);
            Points--;
        }
        foreach (MapPoint map in new List<MapPoint>(MustBeInitialized))
        {
            map.Init(CurrentBiome.PossiblePointsNeutral[UnityEngine.Random.Range(0, CurrentBiome.PossiblePointsNeutral.Count)]);
            MustBeInitialized.Remove(map);
        }
        UpdateMapConnections();
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
        List<MapPoint> list = new();
        foreach (MapPoint map in GetMapPoints())
        {
            if (map == us) continue;
            if (map.transform.position.x > center.x - 0.5f + GetPointStep() && map.transform.position.x < center.x + 0.5f + GetPointStep()) list.Add(map);
        }
        if (list.Count == 0) return GetFarPoints(center, us);
        //ChatGPT, could you make it resort list to go from top to bottom (in y position)?
        list.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        return list;
    }
    private List<MapPoint> GetFarPoints(Vector3 center, MapPoint us = null)
    {
        List<MapPoint> list = new();
        foreach (MapPoint map in GetMapPoints())
        {
            if (map == us) continue;
            if (map.transform.position.x > center.x + 0.5f) list.Add(map);
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
    public List<CREW> GetEnemyNonDormantCrew(int fac = 1)
    {
        List<CREW> list = new();
        foreach (CREW loc in GetEnemyCrew(fac))
        {
            if (!loc.IsNeutral)
            {
                list.Add(loc);
            }
        }
        return list;
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
    public List<CREW> GetAlliedAICrew()
    {
        List<CREW> list = new();
        foreach (CREW loc in GetAllCrews())
        {
            if (loc.IsPlayer()) continue;
            if (loc.GetFaction() == 1)
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
        if (IsServer) crew.Register(RegisteredMapPoints.Count);
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
            Debug.Log($"Registering new SPACE of {Space.name} at ID {SpaceID}");
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

    [Rpc(SendTo.ClientsAndHost)]
    private void ForceOpenMissionScreenRpc()
    {
        UI.ui.MainGameplayUI.ForceMissionScreenAfterStoryEnd();
    }

    public void PerformEvent(ScriptableEvent even)
    {
        //LOOOOOOOOOOOOOOOOOOOOOOOOONG LIST
        CurrentEvent = even;
        string str = even.EventController;
        switch (str)
        {
            case "":
                ForceOpenMissionScreenRpc();
                break;
            case "CombatDebrief":
                break;
            case "GenericRest":
                StartCoroutine(Event_GenericRest());
                ForceOpenMissionScreenRpc();
                break;
            case "GenericLoot":
                StartCoroutine(Event_GenericLoot());
                ForceOpenMissionScreenRpc();
                break;
            case "GenericBattle":
                StartCoroutine(Event_GenericBattle());
                break;
            case "DungeonStorage":
                StartCoroutine(Event_DungeonStorage());
                break;
            case "DungeonExtermination":
                StartCoroutine(Event_DungeonExtermination());
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
    IEnumerator Event_GenericRest()
    {
        ShouldDriftersMove = false;
        AreWeResting.Value = true;
        AreWeCrafting.Value = true;
        if (CurrentEvent.LootTable) ProcessLootTable(CurrentEvent.LootTable, 1f);

        yield break;
    }
    IEnumerator Event_GenericBattle()
    {
        ShouldDriftersMove = true;
        AreWeInDanger.Value = true;
        SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.CombatSoundtrack);
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
        DRIFTER enemyDrifter = CO_SPAWNER.co.SpawnEnemyGroup(EnemyGroup);
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

        yield return new WaitForSeconds(4f);
        LOCALCO.local.CinematicTexRpc("THREATS ELIMINATED");
        SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.InitialSoundtrack);

        yield return new WaitForSeconds(3f);
        EndEvent();
    }

    private bool AreCrewAwayFromHome()
    {
        foreach (CREW Crew in GetEnemyCrew())
        {
            if (Crew.isDead()) continue;
            if (Crew.Space == PlayerMainDrifter.Space) return true;
        }
        foreach (CREW Crew in GetAlliedCrew())
        {
            if (Crew.Space != PlayerMainDrifter.Space) return true;
        }
        return false;
    }

    DUNGEON CurrentDungeon = null;
    public void SetCurrentDungeon(DUNGEON Current)
    {
        CurrentDungeon = Current;
    }

    public DUNGEON GetDungeon()
    {
        return CurrentDungeon;
    }

    public bool CanRespawn(CREW un)
    {
        if (!AreWeInDanger.Value)
        {
            return un.GetFaction() == 1;
        }
        if (PlayerMainDrifter.MedicalModule.IsDisabled()) return false;
        if (GetDungeon() == null) return true;
        return true;
    }
    IEnumerator Event_DungeonStorage()
    {
        ShouldDriftersMove = false;
        AreWeInDanger.Value = false;
        SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.CombatSoundtrack);
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
        CO_SPAWNER.co.SpawnEnemyGroup(EnemyGroup);

        List<Vault> Vaults = CO_SPAWNER.co.SpawnVaultObjectives(CurrentDungeon);
        AlternativeDebriefDialog Debrief = null;
        bool MissionCompleted = false;
        float Death = 0f;
        while (Death < 1 || !MissionCompleted)
        {
            int DeadAmount = GroupDeathAmount(GetEnemyCrew());
            int AliveAmount = GetEnemyCrew().Count - DeadAmount;
            Death = (float)DeadAmount / (float)GetEnemyCrew().Count;

            int Threats = GetEnemyNonDormantCrew().Count;

            EnemyBarRelative.Value = 1f - Death;
            if (AreCrewAwayFromHome())
            {
                AreWeInDanger.Value = true; 
                if (Threats > 0) EnemyBarString.Value = $"THREATS: {Threats}";
            } else
            {
                AreWeInDanger.Value = false; 
                EnemyBarRelative.Value = 1f - Death;
                EnemyBarString.Value = $"REPAIR VAULT";
            }
            if (Vaults[0].GetHealthRelative() >= 1)
            {
                EnemyBarString.Value = $"VAULT SECURED";
                if (!MissionCompleted)
                {
                    MissionCompleted = true;
                    CommunicationGamePaused.Value = true;
                    if (CurrentEvent.HasDebrief()) {
                        Debrief = CurrentEvent.GetDebrief();
                        CO_STORY.co.SetStory(Debrief.ReplaceDialog);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
            if (ShouldDriftersMove)
            {
                break;
            }
        }

        EnemyBarRelative.Value = -1;
        if (!ShouldDriftersMove)
        {
            yield return new WaitForSeconds(4f);
            if (Death >= 1)
            {
                LOCALCO.local.CinematicTexRpc("THREATS ELIMINATED");
            }
            SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.InitialSoundtrack);

            yield return new WaitForSeconds(3f);
            AreWeInDanger.Value = false;
            if (Debrief != null) ProcessLootTable(Debrief.AlternativeLoot, 1f);
        }
        SetCurrentDungeon(null);
    }
    IEnumerator Event_DungeonExtermination()
    {
        ShouldDriftersMove = false;
        AreWeInDanger.Value = false;
        SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.CombatSoundtrack);
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
        CO_SPAWNER.co.SpawnEnemyGroup(EnemyGroup);

        bool MissionCompleted = false;
        float Death = 0f;
        while (Death < 1 && !ShouldDriftersMove)
        {
            int DeadAmount = GroupDeathAmount(GetEnemyCrew());
            int AliveAmount = GetEnemyCrew().Count - DeadAmount;
            Death = (float)DeadAmount / (float)GetEnemyCrew().Count;

            int Threats = GetEnemyNonDormantCrew().Count;

            EnemyBarRelative.Value = 1f - Death;
            if (AreCrewAwayFromHome())
            {
                AreWeInDanger.Value = true;
                if (Threats > 0) EnemyBarString.Value = $"THREATS: {Threats}";
            }
            else
            {
                AreWeInDanger.Value = false;
                EnemyBarRelative.Value = 1f - Death;
                EnemyBarString.Value = $"EXPLORE DUNGEON";
            }
            if (Death == 1) MissionCompleted = true;
            yield return new WaitForSeconds(0.5f);
        }

        EnemyBarRelative.Value = -1;
        if (!ShouldDriftersMove)
        {
            yield return new WaitForSeconds(4f);
            if (Death >= 1)
            {
                LOCALCO.local.CinematicTexRpc("THREATS ELIMINATED");
            }
            SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.InitialSoundtrack);

            yield return new WaitForSeconds(3f);
            EndEvent(MissionCompleted);
        }
    }
    IEnumerator Event_GenericSurvival()
    {
        ShouldDriftersMove = true;
        AreWeInDanger.Value = true;
        SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.CombatSoundtrack);
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
        DRIFTER enemyDrifter = CO_SPAWNER.co.SpawnEnemyGroup(EnemyGroup);
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

        yield return new WaitForSeconds(4f);
        LOCALCO.local.CinematicTexRpc("THREATS ELIMINATED");
       SetCurrentSoundtrack(GetPlayerMapPoint().AssociatedPoint.InitialSoundtrack);

        yield return new WaitForSeconds(3f);
        EndEvent();
    }

    private void EndEvent(bool giveLoot = true)
    {
        AlternativeDebriefDialog Debrief = CurrentEvent.GetDebrief();
        if (CurrentEvent.HasDebrief()) CO_STORY.co.SetStory(Debrief.ReplaceDialog);
        AreWeInDanger.Value = false;
        if (giveLoot) ProcessLootTable(Debrief.AlternativeLoot, 1f);
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

        CREW NewCrewPrefab = table.GetNewCrew();
        CREW NewCrew = null;
        if (NewCrewPrefab)
        {
            NewCrew = CO_SPAWNER.co.SpawnUnitOnShip(NewCrewPrefab, CO.co.PlayerMainDrifter);
            CO_SPAWNER.co.SetQualityLevelOfCrew(NewCrew, 120*GetNewFriendlyCrewModifier());
        }

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
        int ChangeHP = 0;
        List<FixedString64Bytes> ItemTranslate = new();
        foreach (LootItem item in list)
        {
            ChangeAmmo+= Mathf.RoundToInt(item.Resource_Ammunition * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeMaterials += Mathf.RoundToInt(item.Resource_Materials * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeSupplies += Mathf.RoundToInt(item.Resource_Supplies * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeTech += Mathf.RoundToInt(item.Resource_Technology * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeHP += Mathf.RoundToInt(item.Resource_Repairs * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod);
            ChangeXP += Mathf.RoundToInt(item.Resource_XP * UnityEngine.Random.Range(1f - item.Randomness, 1f + item.Randomness) * LootLevelMod * GetCommanderExperienceFactor());
            if (item.ItemDrop)
            {
                ResetWeights();
                i = 0;
                foreach (WeightedLootItem weighted in item.ItemDrop.GetPossibleDrops())
                {
                    AddWeights(i, weighted.Weight);
                    i++;
                }
                ScriptableEquippable newItem = item.ItemDrop.GetPossibleDrops()[GetWeight()].Item;
                AddInventoryItem(newItem);
                ItemTranslate.Add(newItem.GetItemResourceIDFull());
            }
        }
        if (list.Count > 0)
        {
            Resource_Ammo.Value += ChangeAmmo;
            Resource_Materials.Value += ChangeMaterials;
            Resource_Supplies.Value += ChangeSupplies;
            Resource_Tech.Value += ChangeTech;
            if (ChangeHP > 0) PlayerMainDrifter.Heal(ChangeHP);
            foreach (CREW crew in GetAlliedCrew())
            {
                crew.AddXP(ChangeXP);
            }
            //Send report to clients with all faction rep changes
            FixedString64Bytes NewCrewLink = NewCrew == null ? "" : NewCrew.name;
            Debug.Log($"NewCrewLink is {NewCrewLink}");
            OpenRewardScreenRpc(ChangeMaterials, ChangeSupplies, ChangeAmmo, ChangeTech, ChangeHP, ChangeXP, Factions.ToArray(), FactionChanges.ToArray(), ItemTranslate.ToArray(), NewCrewLink);
        }

        if (table.MinimumShopDrops > 0)
        {
            ResetShopList();
            int Drops = UnityEngine.Random.Range(table.MinimumShopDrops, table.MaximumShopDrops);
            ResetWeights();
            int i2 = 0;
            List<ScriptableShopitem> ShopItemList = new();
            foreach (ScriptableShopitem item in table.PossibleShopDrops.GetPossibleDrops())
            {
                AddWeights(i2, item.Weight);
                ShopItemList.Add(item);
                i2++;
            }
            for (i2 = 0; i2 < Drops; i2++)
            {
                ScriptableShopitem shopItem = ShopItemList[GetWeight()];
                CreateShopItem(shopItem);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OpenRewardScreenRpc(int Materials, int Supplies, int Ammo, int Tech, int HP, int XP, Faction[] Facs, int[] FacChanges, FixedString64Bytes[] RewardItemsGained, FixedString64Bytes NewCrew)
    {
        List<FactionReputation> list = new();
        for (int i = 0; i < Facs.Length ;i++)
        {
            FactionReputation newfac = new FactionReputation();
            newfac.Fac = Facs[i];
            newfac.Amount = FacChanges[i];
            list.Add(newfac);
        }
        UI.ui.RewardUI.OpenRewardScreen(Materials, Supplies, Ammo, Tech, HP, XP, list.ToArray(), RewardItemsGained, NewCrew);
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

    public NetworkVariable<int> CurrentSoundtrackID = new(-1);
    public void SetCurrentSoundtrack(Soundtrack track)
    {
        CurrentSoundtrackID.Value = (int)track;
    }
}
