using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModuleWeapon : Module
{
    [Header("References")]
    public Transform Platform;
    public Transform FirePoint;
    public PROJ FireProjectile;
    public Sprite CrosshairSprite;

    [Header("Offensive Stats")]
    public float Damage;
    public float RotationBaseSpeed = 30;
    public int MaxAmmo = 80;
    public float FireCooldown = 2f;
    public float ReloadCooldown = 5f;
    public int ProjectileCount = 1;
    public float AdditionalProjectileDelay = 0f;
    public ResourceCrate.ResourceTypes ResourceType = ResourceCrate.ResourceTypes.AMMUNITION;

    [NonSerialized] public NetworkVariable<float> CurCooldown = new();
    [NonSerialized] public NetworkVariable<int> LoadedAmmo = new();
    
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

        CurHealth.Value = MaxHealth;

        Platform.transform.SetParent(transform.parent);
    }

    public void ReloadAmmo()
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position,8f))
        {
            ResourceCrate crate = col.GetComponent<ResourceCrate>();
            if (crate != null)
            {
                if (crate.ResourceType == ResourceType)
                {
                    StartCoroutine(AttackReloadAmmo());
                    LoadedAmmo.Value = Mathf.FloorToInt(MaxAmmo * crate.ResourceAmount.Value / 10f);
                    crate.RemoveCrate();
                    return;
                }
            }
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (isUsing) Fire(ShootTowards);
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (isLooking)
        {
            float ang = AngleToTurnTarget();
            if (ang > 1f)
            {
                transform.Rotate(Vector3.forward, Mathf.Min(RotationBaseSpeed * CO.co.GetWorldSpeedDeltaFixed(),ang* RotationBaseSpeed*0.1f));
                ang = AngleToTurnTarget();
                if (AngleToTurnTarget() < 0f)
                {
                    transform.Rotate(Vector3.forward, ang);
                    isLooking = false;
                }
            }
            else if (ang < -1f)
            {
                transform.Rotate(Vector3.forward, -Mathf.Min(RotationBaseSpeed * CO.co.GetWorldSpeedDeltaFixed(), -ang * RotationBaseSpeed * 0.1f));
                ang = AngleToTurnTarget();
                if (AngleToTurnTarget() > 0f)
                {
                    transform.Rotate(Vector3.forward, ang);
                    isLooking = false;
                }
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
        if (LoadedAmmo.Value < 1)
        {
            ReloadAmmo();
            return;
        }
        if (!canFire) return;
        StartCoroutine(FireSequence(mouse));
    }
    IEnumerator FireSequence(Vector3 mouse)
    {
        canFire = false;
        for (int i = 0; i < ProjectileCount; i++)
        {
            PROJ proj = Instantiate(FireProjectile, FirePoint.position, transform.rotation);
            proj.Init(Damage, Faction, null, mouse);
            proj.NetworkObject.Spawn();
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
        canFire = false;
        CurCooldown.Value = ReloadCooldown;
        while (CurCooldown.Value > 0f)
        {
            CurCooldown.Value -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        canFire = true;
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
        return getLookVector(transform.rotation);
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
    float UserGunneryMod = 1f;

    [Rpc(SendTo.Server)]
    public void UseRpc(Vector3 vec, float GUNNERY)
    {
        isUsing = true;
        ShootTowards = vec;
        UserGunneryMod = GUNNERY;
    }

    [Rpc(SendTo.Server)]
    public void StopRpc()
    {
        isUsing = false;
    }
}
