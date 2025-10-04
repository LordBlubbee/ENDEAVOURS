
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PROJ : NetworkBehaviour
{
    public float ProjectileSpeed;
    public float InaccuracyDegrees;
    public float MaximumRange = -1;
    public bool StickToWalls;

    [Header("ALTITUDE")]
    public bool UseAltitude = false;
    public float AltitudeInacurracyFactor;

    [Header("IMPACT")]
    public float ImpactSplashRange;
    public float ModuleDamageModifier;
    public float CrewDamageModifier;

    [NonSerialized] public CREW CrewOwner;

    protected List<iDamageable> Damageables = new();

    protected bool isActive = true;
    protected SPACE Space;
    protected float AttackDamage;
    protected int Faction;
    protected float AltitudeRemaining = 0f;
    public void Init(float damage, int fac, SPACE space, Vector3 trt)
    {
        if (UseAltitude)
        {
            AltitudeRemaining = (trt - transform.position).magnitude * UnityEngine.Random.Range(1f - AltitudeInacurracyFactor, 1f + AltitudeInacurracyFactor);
        }
        AttackDamage = damage;
        Faction = fac;
        Space = space;
        transform.Rotate(Vector3.forward, UnityEngine.Random.Range(-InaccuracyDegrees, InaccuracyDegrees));
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (!isActive) return;
        float step = ProjectileSpeed * CO.co.GetWorldSpeedDeltaFixed();
        transform.position += step * getLookVector();
        if (MaximumRange > 0)
        {
            MaximumRange -= step;
            if (MaximumRange < 0)
            {
                BulletImpact();
            }
        }
        if (UseAltitude)
        {
            AltitudeRemaining -= step;
            if (AltitudeRemaining < 0f)
            {
                foreach (Collider2D collision in Physics2D.OverlapCircleAll(transform.position, 0.3f))
                {
                    PotentialHitTarget(collision);
                }
                BulletImpact();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!isActive) return;
        if (UseAltitude) return;
        PotentialHitTarget(collision);
    }
    protected virtual void PotentialHitTarget(Collider2D collision)
    {
        iDamageable crew = collision.GetComponent<iDamageable>();
        if (crew != null)
        {
            if (crew.GetFaction() == Faction) return;
            if (crew.Space != Space) return;
            if (Damageables.Contains(crew)) return;
            if (!crew.CanBeTargeted()) return;
            crew.TakeDamage(AttackDamage, transform.position);
            Damageables.Add(crew);
            BulletImpact();
            return;
        }
        if (Space != null)
        {
            if (collision.tag.Equals("LOSBlocker"))
            {
                if (StickToWalls)
                {
                    isActive = false;
                    transform.SetParent(collision.transform.parent);
                    ExpireSlowlyRpc();
                }
                else
                {
                    BulletImpact();
                }
                return;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected void ExpireSlowlyRpc()
    {
        StartCoroutine(ExpireSlowlyNum());
    }
    IEnumerator ExpireSlowlyNum()
    {
        yield return new WaitForSeconds(30f);
        if (IsServer) Kill();
    }
    protected void BulletImpact()
    {
        Kill();
    }

    protected void Kill()
    {
        NetworkObject.Despawn();
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
}
