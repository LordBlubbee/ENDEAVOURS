using Unity.Netcode;
using UnityEngine;

public class PROJ : NetworkBehaviour
{
    public float ProjectileSpeed;
    public float InaccuracyDegrees;
    public float ProjectileDistance;

    [Header("ALTITUDE")]
    public bool UseAltitude;
    public float AltitudeInacurracyFactor;

    SPACE Space;
    float AttackDamage;
    int Faction;
    public void Init(float damage, int fac, SPACE space)
    {
        AttackDamage = damage;
        Faction = fac;
        Space = space;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        CREW crew = collision.GetComponent<CREW>();
        if (crew != null)
        {
            if (crew.Faction == Faction) return;
            crew.TakeDamage(AttackDamage);
            BulletImpact();
            return;
        }
        if (Space == null)
        {
            DRIFTER drifter = collision.GetComponent<DRIFTER>();
            if (drifter != null)
            {
                if (drifter.Faction == Faction) return;
                drifter.TakeDamage(AttackDamage,transform.position);
                BulletImpact();
                return;
            }
        }
    }

    private void BulletImpact()
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
