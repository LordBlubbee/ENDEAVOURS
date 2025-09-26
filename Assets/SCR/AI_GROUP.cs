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
    public void SetAI(AI_TYPES type, AI_OBJECTIVES objective, List<AI_UNIT> Units)
    {
        AI_Type = type;
        AI_Objective = objective;
        foreach (AI_UNIT unit in Units)
        {
            unit.AddToGroup(this);
        }
    }
    public enum AI_TYPES
    {
        DEFAULT_SHIP,
        LOONCRAB_SWARM
    }
    public enum AI_OBJECTIVES
    {
        WANDER,
        PATROL,
        BOARD
    }

    private void Start()
    {
        StartCoroutine(RunAI());
    }

    IEnumerator RunAI()
    {
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
                case AI_TYPES.LOONCRAB_SWARM:
                    LooncrabAI();
                    break;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void ShipAI()
    {

    }
    private void LooncrabAI()
    {
        switch (AI_Objective)
        {
            case AI_OBJECTIVES.WANDER:
                break;
            case AI_OBJECTIVES.PATROL:
                break;
            case AI_OBJECTIVES.BOARD:
                LooncrabBoard();
                break;
        }
    }
    private void LooncrabBoard()
    {
        foreach (AI_UNIT unit in Units)
        {
            if (unit.getSpace())
            {

            }
            else
            {

            }
        }
    }

    public void RegisterUnit(AI_UNIT unit)
    {
        if (!Units.Contains(unit))
        {
            Units.Add(unit);
        }
    }
}
