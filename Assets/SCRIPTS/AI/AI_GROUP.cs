using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class AI_GROUP : MonoBehaviour
{
    private List<AI_UNIT> Units = new();
    public List<AI_UNIT> GetUnits()
    {
        return Units;
    }
    public AI_TYPES AI_Type { get; private set; }
    public AI_OBJECTIVES AI_Objective { get; private set; }

    [NonSerialized] public int Faction;
    [NonSerialized] public DRIFTER HomeDrifter;
    [NonSerialized] public SPACE HomeSpace;
    private Vector3 MainPoint;
    private float MainObjectiveTimer = 0f;
    public void SetAI(AI_TYPES type, AI_OBJECTIVES objective, int Fac, List<AI_UNIT> Units)
    {
        AI_Type = type;
        AI_Objective = objective;
        Faction = Fac;  
        foreach (AI_UNIT unit in Units)
        {
            unit.AddToGroup(this);
        }
        StartCoroutine(RunAI());
    }
    public void SetAIHome(Vector3 vec)
    {
        MainPoint = vec;
    }

    public void Add(AI_UNIT unit)
    {
        unit.AddToGroup(this);
    }
    public void SetAIHome(DRIFTER dr)
    {
        HomeDrifter = dr;
        HomeSpace = dr.Space;
    }
    public void SetAIHome(SPACE dr)
    {
        HomeSpace = dr;
    }
    public enum AI_TYPES
    {
        SHIP_DEFENSIVE,
        SHIP_BOARDING,
        SWARM
    }
    public enum AI_OBJECTIVES
    {
        WANDER,
        PATROL,
        ENGAGE,
        SHIP
    }
    IEnumerator RunAI()
    {
        /*
         The AI here does nothing but assign each unit in the group to a specific location. There, the AI executes whatever makes sense to it.
         
         */
        while (true)
        {
            if (Units.Count == 0 && HomeDrifter == null)
            {
                Destroy(this.gameObject);
                yield break;
            }
            switch (AI_Type)
            {
                case AI_TYPES.SHIP_DEFENSIVE:
                    ShipAI();
                    break;
                case AI_TYPES.SWARM:
                    SwarmAI();
                    break;
            }
            MainObjectiveTimer -= 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    /*
    Module InteractingModule;
    IEnumerator Interact_Navigation()
    {
        while (InteractingModule != null && InteractingModule.ModuleType == Module.ModuleTypes.NAVIGATION)
        {
            CREW crew = GetClosestEnemyAnywhere();
            if (crew != null)
            {
                InteractingModule.Space.Drifter.SetMoveInput((crew.transform.position - transform.position).normalized * -1, 0.6f + Unit.GetATT_COMMAND() * 0.15f);
                InteractingModule.Space.Drifter.SetLookTowards((crew.transform.position-transform.position).normalized * -1);
            }
            yield return null;
        }
    }
    IEnumerator Interact_Weapons()
    {
        ModuleWeapon wep = InteractingModule as ModuleWeapon;
        while (InteractingModule != null && InteractingModule.ModuleType == Module.ModuleTypes.WEAPON)
        {
            CREW crew = GetClosestEnemyAnywhere();
            if (crew != null)
            {
                wep.SetLookTowards(crew.transform.position);
                if (Mathf.Abs(wep.AngleBetweenPoints(crew.transform.position)) < 10)
                {
                    wep.UseRpc(Vector3.zero, 0.75f + Unit.GetATT_ALCHEMY() * 0.1f + Unit.GetATT_ARMS() * 0.02f);
                }
            }
            yield return null;
        }
        wep.StopRpc();
    }
     */
    private void ShipAI()
    {
        if (!HomeDrifter) return;
        //Defensive non-boarding playstyle

        List<AI_UNIT> UsableUnits = new(Units);
        //Pick the closest unit
        foreach (AI_UNIT un in UsableUnits)
        {
            if (!un)
            {
                Units.Remove(un);
            }
        }
        UsableUnits = new(Units);
        foreach (AI_UNIT un in Units)
        {
            if (un.Unit.GetOrderPoint() != Vector3.zero)
            {
                un.SetObjectiveTarget(un.Unit.GetOrderPoint(),un.Unit.GetOrderTransform());
                UsableUnits.Remove(un);
            }
        }
        List<Module> ManModules = new List<Module>();
        foreach (Module mod in HomeDrifter.Space.GetModules())
        {
            if (mod.GetHealthRelative() < 1) ManModules.Add(mod);
        }
        foreach (ModuleWeapon mod in HomeDrifter.Space.WeaponModules)
        {
            if (mod.EligibleForReload()) ManModules.Add(mod);
        }
        List<CREW> Intruders = new List<CREW>(EnemiesInSpace(HomeDrifter.Space));
        Vector3 PointOfInterest;
        while (UsableUnits.Count > 0)
        {
            AI_UNIT Closest;
            if (ManModules.Count > 0)
            {
                PointOfInterest = ManModules[0].transform.position;
                Closest = GetClosestUnitInGroup(PointOfInterest, UsableUnits);
                Closest.SetObjectiveTarget(Closest.getSpace().GetNearestGridToPoint(PointOfInterest).transform, HomeSpace);
                UsableUnits.Remove(Closest);
                ManModules.Remove(ManModules[0]);
                continue;
            }
            if (Intruders.Count > 0)
            {
                PointOfInterest = Intruders[0].transform.position;
                Closest = GetClosestUnitInGroup(PointOfInterest, UsableUnits);
                Closest.SetObjectiveTarget(Closest.getSpace().GetNearestGridToPoint(PointOfInterest).transform, HomeSpace);
                UsableUnits.Remove(Closest);
                continue;
            }
            Closest = UsableUnits[0];
            if (Closest.DistToObjective(Closest.transform.position) < 8) Closest.SetObjectiveTarget(HomeDrifter.Space.GetRandomGrid().transform, HomeSpace);
            UsableUnits.Remove(Closest);
        }

        /* CONTROL WEAPONS */
        foreach (ModuleWeapon wep in HomeDrifter.Interior.WeaponModules)
        {
            if (CO.co.IsSafe())
            {
                wep.Stop();
                continue;
            }
            if (wep.GetOrderPoint() != Vector3.zero)
            {
                wep.SetLookTowards(wep.GetOrderPoint());
                if (Mathf.Abs(wep.AngleBetweenPoints(wep.GetOrderPoint())) < 3)
                {
                    wep.Use(wep.GetOrderPoint());
                    continue;
                }
                wep.Stop();
                continue;
            }
            if (wep.AutofireActive.Value || HomeDrifter != CO.co.PlayerMainDrifter)
            {
                Vector3 trt = GetClosestEnemyPositionInNebula(wep.transform.position);
                if (trt != Vector3.zero)
                {
                    wep.SetLookTowards(trt);
                    if (Mathf.Abs(wep.AngleBetweenPoints(trt)) < 10)
                    {
                        wep.Use(trt);
                        continue;
                    }
                }
                wep.Stop();
                continue;
            }
            wep.Stop();
        }
    }
    private void SwarmAI()
    {
        switch (AI_Objective)
        {
            case AI_OBJECTIVES.WANDER:
                foreach (AI_UNIT unit in Units)
                {
                    if (unit.GetObjectiveDistance() < 3f)
                    {
                        unit.SetObjectiveTarget(GetRandomPointAround(MainPoint, 12f), unit.getSpace());
                    }
                }
                break;
            case AI_OBJECTIVES.PATROL:
                if (MainObjectiveTimer < 0)
                {
                    MainObjectiveTimer = UnityEngine.Random.Range(10, 15);
                    MainPoint = GetRandomPointAround(GetGroupCenter(), 30f, 40f);
                }
                foreach (AI_UNIT unit in Units)
                {
                    if (unit.GetObjectiveDistance() < 3f)
                    {
                        unit.SetObjectiveTarget(GetRandomPointAround(MainPoint, 12f), unit.getSpace());
                    }
                }
                break;
            case AI_OBJECTIVES.ENGAGE: //Move towards random rooms of nearest enemy drifter
                Debug.Log("Engage...");
                foreach (AI_UNIT unit in Units)
                {
                    if (unit.GetObjectiveDistance() < 4f)
                    {
                        Debug.Log("Setting target");
                        DRIFTER enem = unit.GetClosestEnemyDrifter();
                        if (enem) unit.SetObjectiveTarget(enem.Interior.GetRandomGrid().transform, enem.Interior);
                    }
                }
                break;
        }
    }
    private Vector3 GetGroupCenter()
    {
        if (Units == null || Units.Count == 0)
            return Vector3.zero;

        Vector3 total = Vector3.zero;

        foreach (AI_UNIT unit in Units)
        {
            if (unit != null)
                total += unit.transform.position;
        }

        return total / Units.Count;
    }
    public void RegisterUnit(AI_UNIT unit)
    {
        if (!Units.Contains(unit))
        {
            Units.Add(unit);
        }
    }
    public Vector3 GetRandomPointAround(Vector3 center, float radius)
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float dist = radius;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * dist;
        return center + offset;
    }
    public Vector3 GetRandomPointAround(Vector3 center, float minradius, float maxradius)
    {
        return GetRandomPointAround(center, UnityEngine.Random.Range(minradius, maxradius));
    }

    private AI_UNIT GetClosestUnitInGroup(Vector3 point, List<AI_UNIT> usableUnits)
    {
        if (usableUnits == null || usableUnits.Count == 0)
            return null;

        AI_UNIT closest = null;
        float minDist = float.MaxValue;

        foreach (var unit in usableUnits)
        {
            if (unit == null) continue;

            float dist = (unit.transform.position - point).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = unit;
            }
        }

        return closest;
    }
    public List<CREW> CrewInSpace(SPACE space)
    {
        return space.GetCrew();
    }
    public List<CREW> EnemiesInSpace(SPACE space)
    {
        List<CREW> enemies = new();
        foreach (var crew in CrewInSpace(space))
        {
            if (crew.isDead()) continue;
            if (crew.GetFaction() != Faction && crew.GetFaction() != 0) enemies.Add(crew);
        }
        return enemies;
    }
    public CREW GetClosestEnemy(Vector3 vec, SPACE space)
    {
        List<CREW> enemies = EnemiesInSpace(space);
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = vec;
        foreach (var enemy in enemies)
        {
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public CREW GetClosestEnemyAnywhere(Vector3 vec)
    {
        List<CREW> enemies = CO.co.GetAllCrews();
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = vec;
        foreach (var enemy in enemies)
        {
            if (enemy.GetFaction() == 0 || enemy.GetFaction() == Faction) continue;
            if (enemy.isDead()) continue;
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public Vector3 GetClosestEnemyPositionInNebula(Vector3 vec)
    {
        List<CREW> enemies = CO.co.GetAllCrews();
        Vector3 closest = Vector3.zero;
        float minDist = float.MaxValue;
        Vector3 myPos = vec;
        foreach (var enemy in enemies)
        {
            if ((enemy.transform.position - vec).magnitude > 150) continue;
            if (enemy.GetFaction() == 0 || enemy.GetFaction() == Faction) continue;
            if (enemy.Space != null) continue;
            if (enemy.isDead()) continue;
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.transform.position;
            }
        }
        List<DRIFTER> drifters = CO.co.GetAllDrifters();
        foreach (var enemy in drifters)
        {
            if ((enemy.transform.position - vec).magnitude > 150) continue;
            if (enemy.GetFaction() == 0 || enemy.GetFaction() == Faction) continue;
            if (enemy.isDead()) continue;
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.transform.TransformPoint(new Vector3(UnityEngine.Random.Range(-enemy.RadiusX, enemy.RadiusX), UnityEngine.Random.Range(-enemy.RadiusY, enemy.RadiusY)));
            }
        }
        return closest;
    }
}
