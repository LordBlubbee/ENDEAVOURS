using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static iDamageable;

public class DRIFTER : NetworkBehaviour, iDamageable
{
    [Header("REFERENCES")]
    public SPACE Interior;
    public AI_GROUP CrewGroup;
    public DrifterCollider DrifterCollider;
    public List<ScriptableEquippableModule> StartingModules;
    public List<CREW> StartingCrew;

    public void SetMoveTowards(Vector3 point)
    {
        CurrentLocationPoint = point;
    }
    public float GetDodgeChanceUnboosted()
    {
        if (EngineModule)
        {
            if (EnginesDown()) return 0f;
            float DodgeChance = (0.1f + EngineModule.ModuleLevel.Value * 0.05f) * ((EngineModule.GetHealthRelative() * 0.8f) + 0.2f);
            return Mathf.Min(DodgeChance, 0.9f);
        }
        return 0.1f;
    }
    public float GetDodgeChance()
    {
        if (EngineModule)
        {
            if (EnginesDown()) return 0f;
            float DodgeChance = (0.1f + EngineModule.ModuleLevel.Value * 0.05f) * ((EngineModule.GetHealthRelative() * 0.8f) + 0.2f);
            foreach (Module mod in ModulesWithAbilityActive(Module.ModuleTypes.STEAM_REACTOR))
            {
                ModuleSteamReactor mod2 = (ModuleSteamReactor)mod;
                DodgeChance += mod2.GetDodgeBoost();
            }
            return Mathf.Min(DodgeChance,0.9f);
        }
        return 0.1f;
    }
    [NonSerialized] public Vector3 CurrentLocationPoint;
    [NonSerialized] public Vector3 CurrentTurbulence;
    [NonSerialized] public float CurrentPositionTimer;

    [NonSerialized] public Module EngineModule;
    [NonSerialized] public Module NavModule;
    [NonSerialized] public Module MedicalModule;

    public SpriteRenderer Spr;
    [NonSerialized] public NetworkVariable<int> Faction = new();

    [Header("STATS")]
    public bool IsLoon;
    public float MaxHealth = 2500f;
    public float RadiusX = 60f;
    public float RadiusY = 45f;
    public float MovementSpeed = 5;
    public float AccelerationSpeedMod = 0.25f;
    public float RotationBaseSpeed = 30f;
    public int MaximumCrew = 9;

    [Header("DEATH")]
    public float DeathExplosionsFrequency = 0.2f;
    public float DeathTime = 5f;

    private NetworkVariable<bool> Alive = new(true);

    public bool isDead()
    {
        return !Alive.Value;
    }

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
    public void SetMoveInputRpc(Vector3 mov)
    {
        SetMoveInput(mov);
    }
    [Rpc(SendTo.Server)]
    public void SetLookTowardsRpc(Vector2 mov)
    {
        SetLookTowards(mov);
    }
    public void SetMoveInput(Vector3 mov)
    {
        if (!canReceiveInput) return;
        MoveInput = mov;
    }
    public void SetLookTowards(Vector3 mov)
    {
        if (!canReceiveInput) return;
        LookTowards = (mov-transform.position).normalized;
    }
    public void SetLookDirection(Vector3 mov)
    {
        if (!canReceiveInput) return;
        LookTowards = mov;
    }

    public float GetRotation()
    {
        return RotationBaseSpeed * PilotingEfficiency;
    }

