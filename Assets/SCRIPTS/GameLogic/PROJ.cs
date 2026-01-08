
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
    public float AccelProjectile;
    public float AccelMaximum;
    public float InaccuracyDegrees;
    public float MaximumRange = -1;
    public bool StickToWalls;

    [Header("ALTITUDE")]
    public bool UseAltitude = false;
    public bool AltitudeExplodeEarly = false;
    public bool CanHitWalls = false;
    public bool CanHitDrifters = true;
    public float AltitudeInacurracyFactor;
    public float AltitudeDirectHitCeiling;

    [Header("IMPACT")]
    public iDamageable.DamageType DamageType = iDamageable.DamageType.RANGED;
    public GameObject ImpactVFX;
    public GameObject LaunchVFX;
    public AudioClip[] ImpactSFX;
    public float BlockPenetration = 0f;
    [Tooltip("Lower is better")]
    public float DodgeModifier = 1f;
    public float HullDamageModifier = 1f; //Which factor of damage is done to modules
    public float ModuleDamageModifier = 1f; //Which factor of damage is done to modules
    public float ModuleSplash = 16f;
    public float ArmorDamageModifier = 1f; //Which factor of damage is done to armor
    public float ArmorAbsorptionModifier = 1f; //Which factor of damage is taken by armor
    public float ArmorCriticalChance = 0.05f; //Chance to ignore armor
    public float CrewDamageModifier = 1f; //Which facotr of damage is done to nearby crew members
    public float CrewDamageModifierDirect = 1f; //Which facotr of damage is done to nearby crew members
    public float CrewDamageSplash = 1f; //Which factor of damage is done to nearby crew members
    public bool DealSplash = false;
    public ScriptableBuff ApplyBuff;
    public DamagingZone ZoneImpact;
    public float ZoneChance = 0f;
    [NonSerialized] public CREW CrewOwner;

    private float Expire = 10;

    protected List<GameObject> Damageables = new();

    protected bool hasHitTarget = false;
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
        if (Space) transform.SetParent(Space.transform);
    }

    private void AttemptSpawnZone(Vector3 vec, Transform trans)
    {
        if (UnityEngine.Random.Range(0f, 1f) > ZoneChance) return;
        DamagingZone Zone = Instantiate(ZoneImpact, vec, Quaternion.identity);
        Zone.NetworkObject.Spawn();
        Zone.transform.SetParent(trans);
        Zone.Init(1f);
    }
    private void Start()
    {
        if (LaunchVFX) LaunchVFXAnimation();
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (!isActive) return;
        Expire -= CO.co.GetWorldSpeedDeltaFixed();
        if (Expire < 0)
        {
            Kill();
            return;
        }
        if (ProjectileSpeed < AccelMaximum) ProjectileSpeed += AccelProjectile * CO.co.GetWorldSpeedDeltaFixed();
        float step = ProjectileSpeed * CO.co.GetWorldSpeedDeltaFixed();
        transform.position += step * getLookVector();
        if (MaximumRange > 0)
        {
            MaximumRange -= step;
            if (MaximumRange < 0)
            {
                if (ImpactSFX.Length > 0) ImpactSFXRpc();
                BulletImpact();
            }
        }
        if (UseAltitude)
        {
            AltitudeRemaining -= step;
            if (CanHitWalls)
            {
                foreach (Collider2D collision in Physics2D.OverlapCircleAll(Tip.position, 0.5f))
                {
                    PotentialHitWall(collision.gameObject);
                }
            }
            if (AltitudeRemaining < 0f)
            {
                Collider2D[] cols = Physics2D.OverlapCircleAll(Tip.position, CrewDamageSplash);
                foreach (Collider2D collision in cols)
                {
                    PotentialHitTarget(collision.gameObject);
                }
                AltitudeRemaining = 999;
                if (AltitudeExplodeEarly || hasHitTarget)
                {
                    if (ImpactSFX.Length > 0) ImpactSFXRpc();
                    BulletImpact(); 
                }
            }
        } else
        {
            foreach (Collider2D collision in Physics2D.OverlapCircleAll(Tip.position, 0.5f))
            {
                PotentialHitTarget(collision.gameObject);
            }
        }
    }
    protected virtual void PotentialHitTarget(GameObject collision)
    {
        if (hasImpacted) return;
        if (!isActive) return;
        iDamageable crew = collision.GetComponent<iDamageable>();
        if (crew != null)
        {
            if (crew.GetFaction() == Faction) return;
            if (Damageables.Contains(collision)) return;
            if (!crew.CanBeTargeted(Space)) return;
            DRIFTER drifter = collision.GetComponent<DRIFTER>();
            if (drifter != null)
            {
                //if (!CanHitDrifters) return;
                if (UnityEngine.Random.Range(0f, 1f) > Mathf.Min(drifter.GetDodgeChance() * DodgeModifier,0.9f))
                {
                    if (Space != null) ModuleDamageModifier = 0; //You cannot deal module damage from space to drifters
                    drifter.Impact(this, Tip.position);
                    AttemptSpawnZone(Tip.position, drifter.Interior.transform);
                    isActive = false;
                    StickToWalls = false;
                } else
                {
                    Damageables.Add(collision);
                    CO_SPAWNER.co.SpawnWordsRpc("MISS", transform.position);
                    return;
                }
            } else
            {
                if (Space != crew.Space && crew.Space.Drifter != null) return; // Direct damage not possible if not in same space and the target is in a drifter
                if (crew is Module) crew.TakeDamage(AttackDamage * ModuleDamageModifier, transform.position, DamageType);
                else
                {
                    crew.TakeDamage(AttackDamage * CrewDamageModifierDirect, transform.position, DamageType);
                    if (crew is CREW)
                    {
                        if (ApplyBuff)
                        {
                            ((CREW)crew).AddBuff(ApplyBuff);
                        }
                        if (CrewOwner)
                        {
                            switch (DamageType)
                            {
                                case iDamageable.DamageType.RANGED:
                                    CrewOwner.ArtifactOnRangedTarget((CREW)crew);
                                    break;
                                case iDamageable.DamageType.SPELL:
                                    CrewOwner.ArtifactOnSpellTarget((CREW)crew);
                                    break;
                            }
                            if (((CREW)crew).isDead())
                            {
                                CrewOwner.ArtifactOnKill((CREW)crew);
                            }
                        }
                    }
                }
            }
            Damageables.Add(collision);
            if (DealSplash)
            {
                hasHitTarget = true;
                AttackDamage *= 0.7f;
            }
            else if (StickToWalls)
            {
                if (ImpactSFX.Length > 0) ImpactSFXRpc();
                isActive = false;
                transform.SetParent(collision.transform);
                if (ImpactVFX) ImpactVFXRpc();
                ExpireSlowlyRpc();
            }
            else
            {
                if (ImpactSFX.Length > 0) ImpactSFXRpc();
                isActive = false;
                BulletImpact();
            }
            return;
        } else if (UseAltitude)
        {
            DUNGEON dung = collision.GetComponent<DUNGEON>();
            if (dung != null)
            {
                if (Space != null) return;
                dung.Impact(this, Tip.position);
                if (ImpactSFX.Length > 0) ImpactSFXRpc();
                BulletImpact();
                return;
            }
        }
        if (Space != null)
        {
            if (collision.tag.Equals("LOSBlocker"))
            {
                CollisionRedirector redirector = collision.GetComponent<CollisionRedirector>();
                if (redirector != null)
                {
                    PotentialHitTarget(redirector.Redirect);
                    return;
                }
                if (ImpactSFX.Length > 0) ImpactSFXRpc();
                if (DealSplash)
                {
                    hasHitTarget = true;
                }
                else if (StickToWalls)
                {
                    isActive = false;
                    transform.SetParent(collision.transform.parent);
                    if (ImpactVFX) ImpactVFXRpc();
                    ExpireSlowlyRpc();
                }
                else
                {
                    BulletImpact();
                }
                return;
            }
            BlockAttacks Blocker = collision.GetComponent<BlockAttacks>();
            if (Blocker != null && BlockPenetration < 1)
            {
                if (Damageables.Contains(Blocker.gameObject)) return;
                if (Blocker.GetCrew().GetFaction() == Faction) return;
                Damageables.Add(Blocker.gameObject);
                bool isBlocked = UnityEngine.Random.Range(0f, 1f) + BlockPenetration < Blocker.BlockChanceRanged;
                if (isBlocked)
                {
                    if (Blocker.BlockSound != AUDCO.BlockSoundEffects.NONE) AUDCO.aud.PlayBlockSFXRpc(Blocker.BlockSound, transform.position);
                    else if (ImpactSFX.Length > 0) ImpactSFXRpc();
                    if (Blocker.ReduceDamageModRanged < 1f)
                    {
                        AttackDamage *= (1f - Blocker.ReduceDamageModRanged);
                        PotentialHitTarget(Blocker.GetCrew().gameObject);
                    }
                    else
                    {
                        CO_SPAWNER.co.SpawnDMGRpc(0, transform.position);
                    }
                    BulletImpact();
                }
            }
        }
        
    }
    protected virtual void PotentialHitWall(GameObject collision)
    {
        if (collision.tag.Equals("LOSBlocker"))
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(Tip.position, CrewDamageSplash);
            foreach (Collider2D col2 in cols)
            {
                PotentialHitTarget(col2.gameObject);
            }
            if (ImpactSFX.Length > 0) ImpactSFXRpc();
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

    [Rpc(SendTo.ClientsAndHost)]
    protected void ExpireSlowlyRpc()
    {
        StartCoroutine(ExpireSlowlyNum());
    }

    [Rpc(SendTo.ClientsAndHost)]

    public void ImpactVFXRpc()
    {
        Instantiate(ImpactVFX,Tip.position, Quaternion.identity).transform.SetParent(CO.co.GetTransformAtPoint(Tip.position));
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void ImpactSFXRpc()
    {
        AUDCO.aud.PlaySFX(ImpactSFX, Tip.position, 0.1f);
    }
    public void LaunchVFXAnimation()
    {
        Instantiate(LaunchVFX, transform.position, Quaternion.identity).transform.SetParent(CO.co.GetTransformAtPoint(transform.position));
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
        if (ImpactVFX) ImpactVFXRpc();
        Kill();
    }
    protected void Kill()
    {
        if (!IsSpawned) Destroy(gameObject);
        else NetworkObject.Despawn();
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
