using Unity.Netcode;
using UnityEngine;
using static CO;

public class ResourceCrate : NetworkBehaviour, iDamageable
{
    public float MaxHealth = 100;
    public ResourceTypes ResourceType;
    public NetworkVariable<int> ResourceAmount = new();
    public NetworkVariable<float> CurHealth = new();
    public SPACE Space { get; set; }
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

    public float GetHealthRelative()
    {
        return GetHealth() / GetMaxHealth();
    }
}
