using System;
using System.Collections.Generic;
using TreeEditor;
using Unity.Netcode;
using UnityEngine;

public class DRIFTER : NetworkBehaviour, iDamageable
{
    [Header("REFERENCES")]
    public SPACE Interior;
    public AI_GROUP CrewGroup;
    public DrifterCollider DrifterCollider;
    public List<ScriptableEquippableModule> StartingModules;
    public List<CREW> StartingCrew;

    [NonSerialized] public Vector3 MoveTowardsPoint;
    [NonSerialized] public Vector3 CurrentLocationPoint;
    [NonSerialized] public Vector3 CurrentTurbulence;

    [NonSerialized] public Module EngineModule;
    [NonSerialized] public Module NavModule;

    public SpriteRenderer Spr;
    public int Faction;

    [Header("STATS")]
    public float MaxHealth = 2500f;
    public float MovementSpeed = 5;
    public float AccelerationSpeedMod = 0.25f;
    public float RotationBaseSpeed = 30f;

    private NetworkVariable<Vector3> CurrentMovement = new();
    private NetworkVariable<float> CurrentRotation = new();
    [NonSerialized] public NetworkVariable<bool> IsMainDrifter = new();
    private NetworkVariable<float> CurHealth = new();

    private void Start()
    {
        Init();
    }

    private bool hasInitialized = false;
    private bool canReceiveInput = true;

    public void SetCanReceiveInput(bool can)
    {
        canReceiveInput = can;
    }

    public void Init()
    {
        if (hasInitialized) return;

        hasInitialized = true;

        Interior.Init();

        CO.co.RegisterDrifter(this);

        if (IsServer)
        {
            CurHealth.Value = MaxHealth;
            DrifterCollider.gameObject.SetActive(true);

            CrewGroup.SetAIHome(this);
            CrewGroup.SetAI(AI_GROUP.AI_TYPES.SHIP_DEFENSIVE, AI_GROUP.AI_OBJECTIVES.SHIP, GetFaction(), new());
        }
    }

    float PilotingEfficiency = 1f;
    private Vector3 MoveInput;
    private Vector3 LookTowards = new Vector3(1,0);

    public SPACE Space
    {
        get { return Interior; }
        set
        {

        }
    }

    [Rpc(SendTo.Server)]
    public void SetMoveInputRpc(Vector3 mov, float eff)
    {
        SetMoveInput(mov, eff);
    }
    [Rpc(SendTo.Server)]
    public void SetLookTowardsRpc(Vector2 mov)
    {
        SetLookTowards(mov);
    }
    public void SetMoveInput(Vector3 mov, float eff)
    {
        if (!canReceiveInput) return;
        MoveInput = mov;
        PilotingEfficiency = eff;
    }
    public void SetLookTowards(Vector3 mov)
    {
        if (!canReceiveInput) return;
        LookTowards = (mov-transform.position).normalized;
    }

    public float GetRotation()
    {
        return RotationBaseSpeed * PilotingEfficiency;
    }

    public bool EnginesDown()
    {
        return EngineModule && EngineModule.IsDisabled();
    }
    public float GetMovementSpeed()
    {
        return MovementSpeed;
    }
    public float GetRelativeSpeed()
    {
        return GetCurrentMovement() / GetMovementSpeed();
    }
    public float GetRelativeRotation()
    {
        return Mathf.Abs(CurrentRotation.Value) / RotationBaseSpeed;
    }
    public float GetRotorSpeed()
    {
        return Mathf.Clamp01(GetRelativeSpeed() * 0.6f + GetRelativeRotation() * 0.6f);
    }
    public float GetCurrentMovement()
    {
        return CurrentMovement.Value.magnitude;
    }

    public float GetMovementAccel()
    {
        return AccelerationSpeedMod * PilotingEfficiency * 1.1f;
    }
    void UpdateTurn()
    {
        float ang = AngleToTurnTarget();
        if (EnginesDown())
        {
            ang = 0f;
        }
        float rotGoal = 0f;
        float accelSpeed = GetRotation() * 0.5f;
        if (ang > 1f)
        {
            rotGoal = GetRotation();
        }
        else if (ang < -1f)
        {
            rotGoal = -GetRotation();
        }
        if (rotGoal > CurrentRotation.Value)
        {
            CurrentRotation.Value += accelSpeed * CO.co.GetWorldSpeedDeltaFixed();
        }
        else if (rotGoal < CurrentRotation.Value)
        {
            CurrentRotation.Value -= accelSpeed * CO.co.GetWorldSpeedDeltaFixed();
        }
        float maxRot = Mathf.Min(GetRotation(), Mathf.Abs(ang)*2f);
        CurrentRotation.Value = Mathf.Clamp(CurrentRotation.Value, -maxRot, maxRot);
        transform.Rotate(Vector3.forward,CurrentRotation.Value * CO.co.GetWorldSpeedDeltaFixed());
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        
        UpdateTurn();

        if (MoveInput == Vector3.zero || EnginesDown())
        {
            bool XPOS = CurrentMovement.Value.x > 0;
            bool YPOS = CurrentMovement.Value.y > 0;
            CurrentMovement.Value -= CurrentMovement.Value.normalized * GetMovementAccel() * MovementSpeed * CO.co.GetWorldSpeedDeltaFixed();
            if (XPOS != CurrentMovement.Value.x > 0 || YPOS != CurrentMovement.Value.y > 0 || CurrentMovement.Value.magnitude < 0.1f) CurrentMovement.Value = Vector3.zero;
        } else
        {
            CurrentMovement.Value += MoveInput * GetMovementAccel() * MovementSpeed * CO.co.GetWorldSpeedDeltaFixed();
            if (CurrentMovement.Value.magnitude > MovementSpeed) CurrentMovement.Value = CurrentMovement.Value.normalized * MovementSpeed;
        }

        float towardsang = Mathf.Abs(AngleTowards(CurrentMovement.Value));
        float towardsfactor = 1.2f - Mathf.Clamp((towardsang - 60f) * 0.006f, 0, 0.4f); //The more you look in the correct direction, the faster you move!
        transform.position += CurrentMovement.Value * towardsfactor * CO.co.GetWorldSpeedDeltaFixed();
        //Rigid.MovePosition(transform.position);
    }


