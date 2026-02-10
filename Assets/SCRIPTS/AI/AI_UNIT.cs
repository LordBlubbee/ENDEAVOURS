using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Netcode;
using UnityEngine;
using static AI_GROUP;

public class AI_UNIT : NetworkBehaviour
{
    public CREW Unit;
    public AI_UNIT_TYPES AI_Unit_Type;
    public AI_TACTIC_TYPES AI_Tactic_Type;
    private float AI_TacticTimer;
    private AI_TACTICS AI_Tactic;
    private Vector3 AI_TacticTarget;
    private SPACE AI_TacticSpace;
    bool HasTacticTarget = false;
    private CREW AI_TacticFollow;
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
        switch (tac)
        {
            case AI_TACTICS.DORMANT:
                Unit.SetTagState(CREW.TagStates.DORMANT);
                break;
            default:
                Unit.SetTagState(CREW.TagStates.NONE);
                break;
        }
        AI_Tactic = tac;
        AI_TacticTimer = timer;
        AI_MoveTimer = 0;
        ResetTacticTarget();
    }

    public AI_TACTICS GetTactic()
    {
        return AI_Tactic;
    }
    public void SetTacticTarget(Vector3 target, SPACE space)
    {
        if (space == null) AI_TacticTarget = target;
        else AI_TacticTarget = space.transform.InverseTransformPoint(target);
        HasTacticTarget = true;
        AI_TacticFollow = null;
        AI_TacticSpace = space;
    }
    public void SetTacticTarget(CREW target, SPACE space)
    {
        AI_TacticFollow = target;
        if (space == null) AI_TacticTarget = target.transform.position;
        else ObjectiveTarget = space.transform.InverseTransformPoint(target.transform.position);
        HasTacticTarget = true;
        AI_TacticSpace = space;
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

    public SPACE GetObjectiveSpace()
    {
        return ObjectiveSpace;
    }
    public float GetTacticDistance()
    {
        if (!HasTacticTarget) return 0f;
        return (GetTacticTarget() - transform.position).magnitude;
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

    public void ResetTacticTarget()
    {
        HasTacticTarget = false;
        AI_TacticFollow = null;
        AI_TacticSpace = null;
        AI_TacticTarget = Vector3.zero;
    }
    public Vector3 GetTacticTarget()
    {
        if (AI_TacticFollow != null) return AI_TacticFollow.transform.position;
        if (AI_TacticSpace == null) return AI_TacticTarget;
        return AI_TacticSpace.transform.TransformPoint(AI_TacticTarget);
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
        if (space == null) AI_LookTarget = trt + new Vector3(0.1f, 0, 0);
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
        RETREAT,
        HEAL_ALLIES,
        DORMANT,
        PATROL,
        PREPARE_CHARGE,
        PREPARE_FORMATION,
        FORMATION,
        GREET,
        CONVERSE
    }
    public enum AI_UNIT_TYPES
    {
        CREW,
        LOONCRAB
    }
    public enum AI_TACTIC_TYPES
    {
        NOMADEN_NORMAL,
        NOMADEN_SNIPER,
        LOGIPEDES_NORMAL,
        LOGIPEDES_SNIPER,
        LOGIPEDES_OFFICER,
        BAKUTO_NORMAL,
        BAKUTO_SNIPER,
        BAKUTO_CHOSEN
    }
    public bool IsTacticSniper()
    {
        return AI_Tactic_Type == AI_TACTIC_TYPES.NOMADEN_SNIPER
            || AI_Tactic_Type == AI_TACTIC_TYPES.LOGIPEDES_SNIPER
            || AI_Tactic_Type == AI_TACTIC_TYPES.BAKUTO_SNIPER;
    }
    public bool IsTacticOfficer()
    {
        return AI_Tactic_Type == AI_TACTIC_TYPES.LOGIPEDES_OFFICER
           || AI_Tactic_Type == AI_TACTIC_TYPES.BAKUTO_CHOSEN;
    }
    private void Start()
    {
        if (!IsServer) return;
        if (!ObjectiveSpace) ObjectiveSpace = getSpace();
        if (ObjectiveTarget == Vector3.zero) SetObjectiveTarget(transform.position, getSpace());
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

        if (!Unit.CanFunction()) return;
        if (PatrolDungeon()) return;
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

    private bool PatrolDungeon()
    {
        PotentiallyAlertAllies();
        switch (AI_Tactic)
        {
            case AI_TACTICS.DORMANT:
                if (Unit.GetHealthRelative() < 1f)
                {
                    Unit.IsNeutral = false;
                    break;
                }
                return true;
            case AI_TACTICS.PATROL:
                SetAIMoveTowardsIfDistant(GetObjectiveTarget(), ObjectiveSpace);
                StopLooking();
                /*if (DistToObjective(transform.position) > 1)
                {

                }*/
                CREW crew;
                if (EnemyTargetTimer > 0.5f) crew = GetClosestVisibleEnemyInCone(25f, 120f);
                else crew = GetClosestVisibleEnemyInCone(25f, 70f);
                if (crew)
                {
                    StopMoving();
                    SetLookTowards(crew.transform.position, ObjectiveSpace);
                    EnemyTargetTimer += 0.5f;
                    if (EnemyTargetTimer > 2.5f || Unit.GetHealthRelative() < 1f)
                    {
                        SetEnemyTarget(crew);
                        SetTactic(AI_TACTICS.SKIRMISH, 1);
                        Unit.IsNeutral = false;
                        break;
                    }
                }
                else EnemyTargetTimer = 0;
                return true;
        }
        return false;
    }
    private bool EquipAndUseCombatItem(Vector3 target)
    {
        float Distance = (target - transform.position).magnitude;
        int EquipItem = 0;
        int i = 0;
        CREW Ally = null;
        foreach (ScriptableEquippableWeapon Weapon in Unit.EquippedWeapons)
        {
            i++;
            if (!Weapon) continue;
            switch (i)
            {
                case 1:
                    if (Unit.Slot1Cooldown.Value > 0f) continue;
                    break;
                case 2:
                    if (Unit.Slot2Cooldown.Value > 0f) continue;
                    break;
                case 3:
                    if (Unit.Slot3Cooldown.Value > 0f) continue;
                    break;
            }
            switch (Weapon.ToolPrefab.GetUsageAI())
            {
                case TOOL.ToolAI.MELEE:
                    if (Distance < 8) EquipItem = i;
                    break;
                case TOOL.ToolAI.RANGED:
                    EquipItem = i;
                    break;
                case TOOL.ToolAI.MELEE_AND_SHIELD:
                    EquipItem = i;
                    break;
                case TOOL.ToolAI.MELEE_LONG_SHIELD:
                    EquipItem = i;
                    break;
                case TOOL.ToolAI.TARGET_ALLIES:
                    Ally = GetClosestAlly();
                    if (Ally != null)
                    {
                        if ((Ally.transform.position-transform.position).magnitude < 20) EquipItem = i;
                    }
                    break;
                case TOOL.ToolAI.HEAL_ALLIES:
                    Ally = GetClosestWoundedAlly();
                    if (Ally != null)
                    {
                        if ((Ally.transform.position - transform.position).magnitude < 20) EquipItem = i;
                    }
                    break;
            }
        }
        switch (EquipItem)
        {
            case 0:
                SetLookTowards(target, getSpace());
                return false;
            case 1:
                Unit.EquipWeapon1Rpc();
                break;
            case 2:
                Unit.EquipWeapon2Rpc();
                break;
            case 3:
                Unit.EquipWeapon3Rpc();
                break;
        }
        switch (Unit.EquippedToolObject.GetUsageAI())
        {
            case TOOL.ToolAI.MELEE:
                SetLookTowards(target, getSpace());
                Unit.UseItem1Rpc();
                break;
            case TOOL.ToolAI.RANGED:
                SetLookTowards(target, getSpace());
                Unit.UseItem1Rpc();
                break;
            case TOOL.ToolAI.MELEE_AND_SHIELD:
                SetLookTowards(target, getSpace());
                if (Distance < 8) Unit.UseItem1Rpc();
                else Unit.UseItem2Rpc();
                break;
            case TOOL.ToolAI.MELEE_LONG_SHIELD:
                SetLookTowards(target, getSpace());
                if (Distance < 9) Unit.UseItem1Rpc();
                else Unit.UseItem2Rpc();
                break;
            case TOOL.ToolAI.TARGET_ALLIES:
                if (Ally != null)
                {
                    SetLookTowards(target, getSpace());
                    return false;
                }
                target = Ally.transform.position;
                SetLookTowards(target, getSpace());
                Unit.UseItem1Rpc();
                break;
            case TOOL.ToolAI.HEAL_ALLIES:
                if (Ally != null)
                {
                    SetLookTowards(target, getSpace());
                    return false;
                }
                target = Ally.transform.position;
                SetLookTowards(target, getSpace());
                Unit.UseItem1Rpc();
                break;
        }
        return true;
    }

    private bool HaveMedicalRetreat()
    {
        if (Group.HomeDrifter)
        {

            if (Group.HomeDrifter.MedicalModule) return true;
        }
        return false;
    }

    bool FirstEngagement = true;
    private void AITick_Crew()
    {
        Module mod;
        CREW trt = GetClosestEnemy();
        Vector3 point;
        if (!EnemyTarget || EnemyTargetTimer < 0 || EnemyTarget.isDead())
        {
            EnemyTarget = GetClosestVisibleEnemy();
            EnemyTargetTimer = 5;

        }
        if (EnemyTarget)
        {
            if (AI_TacticTimer < 0) SwitchTacticsCrew();
            else
            {
                if (FirstEngagement)
                {
                    FirstEngagement = false;
                    PlayVCX(ScriptableVoicelist.VoicelineTypes.FIRST_ENGAGE, VoiceHandler.PriorityTypes.PRIORITY, 1f);
                }
            }
            //COMBAT STRATEGEMS
                switch (AI_Tactic)
                {
                    case AI_TACTICS.SKIRMISH:
                    if (AI_TacticTimer > 2f) PlayVCX(ScriptableVoicelist.VoicelineTypes.IN_COMBAT, VoiceHandler.PriorityTypes.IDLE, 0.05f, 1f);
                    if (!HasLineOfSight(EnemyTarget.transform.position))
                    {
                        SetAIMoveTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    } else
                    {
                        SetAIMoveTowards(GetPointAwayFromPoint(EnemyTarget.transform.position, GetAttackStayDistance()), EnemyTarget.Space);
                    }

                        EquipAndUseCombatItem(EnemyTarget.transform.position);

                        if (UnityEngine.Random.Range(0f, 1f) < 0.1f) Unit.Dash();
                        break;
                    case AI_TACTICS.CIRCLE:
                    if (AI_TacticTimer > 2f) PlayVCX(ScriptableVoicelist.VoicelineTypes.IN_COMBAT, VoiceHandler.PriorityTypes.IDLE, 0.05f, 1f);
                    if (Unit.GetHealthRelative() < 0.3f)
                        {
                            SetAIMoveTowards(GetPointTowardsPoint(EnemyTarget.transform.position, -12f), EnemyTarget.Space);
                        }
                        else
                        {
                            SetAIMoveTowardsIfExpired(GetRandomPointAround(EnemyTarget.transform.position, 12f, 18f), EnemyTarget.Space, 3f);
                        }

                        EquipAndUseCombatItem(EnemyTarget.transform.position);

                        if (UnityEngine.Random.Range(0f, 1f) < 0.1f) Unit.Dash();
                        break;
                    case AI_TACTICS.RETREAT:
                        point = GetDiagonalPointTowards(EnemyTarget.transform.position, -12f, LeaningRight);
                        if (HaveMedicalRetreat())
                        {
                            float dist = (Group.HomeDrifter.MedicalModule.transform.position - transform.position).magnitude;
                            if (dist > 16f) point = Group.HomeDrifter.MedicalModule.transform.position;
                            if (getSpace() != Group.HomeSpace) SetAIMoveTowards(getSpace().GetNearestBoardingGridTransformToPoint(Group.HomeSpace.GetNearestBoardingGridTransformToPoint(transform.position).transform.position).transform.position, getSpace());
                            else SetAIMoveTowards(point, Group.HomeSpace);
                            SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);

                        }
                        else
                        {
                            if (Dist(EnemyTarget.transform.position) > 24f)
                            {
                                Unit.EquipMedkitRpc();
                                SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                            }
                            else
                            {

                                EquipAndUseCombatItem(EnemyTarget.transform.position);
                            }
                            SetAIMoveTowards(point, EnemyTarget.Space);
                        }

                        if (UnityEngine.Random.Range(0f, 1f) < 0.5f) Unit.Dash();
                        break;
                    case AI_TACTICS.HEAL_ALLIES:

                    if (AI_TacticTimer > 2f) PlayVCX(ScriptableVoicelist.VoicelineTypes.HEALING, VoiceHandler.PriorityTypes.IDLE, 0.05f, 1f);
                    CREW WoundedAlly = GetClosestWoundedAlly();
                        if (!WoundedAlly)
                        {
                            AI_TacticTimer = 0f;
                            break;
                        }
                        if (Dist(WoundedAlly.transform.position) > 24f)
                        {
                            AI_TacticTimer = 0f;
                            break;
                        }
                        if (Unit.HasLineOfSight(EnemyTarget.transform.position) && (Unit.GetHealthRelative() < 0.8f || Dist(EnemyTarget.transform.position) < 12))
                        {
                            AI_TacticTimer = 0f;
                            break;
                        }
                        Unit.EquipMedkitRpc();
                        if (Dist(WoundedAlly.transform.position) < 8f)
                        {
                            SetAIMoveTowards(GetPointAwayFromPoint(WoundedAlly.transform.position, 2f), WoundedAlly.Space);
                            SetLookTowards(WoundedAlly.transform.position, WoundedAlly.Space);
                            Unit.UseItem1Rpc();
                        }
                        else
                        {
                            SetAIMoveTowards(WoundedAlly.transform.position, WoundedAlly.Space);
                            EquipAndUseCombatItem(EnemyTarget.transform.position);
                            if (UnityEngine.Random.Range(0f, 1f) < 0.1f) Unit.Dash();
                        }
                        break;
                    case AI_TACTICS.SABOTAGE:
                    if (AI_TacticTimer > 2f) PlayVCX(ScriptableVoicelist.VoicelineTypes.IN_COMBAT, VoiceHandler.PriorityTypes.IDLE, 0.05f, 1f);
                    mod = GetClosestEnemyModule();
                        if (mod)
                        {
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
                        }
                        break;
                case AI_TACTICS.FORMATION:
                    if (AI_TacticTimer > 2f) PlayVCX(ScriptableVoicelist.VoicelineTypes.IN_COMBAT, VoiceHandler.PriorityTypes.IDLE, 0.05f, 1f);

                    if (Unit.GetHealthRelative() < 0.3f)
                    {
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.RETREATING, VoiceHandler.PriorityTypes.NORMAL, 0.7f);
                        SetTactic(AI_TACTICS.RETREAT, UnityEngine.Random.Range(8f, 12f));
                    }

                    if (Dist(GetTacticTarget()) > 6f)
                    {
                        SetAIMoveTowards(GetTacticTarget(), AI_TacticSpace);
                    }
                    else if (!HasLineOfSight(EnemyTarget.transform.position))
                    {
                        SetAIMoveTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    }
                    else
                    {
                        SetAIMoveTowards(GetPointAwayFromPoint(EnemyTarget.transform.position, GetAttackStayDistance()), EnemyTarget.Space);
                    }
                    SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    EquipAndUseCombatItem(EnemyTarget.transform.position);
                    break;
                case AI_TACTICS.PREPARE_FORMATION:
                    if (AI_TacticTimer < 1f || GetTacticDistance() < 1f)
                    {
                        CREW FormationFollow = AI_TacticFollow;
                        SetTactic(AI_TACTICS.FORMATION, UnityEngine.Random.Range(10f, 13f));
                        SetTacticTarget(FormationFollow, FormationFollow.Space);
                        break;
                    }
                    SetAIMoveTowards(GetTacticTarget(), AI_TacticSpace);
                    SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    break;
                case AI_TACTICS.CHARGE:
                    if (AI_TacticTimer > 2f) PlayVCX(ScriptableVoicelist.VoicelineTypes.IN_COMBAT, VoiceHandler.PriorityTypes.IDLE, 0.05f, 1f);

                    if (!HasLineOfSight(EnemyTarget.transform.position)) SetAIMoveTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    SetAIMoveTowards(GetPointAwayFromPoint(EnemyTarget.transform.position, GetAttackStayDistance()), EnemyTarget.Space);

                    SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    EquipAndUseCombatItem(EnemyTarget.transform.position);

                    Unit.Dash();
                    break;
                case AI_TACTICS.PREPARE_CHARGE:
                    if (AI_TacticTimer < 1f || GetTacticDistance() < 1f)
                    {
                        if (AI_TacticTimer > 2f) PlayVCX(ScriptableVoicelist.VoicelineTypes.CHARGE, VoiceHandler.PriorityTypes.GUARANTEE, 0.6f);
                        SetTactic(AI_TACTICS.CHARGE, UnityEngine.Random.Range(6f,8f));
                        break;
                    }
                    SetAIMoveTowards(GetTacticTarget(), AI_TacticSpace);
                    SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                    break;
                }
            if (Unit.IsEnemyInFront(GetAttackDistance()))
            {
                Unit.UseItem1Rpc();
            }
            return;
        }

        if (CO.co.IsSafe()) FirstEngagement = true;

        if (OutOfCombatCrewBehavior()) return;
        if (Unit.IsEnemyInFront(GetAttackDistance()))
        {
            Unit.EquipWeapon1Rpc();
            Unit.UseItem1Rpc();
        }
        if (ObjectiveSpace != getSpace())
        {
            if (!AttemptBoard(ObjectiveSpace))
            {
                //Find the nearest boarding point in THIS space to the nearest boarding tile in the OTHER space
                SetAIMoveTowards(getSpace().GetNearestBoardingGridTransformToPoint(ObjectiveSpace.GetNearestBoardingGridTransformToPoint(transform.position).transform.position).transform.position, getSpace());
                SetLookTowards(GetObjectiveTarget(), ObjectiveSpace);
            }
            return;
        }
        if (getSpace() == Group.HomeSpace)
        {
            //We are at home

            //We are not in combat
           
            SetAIMoveTowardsIfDistant(GetObjectiveTarget(), ObjectiveSpace);
            if (DistToObjective(transform.position) > 16)
            {
                StopLooking();
                //SetLookTowards(GetObjectiveTarget(), ObjectiveSpace);
                Unit.EquipWeapon1Rpc();
                return;
            }
            //Repair behavior
            mod = GetClosestFriendlyModule();
            if (mod)
            {
                if (mod.GetHealthRelative() < 1)
                {
                    if (DistToObjective(mod.transform.position) < 16)
                    {
                        SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2f), mod.Space);
                        SetLookTowards(mod.GetTargetPos(), ObjectiveSpace);
                        if (Dist(mod.transform.position) < 4f)
                        {
                            Unit.EquipWrenchRpc();
                            Unit.UseItem1Rpc();
                            PlayVCX(ScriptableVoicelist.VoicelineTypes.REPAIRING, VoiceHandler.PriorityTypes.IDLE, 0.1f);
                        } else
                        {
                            PlayVCX(ScriptableVoicelist.VoicelineTypes.REPAIRING, VoiceHandler.PriorityTypes.NORMAL, 0.7f);
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
            }
            
            return;
        }
        //We are boarding an enemy vessel!!
      
        SetAIMoveTowardsIfDistant(GetObjectiveTarget(), ObjectiveSpace);
        if (DistToObjective(transform.position) > 16)
        {
            StopLooking();
            //SetLookTowards(GetObjectiveTarget(), ObjectiveSpace);
            Unit.EquipWeapon1Rpc();
            return;
        }
    }

    private bool OutOfCombatCrewBehavior()
    {
        if (AI_TacticTimer < 0) SwitchTacticsCrewOutOfCombat();
       
        Vector3 point;
        Module mod;
        switch (AI_Tactic)
        {
            default:
                break;
            case AI_TACTICS.RETREAT:
                Unit.EquipMedkitRpc();
                if (Unit.GetHealthRelative() > 0.9f)
                {
                    AI_TacticTimer = 0f;
                    break;
                }
                if (Group.HomeDrifter.MedicalModule)
                {
                    point = Group.HomeDrifter.MedicalModule.transform.position;
                    if (getSpace() != Group.HomeSpace) SetAIMoveTowards(getSpace().GetNearestBoardingGridTransformToPoint(Group.HomeSpace.GetNearestBoardingGridTransformToPoint(transform.position).transform.position).transform.position, getSpace());
                    else SetAIMoveTowards(point, Group.HomeSpace);
                    SetLookTowards(point, Group.HomeSpace);
                }
             
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f) Unit.Dash();
                if (Unit.IsEnemyInFront(GetAttackDistance()))
                {
                    Unit.EquipWeapon1Rpc();
                    Unit.UseItem1Rpc();
                    break;
                }
                Unit.UseItem2Rpc();
                return true;
            case AI_TACTICS.HEAL_ALLIES:
                CREW WoundedAlly = GetClosestWoundedAlly();
                if (!WoundedAlly)
                {
                    AI_TacticTimer = 0f;
                    break;
                }
                if (Dist(WoundedAlly.transform.position) > 20f)
                {
                    AI_TacticTimer = 0f;
                    break;
                }
               
                SetAIMoveTowards(GetPointAwayFromPoint(WoundedAlly.transform.position, 2f), WoundedAlly.Space);
                SetLookTowards(WoundedAlly.transform.position, WoundedAlly.Space);
                if (Unit.IsEnemyInFront(GetAttackDistance()))
                {
                    Unit.EquipWeapon1Rpc();
                    Unit.UseItem1Rpc();
                    return true;
                }
                Unit.EquipMedkitRpc();
                Unit.UseItem1Rpc();
                if (Dist(WoundedAlly.transform.position) < 8f)
                {
                }
                else
                {
                    if (UnityEngine.Random.Range(0f, 1f) < 0.1f) Unit.Dash();
                }
                return true;
            case AI_TACTICS.SABOTAGE:
                mod = GetClosestEnemyModule();
                if (mod == null)
                {
                    AI_TacticTimer = 0f;
                    break;
                }
                SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2f), mod.Space);
              
                if (Dist(mod.transform.position) < 8f)
                {
                    SetLookTowards(mod.GetTargetPos(), mod.Space);
                    if (Unit.IsEnemyInFront(GetAttackDistance()))
                    {
                        Unit.EquipWeapon1Rpc();
                        Unit.UseItem1Rpc();
                        return true;
                    }
                }
                else
                {
                    StopLooking();
                    Unit.UseItem2Rpc();
                    Unit.Dash();
                }
                return true;
            case AI_TACTICS.GREET:
                if ((GetTacticTarget() - transform.position).magnitude > 16)
                {
                    AI_TacticTimer = 0f;
                    break;
                }
                if ((GetTacticTarget() - transform.position).magnitude < 3)
                {
                    SetAIMoveTowards(GetPointAwayFromPoint(GetTacticTarget(), 3f), AI_TacticFollow.Space);
                }
                SetLookTowards(GetTacticTarget(), AI_TacticFollow.Space);
                return true;
            case AI_TACTICS.CONVERSE:
                SetAIMoveTowards(GetPointAwayFromPoint(GetTacticTarget(), 3f), AI_TacticFollow.Space);
                SetLookTowards(GetTacticTarget(), AI_TacticFollow.Space);
                return true;
        }
        return false;
    }

    public void SwitchTacticsCrewOutOfCombat()
    {
        ResetWeights();
        switch (AI_Unit_Type)
        {
            case AI_UNIT_TYPES.CREW:
                AddWeights(0, 30);
                int RetreatFactor = Mathf.RoundToInt((0.8f - Unit.GetHealthRelative()) * 50);
                if (RetreatFactor > 0) AddWeights(3, RetreatFactor); //Retreat
                CREW Ally = GetClosestWoundedAlly();
                if (Ally != null)
                {
                    if (Dist(Ally.transform.position) < 20f) AddWeights(1, Mathf.RoundToInt((1f-Ally.GetHealthRelative())*50));
                }
                Ally = GetClosestCriticallyWoundedAllyInSpace();
                if (Ally != null)
                {
                    if (Dist(Ally.transform.position) < 20f) AddWeights(1, 30);
                }
                Module mod = GetClosestEnemyModule();
                if (mod != null)
                {
                    if (Dist(mod.transform.position) < 16f) AddWeights(2, 50);
                    else if (Dist(mod.transform.position) < 40f) AddWeights(2, 25);
                }
                CREW ClosestAlly = null;
                if (CO.co.IsSafe())
                {
                    ClosestAlly = GetClosestAlly();
                    if (ClosestAlly.IsPlayer())
                    {
                        if (HasToReportVictory || HasToReportHardVictory)
                        {
                            if (GetVoiceSilenceLevel() > 0f && Dist(ClosestAlly.transform.position) < 7f) AddWeights(4, 60);
                        } else
                        {
                            if (Dist(ClosestAlly.transform.position) < 7f)
                            {
                                if (GetVoiceSilenceLevel() > 0f)
                                    AddWeights(4, 15);
                                else if (GetVoiceSilenceLevel() > 5f)
                                    AddWeights(4, 40);
                            }
                        }
                    } else if (ClosestAlly.GetVoiceHandler() != null)
                    {
                        if (GetVoiceSilenceLevel() > 5f && Dist(ClosestAlly.transform.position) < 7f)
                        {
                            AddWeights(5, 15);
                        }
                    }
                }
                switch (GetWeight())
                {
                    case 0:
                        SetTactic(AI_TACTICS.SKIRMISH, UnityEngine.Random.Range(4f, 8f));
                        break;
                    case 1:
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.HEALING, VoiceHandler.PriorityTypes.NORMAL, 0.7f);
                        SetTactic(AI_TACTICS.HEAL_ALLIES, UnityEngine.Random.Range(17f, 19f));
                        break;
                    case 2:
                        SetTactic(AI_TACTICS.SABOTAGE, UnityEngine.Random.Range(17f, 19f));
                        break;
                    case 3:
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.RETREATING, VoiceHandler.PriorityTypes.NORMAL, 0.7f);
                        SetTactic(AI_TACTICS.RETREAT, UnityEngine.Random.Range(17f, 19f));
                        break;
                    case 4:
                        SetTactic(AI_TACTICS.GREET, UnityEngine.Random.Range(4f, 7f));
                        SetTacticTarget(ClosestAlly, ClosestAlly.Space);
                        SetAIMoveTowards(GetPointAwayFromPoint(ClosestAlly.transform.position, 4f), ClosestAlly.Space);
                        if (HasToThankForPromotion)
                        {
                            HasToThankForPromotion = false;
                            PlayVCX(ScriptableVoicelist.VoicelineTypes.SALUTE_PROMOTION, VoiceHandler.PriorityTypes.GUARANTEE, 1f);
                            break;
                        }
                        if (HasToReportVictory)
                        {
                            HasToReportVictory = false;
                            PlayVCX(ScriptableVoicelist.VoicelineTypes.SALUTE_EASYVICTORY, VoiceHandler.PriorityTypes.GUARANTEE, 1f);
                            break;
                        }
                        if (HasToReportHardVictory)
                        {
                            HasToReportHardVictory = false;
                            PlayVCX(ScriptableVoicelist.VoicelineTypes.SALUTE_HARDVICTORY, VoiceHandler.PriorityTypes.GUARANTEE, 1f);
                            break;
                        }
                        if (CO.co.Resource_Ammo.Value < 30)
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                            {
                                PlayVCX(ScriptableVoicelist.VoicelineTypes.SALUTE_LOWAMMO, VoiceHandler.PriorityTypes.NORMAL, 1f);
                                break;
                            }
                        }
                        if (CO.co.Resource_Supplies.Value < 30)
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                            {

                                PlayVCX(ScriptableVoicelist.VoicelineTypes.SALUTE_LOWSUPPLIES, VoiceHandler.PriorityTypes.NORMAL, 1f);
                                break;
                            }
                        }
                        
                        if (CO.co.PlayerMainDrifter.GetHullDamageRatio() < 0.5f && UnityEngine.Random.Range(0f, 1f) < 0.4f)
                        {
                            PlayVCX(ScriptableVoicelist.VoicelineTypes.SALUTE_WORRIED, VoiceHandler.PriorityTypes.NORMAL, 1f);
                            break;
                        }
                        if (CO.co.PlayerMainDrifter.GetHullDamageRatio() < 0.3f)
                        {
                            PlayVCX(ScriptableVoicelist.VoicelineTypes.SALUTE_WORRIED, VoiceHandler.PriorityTypes.NORMAL, 1f);
                            break;
                        }
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.SALUTE, VoiceHandler.PriorityTypes.NORMAL, 1f);
                        break;
                    case 5:
                        List<Conversation> list = GetVoiceHandler().GetVoicelist().GenericConversations;
                        if (list.Count == 0) break;
                        Conversation convo = list[UnityEngine.Random.Range(0, list.Count)];
                        if (UnityEngine.Random.Range(0f,1f) < 0.7f)
                        {
                            List<Conversation> list2 = new();
                            foreach (Conversation con in GetVoiceHandler().GetVoicelist().TargetedConversations)
                            {
                                if (con.Target.Contains(ClosestAlly.GetVoiceHandler().GetVoicelist()))
                                {
                                    list2.Add(con);
                                }
                            }
                            if (list2.Count > 0)
                            {
                                convo = list2[UnityEngine.Random.Range(0, list2.Count)];
                            }
                        }
                        if (convo == null) break;
                        SetTactic(AI_TACTICS.CONVERSE, 60f);
                        SetTacticTarget(ClosestAlly, ClosestAlly.Space);
                        ClosestAlly.GetAI().SetTactic(AI_TACTICS.CONVERSE, 60f);
                        ClosestAlly.GetAI().SetTacticTarget(Unit, getSpace());
                      
                        CO_SPAWNER.co.SpawnConvo(Unit, ClosestAlly, convo);
                        break;
                }
                break;
        }
    }

    bool HasToThankForPromotion = false;
    bool HasToReportVictory = false;
    bool HasToReportHardVictory = false;

    public void SetThankForPromotion()
    {
        HasToThankForPromotion = true;
    }
    public void SetReportVictory()
    {
        HasToReportVictory = true;
    }
    public void SetReportHardVictory()
    {
        HasToReportHardVictory = true;
    }

    public void ClearOutOfCombatReports()
    {
        HasToReportVictory = false;
        HasToReportHardVictory = false;
    }
    private void SwitchTacticsCrew()
    {
        ClearOutOfCombatReports();
        ResetWeights();
        switch (AI_Unit_Type)
        {
            case AI_UNIT_TYPES.CREW:
                AddWeights(0, 20);
                if (!IsTacticSniper())
                {
                    if (Unit.GetHealthRelative() < 0.8f || Dist(EnemyTarget.transform.position) > 16f) AddWeights(1, 25);
                    else AddWeights(1, 12);
                }
                if (Unit.GetHealthRelative() < 0.4f) AddWeights(2, 40);

                bool canTryToHeal = !(Unit.HasLineOfSight(EnemyTarget.transform.position) && (Unit.GetHealthRelative() < 0.8f || Dist(EnemyTarget.transform.position) < 12));
                if (canTryToHeal)
                {
                    CREW Ally = GetClosestWoundedAlly();
                    if (Ally != null)
                    {
                        if (Dist(Ally.transform.position) < 20f && Dist(EnemyTarget.transform.position) > 14f) AddWeights(3, 20);
                    }
                    Ally = GetClosestCriticallyWoundedAllyInSpace();
                    if (Ally != null)
                    {
                        if (Dist(Ally.transform.position) < 20f && Dist(EnemyTarget.transform.position) > 14f) AddWeights(3, 40);
                    }
                }
                Module mod = GetClosestEnemyModule();
                if (mod != null)
                {
                    if (Dist(mod.transform.position) < 24f && Dist(EnemyTarget.transform.position) > 14f) AddWeights(4, 40);
                }

                if (AI_Tactic_Type == AI_TACTIC_TYPES.BAKUTO_NORMAL || AI_Tactic_Type == AI_TACTIC_TYPES.BAKUTO_CHOSEN)
                {
                    AddWeights(5, 30);
                }
                if (AI_Tactic_Type == AI_TACTIC_TYPES.LOGIPEDES_NORMAL || AI_Tactic_Type == AI_TACTIC_TYPES.LOGIPEDES_OFFICER)
                {
                    AddWeights(6, 30);
                }
                switch (GetWeight())
                {
                    case 0:
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.SKIRMISHING, VoiceHandler.PriorityTypes.NORMAL, 0.5f);
                        SetTactic(AI_TACTICS.SKIRMISH, UnityEngine.Random.Range(4f, 8f));
                        break;
                    case 1:
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.SKIRMISHING, VoiceHandler.PriorityTypes.NORMAL, 0.5f);
                        SetTactic(AI_TACTICS.CIRCLE, UnityEngine.Random.Range(3f, 5f));
                        break;
                    case 2:
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.RETREATING, VoiceHandler.PriorityTypes.NORMAL, 0.7f);
                        SetTactic(AI_TACTICS.RETREAT, UnityEngine.Random.Range(8f, 12f));
                        break;
                    case 3:
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.HEALING, VoiceHandler.PriorityTypes.NORMAL, 0.7f);
                        SetTactic(AI_TACTICS.HEAL_ALLIES, UnityEngine.Random.Range(17f, 19f));
                        break;
                    case 4:
                        SetTactic(AI_TACTICS.SABOTAGE, UnityEngine.Random.Range(6f, 8f));
                        break;
                    case 5:
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.PREPARE_CHARGE, VoiceHandler.PriorityTypes.PRIORITY, 0.8f, 1.5f);
                        SetTactic(AI_TACTICS.PREPARE_CHARGE, 2.5f);
                        SetTacticTarget(transform.position, getSpace());
                        foreach (CREW crew in GetTeam())
                        {
                            crew.GetAI().SetTactic(AI_TACTICS.PREPARE_CHARGE, 2.5f);
                            crew.GetAI().SetTacticTarget(Unit, getSpace());
                        }
                        break;
                    case 6:
                        PlayVCX(ScriptableVoicelist.VoicelineTypes.FORMATION, VoiceHandler.PriorityTypes.PRIORITY, 0.8f, 1.5f);
                        SetTactic(AI_TACTICS.PREPARE_FORMATION, 2.5f);
                        SetTacticTarget(transform.position, getSpace());
                        foreach (CREW crew in GetTeam())
                        {
                            crew.GetAI().SetTactic(AI_TACTICS.PREPARE_FORMATION, 2.5f);
                            crew.GetAI().SetTacticTarget(Unit, getSpace());
                        }
                        break;
                }
                break;
        }
    }
    public void PlayVCX(ScriptableVoicelist.VoicelineTypes typ, VoiceHandler.PriorityTypes pri, float Chance = 1f, float Cooldown = 2.5f)
    {
        Unit.PlayVCX(typ, pri, Chance, Cooldown);
    }
    private List<CREW> GetTeam(float range = 20f)
    {
        List<CREW> List = new();
        foreach (CREW crew in CO.co.GetAlliedAICrew(Unit.GetFaction()))
        {
            if ((crew.transform.position - transform.position).magnitude > range) continue;
            if (crew.Space != getSpace()) continue;
            List.Add(crew);
        }
        return List;
    }

    /*--------------------------------------------------------------------------------------------
     * LOONCRAB
     * --------------------------------------------------------------------------------------------
     */

    private void PotentiallyAlertAllies()
    {
        if (!EnemyTarget) return;
        if (Group.AI_Objective == AI_OBJECTIVES.DORMANT)
        {
            foreach (AI_UNIT crew in Group.GetUnits())
            {
                if (Dist(crew.transform.position) < 12f && Dist(EnemyTarget.transform.position) < 40f && (crew.AI_Tactic == AI_TACTICS.DORMANT || crew.AI_Tactic == AI_TACTICS.PATROL))
                {
                    crew.Unit.IsNeutral = false;
                    crew.SetTactic(AI_TACTICS.NONE, 0f);
                }
            }
        }
    }
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
                        if (Unit.IsEnemyInFront(GetAttackDistance()) && UnityEngine.Random.Range(0f, 1f) < 0.5f)
                        {
                            Unit.UseItem1Rpc();
                        } else
                        {
                            Unit.UseItem2Rpc();
                        }
                        SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                        break;
                    case AI_TACTICS.CHARGE:
                        SetAIMoveTowardsIfExpired(GetPointTowardsPoint(EnemyTarget.transform.position, 24f), EnemyTarget.Space, UnityEngine.Random.Range(2f, 4f));
                        SetLookTowards(EnemyTarget.transform.position, EnemyTarget.Space);
                        Unit.Dash();
                        Unit.UseItem1Rpc();
                        break;
                    case AI_TACTICS.SABOTAGE:
                        mod = GetClosestEnemyModule();
                        if (!mod)
                        {
                            AI_TacticTimer = 0;
                            break;
                        }
                        SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2.5f), mod.Space);
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
            if (Unit.IsEnemyInFront(GetAttackDistance()))
            {
                Unit.UseItem1Rpc();
            }
            mod = GetClosestEnemyModule();
            if (mod && (Dist(mod.transform.position) < 8 || DistToObjective(mod.transform.position) < 20))
            {
                SetAIMoveTowards(GetPointAwayFromPoint(mod.GetTargetPos(), 2.5f), mod.Space);
                SetLookTowards(mod.GetTargetPos(), mod.Space);
                return;
            }
            if (!Unit.IsEnemyInFront(GetAttackDistance()))
            {
                SetAIMoveTowardsIfDistant(GetObjectiveTarget(), ObjectiveSpace);
                if (DistToObjective(transform.position) > 1)
                {
                    SetLookTowards(GetObjectiveTarget(), ObjectiveSpace);
                }
            }
            return;
        }
        DRIFTER dr = GetClosestEnemyDrifter();
        CREW NearestEnemy = GetClosestEnemyAnywhere();
        if (AI_TacticTimer < 0)
        {
            AI_TacticTimer = UnityEngine.Random.Range(4, 9);
            AI_MoveSpeed = UnityEngine.Random.Range(0.8f, 1.2f);
            if (Unit.IsEnemyInFront(GetAttackDistance()) && dr && UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                Unit.UseGrapple(dr.Space.GetNearestGridToPoint(transform.position));
            }
        }
        //AttemptBoard(dr.Space);
        if (NearestEnemy) SetLookTowards(NearestEnemy.transform.position, NearestEnemy.Space);
        else SetLookTowards(dr.transform.position, null);
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
    public List<CREW> WoundedAlliesInSpace()
    {
        List<CREW> allies = new();
        foreach (var crew in CrewInSpace())
        {
            if (crew.isDeadForever()) continue;
            if (crew == Unit) continue;
            if (crew.GetFaction() == Unit.GetFaction() && crew.GetHealthRelative() < 1f) allies.Add(crew);
        }
        return allies;
    }
    public List<CREW> AlliesInSpace()
    {
        List<CREW> allies = new();
        foreach (var crew in CrewInSpace())
        {
            if (crew.isDeadForever()) continue;
            if (crew == Unit) continue;
            if (crew.GetFaction() == Unit.GetFaction()) allies.Add(crew);
        }
        return allies;
    }
    public List<CREW> CriticallyWoundedAlliesInSpace()
    {
        List<CREW> allies = new();
        foreach (var crew in CrewInSpace())
        {
            if (crew.isDeadForever()) continue;
            if (crew == Unit) continue;
            if (crew.GetFaction() == Unit.GetFaction() && crew.isDead()) allies.Add(crew);
        }
        return allies;
    }
    public List<CREW> CriticallyWoundedAlliesAnywhere()
    {
        List<CREW> allies = new();
        foreach (var crew in CO.co.GetAllCrews())
        {
            if (crew.isDeadForever()) continue;
            if (crew == Unit) continue;
            if (crew.GetFaction() == Unit.GetFaction() && crew.isDead()) allies.Add(crew);
        }
        return allies;
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
            if (enemy is DoorSystem) continue;
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
    public CREW GetClosestCriticallyWoundedAllyInSpace()
    {
        List<CREW> enemies = CriticallyWoundedAlliesAnywhere();
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
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
    public CREW GetClosestCriticallyWoundedAlly()
    {
        List<CREW> enemies = CriticallyWoundedAlliesAnywhere();
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
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
    public CREW GetClosestWoundedAlly()
    {
        List<CREW> enemies = WoundedAlliesInSpace();
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
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
    public CREW GetClosestAlly()
    {
        List<CREW> enemies = AlliesInSpace();
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
    public CREW GetClosestAllyPlayer()
    {
        List<CREW> enemies = AlliesInSpace();
        CREW closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;
        foreach (var enemy in enemies)
        {
            if (enemy == Unit) continue; // skip self
            if (!enemy.IsPlayer()) continue;
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
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
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }
    public CREW GetClosestVisibleEnemyInCone(float dis, float cone)
    {
        List<CREW> enemies = GetVisibleEnemies();
        CREW closest = null;
        float minDist = dis;
        Vector3 myPos = transform.position;
        foreach (var enemy in enemies)
        {
            if (Mathf.Abs(Unit.AngleBetweenPoints(enemy.getPos())) > cone) continue;
            float dist = (enemy.getPos() - myPos).magnitude;
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
            case TOOL.ToolAI.MELEE_LONG_SHIELD:
                return 7f;
            case TOOL.ToolAI.RANGED:
                return 30f;
        }
        return 3f;
    }
    public float GetAttackStayDistance()
    {
        if (!Unit.EquippedToolObject) return 10f;
        switch (Unit.EquippedToolObject.AI)
        {
            case TOOL.ToolAI.MELEE:
                return 3f;
            case TOOL.ToolAI.MELEE_LONG_SHIELD:
                return 6f;
            case TOOL.ToolAI.RANGED:
                return 25f;
        }
        return 3f;
    }

    // Returns true if there is a clear line of sight to the target position (no obstacles in between)
    public bool HasLineOfSight(Vector3 target)
    {
        return Unit.HasLineOfSight(target);
    }

    // Returns all visible enemies within a given radius
    public List<CREW> GetVisibleEnemies()
    {
        List<CREW> visibleEnemies = new();
        Vector3 myPos = transform.position;
        foreach (var enemy in EnemiesInSpace())
        {
            if (enemy == Unit) continue; // skip self
            if (enemy.GetTagState() == CREW.TagStates.DORMANT) continue;
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
    public VoiceHandler GetVoiceHandler()
    {
        return Unit.GetVoiceHandler();
    }

    public float GetVoiceSilenceLevel()
    {
        if (Unit.GetVoiceHandler() == null) return -10f;
        return Unit.GetVoiceHandler().TimeOfSilence();
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

