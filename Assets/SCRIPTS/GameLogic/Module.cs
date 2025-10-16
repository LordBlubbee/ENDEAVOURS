using System;
using Unity.Netcode;
using UnityEngine;
using static CO;

public class Module : NetworkBehaviour, iDamageable, iInteractable
{
    // Damageable and Interactable Module

    [Header("MAIN")]
    public string ModuleTag;
    public ModuleTypes ModuleType;
    public Sprite IconSprite;
    //
    public enum ModuleTypes
    {
        NAVIGATION,
        WEAPON,
        INVENTORY,
        MAPCOMMS,
        GENERATOR,
        ARMOR,
        ENGINES,
        MEDICAL,
        LOON_NAVIGATION,
        DRAGGABLE
    }

    public ModuleTypes GetInteractableType()
    {
        return ModuleType;
    }

    [Header("STATS")]
    public float MaxHealth = 100f;
    public float HitboxRadius = 16f;
    [NonSerialized] public int Faction;
    protected bool isDisabled = false;

    protected NetworkVariable<float> CurHealth = new();
    [NonSerialized] public NetworkVariable<int> SpaceID = new();
    [NonSerialized] public NetworkVariable<int> CurrentInteractors = new();

    private void Start()
    {
        if (MaxHealth > 0)
        {
            GamerTag CharacterNameTag = Instantiate(CO_SPAWNER.co.PrefabGamerTag);
            CharacterNameTag.SetObject(this);
            CharacterNameTag.SetFarIcon(IconSprite);
        }
        switch (ModuleType) {

        }
        if (!IsServer)
        {
            if (!Space.GetModules().Contains(this)) Space.GetModules().Add(this);
            return;
        }
        
        Init();
    }

    protected bool hasInitialized = false;

    public bool IsCurrentlyInteracted()
    {
        return CurrentInteractors.Value > 0;
    }
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
        if (CurHealth.Value > 50) isDisabled = false;
        if (fl > 1) CO_SPAWNER.co.SpawnHealRpc(fl, transform.position);
    }
    public void TakeDamage(float fl, Vector3 src)
    {
        if (MaxHealth < 1) return;
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            CurHealth.Value = 0f;
            isDisabled = true;
            //Death
        }
        if (fl > 1) CO_SPAWNER.co.SpawnDMGRpc(fl, src);
    }
    public int GetFaction()
    {
        return Faction;
    }
    public bool CanBeTargeted(SPACE space)
    {
        if (space != Space) return false;
        if (MaxHealth < 1) return false;
        return !isDisabled;
    }

    public bool IsDisabled()
    {
        return isDisabled;
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

    public Transform CenterPos;
    public Vector3 GetTargetPos()
    {
        return CenterPos ? CenterPos.position : transform.position;
    }
}
