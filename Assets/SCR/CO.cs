using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CO : NetworkBehaviour
{
    public static CO co;

    [NonSerialized] public NetworkVariable<bool> HasShipBeenLaunched = new();
    [NonSerialized] public NetworkVariable<bool> AreWeInDanger = new();
    [NonSerialized] public DRIFTER PlayerMainDrifter;
    [NonSerialized] public MapPoint PlayerMapPoint;

    [NonSerialized] public int Resource_Materials;
    [NonSerialized] public int Resource_Supplies;
    [NonSerialized] public int Resource_Ammo;
    [NonSerialized] public int Resource_Tech;
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
        THEM
    }

    [NonSerialized] public List<ScriptableEquippable> Drifter_Inventory = new();

    public ScriptableBiome CurrentBiome;
    private void Start()
    {
        co = this;
        if (IsServer)
        {
        } else
        {
            RequestPlayerMapPointRpc();
        }
    }

    public void StartGame()
    {
        GenerateMap(25, 20);
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
            MapPoint destination = PlayerMapPoint.ConnectedPoints[HasVoteResult()];
            CO_STORY.co.SetStory(destination.AssociatedPoint.InitialDialog);
            StartCoroutine(MoveToPoint(destination));
            PlayerMapPoint = destination;
            ResetMapVotes();
        }
    }

    IEnumerator MoveToPoint(MapPoint destination)
    {
        Vector3 moveDirection = destination.transform.position - PlayerMapPoint.transform.position;
        PlayerMainDrifter.SetLookTowards(moveDirection);
        PlayerMainDrifter.SetMoveInput(moveDirection, 1f);
        PlayerMainDrifter.SetCanReceiveInput(false);
        foreach (LOCALCO local in GetLOCALCO())
        {
            local.ShipTransportFadeAwayRpc(destination.GetName());
        }
        yield return new WaitForSeconds(2.5f);
        PlayerMainDrifter.transform.position = Vector3.zero;
        PlayerMainDrifter.transform.Rotate(Vector3.forward, PlayerMainDrifter.AngleToTurnTarget());
        PlayerMainDrifter.SetMoveInput(Vector3.zero,1f);
        UpdatePlayerMapPointRpc(destination.transform.position);
        foreach (LOCALCO local in GetLOCALCO())
        {
            local.ShipTransportFadeInRpc();
        }
        //Generate Area here


        yield return new WaitForSeconds(1f);
        PlayerMainDrifter.SetCanReceiveInput(true);
    }

    [Rpc(SendTo.Server)]
    private void RequestPlayerMapPointRpc()
    {
        UpdatePlayerMapPointRpc(PlayerMapPoint.transform.position);
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
    }
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
    public void VoteForMapRpc(int ID)
    {
        if (LOCALCO.local.CurrentMapVote.Value == ID) LOCALCO.local.CurrentMapVote.Value = -1;
        else LOCALCO.local.CurrentMapVote.Value = ID;
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
        PlayerMapPoint = mapPoint;
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
        foreach (MapPoint map in GetMapPoints())
        {
            map.ConnectedPoints = GetConnectedPoints(map.transform.position, map);
            map.Init(CurrentBiome.PossiblePointsRandom[UnityEngine.Random.Range(0,CurrentBiome.PossiblePointsRandom.Count)]);
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
        return Time.deltaTime;
    }
    public float GetWorldSpeedDeltaFixed()
    {
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
        RegisteredLOCALCO.Add(loc);
        if (IsServer)
        {
            loc.SetPlayerID(LOCALCO_IDCOUNT);
            LOCALCO_IDCOUNT++;
        }
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
    public List<SPACE> GetAllSpaces()
    {
        //Server Only
        return RegisteredSPACES;
    }
    public void RegisterSpace(SPACE space)
    {
        if (RegisteredSPACES.Contains(space)) return;
        RegisteredSPACES.Add(space);
    }
    public void UnregisterSpace(SPACE space)
    {
        RegisteredSPACES.Remove(space);

        SpaceCount--;
    }
    int SpaceCount = 0;

}
