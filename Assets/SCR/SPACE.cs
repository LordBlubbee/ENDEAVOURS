using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SPACE : NetworkBehaviour
{
    public DRIFTER Drifter;
    [NonSerialized] public List<CREW> CrewInSpace = new();
    private List<Module> Modules = new();
    public List<Transform> RoomTiles;
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

        foreach (Transform trans in RoomTiles)
        {
            RoomLocations.Add(new Vector2(trans.localPosition.x / 8f, trans.localPosition.y / 8f));
        }
        foreach (Transform trans in RoomTiles)
        {
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

        if (!IsServer) return;
        for (int i = 0; i < StartingModuleList.Count; i++)
        {
            Module mod = Instantiate(StartingModuleList[i], StartingModuleLocations[i], Quaternion.identity);
            mod.NetworkObject.Spawn();
            mod.transform.SetParent(transform);
            mod.Init();
            Modules.Add(mod);
        }
    }
    public void AddCrew(CREW crew)
    {
        CrewInSpace.Add(crew);
        crew.space = this;
        crew.transform.SetParent(transform);
    }

    public void RemoveCrew(CREW crew)
    {
        CrewInSpace.Remove(crew);
        crew.space = null;
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
