using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SPACE : NetworkBehaviour
{
    public DRIFTER Drifter;
    [NonSerialized] public List<CREW> CrewInSpace = new();
    private List<Module> Modules = new();
    public List<Module> StartingModuleList;
    public List<Vector3> StartingModuleLocations;

    private bool hasInitialized = false;

    private void Start()
    {
        Init();
    }
    public void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

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
