using System;
using Unity.Netcode;
using UnityEngine;
using static CO;

public class Module : NetworkBehaviour, iDamageable
{
    // Damageable and Interactable Module

    [Header("MAIN")]
    public ModuleTypes ModuleType;
    public enum ModuleTypes
    {
        NAVIGATION,
        WEAPON,
        INVENTORY,
        MAPCOMMS,
        GENERATOR,
        ARMOR_MODULE,
        LOON_NAVIGATION
    }

    [Header("STATS")]
    public float MaxHealth = 100f;
    public float HitboxRadius = 16f;
    [NonSerialized] public int Faction;
    protected bool isDisabled = false;

    protected NetworkVariable<float> CurHealth = new();
    [NonSerialized] public NetworkVariable<int> SpaceID = new();

    private void Start()
    {
        if (!IsServer)
        {
            if (!Space.GetModules().Contains(this)) Space.GetModules().Add(this);
            return;
        }
        
        Init();
    }

    protected bool hasInitialized = false;

    public SPACE Space { get { return CO.co.GetSpace(SpaceID.Value); } set { } }
    public virtual void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        CurHealth.Value = MaxHealth;
    }
    public void Heal(float fl)
    {
        CurHealth.Value = Mathf.Min(MaxHealth, CurHealth.Value + fl);
        if (CurHealth.Value > MaxHealth * 0.5f) isDisabled = false;
    }
    public void TakeDamage(float fl, Vector3 src)
    {
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            CurHealth.Value = 0f;
            isDisabled = true;
            //Death
        }
    }
    public int GetFaction()
    {
        return Faction;
    }
    public bool CanBeTargeted()
    {
        return !isDisabled;
    }
}
