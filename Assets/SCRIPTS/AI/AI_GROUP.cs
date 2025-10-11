using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AI_GROUP : MonoBehaviour
{
    private List<AI_UNIT> Units = new();
    public AI_TYPES AI_Type { get; private set; }
    public AI_OBJECTIVES AI_Objective { get; private set; }

    private DRIFTER HomeDrifter;
    private SPACE HomeSpace;
    private Vector3 MainPoint;
    private float MainObjectiveTimer = 0f;
    public void SetAI(AI_TYPES type, AI_OBJECTIVES objective, List<AI_UNIT> Units)
    {
        AI_Type = type;
        AI_Objective = objective;
        foreach (AI_UNIT unit in Units)
        {
            unit.AddToGroup(this);
        }
    }
    public void SetAIHome(Vector3 vec)
    {
        MainPoint = vec;
    }
    public void SetAIHome(DRIFTER dr)
    {
        HomeDrifter = dr;
    }
    public void SetAIHome(SPACE dr)
    {
        HomeSpace = dr;
    }
    public enum AI_TYPES
    {
        DEFAULT_SHIP,
        SWARM
    }
    public enum AI_OBJECTIVES
    {
        WANDER,
        PATROL,
        ENGAGE
    }

    private void Start()
    {
        StartCoroutine(RunAI());
    }

    IEnumerator RunAI()
    {
        /*
         The AI here does nothing but assign each unit in the group to a specific location. There, the AI executes whatever makes sense to it.
         
         */
        while (true)
        {
            if (Units.Count == 0)
            {
                Destroy(this.gameObject);
                yield break;
            }
            switch (AI_Type)
            {
                case AI_TYPES.DEFAULT_SHIP:
                    ShipAI();
                    break;
                case AI_TYPES.SWARM:
                    SwarmAI();
                    break;
            }
            MainObjectiveTimer--;
            yield return new WaitForSeconds(1f);
        }
    }

    private void ShipAI()
    {
        List<AI_UNIT> UsableUnits = new();
        //Pick the closest unit
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
                        unit.SetObjectiveTarget(GetRandomPointAround(MainPoint, 12f));
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
                        unit.SetObjectiveTarget(GetRandomPointAround(MainPoint, 12f));
                    }
                }
                break;
            case AI_OBJECTIVES.ENGAGE: //Move towards random rooms of nearest enemy drifter
                foreach (AI_UNIT unit in Units)
                {
                    if (unit.GetObjectiveDistance() < 3f)
                    {
                        unit.SetObjectiveTarget(unit.GetClosestEnemyDrifter().Interior.GetRandomGrid().transform);
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
}
