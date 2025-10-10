using Unity.Netcode;
using UnityEngine;
using static CO;
using static Module;

public class ResourceCrate : NetworkBehaviour, iDamageable, iInteractable
{
    public float MaxHealth = 100;
    public ResourceTypes ResourceType;
    public NetworkVariable<int> ResourceAmount = new();
    public NetworkVariable<float> CurHealth = new();
    public NetworkVariable<bool> IsGrabbed = new();
    public SPACE Space { get; set; }
    public ModuleTypes GetInteractableType()
    {
        return ModuleTypes.DRAGGABLE;
    }
    public void Heal(float fl)
    {
    }
    public void TakeDamage(float fl, Vector3 src)
    {
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
    public bool CanBeTargeted()
    {
        return true;
    }
    public float GetHealth()
    {
        return CurHealth.Value;
    }

    public float GetMaxHealth()
    {
        return MaxHealth;
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
