using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PROJ : NetworkBehaviour
{
    public float ProjectileSpeed;
    public float InaccuracyDegrees;
    public float ProjectileDistance;
    public bool StickToWalls;

    [Header("ALTITUDE")]
    public float AltitudeInacurracyFactor;

    [NonSerialized] public CREW CrewOwner;

    protected bool isActive = true;
    protected bool UseAltitude;
    protected SPACE Space;
    protected float AttackDamage;
    protected int Faction;
    protected float AltitudeRemaining = 0f;
    public void Init(float damage, int fac, SPACE space)
    {
        AttackDamage = damage;
        Faction = fac;
        Space = space;
        transform.Rotate(Vector3.forward, UnityEngine.Random.Range(-InaccuracyDegrees, InaccuracyDegrees));
    }
    public void Init(float damage, int fac, SPACE space, Vector3 trt)
    {
        UseAltitude = true;
        AltitudeRemaining = (trt - transform.position).magnitude * UnityEngine.Random.Range(1f- AltitudeInacurracyFactor,1f+ AltitudeInacurracyFactor);
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
            if (!crew.CanBeTargeted()) return;
            crew.TakeDamage(AttackDamage, transform.position);
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
