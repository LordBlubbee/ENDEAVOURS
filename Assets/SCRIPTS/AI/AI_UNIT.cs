using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static AI_GROUP;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class AI_UNIT : NetworkBehaviour
{
    public CREW Unit;
    public AI_UNIT_TYPES AI_Unit_Type;
    private float AI_TacticTimer;
    private AI_TACTICS AI_Tactic;
    private AI_GROUP Group;
    private bool HasObjective = false;
    private Vector3 ObjectiveTarget = Vector3.zero;
    private SPACE ObjectiveSpace;
    private Transform ObjectiveTargetTransform = null;

    private CREW EnemyTarget;
    private float EnemyTargetTimer;

    private Vector3 AI_MoveTarget;
    private SPACE AI_MoveTargetSpace;
    private bool AI_IsMoving;
    private float AI_MoveTimer;
    private Vector3 AI_LookTarget;
    private SPACE AI_LookTargetSpace;
    private bool AI_IsLooking;

    public void SetEnemyTarget(CREW crew)
    {
        EnemyTarget = crew;
        EnemyTargetTimer = 5;
    }
    public Vector3 GetEnemyTargetPosition()
    {
        return EnemyTarget.transform.position;
    }
    public void AddToGroup(AI_GROUP grp)
    {
        Group = grp;
        grp.RegisterUnit(this);
    }
    public void SetTactic(AI_TACTICS tac, float timer)
    {
        AI_Tactic = tac;
        AI_TacticTimer = timer;
        AI_MoveTimer = 0;
    }
    public void SetObjectiveTarget(Vector3 target, SPACE space)
    {
        if (space == null) ObjectiveTarget = target;
        else ObjectiveTarget = space.transform.InverseTransformPoint(target);
        HasObjective = true;
        ObjectiveTargetTransform = null;
        ObjectiveSpace = space;
    }
    public void SetObjectiveTarget(Transform target, SPACE space)
    {
        ObjectiveTargetTransform = target;
        if (space == null) ObjectiveTarget = target.position;
        else ObjectiveTarget = space.transform.InverseTransformPoint(target.position);
        HasObjective = true;
        ObjectiveSpace = space;
    }

    public float GetObjectiveDistance()
    {
        if (!HasObjective) return 0f;
        return (GetObjectiveTarget() - transform.position).magnitude;
    }
    public AI_GROUP.AI_OBJECTIVES CurrentObjective()
    {
        return Group.AI_Objective;
    }
    public float DistToMoveTarget()
    {
        return (GetAI_MoveTarget() - transform.position).magnitude;
    }
    private void SetAIMoveTowards(Vector3 target, SPACE space)
    {
        if (space == null) AI_MoveTarget = target;
        else AI_MoveTarget = space.transform.InverseTransformPoint(target);
        AI_MoveTargetSpace = space;
        AI_IsMoving = true;
    }
    private void SetAIMoveTowardsIfDistant(Vector3 target, SPACE space)
    {
        if ((GetAI_MoveTarget() - target).magnitude > 5f)
        {
            StopMoving();
        }
        SetAIMoveTowards(target, space);
    }
    private bool SetAIMoveTowardsIfExpired(Vector3 target, SPACE space, float movtimer)
    {
        if (AI_MoveTimer > 0f && DistToMoveTarget() > 1f) return false;

        if (space == null) AI_MoveTarget = target;
        else AI_MoveTarget = space.transform.InverseTransformPoint(target);
        AI_IsMoving = true;
        AI_MoveTargetSpace = space;
        AI_MoveTimer = movtimer;
        return true;
    }
    public Vector3 GetObjectiveTarget()
    {
        if (ObjectiveSpace == null) return ObjectiveTarget;
        return ObjectiveSpace.transform.TransformPoint(ObjectiveTarget);
    }
    public Vector3 GetAI_MoveTarget()
    {
        if (AI_MoveTargetSpace == null) return AI_MoveTarget;
        return AI_MoveTargetSpace.transform.TransformPoint(AI_MoveTarget);
    }
    public Vector3 GetAI_LookTarget()
    {
        if (AI_LookTargetSpace == null) return AI_LookTarget;
        return AI_LookTargetSpace.transform.TransformPoint(AI_LookTarget);
    }
    private void StopMoving()
    {
        AI_IsMoving = false;
    }
    private void SetLookTowards(Vector3 trt, SPACE space)
    {
        if (space == null) AI_LookTarget = trt;
        else AI_LookTarget = space.transform.InverseTransformPoint(trt);
        AI_LookTargetSpace = space;
        AI_IsLooking = true;
    }
    private void StopLooking()
    {
        AI_IsLooking = false;
    }
    public enum AI_TACTICS
    {
        NONE,
        SKIRMISH,
        MOVE_COMMAND,
        CHARGE,
        CIRCLE,
        SABOTAGE,
        RETREAT
    }
    public enum AI_UNIT_TYPES
    {
        CREW,
        LOONCRAB
    }
    private void Start()
    {
        if (!IsServer) return;
        if (!ObjectiveSpace) ObjectiveSpace = getSpace();
        StartCoroutine(RunAI());
    }

    private float AI_MoveSpeed = 1f;
    private bool LeaningRight = false;
    IEnumerator RunAI()
    {
        float Tick = 0f;
        LeaningRight = UnityEngine.Random.Range(0f, 1f) < 0.5f;
        float RecalculatePath = 0f;
        while (true)
        {
            Tick -= CO.co.GetWorldSpeedDelta();
            Debug.DrawLine(transform.position, GetObjectiveTarget(), Color.cyan);
            if (Tick < 0f)
            {
                Tick = 0.25f;
                //REEVALUATE
                AITick();
            }
            if (AI_IsMoving)
            {
                if (Dist(GetAI_MoveTarget()) < 0.2f) AI_IsMoving = false;
            }
            if (AI_IsMoving)
            {
                if (getSpace())
                {
                    if (!HasLineOfSight(GetAI_MoveTarget()) && getSpace().IsOnGrid(GetAI_MoveTarget()))
                    {
                        if (path == null || RecalculatePath < 0f)
                        {
                            SetPath(GetAI_MoveTarget());
                            RecalculatePath = 3f;
                        }
                        if (path != null)
                        {
                            RecalculatePath -= 0.25f;
                            if (pathTravelIndex >= path.Count)
                            {
                                path = null;
                            }
                            else
                            {
                                Vector3 trt = getSpace().ConvertGridToWorld(path[pathTravelIndex]);
                                Unit.SetMoveInput((trt - transform.position).normalized);
                                Debug.DrawLine(transform.position, trt, Color.magenta);
                                if (Dist(trt) < 3f)
                                {
                                    pathTravelIndex++;
                                    if (pathTravelIndex >= path.Count) path = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        path = null;
                        Unit.SetMoveInput((GetAI_MoveTarget() - transform.position).normalized * AI_MoveSpeed);
                        Debug.DrawLine(transform.position, GetAI_MoveTarget(), Color.red);
                    }
                }
                else
                {
                    path = null;
                    Unit.SetMoveInput((GetAI_MoveTarget() - transform.position).normalized * AI_MoveSpeed);
                }
            }
            else Unit.SetMoveInput(Vector3.zero);

            if (AI_IsLooking)
            {
                Unit.SetLookTowards(GetAI_LookTarget());
            }
            else
            {
                Unit.SetLookTowards(transform.position + Unit.GetMoveInput() * 100f);
            }
            yield return null;
        }
    }

    private void AITick()
    {
        AI_TacticTimer -= 0.25f;
        EnemyTargetTimer -= 0.25f;
        AI_MoveTimer -= 0.25f;
        if (ObjectiveTargetTransform)
        {
            if (ObjectiveSpace == null) ObjectiveTarget = ObjectiveTargetTransform.position;
            else ObjectiveTarget = ObjectiveSpace.transform.InverseTransformPoint(ObjectiveTargetTransform.position);
        }
        Unit.StopItem1Rpc();
        Unit.StopItem2Rpc();

        switch (AI_Unit_Type)
        {
            case AI_UNIT_TYPES.CREW:
                AITick_Crew();
                break;
            case AI_UNIT_TYPES.LOONCRAB:
                AITick_Looncrab();
                break;
        }

        
    }
    private void AITick_Crew()
    {
        Module mod;
        CREW trt = GetClosestEnemy();
        Vector3 point;
        if (!EnemyTarget || EnemyTargetTimer < 0 || EnemyTarget.isDead())
        {
            EnemyTarget = GetClosestVisibleEnemy();
            EnemyTargetTimer = 7;
        }
        if (EnemyTarget)
        {
            if (AI_TacticTimer < 0) SwitchTacticsCrew();

            switch (AI_Tactic)
            {
                case AI_TACTICS.SKIRMISH:
                    Unit.EquipWeapon1Rpc();
                    if (UnityEngine.Random.Range(0f, 1f) < 0.4f) SetAIMoveTowards(GetPointAwayFromPoint(EnemyTarget.transform.position, GetAttackStayDistance()), EnemyTarget.Space);
                    else SetAIMoveTowards(GetPointAwayFromPoint(EnemyTarget.transform.position, UnityEngine.Random.Range(8f, 20f)), EnemyTarget.Space);
                    SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    if (UnityEngine.Random.Range(0f, 1f) < 0.1f) Unit.UseItem2Rpc();
                    else if (UnityEngine.Random.Range(0f, 1f) < 0.1f) Unit.Dash();
                    break;
                case AI_TACTICS.CIRCLE:
                    Unit.EquipWeapon1Rpc();
                    if (Unit.GetHealthRelative() < 0.3f)
                    {
                        SetAIMoveTowards(GetPointTowardsPoint(EnemyTarget.transform.position, -12f), EnemyTarget.Space);
                    }
                    else
                    {
                        SetAIMoveTowardsIfExpired(GetRandomPointAround(EnemyTarget.transform.position, 5f, 12f), EnemyTarget.Space, 3f);
                    }
                    Unit.UseItem2Rpc();
                    SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    if (UnityEngine.Random.Range(0f, 1f) < 0.1f) Unit.Dash();
                    break;
                case AI_TACTICS.RETREAT:
                    point = GetDiagonalPointTowards(EnemyTarget.transform.position, -12f, LeaningRight);
                    if (Group.HomeDrifter.MedicalModule)
                    {
                        float dist = (Group.HomeDrifter.MedicalModule.transform.position - transform.position).magnitude;
                        if (dist > 16f) point = Group.HomeDrifter.MedicalModule.transform.position;

                        Unit.EquipWeapon1Rpc();
                    } else
                    {
                        if (Dist(EnemyTarget.transform.position) > 24f)
                        {
                            Unit.EquipMedkitRpc();
                        } else
                        {
                            Unit.EquipWeapon1Rpc();
                        }
                    }
                    SetAIMoveTowards(point, EnemyTarget.Space);
                    SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) Unit.Dash();
                    Unit.UseItem2Rpc();
                    break;
            }
            if (Unit.IsEnemyInFront(GetAttackDistance()))
            {
                Unit.UseItem1Rpc();
            }
            return;
        }
        if (getSpace() == Group.HomeSpace)
        {
            //We are at home
           
            //We are not in combat

            if (Unit.GetHealthRelative() < 1f)
            {
                if (Group.HomeDrifter.MedicalModule)
                {
                    SetAIMoveTowards(Group.HomeDrifter.MedicalModule.transform.position, getSpace());
                    StopLooking();
                    Unit.Dash();
                } else
                {
                    StopMoving();
                    StopLooking();
                }
                Unit.EquipMedkitRpc();
                Unit.UseItem2Rpc();
                return;
            }
            if (ObjectiveSpace != getSpace())
            {
                if (!AttemptBoard(ObjectiveSpace))
                {
                    SetAIMoveTowards(getSpace().GetNearestBoardingGridTransformToPoint(ObjectiveSpace.GetNearestBoardingGridTransformToPoint(transform.position).transform.position).transform.position, getSpace());
                    SetLookTowards(GetObjectiveTarget(), ObjectiveSpace);
                }
                return;
            }
            SetAIMoveTowardsIfDistant(GetObjectiveTarget(), ObjectiveSpace);
            if (DistToObjective(transform.position) > 16)
            {
                StopLooking();
                //SetLookTowards(GetObjectiveTarget(), ObjectiveSpace);
                Unit.EquipWeapon1Rpc();
                return;
            }
            mod = GetClosestFriendlyModule();
            if (mod.GetHealthRelative() < 1)
            {
                if (DistToObjective(mod.transform.position) < 8)
                {
                    SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2f), mod.Space);
                    SetLookTowards(mod.GetTargetPos(), ObjectiveSpace);
                    if (Dist(mod.transform.position) < 4f)
                    {
                        Unit.EquipWrenchRpc();
                        Unit.UseItem1Rpc();
                    }
                    return;
                }
                return;
            }
            if (mod is ModuleWeapon)
            {
                if (((ModuleWeapon)mod).EligibleForReload())
                {
                    if (DistToObjective(mod.transform.position) < 8)
                    {
                        SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2f), mod.Space);
                        SetLookTowards(mod.GetTargetPos(), ObjectiveSpace);
                        if (Dist(mod.transform.position) < 4f)
                        {
                            ((ModuleWeapon)mod).ReloadAmmoRpc();
                        }
                        return;
                    }
                }
            }
            return;
        }
        if (Unit.IsEnemyInFront(GetAttackDistance()))
        {
            Unit.UseItem1Rpc();
        }
        //We are in a hostile vessel
        mod = GetClosestEnemyModule();
        if (mod && (Dist(mod.transform.position) < 5 || DistToObjective(mod.transform.position) < 20))
        {
            SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2f), mod.Space);
            SetLookTowards(mod.GetTargetPos(), mod.Space);
            return;
        }
        //Engage!
        //Retreat!
        if (ObjectiveSpace != getSpace())
        {
            if (!AttemptBoard(Group.HomeSpace))
            {
                SetAIMoveTowards(getSpace().GetNearestBoardingGridTransformToPoint(Group.HomeSpace.transform.position).transform.position, getSpace());
                StopLooking();
                //SetLookTowards(getSpace().GetNearestBoardingGridTransformToPoint(Group.HomeSpace.transform.position).transform.position, getSpace());
            }
        } else
        {
            SetAIMoveTowardsIfDistant(GetObjectiveTarget(), ObjectiveSpace);
            StopLooking();
            //SetLookTowards(GetObjectiveTarget(), ObjectiveSpace);
        }
    }

    private void SwitchTacticsCrew()
    {
        ResetWeights();
        switch (AI_Unit_Type)
        {
            case AI_UNIT_TYPES.CREW:
                AddWeights(0, 20);
                if (Unit.GetHealthRelative() < 0.8f || Dist(EnemyTarget.transform.position) > 16f) AddWeights(1, 25);
                else AddWeights(1, 10);
                if (Unit.GetHealthRelative() < 0.4f) AddWeights(2, 40);
                switch (GetWeight())
                {
                    case 0:
                        SetTactic(AI_TACTICS.SKIRMISH, UnityEngine.Random.Range(4, 8));
                        break;
                    case 1:
                        SetTactic(AI_TACTICS.CIRCLE, UnityEngine.Random.Range(4, 8));
                        break;
                    case 2:
                        SetTactic(AI_TACTICS.RETREAT, UnityEngine.Random.Range(8, 12));
                        break;
                }
                break;
        }
    }

    /*--------------------------------------------------------------------------------------------
     * LOONCRAB
     * --------------------------------------------------------------------------------------------
     */
    private void AITick_Looncrab()
    {
        Module mod;
        if (getSpace())
        {
            if (!EnemyTarget || EnemyTargetTimer < 0 || EnemyTarget.isDead())
            {
                EnemyTarget = GetClosestVisibleEnemy();
                EnemyTargetTimer = 3;
            }
            if (EnemyTarget)
            {
                AttemptBoard(EnemyTarget.Space);

                if (AI_TacticTimer < 0) SwitchTacticsLooncrab();

                switch (AI_Tactic)
                {
                    case AI_TACTICS.SKIRMISH:

                        if (UnityEngine.Random.Range(0f,1f) < 0.4f) SetAIMoveTowards(GetPointAwayFromPoint(EnemyTarget.transform.position, GetAttackStayDistance()), EnemyTarget.Space);
                        else SetAIMoveTowards(GetPointAwayFromPoint(EnemyTarget.transform.position, UnityEngine.Random.Range(8f,20f)), EnemyTarget.Space);
                        SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                        break;
                    case AI_TACTICS.CIRCLE:
                        if (Unit.GetHealthRelative() < 0.3f)
                        {
                            SetAIMoveTowards(GetPointTowardsPoint(EnemyTarget.transform.position, -12f), EnemyTarget.Space);
                        }
                        else
                        {
                            SetAIMoveTowardsIfExpired(GetRandomPointAround(EnemyTarget.transform.position, 5f, 12f), EnemyTarget.Space, 3f);
                        }
                        Unit.UseItem2Rpc();
                        SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                        break;
                    case AI_TACTICS.CHARGE:
                        SetAIMoveTowardsIfExpired(GetPointTowardsPoint(EnemyTarget.transform.position, 24f), EnemyTarget.Space, UnityEngine.Random.Range(2f, 4f));
                        SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                        Unit.Dash();
                        break;
                    case AI_TACTICS.SABOTAGE:
                        mod = GetClosestEnemyModule();
                        SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2f), mod.Space);
                        if (Dist(mod.transform.position) < 8f)
                        {
                            SetLookTowards(mod.GetTargetPos(), mod.Space);
                        }
                        else
                        {
                            Unit.UseItem2Rpc();
                            SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                            Unit.Dash();
                        }
                        break;
                }
                if (Unit.IsEnemyInFront(GetAttackDistance()))
                {
                    Unit.UseItem1Rpc();
                }
                return;
            }
            mod = GetClosestEnemyModule();
            if (Unit.IsEnemyInFront(GetAttackDistance()))
            {
                Unit.UseItem1Rpc();
            }
            if (mod && (Dist(mod.transform.position) < 8 || DistToObjective(mod.transform.position) < 20))
            {
                SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2f), mod.Space);
                SetLookTowards(mod.GetTargetPos(), mod.Space);
                return;
            }
            SetAIMoveTowardsIfDistant(GetObjectiveTarget(), ObjectiveSpace);
            if (DistToObjective(transform.position) > 1)
            {
                SetLookTowards(GetObjectiveTarget(), ObjectiveSpace);
            }
            return;
        }
        if (AI_TacticTimer < 0)
        {
            AI_TacticTimer = UnityEngine.Random.Range(4, 9);
            AI_MoveSpeed = UnityEngine.Random.Range(0.8f, 1.2f);
        }
        DRIFTER dr = GetClosestEnemyDrifter();
        AttemptBoard(dr.Space);
        SetLookTowards(dr.transform.position, dr.Space);
        if (Unit.IsEnemyInFront(GetAttackDistance()))
        {
            Unit.UseItem1Rpc();
            SetAIMoveTowards(GetDiagonalPointTowards(dr.transform.position, 24f, LeaningRight), dr.Space);
        }
        else
        {
            SetAIMoveTowardsIfDistant(GetObjectiveTarget(), ObjectiveSpace);
        }
    }
    private void SwitchTacticsLooncrab()
    {
        ResetWeights();
        AddWeights(0, 15);
        if (Unit.GetHealthRelative() < 0.7f || Dist(EnemyTarget.transform.position) > 16f) AddWeights(1, 15);
        AddWeights(2, 12);
        AddWeights(3, 8);
        switch (GetWeight())
        {
            case 0:
                AI_MoveSpeed = UnityEngine.Random.Range(0.8f, 1.2f);
                SetTactic(AI_TACTICS.SKIRMISH, UnityEngine.Random.Range(4, 8));
                break;
            case 1:
                AI_MoveSpeed = UnityEngine.Random.Range(0.8f, 1.2f);
                SetTactic(AI_TACTICS.CIRCLE, UnityEngine.Random.Range(4, 8));
                break;
            case 2:
                AI_MoveSpeed = UnityEngine.Random.Range(1.1f, 1.4f);
                SetTactic(AI_TACTICS.CHARGE, 4);
                break;
            case 3:
                AI_MoveSpeed = UnityEngine.Random.Range(0.8f, 1.2f);
                SetTactic(AI_TACTICS.SABOTAGE, UnityEngine.Random.Range(6, 9));
                break;
        }
    }
   

    private void SetLookTowardsMoveDirection()
    {
        SetLookTowards(transform.position + Unit.GetMoveInput() * 100f, getSpace());
    }
    private bool AttemptBoard(SPACE trt)
    {
        if (!trt) return false;
        if (trt != getSpace())
        {
            WalkableTile boarding = trt.GetNearestBoardingGridTransformToPoint(transform.position);
          
            if (Dist(boarding.transform.position) < 40f)
            {
                SetLookTowards(boarding.transform.position, trt); 
                if (Unit.DefaultToolset == CO_SPAWNER.DefaultEquipmentSet.NONE)
                {
                    Unit.UseGrapple(boarding);
                }
                else
                {
                    Unit.EquipGrappleRpc();

                    if (Mathf.Abs(Unit.AngleBetweenPoints(boarding.transform.position)) < 8) Unit.UseItem1Rpc();
                }
              
                return true;
                //Unit.UseGrapple(boarding);
            }
        }
        return false;
    }
    private void AttemptBoard(CREW trt)
    {
        AttemptBoard(trt.Space);
    }

    /*
     if (pathTravelIndex >= path.Count || path.Count == 0)
                        {
                         //   Debug.Log("Resetting path");
                            path = null;
                        }
                        else SetMoveTowards(CO.co.GetPointWIthSquareRadius(CO.co.ConvertGridToWorld(path[pathTravelIndex]), CO.co.GetGridDistance() * 0.3f));
                        if (Dist(CO.co.ConvertGridToWorld(CO.co.ConvertWorldToGrid(AI_CurrentMoveTarget))) < CO.co.GetGridDistance() * 0.6f)
                        {
                            pathTravelIndex++;
                        }
     
     */

    List<Vector2> path = null;
    int pathTravelIndex = 0;
    public void SetPath(Vector3 trt)
    {
        path = Pathfinder.FindPath(getSpace().ConvertWorldToGrid(transform.position), getSpace().ConvertWorldToGrid(trt), getSpace().GetGrid());
        pathTravelIndex = 0;
    }
    public void DeletePath()
    {
        path = null;
        pathTravelIndex = 0;
    }
    public SPACE getSpace()
    {
        return Unit.Space;
    }
    public List<CREW> CrewInSpace()
    {
        if (getSpace()) return Unit.Space.CrewInSpace;
        return CO.co.GetAllCrews();
    }
    public List<CREW> EnemiesInSpace()
    {
        List<CREW> enemies = new();
        foreach (var crew in CrewInSpace())
        {
            if (crew.isDead()) continue;
            if (isEnemy(crew)) enemies.Add(crew);
        }
        return enemies;
    }
    public bool isEnemy(CREW other)
    {
        return Unit.GetFaction() != other.GetFaction() && other.GetFaction() != 0;
    }

    /* UTILITIES */

    // Returns a random point around a center within a given radius
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
    // Returns a random point away from an enemy, at least minDistance and at most maxDistance from the enemy
    public Vector3 GetRandomPointAwayFromEnemy(CREW enemy, float minDistance, float maxDistance)
    {
        Vector3 direction = (transform.position - enemy.getPos()).normalized;
        float angleOffset = UnityEngine.Random.Range(-Mathf.PI / 4f, Mathf.PI / 4f); // randomize direction a bit
        Vector3 rotatedDir = Quaternion.Euler(0, 0, angleOffset * Mathf.Rad2Deg) * direction;
        float dist = UnityEngine.Random.Range(minDistance, maxDistance);
        return transform.position + rotatedDir * dist;
    }

    // Returns the closest enemy CREW in the same space
    public GameObject GetClosestTarget()
    {
        List<DRIFTER> drifters = CO.co.GetAllDrifters();
        List<CREW> enemies = EnemiesInSpace();
        GameObject closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;

        // Check all drifters
        foreach (var drifter in drifters)
        {
            if (drifter == Unit) continue; // skip self
            float dist = (drifter.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = drifter.gameObject;
            }
        }

        // Check all enemies
        foreach (var enemy in enemies)
        {
            if (enemy == Unit) continue; // skip self
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.gameObject;
            }
        }

        return closest;
    }
    public DRIFTER GetClosestEnemyDrifter()
    {
        List<DRIFTER> enemies = CO.co.GetAllDrifters();
        DRIFTER closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
        foreach (var enemy in enemies)
        {
            if (enemy == Unit) continue; // skip self
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public Module GetClosestEnemyModule()
    {
        if (!getSpace()) return null;
        List<Module> enemies = getSpace().GetModules();
        Module closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
        foreach (var enemy in enemies)
        {
            if (!enemy.CanBeTargeted(getSpace())) continue;
            if (enemy.Faction == Unit.GetFaction()) continue;
            float dist = (enemy.transform.position - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public Module GetClosestFriendlyModule()
    {
        if (!getSpace()) return null;
        List<Module> enemies = getSpace().GetModules();
        Module closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
        foreach (var enemy in enemies)
        {
            if (enemy.Faction != Unit.GetFaction()) continue;
            float dist = (enemy.transform.position - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public CREW GetClosestEnemy()
    {
        List<CREW> enemies = EnemiesInSpace();
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
        foreach (var enemy in enemies)
        {
            if (enemy == Unit) continue; // skip self
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public CREW GetClosestEnemyAnywhere()
    {
        List<CREW> enemies = CO.co.GetAllCrews();
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
        foreach (var enemy in enemies)
        {
            if (enemy == Unit) continue; // skip self
            if (enemy.GetFaction() == 0 || enemy.GetFaction() == Unit.GetFaction()) continue;
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public CREW GetClosestVisibleEnemy()
    {
        List<CREW> enemies = GetVisibleEnemies();
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
        foreach (var enemy in enemies)
        {
            if (enemy == Unit) continue; // skip self
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public Vector3 GetDiagonalPointTowards(Vector3 target, float distance, bool toRight = true)
    {
        Vector3 dir = (target - transform.position).normalized;

        // Rotate direction by ±45° around the Y axis (assuming "up" is Y)
        float angle = toRight ? 45f : -45f;
        Vector3 rotatedDir = Quaternion.Euler(0f, 0f, angle) * dir;

        return transform.position + rotatedDir * distance;
    }
    public Vector3 GetPointTowardsPoint(Vector3 trt, float dis)
    {
        return transform.position + (trt- transform.position).normalized * dis;
    }
    public Vector3 GetPointAwayFromPoint(Vector3 trt, float dis)
    {
        return trt + (transform.position - trt).normalized * dis;
    }
    public float GetAttackDistance()
    {
        if (!Unit.EquippedToolObject) return 4f;
        switch (Unit.EquippedToolObject.AI)
        {
            case TOOL.ToolAI.MELEE:
                return 4.5f;
            case TOOL.ToolAI.RANGED:
                return 30f;
        }
        return 2f;
    }
    public float GetAttackStayDistance()
    {
        if (!Unit.EquippedToolObject) return 10f;
        switch (Unit.EquippedToolObject.AI)
        {
            case TOOL.ToolAI.MELEE:
                return 3f;
            case TOOL.ToolAI.RANGED:
                return 22f;
        }
        return 3f;
    }

    // Returns true if there is a clear line of sight to the target position (no obstacles in between)
    public bool HasLineOfSight(Vector3 target)
    {
        Vector3 origin = transform.position;
        Vector2 direction = (target - origin).normalized;
        float distance = (target - origin).magnitude;

        // Raycast to check for obstacles
        RaycastHit2D[] hit = Physics2D.RaycastAll(origin, direction, distance);
        foreach (RaycastHit2D h in hit)
        {
            if (h.collider.gameObject.tag.Equals("LOSBlocker"))
            {
                // Hit something that is not self and not a trigger, so line of sight is blocked
                return false;
            }
        }
        return true;
    }

    // Returns all visible enemies within a given radius
    public List<CREW> GetVisibleEnemies()
    {
        List<CREW> visibleEnemies = new();
        Vector3 myPos = transform.position;
        foreach (var enemy in EnemiesInSpace())
        {
            if (enemy == Unit) continue; // skip self
            Vector3 enemyPos = enemy.getPos();
            float dist = (enemyPos - myPos).magnitude;
            if (HasLineOfSight(enemyPos) || !getSpace())
            {
                visibleEnemies.Add(enemy);
            }
        }
        return visibleEnemies;
    }
    public float Dist(Vector3 vec)
    {
        return (transform.position-vec).magnitude;
    }
    public float DistToObjective(Vector3 vec)
    {
        if (GetObjectiveTarget() == Vector3.zero) return 0f;
        return (GetObjectiveTarget() - vec).magnitude;
    }
    /// <summary>
    /// Clears all weights.
    /// </summary>
    /// 
    private Dictionary<int, int> weights = new();
    private int totalWeight = 0;
    public void ResetWeights()
    {
        weights.Clear();
        totalWeight = 0;
    }

    /// <summary>
    /// Adds weight for a given ID. If the ID already exists, its weight is increased.
    /// </summary>
    public void AddWeights(int id, int weight)
    {
        if (weight <= 0) return;
        if (weights.ContainsKey(id))
            weights[id] += weight;
        else
            weights[id] = weight;
        totalWeight += weight;
    }

    /// <summary>
    /// Returns a random ID based on the weights, or -1 if empty.
    /// </summary>
    public int GetWeight()
    {
        if (totalWeight == 0) return -1;
        int rand = UnityEngine.Random.Range(1, totalWeight + 1);
        int cumulative = 0;
        foreach (var pair in weights)
        {
            cumulative += pair.Value;
            if (rand <= cumulative)
                return pair.Key;
        }
        return -1;
    }

    /**/
}
public static class Pathfinder
{
// Simple Priority Queue implementation for .NET Framework 4.7.1
private class SimplePriorityQueue<T>
{
    private SortedDictionary<float, Queue<T>> dict = new();

    public int Count { get; private set; } = 0;

    public void Enqueue(T item, float priority)
    {
        if (!dict.TryGetValue(priority, out var queue))
        {
            queue = new Queue<T>();
            dict[priority] = queue;
        }
        queue.Enqueue(item);
        Count++;
    }

    public T Dequeue()
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty");
        var firstPair = dict.First();
        var queue = firstPair.Value;
        var item = queue.Dequeue();
        if (queue.Count == 0)
            dict.Remove(firstPair.Key);
        Count--;
        return item;
    }
}

public static List<Vector2> FindPath(Vector2 start, Vector2 goal, List<Vector2> walkableTiles)
{
    // Snap start and goal to closest walkable tile
    Vector2 startNode = walkableTiles.OrderBy(t => (t - start).sqrMagnitude).First();
    Vector2 goalNode = walkableTiles.OrderBy(t => (t - goal).sqrMagnitude).First();

    Dictionary<Vector2, Vector2> cameFrom = new();
    Dictionary<Vector2, float> costSoFar = new();
    SimplePriorityQueue<Vector2> frontier = new();

    frontier.Enqueue(startNode, 0f);
    cameFrom[startNode] = startNode;
    costSoFar[startNode] = 0f;

    Vector2[] directions = new Vector2[]
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    while (frontier.Count > 0)
    {
        Vector2 current = frontier.Dequeue();

        if ((current - goalNode).sqrMagnitude < 0.01f)
            break;

        foreach (var dir in directions)
        {
            Vector2 neighbor = current + dir;
            if (!walkableTiles.Any(tile => (tile - neighbor).sqrMagnitude < 0.01f))
                continue;

            float newCost = costSoFar[current] + 1f;

            if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
            {
                costSoFar[neighbor] = newCost;
                float priority = newCost + Heuristic(neighbor, goalNode);
                frontier.Enqueue(neighbor, priority);
                cameFrom[neighbor] = current;
            }
        }
    }

    if (!cameFrom.ContainsKey(goalNode))
    {
        Debug.Log("Error: NO path found");
        return null; // No path
    }

    List<Vector2> path = new();
    Vector2 temp = goalNode;
    while (temp != startNode)
    {
        path.Add(temp);
        temp = cameFrom[temp];
    }
    path.Reverse();
    return path;
}
private static float Heuristic(Vector2 a, Vector2 b)
{
    return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
}

