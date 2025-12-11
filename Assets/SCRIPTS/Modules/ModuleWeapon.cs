using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModuleWeapon : Module
{
    [Header("References")]
    public string WeaponName;
    public Transform WeaponTransform;
    public Transform Platform;
    public Transform FirePoint;
    public PROJ FireProjectile;
    public AudioClip[] Fire_SFX;
    public Sprite CrosshairSprite;
    public ResourceCrate.ResourceTypes ResourceType = ResourceCrate.ResourceTypes.AMMUNITION;

    [Header("Offensive Stats")]
    public float Damage;
    public float RotationBaseSpeed = 30;
    public int MaxAmmo = 50;
    public float FireCooldown = 2f;
    public float ReloadCooldown = 5f;
    public int ProjectileCount = 1;
    public float AdditionalProjectileDelay = 0.2f;

    [Header("Level upgrades")]
    public float DamagePerLevel;
    public float ProjectilesPerLevel;
    public float CooldownPerLevel;

    private NetworkVariable<float> Rotation = new();
    [NonSerialized] public NetworkVariable<float> CurCooldown = new();
    [NonSerialized] public NetworkVariable<int> LoadedAmmo = new();
    [NonSerialized] public NetworkVariable<bool> ReloadingCurrently = new();
    [NonSerialized] public NetworkVariable<bool> AutofireActive = new(true);

    [Rpc(SendTo.Server)]
    public void SetAutofireRpc(bool bol)
    {
        AutofireActive.Value = bol;
    }

    public int GetProjectileCount()
    {
        return ProjectileCount + Mathf.FloorToInt(ProjectilesPerLevel * ModuleLevel.Value);
    }
    public float GetDamage()
    {
        return Damage + DamagePerLevel * ModuleLevel.Value;
    }
    public float GetFireCooldown()
    {
        return FireCooldown + CooldownPerLevel * ModuleLevel.Value;
    }

    public bool IsOnCooldown()
    {
        return !canFire;
    }
    public float GetAmmoRatio()
    {
        return (float)LoadedAmmo.Value / (float)MaxAmmo;
    }
    public int GetAmmo()
    {
        return LoadedAmmo.Value;
    }

    private Vector3 LookTowards;
    private bool isLooking = false;

    [Rpc(SendTo.Server)]
    public void SetLookTowardsRpc(Vector2 mov)
    {
        SetLookTowards(mov);
    }
    public void SetLookTowards(Vector2 mov)
    {
        LookTowards = mov;
        isLooking = mov != Vector2.zero;
    }
    public override void Init()
    {
        //Server only
        if (hasInitialized) return;
        hasInitialized = true;

        if (!IsServer) return;
        CurHealth.Value = MaxHealth;

    }

    [Rpc(SendTo.Server)]
    public void ReloadAmmoRpc()
    {
        if (ReloadingCurrently.Value) return;
        if (GetFaction() == 1)
        {
            if (CO.co.Resource_Ammo.Value < 10) return;
            CO.co.Resource_Ammo.Value -= 10;
        }
        CO_SPAWNER.co.SpawnWordsRpc("RELOADING...",transform.position);
        StartCoroutine(AttackReloadAmmo());
    }

    public void ReloadInstantly()
    {
        if (ReloadingCurrently.Value) return;
        StartCoroutine(AttackReloadAmmo());
    }

    public void LoadWeaponsNow()
    {
        StartCoroutine(LoadWeapons());
    }

    public bool EligibleForReload()
    {
        if (CO.co.Resource_Ammo.Value < 10 && GetFaction() == 1) return false;
        if (IsDisabled()) return false;
        if (GetAmmo() > 0) return false;
        return AutofireActive.Value || GetOrderPoint() != Vector3.zero;
    }
    protected override void Frame()
    {
        if (!IsServer)
        {
            WeaponTransform.rotation = Quaternion.Euler(0, 0, Rotation.Value);
            return;
        }
        if (isUsing) Fire(ShootTowards);
        if (IsDisabled()) return;
        if (isLooking)
        {
            float ang = AngleToTurnTarget();
            if (ang > 1f)
            {
                WeaponTransform.Rotate(Vector3.forward, Mathf.Min(RotationBaseSpeed * CO.co.GetWorldSpeedDelta(), ang * RotationBaseSpeed * 0.1f));
                ang = AngleToTurnTarget();
                if (AngleToTurnTarget() < 0f)
                {
                    WeaponTransform.Rotate(Vector3.forward, ang);
                    isLooking = false;
                }
                Rotation.Value = WeaponTransform.rotation.eulerAngles.z;
            }
            else if (ang < -1f)
            {
                WeaponTransform.Rotate(Vector3.forward, -Mathf.Min(RotationBaseSpeed * CO.co.GetWorldSpeedDelta(), -ang * RotationBaseSpeed * 0.1f));
                ang = AngleToTurnTarget();
                if (AngleToTurnTarget() > 0f)
                {
                    WeaponTransform.Rotate(Vector3.forward, ang);
                    isLooking = false;
                }
                Rotation.Value = WeaponTransform.rotation.eulerAngles.z;
            }
            else
            {
                isLooking = false;
            }
        }
    }

    private bool canFire = true;
    private void Fire(Vector3 mouse)
    {
        if (!canFire) return;
        if (LoadedAmmo.Value < 1) return;
        if (IsDisabled()) return;
        StartCoroutine(FireSequence(mouse));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponSFXRpc()
    {
        if (Fire_SFX.Length > 0)
        {
            AUDCO.aud.PlaySFX(Fire_SFX, transform.position, 0.1f);
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void IncendiarySFXRpc()
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.IncendiaryFireAttack, transform.position, 0.1f);
    }


    IEnumerator FireSequence(Vector3 mouse)
    {
        canFire = false;
        for (int i = 0; i < GetProjectileCount(); i++)
        {
            LoadedAmmo.Value--;
            PROJ proj = Instantiate(FireProjectile, FirePoint.position, WeaponTransform.rotation);
            proj.Init(GetDamage(), Faction, null, mouse);
            if (proj.ZoneImpact)
            {
                foreach (Module mod in Space.Drifter.ModulesWithAbilityActive(Module.ModuleTypes.INCENDIARY_STORAGE))
                {
                    ModuleIncendiaryCrates mod2 = (ModuleIncendiaryCrates)mod;
                    proj.AttackDamage *= mod2.GetDamageBonusMod();
                    proj.ZoneChance += mod2.GetFlameBoost();
                    IncendiarySFXRpc();
                }
            }
           
            proj.NetworkObject.Spawn();

            WeaponSFXRpc();

            CurCooldown.Value = AdditionalProjectileDelay;
            while (CurCooldown.Value > 0f)
            {
                CurCooldown.Value -= CO.co.GetWorldSpeedDelta();
                yield return null;
            }
        }
        CurCooldown.Value = GetFireCooldown();
        while (CurCooldown.Value > 0f)
        {
            CurCooldown.Value -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        canFire = true;
    }

    IEnumerator LoadWeapons()
    {
        CurCooldown.Value = GetFireCooldown();
        if (!canFire) yield break;
        canFire = false;
        while (CurCooldown.Value > 0f)
        {
            CurCooldown.Value -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        canFire = true;
    }

    IEnumerator AttackReloadAmmo()
    {
        ReloadingCurrently.Value = true;
        float wait = ReloadCooldown;
        while (wait > 0f)
        {
            wait -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        ReloadingCurrently.Value = false;
        LoadedAmmo.Value = MaxAmmo;
    }
    public float AngleToTurnTarget()
    {
        return AngleBetweenPoints(LookTowards);
    }
    public Vector3 getPos()
    {
        return new Vector3(transform.position.x, transform.position.y, 0);
    }
    protected float AngleTowards(Vector3 towards)
    {
        return Vector2.SignedAngle(getLookVector(), towards);
    }
    public float AngleBetweenPoints(Vector3 towards)
    {
        return AngleBetweenPoints(getPos(), towards);
    }
    protected float AngleBetweenPoints(Vector3 from, Vector3 towards)
    {
        return Vector2.SignedAngle(getLookVector(), towards - from);
    }
    public Vector3 getLookVector()
    {
        return getLookVector(WeaponTransform.rotation);
    }
    protected Vector3 getLookVector(Quaternion rotref)
    {
        float rot = Mathf.Deg2Rad * rotref.eulerAngles.z;
        float dxf = Mathf.Cos(rot);
        float dyf = Mathf.Sin(rot);
        return new Vector3(dxf, dyf, 0);
    }

    private Vector3 ShootTowards = Vector3.zero;
    private bool isUsing = false;
    public void Use(Vector3 vec)
    {
        isUsing = true;
        ShootTowards = vec;
    }
    public void Stop()
    {
        isUsing = false;
    }
}
