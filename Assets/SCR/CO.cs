using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CO : NetworkBehaviour
{
    public static CO co;

    [NonSerialized] public NetworkVariable<bool> HasShipBeenLaunched = new();
    [NonSerialized] public DRIFTER PlayerMainDrifter;
    [NonSerialized] public MapPoint PlayerMapPoint;
    private void Start()
    {
        co = this;
    }

    /*MAP GENERATION*/

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
