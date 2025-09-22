using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_GROUP : MonoBehaviour
{
    private List<AI_UNIT> Units;
    public AI_TYPES AI_Type;
    private AI_STATES AI_State;
    public enum AI_TYPES
    {
        DEFAULT_SHIP,
        LOONCRAB_SWARM
    }
    public enum AI_STATES
    {
        IDLE,
        PATROL,
        ENGAGE
    }

    private void Start()
    {
        StartCoroutine(RunAI());
    }

    IEnumerator RunAI()
    {
        while (true)
        {
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
    }
}
