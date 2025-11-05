using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DUNGEON : NetworkBehaviour
{
    public SPACE Space;
    public AI_GROUP AI;
    public LayerMask DrifterLayer;
    private void Start()
    {
        Init();
    }
    bool hasInitialized = false;
    private NetworkVariable<int> DungeonVariant = new();
    public int MaximumDungeonVariants = 1;
    public List<DungeonTiles> Variants = new List<DungeonTiles>();
    [Serializable] public struct DungeonTiles
    {
        public List<WalkableTile> WalkablesRemoved;
        public List<SpriteRenderer> SpritesRemoved;
        public List<WalkableTile> MainObjectivePossibilities;
        public List<WalkableTile> TrapPossibilities;
    }
    public void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        if (IsServer)
        {
            AI.SetAIHome(this);
            float Dis = 40f;
            bool Distancing = true;
            while (Distancing)
            {
                Dis += 2f;
                if (Dis > 100) break;
                transform.position = CO.co.PlayerMainDrifter.transform.position + (CO.co.PlayerMainDrifter.getLeftVector()).normalized * Dis;
                Distancing = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position,16f, DrifterLayer))
                {
                    if (col.GetComponent<DRIFTER>() != null)
                    {
                        Distancing = true;
                        break;
                    }
                }
            }
            float ang = Vector2.SignedAngle(new Vector3(1,0), CO.co.PlayerMainDrifter.transform.position - transform.position);
            transform.Rotate(Vector3.forward, ang);
        }
        GenerateDungeon();
        Space.Init();
    }

    public void SetDungeonVariant()
    {
        DungeonVariant.Value = UnityEngine.Random.Range(1, MaximumDungeonVariants);
    }
    private void GenerateDungeon()
    {
        //Dungeon is preset, but has different components that can be taken out/removed
        if (DungeonVariant.Value == 0)
        {
            Debug.Log("Error: Variant 0");
            return;
        }

        //Remove different extra rooms
        foreach (WalkableTile tile in Variants[DungeonVariant.Value-1].WalkablesRemoved)
        {
            if (tile == null) continue;
            Space.RoomTiles.Remove(tile);
            tile.gameObject.SetActive(false);
        }
        foreach (SpriteRenderer tile in Variants[DungeonVariant.Value - 1].SpritesRemoved)
        {
            if (tile == null) continue;
            tile.gameObject.SetActive(false);
        }

        if (IsServer)
        {
            //Spawn different traps, rooms, or objectives
        }
    }
}
