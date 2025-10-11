
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PROJ : NetworkBehaviour
{
    public SpriteRenderer Spr;
    public Transform Tip;
    public float ProjectileSpeed;
    public float InaccuracyDegrees;
    public float MaximumRange = -1;
    public bool StickToWalls;

    [Header("ALTITUDE")]
    public bool UseAltitude = false;
    public float AltitudeInacurracyFactor;
    public float AltitudeDirectHitCeiling;

    [Header("IMPACT")]
    [NonSerialized] public float HullDamageModifier = 1f; //Which factor of damage is done to modules
    [NonSerialized] public float ModuleDamageModifier = 1f; //Which factor of damage is done to modules
    [NonSerialized] public float ArmorDamageModifier = 1f; //Which factor of damage is done to armor
    [NonSerialized] public float ArmorAbsorptionModifier = 1f; //Which factor of damage is taken by armor
    [NonSerialized] public float CrewDamageModifier = 0.5f; //Which facotr of damage is done to nearby crew members
    [NonSerialized] public float CrewDamageSplash = 0f; //Which facotr of damage is done to nearby crew members

    [NonSerialized] public CREW CrewOwner;

    protected List<GameObject> Damageables = new();

    protected bool isActive = true;
    protected SPACE Space;
    [NonSerialized] public float AttackDamage;
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
    public void InitAdvanced(float hullMod, float moduleMod,  float crewMod, float crewSplash, float armorMod, float armorAbsorb)
    {
        HullDamageModifier = hullMod;
        ModuleDamageModifier = moduleMod;
        ArmorDamageModifier = armorMod;
        CrewDamageModifier = crewMod;
        CrewDamageSplash = crewSplash;
        ArmorAbsorptionModifier = armorAbsorb;
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
                if (CrewDamageSplash > 0f)
                {
                    foreach (Collider2D collision in Physics2D.OverlapCircleAll(Tip.position, CrewDamageSplash))
                    {
                        PotentialHitTarget(collision.gameObject);
                    }
                }
                BulletImpact();
            }
        } else
        {
            foreach (Collider2D collision in Physics2D.OverlapCircleAll(Tip.position, 0.3f))
            {
                PotentialHitTarget(collision.gameObject);
            }
        }
    }
    protected virtual void PotentialHitTarget(GameObject collision)
    {
        iDamageable crew = collision.GetComponent<iDamageable>();
        if (crew != null)
        {
            if (crew.GetFaction() == Faction) return;
            if (crew.Space != Space) return;
            if (Damageables.Contains(collision)) return;
            if (!crew.CanBeTargeted()) return;
            DRIFTER drifter = collision.GetComponent<DRIFTER>();
            if (drifter != null)
            {
                drifter.Impact(this, transform.position);
            } else
            {
                crew.TakeDamage(AttackDamage, transform.position);
            }
            Damageables.Add(collision);
            if (StickToWalls)
            {
                isActive = false;
                transform.SetParent(collision.transform);
                ExpireSlowlyRpc();
            }
            else
            {
                BulletImpact();
            }
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
        BlockAttacks Blocker = GetComponent<BlockAttacks>();
        if (Blocker != null)
        {
            Debug.Log("Hitting blocker!");
            if (Damageables.Contains(Blocker.gameObject)) return;
            Damageables.Add(Blocker.gameObject);
            bool isBlocked = UnityEngine.Random.Range(0f, 1f) < Blocker.BlockChance;
            if (isBlocked)
            {
                Debug.Log("Blocked");
                if (Blocker.ReduceDamageMod < 1f)
                {
                    Debug.Log("Still deal damage");
                    AttackDamage *= (1f - Blocker.ReduceDamageMod);
                    PotentialHitTarget(Blocker.tool.GetCrew().gameObject);
                }
                BulletImpact();
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
        yield return new WaitForSeconds(10f);
        while (Spr.color.a > 0)
        {
            Spr.color = new Color(1, 1, 1, Spr.color.a - CO.co.GetWorldSpeedDelta());
            yield return null;
        }
        if (IsServer) Kill();
    }

    private bool hasImpacted = false;
    protected void BulletImpact()
    {
        if (hasImpacted) return;
        hasImpacted = true;
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
