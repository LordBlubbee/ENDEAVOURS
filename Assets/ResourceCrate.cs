using System;
using Unity.Netcode;
using UnityEngine;
using static CO;
using static iDamageable;
using static Module;

public class ResourceCrate : NetworkBehaviour, iDamageable
{
    private NetworkVariable<float> MaxHealth = new(100);
    public ResourceTypes ResourceType;
    private NetworkVariable<int> ResourceAmount = new();
    private NetworkVariable<float> CurHealth = new();
    private NetworkVariable<bool> IsGrabbed = new();
    public GameObject DestructionParticles;

    public AudioClip DestructionSFX;
    public float DestructionDamage = 0;
    public float DestructionRadius = 0;
    public SPACE Space { get; set; }
    public void SetSpace(SPACE space)
    {
        Space = space;
        transform.SetParent(space.transform);
    }
    public ModuleTypes GetInteractableType() //Add iInteractable to reactivate, currently unused
    {
        return ModuleTypes.DRAGGABLE;
    }
    public void Heal(float fl)
    {
        if (fl < 0) return;
        CurHealth.Value = Mathf.Clamp(CurHealth.Value + fl, 0, GetMaxHealth());
        CO_SPAWNER.co.SpawnHealRpc(fl, transform.position);
    }
    public void TakeDamage(float fl, Vector3 src, iDamageable.DamageType type)
    {
        if (fl < 0) return;
        if (CurHealth.Value == 0) return;
        CurHealth.Value = Mathf.Clamp(CurHealth.Value - fl, 0, GetMaxHealth());
        if (CurHealth.Value <= 0)
        {
            DestructionRpc();
            GainMaterials();

            if (DestructionRadius > 0)
            {
                foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position, DestructionRadius))
                {
                    iDamageable Crew = col.GetComponent<iDamageable>();
                    if (Crew != null)
                    {
                        if (!Crew.CanBeTargeted(Space)) continue;
                        float DisFactor = 0.5f + (1f - Mathf.Clamp01(Vector3.Distance(transform.position, Crew.transform.position) / DestructionRadius))*0.5f;
                        Crew.TakeDamage(DestructionDamage * DisFactor * (0.8f + GetMaxHealth() * 0.01f), Crew.transform.position, DamageType.ENVIRONMENT_FIRE);
                    }
                }
            }
            RemoveCrate();
        }
        CO_SPAWNER.co.SpawnDMGRpc(fl, transform.position);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void DestructionRpc()
    {
        Instantiate(DestructionParticles, transform.position, Quaternion.identity);
        AUDCO.aud.PlaySFX(DestructionSFX, transform.position, 0.2f);
    }
    public void GainMaterials()
    {
        float fl = ResourceAmount.Value;
        if ((int)fl == 0) return;
        switch (ResourceType)
        {
            case ResourceTypes.MATERIALS:
                CO.co.Resource_Materials.Value += (int)fl;
                CO_SPAWNER.co.SpawnWordsRpc($"<color=yellow>+{fl.ToString("0")} MATERIALS", transform.position);
                break;
            case ResourceTypes.SUPPLIES:
                CO.co.Resource_Supplies.Value += (int)fl;
                CO_SPAWNER.co.SpawnWordsRpc($"<color=green>+{fl.ToString("0")} SUPPLIES", transform.position);
                break;
            case ResourceTypes.AMMUNITION:
                CO.co.Resource_Ammo.Value += (int)fl;
                CO_SPAWNER.co.SpawnWordsRpc($"<color=red>+{fl.ToString("0")} AMMO", transform.position);
                break;
            case ResourceTypes.TECHNOLOGY:
                CO.co.Resource_Tech.Value += (int)fl;
                CO_SPAWNER.co.SpawnWordsRpc($"<color=#00FFFF>+{fl.ToString("0")} TECH", transform.position);
                break;
        }
    }
    public enum ResourceTypes
    {
        MATERIALS,
        SUPPLIES,
        AMMUNITION,
        TECHNOLOGY,
        NONE
    }
    public int GetFaction()
    {
        return 0;
    }
    public bool CanBeTargeted(SPACE space)
    {
        if (space != Space) return false;
        return true;
    }
    public float GetHealth()
    {
        return CurHealth.Value;
    }
    public float GetMaxHealth()
    {
        return MaxHealth.Value;
    }
    public void InitCrate(float health, float resourceamount, ResourceTypes type)
    {
        MaxHealth.Value = health;
        CurHealth.Value = health;
        ResourceAmount.Value = (int)resourceamount;
        ResourceType = type;
    }
    public bool IsDisabled()
    {
        return IsGrabbed.Value;
    }
    public float GetHealthRelative()
    {
        return GetHealth() / GetMaxHealth();
    }

    public void RemoveCrate()
    {
        Debug.Log($"Crate Children: {transform.childCount}");
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Debug.Log($"Despawning child object of ResourceCrate {child.name}");
            NetworkObject ob = child.GetComponent<NetworkObject>();
            if (ob) ob.Despawn();
        }
        NetworkObject.Despawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
}