    [Rpc(SendTo.Server)]
    public void CreateAmmoCrateRpc(Vector3 vec)
    {
        if (CO.co.Resource_Ammo.Value < 10) return;
        CO.co.Resource_Ammo.Value -= 10;
        ResourceCrate ob = Instantiate(CO_SPAWNER.co.PrefabAmmoCrate, vec, Quaternion.identity);
        ob.NetworkObject.Spawn();
        ob.transform.SetParent(transform);
        ob.ResourceAmount.Value = 10;
    }
    public float AngleToTurnTarget()
    {
        return AngleTowards(LookTowards);
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
    public void Heal(float fl)
    {
        CurHealth.Value = Mathf.Min(MaxHealth, CurHealth.Value + fl);
    }
    public void TakeDamage(float fl, Vector3 ImpactArea)
    {
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            CurHealth.Value = 0f;
            //Death
        }
        if (fl > 1) CO_SPAWNER.co.SpawnDMGRpc(fl, ImpactArea);
    }
    public void Impact(PROJ fl, Vector3 ImpactArea)
    {
        float Damage = fl.AttackDamage;
        float AbsorbableDamage = fl.AttackDamage * fl.ArmorAbsorptionModifier;
        Damage -= AbsorbableDamage;
        foreach (Module mod in Interior.GetModules())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (mod is ModuleArmor && !mod.IsDisabled())
            {
                ModuleArmor arm = (ModuleArmor)mod;
                if (Dist < arm.ArmorAuraSize)
                {
                    AbsorbableDamage *= fl.ArmorDamageModifier; //Say, we deal 80 damage with +50% modifier = 120
                    float DamageNeeded = Mathf.Min(arm.CurArmor.Value,AbsorbableDamage); //Say, we need only 80 damage
                    arm.TakeArmorDamage(AbsorbableDamage, ImpactArea);
                    AbsorbableDamage -= DamageNeeded; //We have 40 damage left
                    AbsorbableDamage /= fl.ArmorDamageModifier; //27 damage is returned to main damage mod
                    break;
                }
            }
        }
        Damage += AbsorbableDamage;
        TakeDamage(Damage * fl.HullDamageModifier, ImpactArea);
        foreach (Module mod in Interior.GetModules())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (Dist < mod.HitboxRadius)
            {
                mod.TakeDamage(Damage * fl.ModuleDamageModifier, ImpactArea);
            }
        }
        foreach (CREW mod in Interior.GetCrew())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (Dist < 4f)
            {
                mod.TakeDamage(Damage * fl.CrewDamageSplash, ImpactArea);
            }
        }
    }
    public void Impact(float Damage, Vector3 ImpactArea)
    {
        float AbsorbableDamage = Damage;
        Damage -= AbsorbableDamage;
        foreach (Module mod in Interior.GetModules())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (mod is ModuleArmor && !mod.IsDisabled())
            {
                ModuleArmor arm = (ModuleArmor)mod;
                if (Dist < arm.ArmorAuraSize)
                {
                    AbsorbableDamage *= 0.5f; //Say, we deal 80 damage with +50% modifier = 120
                    float DamageNeeded = Mathf.Min(arm.CurArmor.Value, AbsorbableDamage); //Say, we need only 80 damage
                    arm.TakeArmorDamage(AbsorbableDamage, ImpactArea);
                    AbsorbableDamage -= DamageNeeded; //We have 40 damage left
                    AbsorbableDamage /= 0.5f; //27 damage is returned to main damage mod
                    break;
                }
            }
        }
        Damage += AbsorbableDamage;
        TakeDamage(Damage * 1f, ImpactArea);
    }
    public int GetFaction()
    {
        return Faction;
    }
    public bool CanBeTargeted(SPACE space)
    {
        if (space == Space) return false;
        return true;
    }

    public float GetHealth()
    {
        return CurHealth.Value;
    }

    public float GetMaxHealth()
    {
        return MaxHealth;
    }

    public float GetHealthRelative()
    {
        return GetHealth() / GetMaxHealth();
    }
}
public class DrifterModule
{
    public ScriptableEquippableModule EquippableModule;
    public Module Module;
}