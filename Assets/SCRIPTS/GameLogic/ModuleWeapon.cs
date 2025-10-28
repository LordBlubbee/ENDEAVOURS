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

    [Header("Offensive Stats")]
    public float Damage;
    public float HullDamageMod = 1f;
    public float ModuleDamageMod = 1f;
    public float CrewDamageMod = 0.5f;
    public float CrewDamageSplash = 1f;
    public float ArmorDamageMod = 1f;
    public float ArmorDamageAbsorption = 1f;
    public float RotationBaseSpeed = 30;
    public int MaxAmmo = 50;
    public float FireCooldown = 2f;
    public float ReloadCooldown = 5f;
    public int ProjectileCount = 1;
    public float AdditionalProjectileDelay = 0f;
    public ResourceCrate.ResourceTypes ResourceType = ResourceCrate.ResourceTypes.AMMUNITION;

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
    IEnumerator FireSequence(Vector3 mouse)
    {
        canFire = false;
        for (int i = 0; i < ProjectileCount; i++)
        {
            LoadedAmmo.Value--;
            PROJ proj = Instantiate(FireProjectile, FirePoint.position, WeaponTransform.rotation);
            proj.Init(Damage, Faction, null, mouse);
            proj.InitAdvanced(HullDamageMod, ModuleDamageMod, CrewDamageMod, CrewDamageSplash, ArmorDamageMod, ArmorDamageAbsorption);
            proj.NetworkObject.Spawn();

            WeaponSFXRpc();

            CurCooldown.Value = AdditionalProjectileDelay;
            while (CurCooldown.Value > 0f)
            {
                CurCooldown.Value -= CO.co.GetWorldSpeedDelta();
                yield return null;
            }
        }
        CurCooldown.Value = FireCooldown;
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
