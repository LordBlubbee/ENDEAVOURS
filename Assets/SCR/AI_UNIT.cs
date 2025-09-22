using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AI_GROUP;

public class AI_UNIT : MonoBehaviour
{
    public CREW Unit;
    public AI_UNIT_TYPES AI_Unit_Type;
    private AI_TACTICS AI_Objective;
    private AI_GROUP Group;
    private Vector3 ObjectiveTarget;
    private Vector3 AI_MoveTarget;
    private bool AI_IsMoving;
    private Vector3 AI_LookTarget;
    private bool AI_IsLooking;
    private bool AI_IsUsingItem1;
    private bool AI_IsUsingItem2;
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
            }
            if (AI_IsMoving) Unit.SetMoveInput((AI_MoveTarget-transform.position).normalized);
            else Unit.SetMoveInput(Vector3.zero);
            if (AI_IsLooking) Unit.SetLookTowards(AI_LookTarget);
            else Unit.SetLookTowards(Vector3.zero);
            if (AI_IsUsingItem1) Unit.UseItem1();
            if (AI_IsUsingItem2) Unit.UseItem2();

            yield return null;
        }
    }

    List<Vector2Int> path = new();
    int pathTravelIndex = 0;
    public void SetPath(Vector3 trt)
    {
        //path = Pathfinder.FindPath(CO.co.ConvertWorldToGrid(transform.position), CO.co.ConvertWorldToGrid(trt), CO.co.RoomGrid);
        pathTravelIndex = 0;
    }
    public void DeletePath()
    {
        path = null;
        pathTravelIndex = 0;
    }
    private SPACE getSpace()
    {
        return Unit.space;
    }
}
public static class Pathfinder
{
    // Simple Priority Queue implementation for .NET Framework 4.7.1
    private class SimplePriorityQueue<T>
    {
        private SortedDictionary<int, Queue<T>> dict = new();

        public int Count { get; private set; } = 0;

        public void Enqueue(T item, int priority)
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

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, List<Vector2Int> walkableTiles)
    {
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, int> costSoFar = new();
        SimplePriorityQueue<Vector2Int> frontier = new();

        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (!walkableTiles.Contains(neighbor))
                    continue;

                int newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    int priority = newCost + Heuristic(neighbor, goal);
                    frontier.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal)) return null; // No path

        // Reconstruct path
        List<Vector2Int> path = new();
        Vector2Int temp = goal;
        while (temp != start)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }
        path.Reverse();
        return path;
    }

    // You should already have this function, but if not:
    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}

