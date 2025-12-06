using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SPACE : NetworkBehaviour
{
    public DRIFTER Drifter;
    public DUNGEON Dungeon;
    [NonSerialized] public NetworkVariable<int> SpaceID = new();
    [NonSerialized] public List<CREW> CrewInSpace = new();
    public List<DoorPlaceholder> DoorPlacerholders = new();
    private List<Module> Modules = new();
    public List<WalkableTile> RoomTiles = new();
    public List<GameObject> EmptyTiles = new();
    private List<Vector2> RoomLocations = new();
    private List<Vector2> EmptyLocations = new();
    public Vector3 Bridge;
    public Vector3 Storage;
    public List<Vector3> CoreModuleLocations;
    [NonSerialized] public List<Module> CoreModules = new();
    public List<float> CoreModuleRotations = new();
    public List<Vector3> WeaponModuleLocations = new();
    public List<float> WeaponModuleRotations = new();
    [NonSerialized] public List<Module> WeaponModules = new();
    public List<Vector3> SystemModuleLocations = new();
    public List<float> SystemModuleRotations = new();
    [NonSerialized] public List<Module> SystemModules = new();

    public List<Module> GetNonWeaponModules()
    {
        List<Module> modules = new List<Module>(CoreModules);
        modules.AddRange(SystemModules);
        return modules;
    }

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
        foreach (GameObject tile in EmptyTiles)
        {
            Transform trans = tile.transform;
            EmptyLocations.Add(new Vector2(trans.localPosition.x / 8f, trans.localPosition.y / 8f));
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
                        if (!EmptyLocations.Contains(check))
                        {
                            GameObject ob = Instantiate(PrefabEmpty, transform);
                            ob.transform.localPosition = trans.localPosition + new Vector3(ix * 8, iy * 8);
                            EmptyTiles.Add(ob);
                            EmptyLocations.Add(check);
                        }
                        if (!tile.canBeBoarded)
                        {
                            if (Mathf.Abs(ix) + Mathf.Abs(iy) < 2)
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
        }
        CO.co.RegisterSpace(this);

        foreach (DoorPlaceholder door in DoorPlacerholders)
        {
            door.InitDoor(this);
        }

        if (!IsServer) return;

        if (Drifter)
        {
            foreach (ScriptableEquippableModule mod in Drifter.StartingModules)
            {
                AddModule(mod, true);
            }
        }
        
        /*
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
        */
    }
    [Rpc(SendTo.Server)]
    public void AddModuleRpc(string resourceLink)
    {
        AddModule(Resources.Load<ScriptableEquippableModule>(resourceLink), false);
    }
    public bool AddModule(ScriptableEquippableModule module, bool Initial)
    {
        Module mod = null;
        Vector3 vec = Vector3.zero;
        float rot = 0;
        switch (module.EquipType)
        {
            case ScriptableEquippableModule.EquipTypes.CORE:
                foreach (Vector3 trymod in CoreModuleLocations)
                {
                    bool tryLocation = true;
                    foreach (Module modloc in CoreModules)
                    {
                        if ((modloc.transform.localPosition-trymod).magnitude < 0.1f)
                        {
                            tryLocation = false;
                            break;
                        }
                    }
                    if (tryLocation)
                    {
                        mod = Instantiate(module.PrefabModule, Vector3.zero, Quaternion.identity);
                        vec = CoreModuleLocations[CoreModules.Count];
                        rot = CoreModuleRotations[CoreModules.Count];
                        CoreModules.Add(mod);
                        if (mod.ModuleType == Module.ModuleTypes.ENGINES) Drifter.EngineModule = mod;
                        else if (mod.ModuleType == Module.ModuleTypes.NAVIGATION) Drifter.NavModule = mod;
                        else if (mod.ModuleType == Module.ModuleTypes.MEDICAL) Drifter.MedicalModule = mod;
                        break;
                    }
                }
                break;
            case ScriptableEquippableModule.EquipTypes.WEAPON:
                foreach (Vector3 trymod in WeaponModuleLocations)
                {
                    bool tryLocation = true;
                    foreach (Module modloc in WeaponModules)
                    {
                        if ((modloc.transform.localPosition - trymod).magnitude < 0.1f)
                        {
                            tryLocation = false;
                            break;
                        }
                    }
                    if (tryLocation)
                    {
                        mod = Instantiate(module.PrefabModule, Vector3.zero, Quaternion.identity);
                        vec = WeaponModuleLocations[WeaponModules.Count];
                        rot = WeaponModuleRotations[WeaponModules.Count];
                        WeaponModules.Add(mod);
                        break;
                    }
                }
                break;
            case ScriptableEquippableModule.EquipTypes.SYSTEM:
                foreach (Vector3 trymod in SystemModuleLocations)
                {
                    bool tryLocation = true;
                    foreach (Module modloc in SystemModules)
                    {
                        if ((modloc.transform.localPosition - trymod).magnitude < 0.1f)
                        {
                            tryLocation = false;
                            break;
                        }
                    }
                    if (tryLocation)
                    {

                        mod = Instantiate(module.PrefabModule, Vector3.zero, Quaternion.identity);
                        vec = SystemModuleLocations[SystemModules.Count];
                        rot = SystemModuleRotations[SystemModules.Count];
                        SystemModules.Add(mod);
                        break;
                    }
                }
                break;
            case ScriptableEquippableModule.EquipTypes.LOON:
               /* foreach (Vector3 trymod in LoonModuleLocations)
                {
                    bool tryLocation = true;
                    foreach (Module modloc in LoonModules)
                    {
                        if ((modloc.transform.localPosition - trymod).magnitude < 0.1f)
                        {
                            tryLocation = false;
                            break;
                        }
                    }
                    if (tryLocation)
                    {

                        mod = Instantiate(module.PrefabModule, Vector3.zero, Quaternion.identity);
                        vec = LoonModuleLocations[LoonModules.Count];
                        LoonModules.Add(mod);
                        break;
                    }
                }*/
                break;
        }
        if (mod == null) return false;
        mod.NetworkObject.Spawn();
        mod.transform.SetParent(transform);
        mod.transform.localPosition = vec;
        mod.transform.Rotate(Vector3.forward, rot);
        mod.SpaceID.Value = SpaceID.Value;
        mod.Faction = Drifter.GetFaction();
        mod.SetHomeDrifter(Drifter);
        Modules.Add(mod);
        mod.Init();
        if (mod is ModuleWeapon weaponMod && Initial)
        {
            ((ModuleWeapon)mod).ReloadInstantly();
        }
        return true;
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

    public WalkableTile GetNearestGridToPoint(Vector3 point)
    {
        Vector2 grid = ConvertWorldToGrid(point);
        if (RoomLocations.Contains(grid)) return GetCurrentGrid(point);
        float minDist = 9999f;
        WalkableTile trt = null;
        foreach (WalkableTile loc in GetGridObjects())
        {
            float dist = (loc.transform.position - point).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                trt = loc;
            }
        }
        return trt;
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

    public bool IsOnGrid(Vector3 here)
    {
        return GetCurrentGrid(here) != null;
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
    public List<WalkableTile> GetGridObjects()
    {
        return RoomTiles;
    }
    public WalkableTile GetRandomGrid()
    {
        if (RoomTiles == null || RoomTiles.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, RoomTiles.Count);
        return RoomTiles[index];
    }
    public WalkableTile GetRandomUnboardableGrid()
    {
        if (RoomTiles == null || RoomTiles.Count == 0)
            return null;

        List<WalkableTile> tiles = new();
        foreach (WalkableTile tile in RoomTiles)
        {
            if (!tile.canBeBoarded) tiles.Add(tile);
        }
        int index = UnityEngine.Random.Range(0, tiles.Count);
        return tiles[index];
    }
    public void AddCrew(CREW crew)
    {
        if (CrewInSpace.Contains(crew)) return;
        CrewInSpace.Add(crew);
        crew.Space = this; //This does nothing ;~;
        if (IsServer)
        {
            crew.SpaceID.Value = SpaceID.Value;
            Debug.Log($"Adding {crew.name} to space {this.name} with ID {SpaceID.Value}");
            crew.transform.SetParent(transform);
            crew.transform.localPosition = new Vector3(crew.transform.localPosition.x, crew.transform.localPosition.y, -0.5f);
        }
    }
    public void RemoveCrew(CREW crew)
    {
        Debug.Log("Removing crew: " + crew.name);
        CrewInSpace.Remove(crew);
        if (IsServer)
        {
            crew.Space = null;
            crew.SpaceID.Value = 0;
            crew.transform.SetParent(null);
        }
    }

    public List<CREW> GetCrew()
    {
        return CrewInSpace;
    }


    public List<Module> GetModules()
    {
        return Modules;
    }

    public void RemoveModule(Module mod)
    {
        if (CoreModules.Contains(mod))
        {
            Debug.Log("Error: Removing core module");
            return;
        }
        Modules.Remove(mod);
        if (WeaponModules.Contains(mod)) WeaponModules.Remove(mod);
        if (SystemModules.Contains(mod)) SystemModules.Remove(mod);
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
