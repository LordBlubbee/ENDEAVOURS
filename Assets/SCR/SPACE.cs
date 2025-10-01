using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SPACE : NetworkBehaviour
{
    public DRIFTER Drifter;
    [NonSerialized] public NetworkVariable<int> SpaceID = new();
    [NonSerialized] public List<CREW> CrewInSpace = new();
    private List<Module> Modules = new();
    public List<WalkableTile> RoomTiles;
    private List<Vector2> RoomLocations = new();
    public List<Module> StartingModuleList;
    public List<Vector3> StartingModuleLocations;

    public GameObject PrefabWall;
    public GameObject PrefabCorner;
    public GameObject PrefabEmpty;

    private bool hasInitialized = false;

    private void Start()
    {
        Init();
    }
    public void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        foreach (WalkableTile tile in RoomTiles)
        {
            Transform trans = tile.transform;
            tile.Space = this;
            RoomLocations.Add(new Vector2(trans.localPosition.x / 8f, trans.localPosition.y / 8f));
        }
        foreach (WalkableTile tile in RoomTiles)
        {
            Transform trans = tile.transform;
            Vector2 here = new Vector2(trans.localPosition.x / 8f, trans.localPosition.y / 8f);
            for (int ix = -1; ix < 2; ix++)
            {
                for (int iy = -1; iy < 2; iy++)
                {
                    Vector2 check = here + new Vector2(ix, iy);
                    if (!RoomLocations.Contains(check))
                    {
                        Instantiate(PrefabEmpty, transform).transform.localPosition = trans.localPosition + new Vector3(ix * 8, iy * 8);
                        if (Mathf.Abs(ix)+Mathf.Abs(iy) < 2)
                        {
                            GameObject wall = Instantiate(PrefabWall, transform);
                            wall.transform.localPosition = trans.localPosition + new Vector3(ix * 5, iy * 5);
                            if (Mathf.Abs(iy) != 0) wall.transform.Rotate(Vector3.forward, 90);
                        }
                        else if (Mathf.Abs(ix) + Mathf.Abs(iy) == 2)
                        {
                            if (!RoomLocations.Contains(here + new Vector2(ix, 0)) && !RoomLocations.Contains(here + new Vector2(0, iy)))
                            {
                                GameObject wall = Instantiate(PrefabCorner, transform);
                                wall.transform.localPosition = trans.localPosition + new Vector3(ix * 4, iy * 4);
                                if (ix == -1 && iy == -1) wall.transform.Rotate(Vector3.forward, -90);
                                else if (ix == -1 && iy == 1) wall.transform.Rotate(Vector3.forward, 180);
                                else if (ix == 1 && iy == 1) wall.transform.Rotate(Vector3.forward, 90);
                            }
                        }
                    }
                }
            }
        }
        CO.co.RegisterSpace(this);

        if (!IsServer) return;


        for (int i = 0; i < StartingModuleList.Count; i++)
        {
            Module mod = Instantiate(StartingModuleList[i], Vector3.zero, Quaternion.identity);
            mod.NetworkObject.Spawn();
            mod.transform.SetParent(transform);
            mod.transform.localPosition = StartingModuleLocations[i];
            mod.SpaceID.Value = SpaceID.Value;
            mod.Init();
            Modules.Add(mod);
        }
    }
    public Vector2 ConvertWorldToGrid(Vector3 pos)
    {
        Vector3 local = transform.InverseTransformPoint(pos);
        return new Vector2(local.x / 8f, local.y / 8f);
    }
    public Vector3 ConvertGridToWorld(Vector2 grid)
    {
        return transform.TransformPoint(new Vector3(grid.x * 8f, grid.y * 8f));
    }

    public Vector3 GetNearestGridToPoint(Vector3 point)
    {
        Vector2 grid = ConvertWorldToGrid(point);
        if (RoomLocations.Contains(grid)) return ConvertGridToWorld(grid);
        float minDist = 9999f;
        Vector2 trt = grid;
        foreach (Vector2 loc in GetGrid())
        {
            float dist = (ConvertGridToWorld(loc) - point).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                trt = loc;
            }
        }
        return ConvertGridToWorld(trt);
    }

    public WalkableTile GetCurrentGrid(Vector3 here)
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(here, 0.1f))
        {
            WalkableTile tile = col.GetComponent<WalkableTile>();
            if (tile)
            {
                return tile;
            }
        }
        return null;
    }

    //[NonSerialized] public NetworkVariable<bool> IsOnBoardableTile = new();
    //[NonSerialized] public NetworkVariable<bool> IsTargetedTileBoardable = new();
    public bool isCurrentGridBoardable(Vector3 here)
    {
        WalkableTile tile = GetCurrentGrid(here);
        if (tile == null) return false;
        return tile.canBeBoarded;
    }
    public WalkableTile GetNearestBoardingGridTransformToPoint(Vector3 point)
    {
        WalkableTile nearestTile = null;
        float minDist = float.MaxValue;
        foreach (var tile in RoomTiles)
        {
            if (!tile.canBeBoarded) continue;
            float dist = (tile.transform.position - point).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearestTile = tile;
            }
        }
        return nearestTile;
    }

    public List<Vector2> GetGrid()
    {
        return RoomLocations;
    }
    public void AddCrew(CREW crew)
    {
        if (CrewInSpace.Contains(crew)) return;
        CrewInSpace.Add(crew);
        crew.Space = this;
        crew.SpaceID.Value = SpaceID.Value;
        crew.transform.SetParent(transform);
        crew.transform.localPosition = new Vector3(crew.transform.localPosition.x, crew.transform.localPosition.y, -0.5f);
    }
    public void RemoveCrew(CREW crew)
    {
        CrewInSpace.Remove(crew);
        crew.Space = null;
        crew.SpaceID.Value = 0;
        crew.transform.SetParent(null);
    }

    public List<Module> GetModules()
    {
        return Modules;
    }
    public Module NearestModule(Vector3 vec)
    {
        Module trt = null;
        float maxrange = 9999f;
        foreach (Module mod in Modules)
        {
            Debug.Log(mod.name);
            float dist = (mod.transform.position - vec).magnitude;
            if (dist < maxrange)
            {
                maxrange = dist;
                trt = mod;
            }
        }
        return trt;
    }
}
