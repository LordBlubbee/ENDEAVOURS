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
    private AI_TACTICS AI_Tactic;
    private AI_GROUP Group;
    private Vector3 ObjectiveTarget;
    private Vector3 AI_MoveTarget;
    private bool AI_IsMoving;
    private Vector3 AI_LookTarget;
    private bool AI_IsLooking;

    public void AddToGroup(AI_GROUP grp)
    {
        Group = grp;
        grp.RegisterUnit(this);
    }
    public void SetTactic(Vector3 target)
    {
        ObjectiveTarget = target;
    }
    public AI_GROUP.AI_OBJECTIVES CurrentObjective()
    {
        return Group.AI_Objective;
    }
    private void SetMoveTowards(Vector3 trt)
    {
        AI_MoveTarget = trt;
        AI_IsMoving = true;
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
        SKIRMISH,
        MOVE_COMMAND,
        CHARGE,
        CIRCLE,
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
        CREW trt = GetClosestEnemy();
        if (trt)
        {
            AttemptBoard(trt);
            SetMoveTowards(trt.transform.position);
            SetLookTowards(trt.transform.position);
            //Unit.UseItem1();
        }
        else
        {
            StopMoving();
            StopLooking();
        }
    }
    private void AttemptBoard(CREW trt)
    {
        if (trt.Space != getSpace())
        {
            if (Dist(trt.Space.GetNearestGridToPoint(transform.position)) < 25f+Unit.GetATT_COMMUNOPATHY())
            {
                Unit.UseGrapple(trt.Space);
            }
        }
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
        float dist = UnityEngine.Random.Range(0f, radius);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * dist;
        return center + offset;
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

