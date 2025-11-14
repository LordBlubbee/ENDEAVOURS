using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static iDamageable;

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
    private NetworkVariable<FixedString32Bytes> DungeonVariantData = new();
    public List<DungeonAlteration> Alterations = new List<DungeonAlteration>();
    public List<WalkableTile> MainObjectivePossibilities;
    public List<WalkableTile> TrapPossibilities;

    [Serializable]
    public struct DungeonAlteration
    {
        public List<DungeonVariants> Variants;
    }
    [Serializable] public struct DungeonVariants
    {
        public List<WalkableTile> WalkablesRemoved;
        public List<SpriteRenderer> SpritesRemoved;
    }
    public void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        StartCoroutine(CreateSoundwaves());
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
    IEnumerator CreateSoundwaves()
    {
        float Timer = 0f;
        while (true)
        {
            Timer -= CO.co.GetWorldSpeedDelta();
            if (Timer < 0f)
            {
                foreach (CREW Crew in Space.GetCrew())
                {
                    float Dis = (LOCALCO.local.GetPlayer().transform.position - Crew.transform.position).magnitude;
                    if (UnityEngine.Random.Range(0f, 1f) < 0.6f && Dis < 45)
                    {
                        CO_SPAWNER.co.SpawnSoundwave(Crew.transform.position, 1.5f - Dis/45f);
                    }
                }
                Timer = UnityEngine.Random.Range(2f, 5f);
            }
            yield return null;
        }
    }
    private void GenerateDungeon()
    {
        //Dungeon is preset, but has different components that can be taken out/removed
        string Data = DungeonVariantData.Value.ToString();
        Debug.Log($"Receiving dungeon data {Data}");
        int i = 0;
        foreach (DungeonAlteration alt in Alterations)
        {
            char c = Data[i];
            int ID = c - '0';
            Debug.Log($"Digit {i} equals {ID}");
            if (ID == 0)
            {
                Debug.Log("Error: Variant 0");
                return;
            }
            //Remove different extra rooms
            DungeonVariants ChosenVar = alt.Variants[ID-1];
            foreach (WalkableTile tile in ChosenVar.WalkablesRemoved)
            {
                if (tile == null) continue;
                Space.RoomTiles.Remove(tile);
                MainObjectivePossibilities.Remove(tile);
                TrapPossibilities.Remove(tile);
                tile.gameObject.SetActive(false);
            }
            foreach (SpriteRenderer tile in ChosenVar.SpritesRemoved)
            {
                if (tile == null) continue;
                tile.gameObject.SetActive(false);
            }
            i++;
        }
    }
    public void Impact(PROJ fl, Vector3 ImpactArea)
    {
        float Damage = fl.AttackDamage;
        foreach (CREW mod in Space.GetCrew())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (Dist < fl.CrewDamageSplash)
            {
                mod.TakeDamage(Damage * fl.CrewDamageModifier, mod.transform.position, DamageType.TRUE);
            }
        }
    }
    public void SetDungeonVariant()
    {
        string str = "";
        foreach (DungeonAlteration alter in Alterations)
        {
            int ID = UnityEngine.Random.Range(0, alter.Variants.Count) + 1;
            str += ID.ToString();
        }
        DungeonVariantData.Value = str;
        Debug.Log($"Setting dungeon data {DungeonVariantData.Value}");
    }
    public void DespawnAndUnregister()
    {
        Debug.Log("Despawn and unregister");
        CO.co.UnregisterSpace(Space);
        NetworkObject.Despawn();
    }
}