    public bool EnginesDown()
    {
        if (isDead()) return true;
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
        float delta = CO.co.GetWorldSpeedDelta();
        float ang = AngleToTurnTarget() + RotationTurbulence;
        if (EnginesDown() || !CO.co.AreDriftersMoving())
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
            CurrentRotation.Value += accelSpeed * delta;
        }
        else if (rotGoal < CurrentRotation.Value)
        {
            CurrentRotation.Value -= accelSpeed * delta;
        }
        float maxRot = Mathf.Min(GetRotation(), Mathf.Abs(ang)*2f);
        CurrentRotation.Value = Mathf.Clamp(CurrentRotation.Value, -maxRot, maxRot);
        transform.Rotate(Vector3.forward,CurrentRotation.Value * delta);
    }

    private float RotationTurbulence = 0f;
    private void Update()
    {
        if (!IsServer) return;
        
        float delta = CO.co.GetWorldSpeedDelta();

        UpdateTurn();

        if (MoveInput == Vector3.zero || EnginesDown() || !CO.co.AreDriftersMoving()) // 
        {
            bool XPOS = CurrentMovement.Value.x > 0;
            bool YPOS = CurrentMovement.Value.y > 0;
            CurrentMovement.Value -= CurrentMovement.Value.normalized * GetMovementAccel() * GetMovementSpeed() * delta;
            if (XPOS != CurrentMovement.Value.x > 0 || YPOS != CurrentMovement.Value.y > 0 || CurrentMovement.Value.magnitude < 0.1f) CurrentMovement.Value = Vector3.zero;
        } else
        {
            CurrentMovement.Value += MoveInput * GetMovementAccel() * GetMovementSpeed() * delta;
            if (CurrentMovement.Value.magnitude > GetMovementSpeed()) CurrentMovement.Value = CurrentMovement.Value.normalized * MovementSpeed;
        }
        if (IsLoon)
        {
            //Normal 2D space movement
            float towardsang = Mathf.Abs(AngleTowards(CurrentMovement.Value));
            float towardsfactor = 1.2f - Mathf.Clamp((towardsang - 60f) * 0.006f, 0, 0.4f); //The more you look in the correct direction, the faster you move!
            transform.position += CurrentMovement.Value * towardsfactor * delta;
        } else
        {
            Vector3 Target = CurrentLocationPoint + CurrentTurbulence;
            Vector3 Dir = (Target - transform.position).normalized;
            float Dis = (Target - transform.position).magnitude;
            transform.position += Dir * GetMovementSpeed() * Mathf.Min(0.05f * Dis,1f) * delta * 2f;
            if (Dis < 0.3f && GetCurrentMovement() > 0.5f)
            {
                float Turb = GetCurrentMovement();
                CurrentTurbulence = new Vector3(UnityEngine.Random.Range(-Turb, Turb), UnityEngine.Random.Range(-Turb, Turb));
                RotationTurbulence = UnityEngine.Random.Range(-Turb, Turb);
            }
        }
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
    public Vector3 getRightVector()
    {
        return getRightVector(transform.rotation);
    }

    public Vector3 getLeftVector()
    {
        return getLeftVector(transform.rotation);
    }

    protected Vector3 getLookVector(Quaternion rotref)
    {
        float rot = Mathf.Deg2Rad * rotref.eulerAngles.z;
        float dxf = Mathf.Cos(rot);
        float dyf = Mathf.Sin(rot);
        return new Vector3(dxf, dyf, 0);
    }
    protected Vector3 getRightVector(Quaternion rotref)
    {
        float rot = Mathf.Deg2Rad * (rotref.eulerAngles.z - 90f); // rotate -90° for right
        float dxf = Mathf.Cos(rot);
        float dyf = Mathf.Sin(rot);
        return new Vector3(dxf, dyf, 0);
    }

    protected Vector3 getLeftVector(Quaternion rotref)
    {
        float rot = Mathf.Deg2Rad * (rotref.eulerAngles.z + 90f); // rotate +90° for left
        float dxf = Mathf.Cos(rot);
        float dyf = Mathf.Sin(rot);
        return new Vector3(dxf, dyf, 0);
    }
    public void Heal(float fl)
    {
        CurHealth.Value = Mathf.Min(MaxHealth, CurHealth.Value + fl);
    }

    public void SetHealth(float health)
    {
        CurHealth.Value = health;
    }
    public void TakeDamage(float fl, Vector3 ImpactArea, DamageType type)
    {
        if (isDead()) return;
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            CurHealth.Value = 0f;
            //Death
            Die();
        }
        if (fl > 10)
        {
            foreach (Module mod in ModulesWithAbilityActive(Module.ModuleTypes.VENGEANCE_PLATING))
            {
                ModuleVengeancePlating mod2 = (ModuleVengeancePlating)mod;
                mod2.FireShrapnel(ImpactArea);
            }
        }
        if (fl > 1) CO_SPAWNER.co.SpawnDMGRpc(fl, ImpactArea);
    }

    public void Die()
    {
        if (isDead()) return;
        Alive.Value = false;
        if (GetFaction() != 1)
        {
            LOCALCO.local.CinematicTexRpc("ENEMY DRIFTER DESTROYED");
        }
        StartCoroutine(DrifterDeathAnimation());
    }
    public void Disable()
    {
        if (isDead()) return;
        Alive.Value = false;
        foreach (Module mod in Interior.GetModules())
        {
            mod.Die(true);
        }
    }

    IEnumerator DrifterDeathAnimation()
    {
        float Death = DeathTime * UnityEngine.Random.Range(0.7f, 1.3f);
        float ExplosionTimer = 0f;
        while (Death > 0f)
        {
            ExplosionTimer -= Time.deltaTime;
            Death -= Time.deltaTime;
            if (ExplosionTimer <= 0f)
            {
                Vector3 ExplPos = transform.TransformPoint(new Vector3(UnityEngine.Random.Range(-RadiusX, RadiusX), UnityEngine.Random.Range(-RadiusY, RadiusY), 0)*0.85f);
                if (UnityEngine.Random.Range(0f, 1f) < 0.7f)
                {
                    CO_SPAWNER.co.SpawnExplosionMediumRpc(ExplPos);
                    ExplosionTimer = DeathExplosionsFrequency * UnityEngine.Random.Range(0.4f, 0.7f);
                } else
                {
                    CO_SPAWNER.co.SpawnExplosionLargeRpc(ExplPos);
                    ExplosionTimer = DeathExplosionsFrequency * UnityEngine.Random.Range(0.7f, 1.2f);
                }
                if (Death < 1f)
                {
                    ExplosionTimer *= 0.2f;
                }
                foreach (Module mod in Interior.GetModules())
                {
                    if ((mod.transform.position - ExplPos).magnitude < 5f)
                    {
                        mod.Die(true);
                    }
                }
                foreach (CREW mod in Interior.GetCrew())
                {
                    if (mod.GetFaction() == GetFaction())
                    {
                        if ((mod.transform.position - ExplPos).magnitude < 9f)
                        {
                            mod.DieForever();
                        }
                    }
                }
            }
            yield return null;
        }
        foreach (Module mod in Interior.GetModules())
        {
            mod.Die(true);
        }
        foreach (CREW mod in Interior.GetCrew())
        {
            if (mod.GetFaction() == GetFaction())
            {
                mod.DieForever();
            }
        }
    }
    public void Impact(PROJ fl, Vector3 ImpactArea)
    {
        float Damage = fl.AttackDamage;
        float AbsorbableDamage = fl.AttackDamage * fl.ArmorAbsorptionModifier;
        float DamageToHull = fl.AttackDamage;
        Damage -= AbsorbableDamage;
        ModuleArmor ClosestArmor = null;
        float MaxDis = 9999f;
        foreach (Module mod in Interior.SystemModules)
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (mod is ModuleArmor)
            {
                ModuleArmor arm = (ModuleArmor)mod;
                if (Dist < MaxDis)
                {
                    ClosestArmor = arm;
                    MaxDis = Dist;
                }
            }
        }
        if (ClosestArmor)
        {
            if (ClosestArmor.CanAbsorbArmor())
            {
                AbsorbableDamage *= fl.ArmorDamageModifier; //Say, we deal 80 damage with +50% modifier = 120
                DamageToHull -= ClosestArmor.GetArmor();
                float DamageNeeded = Mathf.Min(ClosestArmor.CurArmor.Value, AbsorbableDamage); //Say, we need only 80 damage
                ClosestArmor.TakeArmorDamage(AbsorbableDamage, ClosestArmor.transform.position);
                AbsorbableDamage -= DamageNeeded; //We have 40 damage left
                AbsorbableDamage /= fl.ArmorDamageModifier; //27 damage is returned to main damage mod
            }
            if (!ClosestArmor.IsDisabled())
            {
                DamageToHull /= 2f;
            }
        }
        Damage += AbsorbableDamage;
        if (Damage < 1)
        {
            CO_SPAWNER.co.SpawnArmorImpactRpc(ImpactArea);
            return;
        }
        if (DamageToHull > 0) TakeDamage(DamageToHull * fl.HullDamageModifier * GetHullDamageRatio(), ImpactArea, DamageType.TRUE);
        foreach (Module mod in Interior.GetModules())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (Dist < fl.ModuleSplash)
            {
                mod.TakeDamage(Damage * fl.ModuleDamageModifier, mod.transform.position, DamageType.BOMBARDMENT);
            }
        }
        foreach (CREW mod in Interior.GetCrew())
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (Dist < fl.CrewDamageSplash)
            {
                mod.TakeDamage(Damage * fl.CrewDamageModifier, mod.transform.position, DamageType.BOMBARDMENT);
            }
        }
    }
    public float GetHullDamageRatio()
    {
        if (!NavModule) return 1.2f;
        if (NavModule.IsDisabled()) return 1.2f;
        return 1f - NavModule.ModuleLevel.Value*0.1f;
    }
    public void Impact(float Damage, Vector3 ImpactArea)
    {
        float AbsorbableDamage = Damage;
        Damage -= AbsorbableDamage;
        ModuleArmor ClosestArmor = null;
        float MaxDis = 9999f;
        foreach (Module mod in Interior.SystemModules)
        {
            float Dist = (mod.transform.position - ImpactArea).magnitude;
            if (mod is ModuleArmor)
            {
                ModuleArmor arm = (ModuleArmor)mod;
                if (Dist < MaxDis)
                {
                    ClosestArmor = arm;
                    MaxDis = Dist;
                }
            }
        }
        if (ClosestArmor)
        {
            if (ClosestArmor.CanAbsorbArmor())
            {
                AbsorbableDamage *= 0.4f; //Say, we deal 80 damage with +50% modifier = 120
                float DamageNeeded = Mathf.Min(ClosestArmor.CurArmor.Value, AbsorbableDamage); //Say, we need only 80 damage
                ClosestArmor.TakeArmorDamage(AbsorbableDamage, ClosestArmor.transform.position);
                AbsorbableDamage -= DamageNeeded; //We have 40 damage left
                AbsorbableDamage /= 0.4f; //27 damage is returned to main damage mod
            }
            if (!ClosestArmor.IsDisabled())
            {
                AbsorbableDamage /= 2f;
            }
        }
        Damage += AbsorbableDamage * 0.5f * GetHullDamageRatio();
        TakeDamage(Damage, ImpactArea, DamageType.TRUE);
    }
    public int GetFaction()
    {
        return Faction.Value;
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

    public override void OnNetworkDespawn()
    {
        if (UI_CommandInterface.co) UI_CommandInterface.co.HandleDespawningOfDrifter(this);
        base.OnNetworkDespawn();
    }
    public void DespawnAndUnregister()
    {
        Debug.Log("Despawn and unregister");
        CO.co.UnregisterDrifter(this);
        foreach (Module mod in new List<Module>(Interior.GetModules()))
        {
            mod.NetworkObject.Despawn();
        }
        NetworkObject.Despawn();
    }

    /*public Type GetModuleType(Module.ModuleTypes type)
    {
        return type switch
        {

            Module.ModuleTypes.DOOR_CONTROLLER => typeof(ModuleDoorControls),
            _ => typeof(Module)
        };
    }*/
    public List<Module> ModulesWithAbilityActive(Module.ModuleTypes Type)
    {
        List<Module> modules = new List<Module>();
        foreach (Module mod in Interior.SystemModules)
        {
            if (mod.ModuleType == Type)
            {
                if (mod is ModuleEffector)
                {
                    if (((ModuleEffector)mod).IsEffectActive())
                    {
                        modules.Add(mod);
                    }
                } else
                {
                    modules.Add(mod);
                }
            }
        }
        return modules;
    }
}
public class DrifterModule
{
    public ScriptableEquippableModule EquippableModule;
    public Module Module;
}