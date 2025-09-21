using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CO : NetworkBehaviour
{
    public static CO co;

    [NonSerialized] public NetworkVariable<bool> HasShipBeenLaunched = new();
    [NonSerialized] public DRIFTER PlayerMainDrifter;
    private void Start()
    {
        co = this;
    }

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
        RegisteredMapPoints.Add(crew);
    }
    public void UnregisterMapPoint(MapPoint crew)
    {
        RegisteredMapPoints.Remove(crew);
    }
}
