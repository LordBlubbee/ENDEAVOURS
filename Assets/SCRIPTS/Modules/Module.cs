using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static CO;
using static iDamageable;

public class Module : NetworkBehaviour, iDamageable, iInteractable
{
    // Damageable and Interactable Module
    [NonSerialized] public NetworkVariable<Vector3> OrderPoint = new();

    public bool HealthOnlyWhileDamaged = false;
    public bool HarmDrifterWhenDisabled = true;
    public float OutsideDamageResistance = 1f;
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

    public bool HasOrderPoint()
    {
        return OrderPoint.Value != Vector3.zero;
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
            CREW crew = col.GetComponent<CREW>();
            if (crew != null) {
                if (crew.Space) continue;
                OrderTransform = null;
                if (OrderTransform)
                {
                    OrderPointLocal = OrderTransform.transform.InverseTransformPoint(vec);
                    OrderPoint.Value = OrderTransform.transform.TransformPoint(OrderPointLocal);
                } else
                {
                    OrderPointLocal = vec;
                    OrderPoint.Value = vec;
                }
                return;
            }
        }
        foreach (Collider2D col in Physics2D.OverlapCircleAll(vec, 0.1f))
        {
            if (col.GetComponent<SPACE>() != null)
            {
                OrderTransform = col.GetComponent<SPACE>();
                if (OrderTransform)
                {
                    OrderPointLocal = OrderTransform.transform.InverseTransformPoint(vec);
                    OrderPoint.Value = OrderTransform.transform.TransformPoint(OrderPointLocal);
                }
                else
                {
                    OrderPointLocal = vec;
                    OrderPoint.Value = vec;
                }
                return;
            }
        }
    }

    [Header("MAIN")]
    public string ModuleTag;
    public ModuleTypes ModuleType;
    public Sprite IconSprite;
    public bool ShowModule = true;
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
        DRAGGABLE,
        VAULT,
        DOOR,
        DOOR_CONTROLLER,
        STEAM_REACTOR,
        SPEAKERS_BUFF,
        SPEAKERS_DEBUFF,
        SPEAKERS_HEAL,
        INCENDIARY_STORAGE,
        VENGEANCE_PLATING
    }
    public ModuleTypes GetInteractableType()
    {
        return ModuleType;
    }

    [Rpc(SendTo.Server)]
    public void SendUpgradeRpc()
    {
        if (ModuleLevel.Value == MaxModuleLevel-1) return;
        if (ModuleUpgradeMaterials[ModuleLevel.Value] > CO.co.Resource_Materials.Value) return;
        if (ModuleUpgradeTechs[ModuleLevel.Value] > CO.co.Resource_Tech.Value) return;
        CO.co.Resource_Materials.Value -= ModuleUpgradeMaterials[ModuleLevel.Value];
        CO.co.Resource_Tech.Value -= ModuleUpgradeTechs[ModuleLevel.Value];
        UpgradeLevel();
        CO.co.RequestModuleUpdateRpc();
    }

    public void UpgradeLevel()
    {
        DeactivateModule();
        ModuleLevel.Value++;
        if (CO.co.IsSafe() || GetFaction() != 1) Heal(999);

        ActivateModule();
    }

    [Rpc(SendTo.Server)]
    public void SalvageRpc()
    {
        if (Space.CoreModules.Contains(this)) return;
        CO.co.Resource_Materials.Value += GetMaterialSalvageWorth();
        CO.co.Resource_Tech.Value += GetTechSalvageWorth();
        CO.co.AddInventoryItem(ShowAsModule);
        Space.RemoveModule(this);

        DeactivateModule();

        NetworkObject.Despawn();
    }

    public override void OnNetworkDespawn()
    {
        Space.RemoveModule(this);
    }
    public int GetMaterialSalvageWorth()
    {
        int Worth = 0;
        for (int i = 0; i < ModuleLevel.Value; i++)
        {
            Worth += ModuleUpgradeMaterials[i];
        }
        return Mathf.RoundToInt(Worth * 0.5f);
    }
    public int GetTechSalvageWorth()
    {
        int Worth = 0;
        for (int i = 0; i < ModuleLevel.Value; i++)
        {
            Worth += ModuleUpgradeTechs[i];
        }
        return Mathf.RoundToInt(Worth * 0.5f);
    }

    [Header("STATS")]
    public float MaxHealth = 100f;
   // public float HitboxRadius = 16f;
    [NonSerialized] public NetworkVariable<int> Faction = new();
    [NonSerialized] public NetworkVariable<bool> isDisabled = new();
    [NonSerialized] public NetworkVariable<bool> PermanentlyDead = new();
    [NonSerialized] public NetworkVariable<float> ExtraMaxHealth = new();

    protected NetworkVariable<float> CurHealth = new();

    [Header("RESOURCES")]
    public int ModuleWorth = 50;
    public int MaxModuleLevel => ModuleUpgradeMaterials.Length;
    public int[] ModuleUpgradeMaterials = new int[] { 40, 60, 100, 200, 300 };
    public int[] ModuleUpgradeTechs = new int[] { 0,0,0,0,0 };


    [NonSerialized] public NetworkVariable<int> ModuleLevel = new();
    [NonSerialized] public NetworkVariable<int> SpaceID = new();
    [NonSerialized] public NetworkVariable<int> CurrentInteractors = new();

    private DRIFTER HomeDrifter;
    public DRIFTER GetHomeDrifter()
    {
        return HomeDrifter;
    }
    public void SetHomeDrifter(DRIFTER drifter)
    {
        HomeDrifter = drifter;
    }
    private void Start()
    {
        if (MaxHealth > 0)
        {
            GamerTag CharacterNameTag = Instantiate(CO_SPAWNER.co.PrefabGamerTag);
            CharacterNameTag.SetModuleObject(this);
            CharacterNameTag.SetFarIcon(IconSprite);
            if (HealthOnlyWhileDamaged) CharacterNameTag.SetDamagedOnly();
        }
        switch (ModuleType) {

        }
        Init();
        if (!IsServer)
        {
            StartCoroutine(ClientInitModule());
        }
    }
    IEnumerator ClientInitModule()
    {
        while (Space == null) yield return new WaitForSeconds(0.1f); 
        Debug.Log($"Module spawned. Trying to add to {Space}");
        if (!Space.GetModules().Contains(this))
        {
            Debug.Log($"It was not yet added. Continuing...");
            Space.GetModules().Add(this);

        }
        switch (GetInteractableType())
        {
            case ModuleTypes.NAVIGATION:
                if (Space.CoreModules.Contains(this)) break;
                Space.CoreModules.Add(this);
                break;
            case ModuleTypes.INVENTORY:
                if (Space.CoreModules.Contains(this)) break;
                Space.CoreModules.Add(this);
                break;
            case ModuleTypes.ENGINES:
                if (Space.CoreModules.Contains(this)) break;
                Space.CoreModules.Add(this);
                break;
            case ModuleTypes.MEDICAL:
                if (Space.CoreModules.Contains(this)) break;
                Space.CoreModules.Add(this);
                break;
            case ModuleTypes.MAPCOMMS:
                if (Space.CoreModules.Contains(this)) break;
                Space.CoreModules.Add(this);
                break;
            case ModuleTypes.DOOR:
                break;
            default:
                /*while (Space.SystemModules.Count <= ListPositionModuleSublist.Value)
                {
                    Space.SystemModules.Add(null);
                }
                Space.SystemModules[ListPositionModuleSublist.Value] = this;*/
                if (Space.SystemModules.Contains(this)) break;
                Space.SystemModules.Add(this);
                break;
            case ModuleTypes.WEAPON:
                /* while (Space.WeaponModules.Count <= ListPositionModuleSublist.Value)
                 {
                     Space.WeaponModules.Add(null);
                 }
                 Space.WeaponModules[ListPositionModuleSublist.Value] = this;*/
                if (Space.WeaponModules.Contains(this)) break;
                Space.WeaponModules.Add(this as ModuleWeapon);
                break;
        }

        Debug.Log($"{Space} now has {Space.GetModules().Count} modules");
    }
    protected virtual void ActivateModule()
    {
        //When upgraded or when first activated
    }
    protected virtual void DeactivateModule()
    {
        //When disabled or despawned
    }

    float TakeDamageFromDisablement = 0f;
    void Update()
    {
        if (IsServer)
        {
            if (OrderTransform != null) OrderPoint.Value = OrderTransform.transform.TransformPoint(OrderPointLocal);

            if (HomeDrifter && IsDisabled() && !CO.co.IsSafe() && HarmDrifterWhenDisabled)
            {
                TakeDamageFromDisablement += 1f * CO.co.GetWorldSpeedDelta();
                if (TakeDamageFromDisablement > 15f)
                {
                    TakeDamageFromDisablement -= 5f;
                    HomeDrifter.TakeDamage(5f, transform.position, DamageType.TRUE);
                }
            } else
            {
                TakeDamageFromDisablement = 0f;
            }
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
        ActivateModule();
    }
    public virtual void Heal(float fl)
    {
        if (IsDisabledForever()) return;
        CurHealth.Value = Mathf.Min(GetMaxHealth(), CurHealth.Value + fl);
        if (CurHealth.Value > 99)
        {
            isDisabled.Value = false;
            ActivateModule();
        }
        if (fl > 1) CO_SPAWNER.co.SpawnHealRpc(fl, transform.position);
    }
    public virtual void TakeDamage(float fl, Vector3 src, DamageType type)
    {
        if (MaxHealth < 1) return;
        if (type == DamageType.BOMBARDMENT) fl *= OutsideDamageResistance;
        else
        {
            fl /= 1f + (ModuleLevel.Value * 0.2f);
        }
        if (fl < 0) return;
            CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            //Death
            Die();
        }
        if (fl > 1 && fl < 1000) CO_SPAWNER.co.SpawnDMGRpc(fl, src);
    }

    public void Die(bool Permanent = false)
    {
        CurHealth.Value = 0f;
        isDisabled.Value = true;
        PermanentlyDead.Value = Permanent;
        DeactivateModule();
    }
    public int GetFaction()
    {
        return Faction.Value;
    }
    public bool CanBeTargeted(SPACE space)
    {
        if (space != Space) return false;
        if (MaxHealth < 1) return false;
        return !IsDisabled();
    }
    public virtual bool IsDisabled()
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
        return MaxHealth + ModuleLevel.Value * 100f + ExtraMaxHealth.Value;
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
