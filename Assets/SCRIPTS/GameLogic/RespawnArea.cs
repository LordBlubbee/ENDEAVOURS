using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnArea : NetworkBehaviour
{
    public int Faction;
    public float BaseRespawnDelay = 20f;
    public bool ActivelySpawning = false;
    private float CurrentRespawnDelay = 0f;
    SPACE Space { get; set; }

    public void SetSpace(SPACE space)
    {
        Space = space;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (!ActivelySpawning) return;
        CurrentRespawnDelay -= CO.co.GetWorldSpeedDelta();
        if (CurrentRespawnDelay < 0)
        {
            CurrentRespawnDelay = 2;
            foreach (CREW un in CO.co.GetAlliedCrew(Faction))
            {
                if (!un.isDead()) continue;
                if (un.isDeadButReviving()) continue;
                un.ForceRevive();
                un.TeleportCrewMember(transform.position, Space);
                CurrentRespawnDelay = BaseRespawnDelay;
                break;
            }
        }
    }
    private void Despawn()
    {
        NetworkObject.Despawn();
    }
}
