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

    private bool isActive = true;
    private bool UseAltitude;
    SPACE Space;
    float AttackDamage;
    int Faction;
    float AltitudeRemaining = 0f;
    public void Init(float damage, int fac, SPACE space)
    {
        AttackDamage = damage;
        Faction = fac;
        Space = space;
    }
    public void Init(float damage, int fac, SPACE space, Vector3 trt)
    {
        UseAltitude = true;
        AltitudeRemaining = (trt - transform.position).magnitude * Random.Range(1f- AltitudeInacurracyFactor,1f+ AltitudeInacurracyFactor);
        AttackDamage = damage;
        Faction = fac;
        Space = space;
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

    private void PotentialHitTarget(Collider2D collision)
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
        /*CREW crew = collision.GetComponent<CREW>();
        if (crew != null)
        {
            if (crew.Faction == Faction) return;
            if (crew.Space != Space) return;
            crew.TakeDamage(AttackDamage, transform.position);
            BulletImpact();
            return;
        }
        if (Space == null)
        {
            DRIFTER drifter = collision.GetComponent<DRIFTER>();
            if (drifter != null)
            {
                if (drifter.Faction == Faction) return;
                if (drifter.Interior != Space) return;
                drifter.TakeDamage(AttackDamage, transform.position);
                BulletImpact();
                return;
            }
        }*/
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ExpireSlowlyRpc()
    {
        StartCoroutine(ExpireSlowlyNum());
    }
    IEnumerator ExpireSlowlyNum()
    {
        yield return new WaitForSeconds(30f);
        if (IsServer) Kill();
    }
    private void BulletImpact()
    {
        Kill();
    }

    private void Kill()
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
