using Unity.Netcode;
using UnityEngine;
using static CO;
using static Module;

public class ResourceCrate : NetworkBehaviour, iDamageable, iInteractable
{
    private NetworkVariable<float> MaxHealth = new(100);
    public ResourceTypes ResourceType;
    private NetworkVariable<int> ResourceAmount = new();
    private NetworkVariable<float> CurHealth = new();
    private NetworkVariable<bool> IsGrabbed = new();
    public GameObject DestructionParticles;
    public SPACE Space { get; set; }
    public ModuleTypes GetInteractableType()
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
        CurHealth.Value = Mathf.Clamp(CurHealth.Value - fl, 0, GetMaxHealth());
        if (CurHealth.Value <= 0)
        {
            DestructionRpc();
            RemoveCrate();
        }
        CO_SPAWNER.co.SpawnDMGRpc(fl, transform.position);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void DestructionRpc()
    {
        Instantiate(DestructionParticles, transform.position, Quaternion.identity);
    }
    public void GainMaterials(float fl)
    {
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
        TECHNOLOGY
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
        NetworkObject.Despawn();
    }
}
