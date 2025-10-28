using System;
using Unity.Netcode;
using UnityEngine;
using static CO;

public class Module : NetworkBehaviour, iDamageable, iInteractable
{
    // Damageable and Interactable Module
    [NonSerialized] public NetworkVariable<Vector3> OrderPoint = new();
    private Vector3 OrderPointLocal;
    private SPACE OrderTransform;
    public Vector3 GetOrderPoint()
    {
        return OrderPoint.Value;
    }
    public SPACE GetOrderTransform()
    {
        return OrderTransform;
    }

    [Rpc(SendTo.Server)]
    public void SetOrderPointRpc(Vector3 vec)
    {
        if (vec == Vector3.zero)
        {
            OrderTransform = null;
            OrderPointLocal = Vector3.zero;
            OrderPoint.Value = Vector3.zero;
            return;
        }
        foreach (Collider2D col in Physics2D.OverlapCircleAll(vec,0.1f))
        {
            if (col.GetComponent<CREW>() != null) {
                OrderTransform = col.GetComponent<SPACE>();
                OrderPointLocal = OrderTransform.transform.InverseTransformPoint(vec);
                OrderPoint.Value = OrderTransform.transform.TransformPoint(OrderPointLocal);
                return;
            }
        }
        foreach (Collider2D col in Physics2D.OverlapCircleAll(vec, 0.1f))
        {
            if (col.GetComponent<SPACE>() != null)
            {
                OrderTransform = col.GetComponent<SPACE>();
                OrderPointLocal = OrderTransform.transform.InverseTransformPoint(vec);
                OrderPoint.Value = OrderTransform.transform.TransformPoint(OrderPointLocal);
                return;
            }
        }
    }

    [Header("MAIN")]
    public string ModuleTag;
    public ModuleTypes ModuleType;
    public Sprite IconSprite;
    public ScriptableEquippableModule ShowAsModule;
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
    [NonSerialized] public NetworkVariable<bool> isDisabled = new();
    [NonSerialized] public NetworkVariable<bool> PermanentlyDead = new();

    protected NetworkVariable<float> CurHealth = new();
    [NonSerialized] public NetworkVariable<int> ModuleLevel = new();
    [NonSerialized] public NetworkVariable<int> SpaceID = new();
    [NonSerialized] public NetworkVariable<int> CurrentInteractors = new();

    private void Start()
    {
        if (MaxHealth > 0)
        {
            GamerTag CharacterNameTag = Instantiate(CO_SPAWNER.co.PrefabGamerTag);
            CharacterNameTag.SetModuleObject(this);
            CharacterNameTag.SetFarIcon(IconSprite);
        }
        switch (ModuleType) {

        }
        Init();
        if (!IsServer)
        {
            Debug.Log($"Module spawned. Trying to add to {Space}");
            if (!Space.GetModules().Contains(this))
            {
                Space.GetModules().Add(this);
                switch (GetInteractableType())
                {
                    case ModuleTypes.NAVIGATION:
                        Space.CoreModules.Add(this);
                        break;
                    case ModuleTypes.INVENTORY:
                        Space.CoreModules.Add(this);
                        break;
                    case ModuleTypes.ENGINES:
                        Space.CoreModules.Add(this);
                        break;
                    case ModuleTypes.MEDICAL:
                        Space.CoreModules.Add(this);
                        break;
                    case ModuleTypes.MAPCOMMS:
                        Space.CoreModules.Add(this);
                        break;
                    default:
                        Space.SystemModules.Add(this);
                        break;
                    case ModuleTypes.WEAPON:
                        Space.WeaponModules.Add(this as ModuleWeapon);
                        break;
                }
            }
           
            Debug.Log($"{Space} now has {Space.GetModules().Count} modules");
            return;
        }
        
    }
    void Update()
    {
        if (IsServer)
        {
            if (OrderTransform != null) OrderPoint.Value = OrderTransform.transform.TransformPoint(OrderPointLocal);
        }
        Frame();
    }
    protected virtual void Frame()
    {

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

        if (!IsServer) return;
        CurHealth.Value = MaxHealth;
    }
    public void Heal(float fl)
    {
        if (IsDisabledForever()) return;
        CurHealth.Value = Mathf.Min(MaxHealth, CurHealth.Value + fl);
        if (CurHealth.Value > 99) isDisabled.Value = false;
        if (fl > 1) CO_SPAWNER.co.SpawnHealRpc(fl, transform.position);
    }
    public void TakeDamage(float fl, Vector3 src)
    {
        if (MaxHealth < 1) return;
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            //Death
            Die();
        }
        if (fl > 1) CO_SPAWNER.co.SpawnDMGRpc(fl, src);
    }

    public void Die(bool Permanent = false)
    {
        CurHealth.Value = 0f;
        isDisabled.Value = true;
        PermanentlyDead.Value = Permanent;
    }
    public int GetFaction()
    {
        return Faction;
    }
    public bool CanBeTargeted(SPACE space)
    {
        if (space != Space) return false;
        if (MaxHealth < 1) return false;
        return !IsDisabled();
    }
    public bool IsDisabled()
    {
        return isDisabled.Value;
    }
    public bool IsDisabledForever()
    {
        return PermanentlyDead.Value;
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
