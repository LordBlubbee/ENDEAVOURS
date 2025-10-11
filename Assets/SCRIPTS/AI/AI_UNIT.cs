using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static AI_GROUP;

public class AI_UNIT : NetworkBehaviour
{
    public CREW Unit;
    public AI_UNIT_TYPES AI_Unit_Type;
    private float AI_TacticTimer;
    private AI_TACTICS AI_Tactic;
    private AI_GROUP Group;
    private Vector3 ObjectiveTarget = Vector3.zero;
    private Transform ObjectiveTargetTransform = null;

    private CREW EnemyTarget;
    private float EnemyTargetTimer;

    private Vector3 AI_MoveTarget;
    private bool AI_IsMoving;
    private float AI_MoveTimer;
    private Vector3 AI_LookTarget;
    private bool AI_IsLooking;

    public void SetEnemyTarget(CREW crew)
    {
        EnemyTarget = crew;
        EnemyTargetTimer = 5;
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
    public void SetObjectiveTarget(Vector3 target)
    {
        ObjectiveTarget = target;
        ObjectiveTargetTransform = null;
    }
    public void SetObjectiveTarget(Transform target)
    {
        ObjectiveTargetTransform = target;
        ObjectiveTarget = target.position;
    }

    public float GetObjectiveDistance()
    {
        if (ObjectiveTarget == Vector3.zero) return 0f;
        return (ObjectiveTarget - transform.position).magnitude;
    }
    public AI_GROUP.AI_OBJECTIVES CurrentObjective()
    {
        return Group.AI_Objective;
    }

    public float DistToMoveTarget()
    {
        return (AI_MoveTarget - transform.position).magnitude;
    }
    private void SetMoveTowards(Vector3 trt)
    {
        AI_MoveTarget = trt;
        AI_IsMoving = true;
    }
    private bool SetMoveTowardsIfExpired(Vector3 trt, float movtimer)
    {
        if (AI_MoveTimer > 0f && DistToMoveTarget() > 1f) return false;
        AI_MoveTarget = trt;
        AI_IsMoving = true;
        AI_MoveTimer = movtimer;
        return true;
    }
    private void StopMoving()
    {
        AI_IsMoving = false;
    }
    private void SetLookTowards(Vector3 trt)
    {
        AI_LookTarget = trt;
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
        INTERACT_NAVIGATION,
        INTERACT_WEAPONS,
        INTERACT_REPAIR
    }
    public enum AI_UNIT_TYPES
    {
        CREW,
        LOONCRAB
    }
    private void Start()
    {
        if (!IsServer) return;
        StartCoroutine(RunAI());
    }
    IEnumerator RunAI()
    {
        float Tick = 0f;
        while (true)
        {
            Tick -= CO.co.GetWorldSpeedDelta();
            if (Tick < 0f)
            {
                Tick = 0.25f;
                //REEVALUATE
                AITick();
            }
            if (AI_IsMoving)
            {
                if (getSpace() && !HasLineOfSight(AI_MoveTarget))
                {
                    if (path == null)
                    {
                        SetPath(AI_MoveTarget);
                    }
                    if (path != null)
                    {
                        if (pathTravelIndex >= path.Count)
                        {
                            path = null;
                        } else
                        {
                            Vector3 trt = getSpace().ConvertGridToWorld(path[pathTravelIndex]);
                            Unit.SetMoveInput((trt - transform.position).normalized);
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
                    Unit.SetMoveInput((AI_MoveTarget - transform.position).normalized);
                }
            }
            else Unit.SetMoveInput(Vector3.zero);
            if (AI_IsLooking) Unit.SetLookTowards(AI_LookTarget);
            else Unit.SetLookTowards(Vector3.zero);

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
            ObjectiveTarget = ObjectiveTargetTransform.position;
        }

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
        CREW trt = GetClosestEnemy();
    }
    private void AITick_Looncrab()
    {
        Module mod;
        if (getSpace())
        {
            if (!EnemyTarget || EnemyTargetTimer < 0)
            {
                EnemyTarget = GetClosestVisibleEnemy();
            }
            if (EnemyTarget)
            {
                EnemyTargetTimer = 5;
                AttemptBoard(EnemyTarget.Space);

                if (AI_TacticTimer < 0) SwitchTacticsLooncrab();

                switch (AI_Tactic)
                {
                    case AI_TACTICS.SKIRMISH:
                       
                        SetMoveTowards(EnemyTarget.transform.position);
                        SetLookTowards(EnemyTarget.transform.position);
                        break;
                    case AI_TACTICS.CIRCLE:
                        if (Unit.GetHealthRelative() < 0.3f)
                        {
                            SetMoveTowards(GetPointTowardsPoint(EnemyTarget.transform.position,-12f));
                        }
                        else
                        {
                            SetMoveTowardsIfExpired(GetRandomPointAround(EnemyTarget.transform.position,5f,12f), 3f);
                        }
                        Unit.UseItem2();
                        SetLookTowards(EnemyTarget.transform.position);
                        break;
                    case AI_TACTICS.CHARGE:
                        SetMoveTowardsIfExpired(GetPointTowardsPoint(EnemyTarget.transform.position, 24f), UnityEngine.Random.Range(2f,4f));
                        SetLookTowards(EnemyTarget.transform.position);
                        Unit.Dash();
                        break;
                    case AI_TACTICS.SABOTAGE:
                        mod = GetClosestEnemyModule();
                        SetMoveTowards(mod.transform.position);
                        if (Dist(mod.transform.position) < 8f)
                        {
                            SetLookTowards(mod.transform.position);
                        } else
                        {
                            Unit.UseItem2();
                            SetLookTowards(EnemyTarget.transform.position);
                            Unit.Dash();
                        }
                        break;
                }
                if (Unit.IsEnemyInFront(5f))
                {
                    Unit.UseItem1();
                }
                return;
            }
            mod = GetClosestEnemyModule();
            if (mod && Dist(mod.transform.position) < 20)
            {
                SetMoveTowards(mod.transform.position);
                SetLookTowards(mod.transform.position);
                return;
            }
            SetMoveTowards(ObjectiveTarget);
            SetLookTowards(Vector3.zero); 
            if (Unit.IsEnemyInFront(5f))
            {
                Unit.UseItem1();
            }
            return;
        }
        AttemptBoard(GetClosestEnemyDrifter().Space);
        SetMoveTowards(ObjectiveTarget);
        SetLookTowards(Vector3.zero);
        if (Unit.IsEnemyInFront(5f))
        {
            Unit.UseItem1();
        }
    }

    private void SwitchTacticsLooncrab()
    {
        ResetWeights();
        AddWeights(0, 15);
        AddWeights(1, 10);
        if (Dist(EnemyTarget.transform.position) > 16f) AddWeights(2, 10);
        AddWeights(3, 5);
        switch (GetWeight())
        {
            case 0:
                SetTactic(AI_TACTICS.SKIRMISH, UnityEngine.Random.Range(4,8));
                break;
            case 1:
                SetTactic(AI_TACTICS.CIRCLE, UnityEngine.Random.Range(4, 8));
                break;
            case 2:
                SetTactic(AI_TACTICS.CHARGE, 4);
                break;
            case 3:
                SetTactic(AI_TACTICS.SABOTAGE, UnityEngine.Random.Range(6, 9));
                break;
        }
    }

    private void SetLookTowardsMoveDirection()
    {
        SetLookTowards(transform.position + Unit.GetMoveInput() * 100f);
    }
    private void AttemptBoard(SPACE trt)
    {
        if (trt != getSpace())
        {
            WalkableTile boarding = trt.GetNearestBoardingGridTransformToPoint(transform.position);
            if (Dist(boarding.transform.position) < 25f+Unit.GetATT_COMMUNOPATHY())
            {
                Unit.UseGrapple(boarding);
            }
        }
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
            if (isEnemy(crew)) enemies.Add(crew);
        }
        return enemies;
    }
    public bool isEnemy(CREW other)
    {
        return Unit.Faction != other.Faction && other.Faction != 0;
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
            if (!enemy.CanBeTargeted()) continue;
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

    public Vector3 GetPointTowardsPoint(Vector3 trt, float dis)
    {
        return transform.position + (trt- transform.position).normalized * dis;
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

