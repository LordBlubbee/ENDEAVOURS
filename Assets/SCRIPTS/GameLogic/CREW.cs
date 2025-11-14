using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CREW : NetworkBehaviour, iDamageable
{
    [Header("LOGISTICS")]
    public string UnitName;
    [TextArea(3,8)]
    public string UnitDescription;
    public ScriptableEquippable.Rarities UnitRarity;

    public float HealthIncreasePerLevelup;
    public float[] PointIncreasePerLevelup;

    [Rpc(SendTo.Server)]
    public void BuyLevelupRpc()
    {
        int Cost = GetLevelupCost();
        if (CO.co.Resource_Supplies.Value < Cost) return;
        CO.co.Resource_Supplies.Value -= Cost;
        AddUpgradeLevel(1);
    }

    public int GetLevelupCost(int num = -1) {
        if (num == -1) num = UpgradeLevel.Value;
        switch (num)
        {
            case 0:
                return 30;
            case 1:
                return 50;
            case 2:
                return 70;
            case 3:
                return 80;
            case 4:
                return 90;
            case 5:
                return 100;
            case 6:
                return 120;
            case 7:
                return 140;
        }
        return 150;
    }
    public void AddUpgradeLevel(int Levels)
    {
        CREW crew = this;
        UpgradeLevel.Value = Levels;
        crew.ModifyHealthMax += crew.HealthIncreasePerLevelup * Levels;
        crew.AddAttributes(
                Mathf.FloorToInt(crew.PointIncreasePerLevelup[0] * Levels),
                 Mathf.FloorToInt(crew.PointIncreasePerLevelup[1] * Levels),
                  Mathf.FloorToInt(crew.PointIncreasePerLevelup[2] * Levels),
                   Mathf.FloorToInt(crew.PointIncreasePerLevelup[3] * Levels),
                    Mathf.FloorToInt(crew.PointIncreasePerLevelup[4] * Levels),
                     Mathf.FloorToInt(crew.PointIncreasePerLevelup[5] * Levels),
                      Mathf.FloorToInt(crew.PointIncreasePerLevelup[6] * Levels),
                       Mathf.FloorToInt(crew.PointIncreasePerLevelup[7] * Levels)
            );
    }

    [Header("REFERENCES")]
    private Rigidbody2D Rigid;
    private Collider2D Col;
    public SpriteRenderer Spr;
    public SpriteRenderer Stripes;
    public bool ControlledByPlayer = false;
    public bool IsNeutral = false;
    public ANIM AnimationController { get; private set; }
    private List<AnimTransform> AnimTransforms = new List<AnimTransform>();
    private ANIM.AnimationState animDefaultIdle = ANIM.AnimationState.MI_IDLE;
    private ANIM.AnimationState animDefaultMove = ANIM.AnimationState.MI_MOVE;

    private bool UseItem1_Input = false;
    private bool UseItem2_Input = false;
    private float NextDashCost = 30;

    [NonSerialized] public NetworkVariable<int> PlayerController = new(); //0 = No Player Controller
    [NonSerialized] public NetworkVariable<int> Faction = new();
    [NonSerialized] public NetworkVariable<int> UpgradeLevel = new();
    [NonSerialized] public NetworkVariable<float> BleedingTime = new();
    public bool IsPlayer()
    {
        return ControlledByPlayer;
    }
    public int GetPlayerController()
    {
        return PlayerController.Value;
    }

    [Header("STATS")]
    public float MaxHealth = 100f;
    public float NaturalStaminaRegen = 25f;
    public float MovementSpeed = 7;
    public float RotationDistanceFactor = 4;
    public float RotationBaseSpeed = 90;
    public bool BleedOut = true;

    [Header("ATTRIBUTES")]
    [NonSerialized] public NetworkVariable<int> SkillPoints = new NetworkVariable<int>(0); //Not used in initial character creation
    [NonSerialized] public NetworkVariable<int> XPPoints = new NetworkVariable<int>(0); //Not used in initial character creation
    [NonSerialized] public NetworkVariable<Vector3> OrderPoint = new();

    private DRIFTER HomeDrifter;
    public DRIFTER GetHomeDrifter()
    {
        return HomeDrifter;
    }
    public void SetHomeDrifter(DRIFTER drifter)
    {
        HomeDrifter = drifter;
    }
    public void AddXP(int amount)
    {
        XPPoints.Value += amount;
        while (XPPoints.Value > 100)
        {
            XPPoints.Value = 0;
            SkillPoints.Value+= 3;
        }
    }

    private Vector3 OrderPointLocal;
    private SPACE OrderTransform;
    public Vector3 GetOrderPoint()
    {
        return OrderPoint.Value;
    }
    public SPACE GetOrderTransform()
    {
        return OrderTransform;
    }

    [Rpc(SendTo.Server)]
    public void SetOrderPointRpc(Vector3 vec)
    {
        if (vec == Vector3.zero)
        {
            OrderTransform = null;
            OrderPointLocal = Vector3.zero;
            OrderPoint.Value = Vector3.zero;
            return;
        }
        foreach (Collider2D col in Physics2D.OverlapCircleAll(vec, 0.1f))
        {
            if (col.GetComponent<SPACE>() != null)
            {
                OrderTransform = col.GetComponent<SPACE>();
                OrderPointLocal = OrderTransform.transform.InverseTransformPoint(vec);
                OrderPoint.Value = OrderTransform.transform.TransformPoint(OrderPointLocal);
                return;
            }
        }
    }
    //Modified attributes from equipment and buffs
    [NonSerialized] public int[] ModifyAttributes = new int[8];
    [NonSerialized] public float ModifyHealthMax;
    [NonSerialized] public float ModifyHealthRegen;
    [NonSerialized] public float ModifyStaminaMax;
    [NonSerialized] public float ModifyStaminaRegen;
    [NonSerialized] public float ModifyMovementSpeed;
    [NonSerialized] public float ModifyMovementSlow;
    [NonSerialized] public float ModifyAnimationSpeed;
    [NonSerialized] public float ModifyAnimationSlow;

    [NonSerialized] public float ModifyMeleeDamage;
    [NonSerialized] public float ModifyRangedDamage;
    [NonSerialized] public float ModifySpellDamage;

    private List<BUFF> Buffs = new();
    public void AddBuff(ScriptableBuff buf)
    {
        foreach (BUFF buff in Buffs)
        {
            if (buff.GetScriptable() == buf)
            {
                buff.AddBuff(buf, this);
                return;
            }
        }
        if (buf.BuffParticles != CO_SPAWNER.BuffParticles.NONE) AddBuffParticlesRpc(buf.BuffParticles);
        Buffs.Add(new BUFF(buf, this));
    }
    public void BuffTick(float time)
    {
        foreach (BUFF buf in new List<BUFF>(Buffs))
        {
            buf.TickBuff(time);
            if (buf.IsExpired())
            {
                buf.RemoveBuff(this);
                if (buf.BuffParticles != CO_SPAWNER.BuffParticles.NONE) RemoveBuffParticlesRpc(buf.BuffParticles);
                Buffs.Remove(buf);
            }
        }
    }

    private List<ParticleBuff> BuffParticles = new();

    [Rpc(SendTo.ClientsAndHost)]
    private void AddBuffParticlesRpc(CO_SPAWNER.BuffParticles type)
    {
        ParticleBuff BuffPart = GetBuff(type);
        ParticleBuff newBuffPart = Instantiate(BuffPart, transform);
        BuffParticles.Add(newBuffPart);
    }

    private ParticleBuff GetBuff(CO_SPAWNER.BuffParticles type)
    {
        return CO_SPAWNER.co.BuffParticleList[((int)type - 1)];
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void RemoveBuffParticlesRpc(CO_SPAWNER.BuffParticles type)
    {
        ParticleBuff BuffPart = GetBuff(type);
        foreach (ParticleBuff obj in new List<ParticleBuff>(BuffParticles))
        {
            if (obj.ParticleType == BuffPart.ParticleType)
            {
                Destroy(obj.gameObject);
                BuffParticles.Remove(obj);
                return;
            }
        }
    }

    public NetworkVariable<int> ATT_PHYSIQUE = new NetworkVariable<int>(2); //Buffs maximum health, melee damage
    public NetworkVariable<int> ATT_ARMS = new NetworkVariable<int>(2); //Buffs ranged damage, reload (+gunnery)
    public NetworkVariable<int> ATT_DEXTERITY = new NetworkVariable<int>(2);//Buffs movement speed, stamina
    public NetworkVariable<int> ATT_COMMUNOPATHY = new NetworkVariable<int>(2);//Buffs magic damage, camera range, senses
    public NetworkVariable<int> ATT_COMMAND = new NetworkVariable<int>(2); //Buffs piloting maneuverability, dodge chance
    public NetworkVariable<int> ATT_ENGINEERING = new NetworkVariable<int>(2); //Buffs repair speed
    public NetworkVariable<int> ATT_ALCHEMY = new NetworkVariable<int>(2); //Buffs usage of large heavy weapons
    public NetworkVariable<int> ATT_MEDICAL = new NetworkVariable<int>(2); //Buffs healing abilities (+regeneration)
    private NetworkVariable<FixedString64Bytes> CharacterBackgroundLink = new(); //Buffs healing abilities (+regeneration)
    public ScriptableBackground CharacterBackground;
    public CO_SPAWNER.DefaultEquipmentSet DefaultToolset;
    public ScriptableEquippableWeapon[] EquippedWeapons = new ScriptableEquippableWeapon[3];
    public ScriptableEquippableArtifact EquippedArmor = null;
    public ScriptableEquippableArtifact[] EquippedArtifacts = new ScriptableEquippableArtifact[3];

    private List<ArtifactAbility> ArtifactAbilities = new(); //Server only

    private ArtifactAbility GetArtifactAbility(ScriptableEquippableArtifact art)
    {
        switch (art.Ability)
        {
            default:
                return null;
            case ScriptableEquippableArtifact.ArtifactAbilityTypes.KNUCKLE_RAGE:
                return new ArtifactKnuckles(this);
            case ScriptableEquippableArtifact.ArtifactAbilityTypes.RED_POWDER:
                return new ArtifactRedPowder(this);
            case ScriptableEquippableArtifact.ArtifactAbilityTypes.CANDLE_BLAST:
                return new ArtifactFaithfulCandle(this, art);
        }
    } 
    private void AddArtifactAbility(ArtifactAbility ability)
    {
        if (ability == null) return;
        ArtifactAbilities.Add(ability);
        Debug.Log($"Successfully added new ability. Abilities: {ArtifactAbilities.Count}");
    }
    private void RemoveArtifactAbility(ArtifactAbility ability)
    {
        if (ability == null) return;
        foreach (ArtifactAbility abi in new List<ArtifactAbility>(ArtifactAbilities))
        {
            if (abi.GetType() == ability.GetType())
            {
                ArtifactAbilities.Remove(ability);
                return;
            }
        }
        Debug.Log("ERROR: Artifact ability to remove not found");
    }

    private void ArtifactOnMelee()
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            ability.OnMelee();
        }
    }
    public void ArtifactOnMeleeTarget(CREW crew)
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            ability.OnEnemyHitMelee(crew);
        }
    }
    public void ArtifactOnRangedTarget(CREW crew)
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            ability.OnEnemyHitRanged(crew);
        }
    }
    public void ArtifactOnSpellTarget(CREW crew)
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            ability.OnEnemyHitSpell(crew);
        }
    }
    private float ArtifactOnPreventDamageMelee(float dam)
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            dam = ability.OnPreventDamageMelee(dam);
        }
        return dam;
    }
    private float ArtifactOnPreventDamageRanged(float dam)
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            dam = ability.OnPreventDamageRanged(dam);
        }
        return dam;
    }
    private float ArtifactOnPreventDamageSpell(float dam)
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            dam = ability.OnPreventDamageSpell(dam);
        }
        return dam;
    }
    private void ArtifactOnRanged()
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            Debug.Log("Artifact on ranged...");
            ability.OnRanged();
        }
    }
    private void ArtifactOnSpell()
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            ability.OnSpell();
        }
    }
    private void ArtifactOnDash()
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            ability.OnDash();
        }
    }
    private void ArtifactOnDamaged()
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            ability.OnDamaged(this);
        }
    }
    public void ArtifactOnKill(CREW crew)
    {
        foreach (ArtifactAbility ability in ArtifactAbilities)
        {
            ability.OnEnemyKill(crew);
        }
    }

    //Utilities
    [NonSerialized] public Transform DraggingObject;

    [Rpc(SendTo.Server)]
    public void UpdateAttributesRpc(int phys, int arms, int dex, int comm, int pil, int eng, int gun, int med)
    {
        UpdateAttributes(phys,arms,dex,comm,pil,eng,gun,med);
    }
    public void UpdateAttributes(int phys, int arms, int dex, int comm, int pil, int eng, int gun, int med)
    {
        ATT_PHYSIQUE.Value = phys;
        ATT_ARMS.Value = arms; //Ranged attack
        ATT_DEXTERITY.Value = dex; //Buffs movement speed, stamina
        ATT_COMMUNOPATHY.Value = comm; //Buffs magic damage, camera range, senses
        ATT_COMMAND.Value = pil; //Buffs piloting maneuverability, dodge chance
        ATT_ENGINEERING.Value = eng; //Buffs repair speed
        ATT_ALCHEMY.Value = gun; //Buffs usage of large heavy weapons
        ATT_MEDICAL.Value = med; //Buffs healing abilities (+regeneration)
        CurHealth.Value = GetMaxHealth();
    }
    public void AddAttributes(int phys, int arms, int dex, int comm, int pil, int eng, int gun, int med)
    {
        ATT_PHYSIQUE.Value += phys;
        ATT_ARMS.Value += arms; //Ranged attack
        ATT_DEXTERITY.Value += dex; //Buffs movement speed, stamina
        ATT_COMMUNOPATHY.Value += comm; //Buffs magic damage, camera range, senses
        ATT_COMMAND.Value += pil; //Buffs piloting maneuverability, dodge chance
        ATT_ENGINEERING.Value += eng; //Buffs repair speed
        ATT_ALCHEMY.Value += gun; //Buffs usage of large heavy weapons
        ATT_MEDICAL.Value += med; //Buffs healing abilities (+regeneration)
        CurHealth.Value = GetMaxHealth();
    }
    // ==========================
    // GET FUNCTIONS
    // ==========================
    public void SetCharacterBackground(ScriptableBackground back)
    {
        CharacterBackground = back;
        CharacterBackgroundLink.Value = back ? back.ResourcePath : "";
    }
    public int GetATT_PHYSIQUE()
    {
        if (CharacterBackground) return ATT_PHYSIQUE.Value + CharacterBackground.Background_ATT_BONUS[0] + ModifyAttributes[0];
        return ATT_PHYSIQUE.Value;
    }

    public int GetATT_ARMS()
    {
        if (CharacterBackground) return ATT_ARMS.Value + CharacterBackground.Background_ATT_BONUS[1] + ModifyAttributes[1];
        return ATT_ARMS.Value;
    }

    public int GetATT_DEXTERITY()
    {
        if (CharacterBackground) return ATT_DEXTERITY.Value + CharacterBackground.Background_ATT_BONUS[2] + ModifyAttributes[2];
        return ATT_DEXTERITY.Value;
    }

    public int GetATT_COMMUNOPATHY()
    {
        if (CharacterBackground) return ATT_COMMUNOPATHY.Value + CharacterBackground.Background_ATT_BONUS[3] + ModifyAttributes[3];
        return ATT_COMMUNOPATHY.Value;
    }

    public int GetATT_COMMAND()
    {
        if (CharacterBackground) return ATT_COMMAND.Value + CharacterBackground.Background_ATT_BONUS[4] + ModifyAttributes[4];
        return ATT_COMMAND.Value;
    }

    public int GetATT_ENGINEERING()
    {
        if (CharacterBackground) return ATT_ENGINEERING.Value + CharacterBackground.Background_ATT_BONUS[5] + ModifyAttributes[5];
        return ATT_ENGINEERING.Value;
    }

    public int GetATT_ALCHEMY()
    {
        if (CharacterBackground) return ATT_ALCHEMY.Value + CharacterBackground.Background_ATT_BONUS[6] + ModifyAttributes[6];
        return ATT_ALCHEMY.Value;
    }

    public int GetATT_MEDICAL()
    {
        if (CharacterBackground) return ATT_MEDICAL.Value + CharacterBackground.Background_ATT_BONUS[7] + ModifyAttributes[7];
        return ATT_MEDICAL.Value;
    }

    public int GetATT(int ID)
    {
        NetworkVariable<int> ATT;
        switch (ID)
        {
            case 0: ATT = ATT_PHYSIQUE; break;
            case 1: ATT = ATT_ARMS; break;
            case 2: ATT = ATT_DEXTERITY; break;
            case 3: ATT = ATT_COMMUNOPATHY; break;
            case 4: ATT = ATT_COMMAND; break;
            case 5: ATT = ATT_ENGINEERING; break;
            case 6: ATT = ATT_ALCHEMY; break;
            case 7: ATT = ATT_MEDICAL; break;
            default: return 0; // Invalid ID, do nothing
        }
        return ATT.Value;
    }
    public int GetATT_Total(int ID)
    {
        NetworkVariable<int> ATT;
        switch (ID)
        {
            case 0: ATT = ATT_PHYSIQUE; break;
            case 1: ATT = ATT_ARMS; break;
            case 2: ATT = ATT_DEXTERITY; break;
            case 3: ATT = ATT_COMMUNOPATHY; break;
            case 4: ATT = ATT_COMMAND; break;
            case 5: ATT = ATT_ENGINEERING; break;
            case 6: ATT = ATT_ALCHEMY; break;
            case 7: ATT = ATT_MEDICAL; break;
            default: return 0; // Invalid ID, do nothing
        }
        return ATT.Value + CharacterBackground.Background_ATT_BONUS[ID] + ModifyAttributes[ID];
    }

    // ==========================
    // INCREASE FUNCTIONS (RPC only increments by +1)
    // ==========================

    [Rpc(SendTo.Server)]
    public void IncreaseATTRpc(int ID)
    {
        NetworkVariable<int> ATT;
        switch (ID)
        {
            case 0: ATT = ATT_PHYSIQUE; break;
            case 1: ATT = ATT_ARMS; break;
            case 2: ATT = ATT_DEXTERITY; break;
            case 3: ATT = ATT_COMMUNOPATHY; break;
            case 4: ATT = ATT_COMMAND; break;
            case 5: ATT = ATT_ENGINEERING; break;
            case 6: ATT = ATT_ALCHEMY; break;
            case 7: ATT = ATT_MEDICAL; break;
            default: return; // Invalid ID, do nothing
        }
        int LevelNeed = 1;
        switch (ATT.Value)
        {
            case < 3:
                LevelNeed = 1;
                break;
            case 3:
                LevelNeed = 2;
                break;
            case 4:
                LevelNeed = 3;
                break;
            case 5:
                LevelNeed = 4;
                break;
            case 6:
                LevelNeed = 5;
                break;
            default:
                LevelNeed = 6;
                break;
        }
        if (SkillPoints.Value < LevelNeed) return;
        SkillPoints.Value-= LevelNeed;
        ATT.Value++;
        CO.co.UpdateATTUIRpc();
    }


    [NonSerialized] public NetworkVariable<FixedString64Bytes> CharacterName = new();
    [NonSerialized] public NetworkVariable<Vector3> CharacterNameColor = new();

    private GamerTag CharacterNameTag;

    public TOOL EquippedToolObject { protected set; get; }

    private NetworkVariable<float> CurHealth = new();
    private NetworkVariable<float> CurStamina = new();
    private NetworkVariable<float> CurGrappleCooldown = new();

    private NetworkVariable<bool> Alive = new();
    private NetworkVariable<bool> DeadForever = new();

    public bool isDead()
    {
        return !Alive.Value;
    }
    public bool isDeadForever()
    {
        return DeadForever.Value;
    }
    public bool isDeadButReviving()
    {
        return DeadForever.Value && BleedingTime.Value < 0;
    }
    public bool CanFunction()
    {
        return !isDead();
    }
    public float GetGrappleCooldown()
    {
        return CurGrappleCooldown.Value;
    }
    
    [NonSerialized] public NetworkVariable<int> SpaceID = new();
    public SPACE Space { get {
            return CO.co.GetSpace(SpaceID.Value); 
        } set { } }

    bool hasInitialized = false;

    /*
    How does the AI work? 
        -A separate module attached to CREW gives it orders by deciding its inputs
    
    How does enemy crew AI work?
        -Ship AI diverts tasks by issuing a directive to each enemy CREW member with a LOCATION
        -Each CREW MEMBER does its best with the location they have.
            >Man
            >
            >
            >If told to man a Loon, the crew member will wait on other crew members, then launch
        -

    How does boarding and ship entry work?
        -
        -
        -

    How does enemy creature AI work?
        -
        -
        -
     
     */
    private void Start()
    {
        Init();
    }
    public void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;
        CO.co.RegisterCrew(this);

        Rigid = GetComponent<Rigidbody2D>();
        Col = GetComponent<Collider2D>();

        AnimTransforms.Add(new AnimTransform(Spr.transform));
        AnimTransforms.Add(new AnimTransform());
        AnimTransforms.Add(new AnimTransform());

        AnimationController = new ANIM(AnimTransforms);

        CharacterNameTag = Instantiate(CO_SPAWNER.co.PrefabGamerTag);
        CharacterNameTag.SetPlayerAndName(this, CharacterName.Value.ToString(), new Color(CharacterNameColor.Value.x, CharacterNameColor.Value.y, CharacterNameColor.Value.z));
        
        if (!IsServer)
        {
            if (CharacterBackgroundLink.Value != "")
            {
                CharacterBackground = Resources.Load<ScriptableBackground>(($"OBJ/SCRIPTABLES/BACKGROUNDS/{CharacterBackgroundLink.Value.ToString()}"));
            }
            if (CurrentToolIDNetwork.Value > -9)
            {
                if (EquippedWeapons[CurrentToolIDNetwork.Value]) LocallyEquip(EquippedWeapons[CurrentToolIDNetwork.Value].ToolPrefab);
                else LocallyEquip(null);
            }
        }
        if (CharacterBackground)
        {
            if (IsPlayer()) Spr.sprite = CharacterBackground.Sprite_Player;
            if (Stripes)
            {
                if (IsPlayer()) Stripes.sprite = CharacterBackground.Sprite_Stripes;
                Color col = new Color(CharacterNameColor.Value.x, CharacterNameColor.Value.y, CharacterNameColor.Value.z);
                if (!IsPlayer()) col = CharacterBackground.BackgroundColor;
                Stripes.color = col;
            }
        }
        
        //
        StartCoroutine(PeriodicUpdates());
        if (IsServer) {
            StartCoroutine(PeriodicUpdateCommander());
            Alive.Value = true;
            CurHealth.Value = GetMaxHealth();
            CurStamina.Value = GetMaxStamina();

            if (EquippedWeapons[0] != null)
            {
                EquipWeaponPrefab(0);
            }
        }
    }
    public int GetCurrentCommanderLevel()
    {
        return NearbyCommander;
    }
    int NearbyCommander = 0;
    IEnumerator PeriodicUpdateCommander()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            NearbyCommander = 0;
            Vector3 checkHit = transform.position;
            foreach (Collider2D col in Physics2D.OverlapCircleAll(checkHit, 25f))
            {
                CREW trt = col.GetComponent<CREW>();
                float Dis = (col.transform.position - transform.position).magnitude;
                if (trt == null) continue;
                if (col.gameObject == gameObject)
                {
                    continue;
                }
                if (trt.Space != Space) continue;
                if (trt.GetFaction() != GetFaction()) continue;
                NearbyCommander = Mathf.Max(trt.GetATT_COMMAND(),NearbyCommander);
            }
            BuffTick(0.5f);
        }
    }
    IEnumerator PeriodicUpdates()
    {
        while (true)
        {
            CharacterNameTag.SetPlayerAndName(this, CharacterName.Value.ToString(), new Color(CharacterNameColor.Value.x, CharacterNameColor.Value.y, CharacterNameColor.Value.z));
            UpdateColorBasedOnLife();
            yield return new WaitForSeconds(1);
        }
    }
    [Rpc(SendTo.Everyone)]
    public void RegisterPlayerOnLOCALCORpc()
    {
        StartCoroutine(RegisterPlayerWait());
        /*if (!IsPlayerControlled()) Debug.Log("Error: Is not player controlled");
        else CO.co.GetLOCALCO(PlayerController.Value).SetPlayerObject(this);*/
    }

    IEnumerator RegisterPlayerWait()
    {
        while (PlayerController.Value == 0)
        {
            yield return null;
        }
        RegisterAsLocalPlayer();
    }
    private void RegisterAsLocalPlayer()
    {
        CO.co.GetLOCALCO(PlayerController.Value).SetPlayerObject(this);
    }
    public void DespawnAndUnregister()
    {
        if (Space) Space.RemoveCrew(this);
        CO.co.UnregisterCrew(this);
        NetworkObject.Despawn();
    }

    [Rpc(SendTo.Server)]
    public void DismissCrewmemberRpc()
    {
        CO.co.Resource_Supplies.Value += GetDismissValue();
        DespawnAndUnregister();
    }

    public int GetDismissValue()
    {
        int num = 20;
        int Level = UpgradeLevel.Value;
        while (Level > 0)
        {
            Level--;
            num += GetLevelupCost()/2;
        }
        return num;
    }

    /*Decide Inputs*/

    private Vector3 MoveInput;
    private Vector3 LookTowards;
    private NetworkVariable<bool> isMoving = new(false);
    private bool isLooking = false;

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
        MoveInput = mov;
        if (IsServer) isMoving.Value = mov != Vector3.zero;
    }

    public Vector3 GetMoveInput()
    {
        return MoveInput;
    }
    public Vector3 GetLookTowards()
    {
        return LookTowards;
    }
    public void SetLookTowards(Vector2 mov)
    {
        LookTowards = mov;
        isLooking = mov != Vector2.zero;
    }
    [Rpc(SendTo.Server)]
    public void DashRpc()
    {
        Dash();
    }
    [Rpc(SendTo.Server)]
    public void UseItem1Rpc()
    {
        UseItem1_Input = true;
    }
    [Rpc(SendTo.Server)]
    public void UseItem2Rpc()
    {
        UseItem2_Input = true;
    }
    [Rpc(SendTo.Server)]
    public void StopItem1Rpc()
    {
        UseItem1_Input = false;
    }
    [Rpc(SendTo.Server)]
    public void StopItem2Rpc()
    {
        UseItem2_Input = false;
    }
    [Rpc(SendTo.Server)]
    public void EquipWrenchRpc()
    {
        EquipTool(CO_SPAWNER.co.GetPrefabWrench(DefaultToolset), -2);
        if (IsPlayerControlled()) EquipWeaponUpdateUIRpc(-2);
    }
    [Rpc(SendTo.Server)]
    public void EquipGrappleRpc()
    {
        EquipTool(CO_SPAWNER.co.GetPrefabGrapple(DefaultToolset), -1);
        if (IsPlayerControlled()) EquipWeaponUpdateUIRpc(-1);
    }
    [Rpc(SendTo.Server)]
    public void EquipMedkitRpc()
    {
        EquipTool(CO_SPAWNER.co.GetPrefabMedkit(DefaultToolset), -3);
        if (IsPlayerControlled()) EquipWeaponUpdateUIRpc(-3);
    }
    [Rpc(SendTo.Server)]
    public void EquipWeapon1Rpc()
    {
        EquipWeaponPrefab(0);
    }
    [Rpc(SendTo.Server)]
    public void EquipWeapon2Rpc()
    {
        EquipWeaponPrefab(1);
    }
    [Rpc(SendTo.Server)]
    public void EquipWeapon3Rpc()
    {
        EquipWeaponPrefab(2);
    }

    private bool IsGrappling = false;
    public bool UsePhysics()
    {
        return !IsGrappling;
    }

    public void StopDragging()
    {

        DraggingObject = null;
    }
    public void StartDragging(Transform trans)
    {
        DraggingObject = trans;
    }

    IEnumerator GrappleNum(Transform trt)
    {
        StopDragging();
        Col.enabled = false;
        IsGrappling = true;
        Vector3 moveAdd = new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f));
        while ((trt.position+ moveAdd - transform.position).magnitude > 0.5f)
        {
            transform.position += (trt.position+ moveAdd - transform.position).normalized * ((MovementSpeed + (trt.position+ moveAdd - transform.position).magnitude) * 2f * CO.co.GetWorldSpeedDelta());
            yield return null;
        }
        IsGrappling = false;
        Col.enabled = true;
    }

    public void AddToSpace(SPACE newSpace)
    {
        Debug.Log($"{this.name} Adding to {newSpace}");
        newSpace.AddCrew(this);
        AddToSpaceRpc(newSpace.SpaceID.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddToSpaceRpc(int spaceID)
    {
        CO.co.GetSpace(spaceID).AddCrew(this);
    }
    public void UseGrapple(WalkableTile tile)
    {
        if (IsGrappling) return;
        if (Space) Space.RemoveCrew(this);
        AnimationComboWeapon1 = 0;
        AnimationComboWeapon2 = 0;
        AddToSpace(tile.Space);
        StartCoroutine(GrappleNum(tile.transform));
        Debug.Log($"{this.name} Using grapple!");
    }
    /*Use Inputs*/
    public void Dash()
    {
        if (!isMoving.Value) return;
        if (isDashing) return;
        if (!CanFunction()) return;
        if (!ConsumeStamina(NextDashCost)) return;
        NextDashCost = GetDashCost();
        AnimationComboWeapon1 = 0;
        AnimationComboWeapon2 = 0;
        CurrentReload = 0;
        DashingDamageBuff = 0.6f;
        StartCoroutine(DashNumerator());
        if (EquippedToolObject)
        {
            if (EquippedToolObject.ActionUse1 == TOOL.ToolActionType.RANGED_ATTACK || EquippedToolObject.ActionUse1 == TOOL.ToolActionType.SPELL_ATTACK)
            {
                setAnimationRpc(ANIM.AnimationState.MI_DASH);
            }
        }
        ArtifactOnDash();
    }

    bool isDashing = false;
    float DashingDamageBuff = 0f;
    IEnumerator DashNumerator()
    {
        isDashing = true;
        float Speed = 1f;
        Vector3 dir = MoveInput.normalized; // ensure direction is normalized

        while (Speed > 0f && UsePhysics())
        {
            float mov = 5f * MovementSpeed * Speed * CO.co.GetWorldSpeedDeltaFixed();

            // Predict the new position
            Vector3 newPos = transform.position + mov * dir;

            // Perform a 2D raycast in the dash direction to detect obstacles
            RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, dir, mov, LayerMask.GetMask("InSpace"));
            // You can change the layer mask to whatever you use for obstacles, e.g. LayerMask.GetMask("Walls")
            bool hasMoved = false;
            foreach (RaycastHit2D col in hit)
            {
                if (col.collider.gameObject == gameObject) continue;
                if (!col.collider.gameObject.tag.Equals("LOSBlocker")) continue;
                // Stop the dash at the hit point (slightly before, to avoid clipping)
                newPos = new Vector3(col.point.x, col.point.y) - dir * 0.05f;
                transform.position = newPos;
                Rigid.MovePosition(newPos);
                hasMoved = true;
                break; // stop dashing since we hit something
            }
            if (!hasMoved)
            {
                // Move normally if no obstacle was hit
                transform.position = newPos;
                Rigid.MovePosition(newPos);
            }

            Speed -= CO.co.GetWorldSpeedDeltaFixed() * 3f;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        while (!isDashing && DashingDamageBuff > 0)
        {
            DashingDamageBuff -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
    }

    public void Push(float Power, float Duration, Vector3 dir)
    {
        StartCoroutine(PushNumerator(Power,Duration,dir));
    }
    IEnumerator PushNumerator(float Power, float Duration, Vector3 dir)
    {
        float Speed = 1f;
        while (Speed > 0f && !isDashing && UsePhysics())
        {
            transform.position += 2f * Power * Speed * CO.co.GetWorldSpeedDeltaFixed() * dir;
            Rigid.MovePosition(transform.position);
            Speed -= CO.co.GetWorldSpeedDeltaFixed() / Duration;
            yield return new WaitForFixedUpdate();
        }
    }
    int SelectedWeaponAbility = 0;
    int AnimationComboWeapon1 = 0;
    int AnimationComboWeapon2 = 0;
    public void UseItem1()
    {
        if (!CanFunction()) return;
        if (EquippedToolObject == null) return;
        if (EquippedToolObject.ActionUse1 == TOOL.ToolActionType.NONE) return;
        SelectedWeaponAbility = 0;
        if (AnimationComboWeapon1 >= EquippedToolObject.attackAnimations1.Count)
        {
            AnimationComboWeapon1 = 0;
        }
        if (!canStrike) return;
        if (GetStamina() < EquippedToolObject.UsageStamina1) return;
         //Reset the other combo
        setAnimationToClientsOnlyRpc(EquippedToolObject.attackAnimations1[AnimationComboWeapon1]);
        if (!setAnimationLocally(EquippedToolObject.attackAnimations1[AnimationComboWeapon1], 3)) return;
        canStrikeMelee = true;
        hasCreatedSound = false;
        MeleeHits = new();
        ConsumeStamina(EquippedToolObject.UsageStamina1);
        AnimationComboWeapon1++;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponSFXRpc()
    {
        if (!EquippedToolObject) return;
        if (SelectedWeaponAbility == 0)
        {
            if (EquippedToolObject.Action1_SFX.Length > 0)
            {
                AUDCO.aud.PlaySFX(EquippedToolObject.Action1_SFX, transform.position, 0.1f);
            }
        }
        else if (SelectedWeaponAbility == 1)
        {
            if (EquippedToolObject.Action2_SFX.Length > 0)
            {
                AUDCO.aud.PlaySFX(EquippedToolObject.Action2_SFX, transform.position, 0.1f);
            }
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponHitSFXRpc()
    {
        if (!EquippedToolObject) return;
        if (EquippedToolObject.Action1_SFX_Hit.Length > 0)
        {
            AUDCO.aud.PlaySFX(EquippedToolObject.Action1_SFX_Hit, transform.position, 0.1f);
        }
    }
    public void UseItem2()
    {
        if (!CanFunction()) return;
        if (EquippedToolObject == null) return;
        if (EquippedToolObject.ActionUse2 == TOOL.ToolActionType.NONE) return;
        SelectedWeaponAbility = 1;
        if (AnimationComboWeapon2 >= EquippedToolObject.attackAnimations2.Count)
        {
            AnimationComboWeapon2 = 0;
        }
        if (!canStrike) return;
        if (GetStamina() < EquippedToolObject.UsageStamina2) return;
        AnimationComboWeapon1 = 0;
        setAnimationToClientsOnlyRpc(EquippedToolObject.attackAnimations2[AnimationComboWeapon2]);
        if (!setAnimationLocally(EquippedToolObject.attackAnimations2[AnimationComboWeapon2],4)) return;
        canStrikeMelee = true;
        hasCreatedSound = false;
        MeleeHits = new();
        ConsumeStamina(EquippedToolObject.UsageStamina2);
        AnimationComboWeapon2++;
    }

    private float LastStaminaUsed = 0f;
    
    public void DieForever()
    {
        Die();
        DeadForever.Value = true;
        BleedingTime.Value = 0;
    }

    [Rpc(SendTo.Server)]
    public void RespawnASAPRpc()
    {
        DeadForever.Value = true;
        BleedingTime.Value = 0;
    }

    private bool hasCreatedSound = false;

    private void StrikeUpdate()
    {
        if (EquippedToolObject == null) return;
        if (EquippedToolObject.strikePoints.Count == 0) return;
        if (AnimationController.isCurrentlyStriking())
        {
            if (AnimationController.CurrentStrikePower() > 0.1f)
            {
                switch (SelectedWeaponAbility == 0 ? EquippedToolObject.ActionUse1 : EquippedToolObject.ActionUse2)
                {
                    case TOOL.ToolActionType.MELEE_ATTACK:
                        if (!canStrikeMelee) return;
                        ArtifactOnMelee();
                        StrikeMelee();
                        break;
                    case TOOL.ToolActionType.RANGED_ATTACK:
                        if (!canStrike) return;
                        StrikeRanged(LookTowards);
                        break;
                    case TOOL.ToolActionType.SPELL_ATTACK:
                        if (!canStrike) return;
                        StrikeSpell(LookTowards);
                        break;
                    case TOOL.ToolActionType.REPAIR:
                        if (!canStrikeMelee) return;
                        StrikeRepair();
                        break;
                    case TOOL.ToolActionType.HEAL_OTHERS:
                        if (!canStrikeMelee) return;
                        StrikeHealOthers();
                        break;
                    case TOOL.ToolActionType.HEAL_SELF:
                        if (!canStrikeMelee) return;
                        StrikeHealSelf();
                        break;
                    case TOOL.ToolActionType.BLOCK:
                        foreach (BlockAttacks blocker in EquippedToolObject.Blockers)
                        {
                            blocker.SetActive(true);
                        }
                        break;
                    case TOOL.ToolActionType.MELEE_AND_BLOCK:
                        foreach (BlockAttacks blocker in EquippedToolObject.Blockers)
                        {
                            blocker.SetActive(true);
                        }
                        if (!canStrikeMelee) return;
                        ArtifactOnMelee();
                        StrikeMelee();
                        break;
                }
            }
            if (SelectedWeaponAbility == 0 && !hasCreatedSound)
            {
                hasCreatedSound = true;
                if (EquippedToolObject.Action1_SFX.Length > 0) WeaponSFXRpc();
            }
            return;
        }
    }

    List<GameObject> MeleeHits = new();
    public bool IsTargetEnemy(iDamageable dam)
    {
        return IsTargetEnemy(dam.GetFaction());
    }
    public bool IsTargetEnemy(int targetFac)
    {
        return targetFac != GetFaction() && targetFac != 0;
    }
    public bool IsEnemyInFront(float dis)
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position + getLookVector() * dis * 0.5f, dis * 0.6f))
        {
            iDamageable crew = col.GetComponent<iDamageable>();
            if (crew != null)
            {
                if (!IsTargetEnemy(crew)) continue;
                if (!crew.CanBeTargeted(Space)) continue;
                return true;
            }
        }
        return false;
    }
    private void StrikeMelee()
    {
        foreach (Transform hitTrans in EquippedToolObject.strikePoints)
        {
            Vector3 checkHit = hitTrans.position;
            foreach (Collider2D col in Physics2D.OverlapCircleAll(checkHit, 0.3f))
            {
                iDamageable crew = col.GetComponent<iDamageable>();
                if (crew != null)
                {
                    if (!Melee(crew, checkHit, SelectedWeaponAbility == 0 ? EquippedToolObject.attackDamage1 : EquippedToolObject.attackDamage2)) continue;
                    WeaponHitSFXRpc();
                    return;
                }
                BlockAttacks Blocker = col.GetComponent<BlockAttacks>();
                if (Blocker != null)
                {
                    if (MeleeHits.Contains(Blocker.gameObject)) continue;
                    if (!IsTargetEnemy(Blocker.tool.GetCrew().GetFaction())) continue;
                    if (!Blocker.tool.GetCrew().CanBeTargeted(Space)) continue;
                    MeleeHits.Add(Blocker.gameObject);
                    float Penetration = 0;
                    if (DashingDamageBuff > 0) Penetration += 0.3f;
                    bool isBlocked = UnityEngine.Random.Range(0f, 1f) + Penetration < Blocker.BlockChanceMelee;
                    if (isBlocked)
                    {
                        if (Blocker.BlockSound != AUDCO.BlockSoundEffects.NONE) AUDCO.aud.PlayBlockSFXRpc(Blocker.BlockSound, transform.position);
                        else WeaponHitSFXRpc();
                        if (Blocker.ReduceDamageModMelee < 1f)
                        {
                            float dmg = SelectedWeaponAbility == 0 ? EquippedToolObject.attackDamage1 : EquippedToolObject.attackDamage2;
                            Melee(Blocker.tool.GetCrew(), checkHit, dmg * (1f - Blocker.ReduceDamageModMelee));
                        }
                        else
                        {
                            CO_SPAWNER.co.SpawnImpactRpc(checkHit);
                        }
                        canStrikeMelee = false;
                    }
                }
            }
        }
    }
    private bool Melee(iDamageable crew, Vector3 checkHit, float dmg)
    {
        if (crew.GetFaction() == GetFaction()) return false;
        if (!crew.CanBeTargeted(Space)) return false;
        dmg += ModifyMeleeDamage;
        dmg *= AnimationController.CurrentStrikePower();
        dmg *= 0.7f + 0.1f * GetATT_PHYSIQUE() + 0.02f * GetCurrentCommanderLevel();
        if (DashingDamageBuff > 0)
        {
            DashingDamageBuff = 0;
            dmg *= 2;
        }
        if (crew is DRIFTER) ((DRIFTER)crew).Impact(dmg * 0.5f, checkHit);
        else
        {
            crew.TakeDamage(dmg, checkHit, iDamageable.DamageType.MELEE);
            if (crew is CREW)
            {
                if (EquippedToolObject.ApplyBuff)
                {
                    ((CREW)crew).AddBuff(EquippedToolObject.ApplyBuff);
                }
                ArtifactOnMeleeTarget((CREW)crew);
                if (((CREW)crew).isDead())
                {
                    ArtifactOnKill((CREW)crew);
                }
            }
        }
        CO_SPAWNER.co.SpawnImpactRpc(checkHit);
        canStrikeMelee = false; //Turn off until animation ends
        return true;
    }

    private void StrikeRanged(Vector3 trt)
    {
        PROJ proj = Instantiate(SelectedWeaponAbility == 0 ? EquippedToolObject.RangedPrefab1 : EquippedToolObject.RangedPrefab2, EquippedToolObject.strikePoints[0].position, EquippedToolObject.transform.rotation);
        float dmg = SelectedWeaponAbility == 0 ? EquippedToolObject.attackDamage1 : EquippedToolObject.attackDamage2;
        dmg += ModifyRangedDamage;
        dmg *= AnimationController.CurrentStrikePower();
        dmg *= 0.7f + 0.1f * GetATT_ARMS() + 0.02f * GetCurrentCommanderLevel();
        proj.NetworkObject.Spawn();
        proj.Init(dmg, GetFaction(), Space, trt);
        proj.CrewOwner = this;
        float reload = SelectedWeaponAbility == 0 ? EquippedToolObject.Reload1 : EquippedToolObject.Reload2;
        reload /= 0.6f + 0.05f * GetATT_ARMS() + 0.05f * GetATT_ALCHEMY();
        StartCoroutine(AttackCooldown(reload));
        ArtifactOnRanged();
    }
    private void StrikeSpell(Vector3 trt)
    {
        PROJ proj = Instantiate(SelectedWeaponAbility == 0 ? EquippedToolObject.RangedPrefab1 : EquippedToolObject.RangedPrefab2, EquippedToolObject.strikePoints[0].position, EquippedToolObject.transform.rotation);
        float dmg = SelectedWeaponAbility == 0 ? EquippedToolObject.attackDamage1 : EquippedToolObject.attackDamage2;
        dmg += ModifySpellDamage;
        dmg *= AnimationController.CurrentStrikePower();
        dmg *= 0.7f + 0.1f * GetATT_COMMUNOPATHY() + 0.02f * GetCurrentCommanderLevel();
        if (GetATT_COMMUNOPATHY() < 4) dmg *= 0.25f * GetATT_COMMUNOPATHY();
        proj.NetworkObject.Spawn();
        proj.Init(dmg, GetFaction(), Space, trt);
        proj.CrewOwner = this;
        float reload = SelectedWeaponAbility == 0 ? EquippedToolObject.Reload1 : EquippedToolObject.Reload2;
        reload /= 0.4f + 0.08f * GetATT_COMMUNOPATHY() + 0.08f * GetATT_ALCHEMY();
        StartCoroutine(AttackCooldown(reload));
        ArtifactOnSpell();
    }
    private void StrikeRepair()
    {
        foreach (Transform hitTrans in EquippedToolObject.strikePoints)
        {
            Vector3 checkHit = hitTrans.position;
            foreach (Collider2D col in Physics2D.OverlapCircleAll(checkHit, 1f))
            {
                iDamageable crew = col.GetComponent<iDamageable>();
                if (crew != null)
                {
                    if (crew.GetFaction() != GetFaction() && crew.GetFaction() != 0) continue;
                    if (crew.Space != Space) continue;
                    if (!(crew is Module)) continue; 
                    float dmg = SelectedWeaponAbility == 0 ? EquippedToolObject.attackDamage1 : EquippedToolObject.attackDamage2;
                    dmg *= AnimationController.CurrentStrikePower();
                    dmg *= 0.4f + 0.2f * GetATT_ENGINEERING() + 0.05f * GetCurrentCommanderLevel();
                    if (CO.co.IsSafe()) dmg *= 5;
                    crew.Heal(dmg);
                    canStrikeMelee = false; //Turn off until animation ends
                    return;
                }
            }
        }
    }
    private void StrikeHealOthers()
    {
        CREW crew = null;
        float MaxDis = 999f;
        foreach (Transform hitTrans in EquippedToolObject.strikePoints)
        {
            Vector3 checkHit = hitTrans.position;
            foreach (Collider2D col in Physics2D.OverlapCircleAll(checkHit, 2f))
            {
                CREW trt = col.GetComponent<CREW>();
                float Dis = (col.transform.position - transform.position).magnitude;
                if (trt == null) continue;
                if (Dis < MaxDis)
                {
                    if (col.gameObject == gameObject)
                    {
                        continue;
                    }
                    if (trt.Space != Space) continue;
                    if (trt.GetFaction() != GetFaction()) continue;
                    if (trt.GetHealthRelative() >= 1f) continue;
                    crew = trt;
                    MaxDis = Dis;
                }
            }
        }
        if (crew != null)
        {
            Debug.Log($"Target healing {crew.transform.gameObject.name} done!");
            //if (crew.GetFaction() != Faction) return;
            float dmg = SelectedWeaponAbility == 0 ? EquippedToolObject.attackDamage1 : EquippedToolObject.attackDamage2;
            dmg *= AnimationController.CurrentStrikePower();
            dmg *= GetHealingSkill();
            if (CO.co.IsSafe()) dmg *= 5;
            crew.Heal(dmg);
            canStrikeMelee = false; //Turn off until animation ends
            return;
        } else
        {
            StrikeHealSelf(EquippedToolObject.attackDamage2);
        }
    }
    private void StrikeHealSelf(float dmg = 0)
    {
        canStrikeMelee = false; //Turn off until animation ends
        if (dmg == 0) dmg = SelectedWeaponAbility == 0 ? EquippedToolObject.attackDamage1 : EquippedToolObject.attackDamage2;
        dmg *= AnimationController.CurrentStrikePower();
        dmg *= GetHealingSkill();
        if (CO.co.IsSafe()) dmg *= 5;
        Heal(dmg);
    }

    bool canStrikeMelee = true;
    bool canStrike = true;
    float CurrentReload = 0f;
    IEnumerator AttackCooldown(float col)
    {
        canStrike = false;
        CurrentReload = col;
        while (CurrentReload > 0f)
        {
            CurrentReload -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        canStrike = true;
    }
    private void AnimationUpdate()
    {
        AnimationController.animationFrame(1f * (1f+ ModifyAnimationSpeed) / (1f + ModifyAnimationSlow));
        if (IsServer)
        {
            if (AnimationController.getAnimationMoveForward() != 0)
            {
                transform.position += AnimationController.getAnimationMoveForward() * getLookVector() * CO.co.GetWorldSpeedDelta() * (1f+ModifyMovementSpeed) / (1f + ModifyMovementSlow);
            }
        }
    }
    public bool setAnimationLocally(ANIM.AnimationState stat, int pr = 99)
    {
        return AnimationController.setAnimation(stat, pr);
    }
    public void setAnimationIfNotAlready(ANIM.AnimationState stat, int pr = 99)
    {
        if (!hasInitialized) return;
        if (GetCurrentAnimation() == stat) return;
        AnimationController.setAnimation(stat, pr);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void setAnimationRpc(ANIM.AnimationState stat, int pr = 99)
    {
        if (!hasInitialized) return;
        AnimationController.setAnimation(stat, pr);
    }
    [Rpc(SendTo.NotServer)]
    public void setAnimationToClientsOnlyRpc(ANIM.AnimationState stat, int pr = 99)
    {
        AnimationController.setAnimation(stat, pr);
    }
    private void Update()
    {
        AnimationUpdate();
        MoveAndLook();
        if (!IsServer) return;
        if (OrderTransform != null) OrderPoint.Value = OrderTransform.transform.TransformPoint(OrderPointLocal);
        StrikeUpdate();
        if (GetStamina() < GetMaxStamina())
        {
            LastStaminaUsed += CO.co.GetWorldSpeedDelta();
            AddStamina(CO.co.GetWorldSpeedDelta() * GetStaminaRegen() * (LastStaminaUsed + 1f) * 0.25f);
            if (LastStaminaUsed > 2f)
            {
                AnimationComboWeapon1 = 0;
                AnimationComboWeapon2 = 0;
            }
        }
        if (UseItem1_Input) UseItem1();
        if (UseItem2_Input) UseItem2();

        if (isDead())
        {
            if (!isDeadForever())
            {
                BleedingTime.Value -= CO.co.GetWorldSpeedDelta();
                if (BleedingTime.Value < 0)
                {
                    DeadForever.Value = true;
                    BleedingTime.Value = 0;
                }
            }
            else if (HomeDrifter)
            {
                if (!HomeDrifter.MedicalModule.IsDisabled() && (Space == HomeDrifter.Space))
                {
                    if ((transform.position - HomeDrifter.MedicalModule.transform.position).magnitude > 8)
                    {
                        transform.position = HomeDrifter.MedicalModule.transform.position + new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f));
                        if (Space != HomeDrifter.Space)
                        {
                            Space.RemoveCrew(this);
                            HomeDrifter.Space.AddCrew(this);
                        }
                    }
                    BleedingTime.Value -= CO.co.GetWorldSpeedDelta();
                    if (BleedingTime.Value < -20 || CO.co.IsSafe())
                    {
                        DeadForever.Value = false;
                        SetAlive();
                        Heal(1);
                    }
                }
                else
                {
                    BleedingTime.Value = 0;
                }
            }
        }

        DamageHealingUpdate();
    }

    public ANIM.AnimationState GetCurrentAnimation()
    {
         return AnimationController.getAnimationState();
    }
    public void SetAlive()
    {
        Alive.Value = true;
        setAnimationIfNotAlready(ANIM.AnimationState.MI_IDLE);
        SetColorAliveRpc();
    }
    private void UpdateColorBasedOnLife()
    {
        Spr.color = isDead() ? new Color(0.5f,0.3f,0.3f) : Color.white;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetColorAliveRpc()
    {
        Spr.color = Color.white;
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void SetColorDeadRpc()
    {
        Spr.color = new Color(0.5f, 0.3f, 0.3f);
    }
    private void MoveAndLook()
    {
        if (!CanFunction())
        {
            if (isDead() && !isDeadForever() && IsServer)
            {
                if (GetHealth() > 50)
                {
                    SetAlive();
                }
            }
            return;
        }
        float delta = CO.co.GetWorldSpeedDelta();
        if (isLooking)
        {
            float ang = AngleToTurnTarget();
            if (ang > 1f)
            {
                transform.Rotate(Vector3.forward, (Mathf.Abs(ang) * RotationDistanceFactor + RotationBaseSpeed) * delta);
                ang = AngleToTurnTarget();
                if (AngleToTurnTarget() < 0f)
                {
                    transform.Rotate(Vector3.forward, ang);
                    isLooking = false;
                }
            }
            else if (ang < -1f)
            {
                transform.Rotate(Vector3.forward, -(Mathf.Abs(ang) * RotationDistanceFactor + RotationBaseSpeed) * delta);
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
        if (!UsePhysics()) return;
        if (DraggingObject)
        {
            float Dist = (DraggingObject.transform.position - transform.position).magnitude;
            DraggingObject.transform.position = transform.position + (DraggingObject.transform.position - transform.position).normalized * Mathf.Min(Dist, 3.25f);
        }
        if (isMoving.Value)
        {
            if (IsServer) setAnimationIfNotAlready(animDefaultMove, 1);
            float towardsang = Mathf.Abs(AngleTowards(MoveInput));
            float towardsfactor = 1.1f - Mathf.Clamp((towardsang - 70f) * 0.005f, 0, 0.5f); //The more you look in the correct direction, the faster you move!
            transform.position += MoveInput * GetSpeed() * towardsfactor * delta;
            Rigid.MovePosition(transform.position);
            //Rigid.MovePosition(transform.position + MoveInput * GetSpeed() * towardsfactor * CO.co.GetWorldSpeedDelta());

        }
        else
        {
            if (IsServer) setAnimationRpc(animDefaultIdle, 1);
        }
    }

    /*WEAPONS*/
    public void EquipWeaponPrefab(int ID)
    {
        //Works on Server
        if (!CanFunction()) return;
        if (EquippedWeapons[ID]) EquipTool(EquippedWeapons[ID].ToolPrefab, ID);
        else EquipTool(null, ID);
        if (IsPlayerControlled()) EquipWeaponUpdateUIRpc(ID);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EquipWeaponUpdateUIRpc(int ID)
    {
        //Works on Owner
        if (LOCALCO.local.GetPlayerID() != GetPlayerController()) return;
        UI.ui.MainGameplayUI.EquipWeaponUI(ID);
    }

    int CurrentToolID = -9;
    NetworkVariable<int> CurrentToolIDNetwork = new(-9);
    public void EquipTool(TOOL tol, int ID)
    {
        //Works on Server
        if (CurrentToolID == ID) return;
        if (!CanFunction()) return;
        CurrentToolID = ID;
        CurrentToolIDNetwork.Value = ID;
        EquipToolLocallyRpc(ID); //Send to Clients
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EquipToolLocallyRpc(int ID)
    {
        switch (ID)
        {
            default:
                if (EquippedWeapons[ID]) LocallyEquip(EquippedWeapons[ID].ToolPrefab);
                else LocallyEquip(null);
                break;
            case -1:
                LocallyEquip(CO_SPAWNER.co.GetPrefabGrapple(DefaultToolset));
                break;
            case -2:
                LocallyEquip(CO_SPAWNER.co.GetPrefabWrench(DefaultToolset));
                break;
            case -3:
                LocallyEquip(CO_SPAWNER.co.GetPrefabMedkit(DefaultToolset));
                break;
        }
    }

    private void LocallyEquip(TOOL tol)
    {
        //Works on Client
        if (!hasInitialized)
        {
            Init();
        }
      
        if (tol == null)
        {
            if (EquippedToolObject)
            {
                if (EquippedToolObject.Tool2)
                {
                    Destroy(EquippedToolObject.Tool2.gameObject);
                }
                Destroy(EquippedToolObject.gameObject);
            }
            EquippedToolObject = null;
            return;
        }
        if (EquippedToolObject)
        {
            if (EquippedToolObject.Tool2)
            {
                Destroy(EquippedToolObject.Tool2.gameObject);
            }
            Destroy(EquippedToolObject.gameObject);
        }

        EquippedToolObject = Instantiate(tol, transform);
        EquippedToolObject.Init(this);
        EquippedToolObject.transform.localPosition = new Vector3(EquippedToolObject.localX, EquippedToolObject.localY, -0.002f);
        EquippedToolObject.transform.Rotate(Vector3.forward, EquippedToolObject.localRot);
        AnimTransforms[1].setTransform(EquippedToolObject.transform);
        if (EquippedToolObject.Tool2)
        {
            EquippedToolObject.Tool2.transform.SetParent(transform);
            EquippedToolObject.Tool2.transform.localPosition = new Vector3(EquippedToolObject.Tool2.transform.localPosition.x, EquippedToolObject.Tool2.transform.localPosition.y, -0.002f);
            AnimTransforms[2].setTransform(EquippedToolObject.Tool2.transform);
        }
        setAnimationRpc(ANIM.AnimationState.MI_IDLE);
        AnimationComboWeapon1 = 0;
        AnimationComboWeapon2 = 0;
    }

    /*GETTERS AND SETTERS*/
    public float GetHealth()
    {
        return CurHealth.Value;
    }
    public float GetMaxHealth()
    {
        return MaxHealth * (0.8f + 0.1f * GetATT_PHYSIQUE()) + ModifyHealthMax;
    }
    public float GetHealingSkill()
    {
        return (0.4f + 0.2f * GetATT_MEDICAL() + 0.05f * GetCurrentCommanderLevel());
    }
    public float GetStaminaRegen()
    {
        return NaturalStaminaRegen * (0.8f + 0.08f * GetATT_DEXTERITY() + 0.08f * GetCurrentCommanderLevel()) * (1+ ModifyStaminaRegen);
    }
    public float GetDashCost()
    {
        return 30;
    }
    public float GetHealthRelative()
    {
        return GetHealth() / GetMaxHealth();
    }
    public float GetStamina()
    {
        return CurStamina.Value;
    }
    public float GetStaminaRelative()
    {
        return GetStamina() / GetMaxStamina();
    }
    public bool ConsumeStamina(float fl)
    {
        if (fl > CurStamina.Value) return false;
        if (fl > 25) LastStaminaUsed = 0f;
        else if (fl > 0) LastStaminaUsed = 1f;
        CurStamina.Value -= fl;
        return true;
    }

    private float lastDamageTime = 0f;

    private void DamageHealingUpdate()
    {
        if (lastDamageTime > 0f)
        {
            lastDamageTime -= CO.co.GetWorldSpeedDelta();
            return;
        }
    }
    public void Heal(float fl)
    {
        if (isDeadForever()) return;
        float Diff = CurHealth.Value;
        CurHealth.Value = Mathf.Min(GetMaxHealth(), CurHealth.Value + fl);
        Diff -= CurHealth.Value;
        if (CurHealth.Value > 49 && isDead())
        {
            SetAlive();
        }
        if (Diff > -1) return;
        CO_SPAWNER.co.SpawnHealRpc(fl, transform.position);
    }
    public void TakeDamage(float fl, Vector3 src, iDamageable.DamageType type)
    {
        if (isDeadForever()) return;
        if (isDashing) return;
        if (DashingDamageBuff > 0f) fl *= 0.5f;
        switch (type)
        {
            case iDamageable.DamageType.MELEE:
                fl = ArtifactOnPreventDamageMelee(fl);
                break;
            case iDamageable.DamageType.RANGED:
                fl = ArtifactOnPreventDamageRanged(fl);
                break;
            case iDamageable.DamageType.SPELL:
                fl = ArtifactOnPreventDamageSpell(fl);
                break;
        }
        lastDamageTime = 7f;
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            CurHealth.Value = 0f;
            if (!BleedOut) DieForever();
            else Die();
        }
        CO_SPAWNER.co.SpawnDMGRpc(fl, src);
        ArtifactOnDamaged();
    }

    public void EquipWeapon(int slot, ScriptableEquippableWeapon wep)
    {
        if (EquippedWeapons[slot] == wep) return;
        EquippedWeapons[slot] = wep;
        if (IsServer)
        {
            CurrentToolID = -9;
            if (wep == null)
            {
                EquipWeaponLocallyRpc(slot, ""); 
                EquipTool(null, slot);
            }
            else
            {
                EquipWeaponLocallyRpc(slot, wep.GetItemResourceIDShort());
                EquipTool(wep.ToolPrefab, slot);
            }
        } else
        {
            LocallyEquip(EquippedWeapons[slot].ToolPrefab);
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void EquipWeaponLocallyRpc(int slot, string wep)
    {
        if (IsServer) return;
        CurrentToolID = -9;
        if (wep == null) EquipWeapon(slot, null);
        else EquipWeapon(slot, Resources.Load<ScriptableEquippableWeapon>($"OBJ/SCRIPTABLES/ITEMS/Weapons/{wep}"));
    }
    [Rpc(SendTo.Server)]
    public void EquipWeaponRpc(int slot, string wep)
    {
        CurrentToolID = -9;
        if (wep == null) EquipWeapon(slot, null);
        EquipWeapon(slot, Resources.Load<ScriptableEquippableWeapon>($"OBJ/SCRIPTABLES/ITEMS/Weapons/{wep}"));
    }
    public void EquipArmor(ScriptableEquippableArtifact wep)
    {
        if (EquippedArmor == wep) return;
        if (EquippedArmor)
        {
            UnapplyArtifact(EquippedArmor);
            if (IsServer) RemoveArtifactAbility(GetArtifactAbility(EquippedArmor));
        }
        EquippedArmor = wep;
        if (EquippedArmor)
        {
            ApplyArtifact(EquippedArmor);
            if (IsServer) AddArtifactAbility(GetArtifactAbility(EquippedArmor));
        }
        if (IsServer)
        {
            if (wep == null) EquipArmorLocallyRpc("");
            else EquipArmorLocallyRpc(wep.GetItemResourceIDShort());
        }
    }

    private void ApplyArtifact(ScriptableEquippableArtifact wep)
    {
        // Apply all flat modifiers
        ModifyHealthMax += wep.ModifyHealthMax;
        ModifyHealthRegen += wep.ModifyHealthRegen;
        ModifyStaminaMax += wep.ModifyStaminaMax;
        ModifyStaminaRegen += wep.ModifyStaminaRegen;
        ModifyMovementSpeed += wep.ModifyMovementSpeed;

        ModifyMeleeDamage += wep.ModifyMeleeDamage;
        ModifyRangedDamage += wep.ModifyRangedDamage;
        ModifySpellDamage += wep.ModifySpellDamage;

        // Apply array modifiers if applicable
        if (wep.ModifyAttributes != null && ModifyAttributes != null)
        {
            int len = Mathf.Min(ModifyAttributes.Length, wep.ModifyAttributes.Length);
            for (int i = 0; i < len; i++)
                ModifyAttributes[i] += wep.ModifyAttributes[i];
        }
    }

    private void UnapplyArtifact(ScriptableEquippableArtifact wep)
    {
        // Remove all flat modifiers
        ModifyHealthMax -= wep.ModifyHealthMax;
        ModifyHealthRegen -= wep.ModifyHealthRegen;
        ModifyStaminaMax -= wep.ModifyStaminaMax;
        ModifyStaminaRegen -= wep.ModifyStaminaRegen;
        ModifyMovementSpeed -= wep.ModifyMovementSpeed;

        ModifyMeleeDamage -= wep.ModifyMeleeDamage;
        ModifyRangedDamage -= wep.ModifyRangedDamage;
        ModifySpellDamage -= wep.ModifySpellDamage;

        // Remove array modifiers if applicable
        if (wep.ModifyAttributes != null && ModifyAttributes != null)
        {
            int len = Mathf.Min(ModifyAttributes.Length, wep.ModifyAttributes.Length);
            for (int i = 0; i < len; i++)
                ModifyAttributes[i] -= wep.ModifyAttributes[i];
        }
    }

    [Rpc(SendTo.Server)]
    public void EquipArmorRpc(string wep)
    {
        if (wep == null) EquipArmor(null);
        EquipArmor(Resources.Load<ScriptableEquippableArtifact>($"OBJ/SCRIPTABLES/ITEMS/Armor/{wep}"));
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void EquipArmorLocallyRpc(string wep)
    {
        if (IsServer) return;
        if (wep == null) EquipArmor(null);
        else EquipArmor(Resources.Load<ScriptableEquippableArtifact>($"OBJ/SCRIPTABLES/ITEMS/Armor/{wep}"));
    }
    public void EquipArtifact(int slot, ScriptableEquippableArtifact wep)
    {
        if (EquippedArtifacts[slot] == wep) return;
        if (EquippedArtifacts[slot])
        {
            UnapplyArtifact(EquippedArtifacts[slot]);
            if (IsServer) RemoveArtifactAbility(GetArtifactAbility(EquippedArtifacts[slot]));
        }
        EquippedArtifacts[slot] = wep;
        if (EquippedArtifacts[slot])
        {
            ApplyArtifact(EquippedArtifacts[slot]);
            if (IsServer) AddArtifactAbility(GetArtifactAbility(EquippedArtifacts[slot]));
        }
        if (IsServer)
        {
            if (wep == null) EquipArtifactLocallyRpc(slot, "");
            else EquipArtifactLocallyRpc(slot, $"OBJ/SCRIPTABLES/ITEMS/Artifacts/{wep}");
        }
    }
    [Rpc(SendTo.Server)]
    public void EquipArtifactRpc(int slot, string wep)
    {
        if (wep == null) EquipArtifact(slot, null);
        EquipArtifact(slot, Resources.Load<ScriptableEquippableArtifact>($"OBJ/SCRIPTABLES/ITEMS/Artifacts/{wep}"));
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void EquipArtifactLocallyRpc(int slot, string wep)
    {
        if (IsServer) return;
        if (wep == null) EquipArtifact(slot, null);
        else EquipArtifact(slot, Resources.Load<ScriptableEquippableArtifact>($"OBJ/SCRIPTABLES/ITEMS/Artifacts/{wep}"));
    }
    public void Die()
    {
        Alive.Value = false;
        BleedingTime.Value = 60;
        EquipWeapon1Rpc();
        setAnimationRpc(ANIM.AnimationState.MI_DEAD1);
        SetColorDeadRpc();
    }
    public void AddStamina(float fl)
    {
        CurStamina.Value = Mathf.Min(GetMaxStamina(), CurStamina.Value + fl);
    }

    public float GetMaxStamina()
    {
        return 80 + GetATT_DEXTERITY() * 5 + GetATT_MEDICAL() * 5 + ModifyStaminaMax;
    }
    private bool IsPlayerControlled()
    {
        return PlayerController.Value > 0;
    }
    public float GetSpeed()
    {
        return MovementSpeed * (0.8f+GetATT_DEXTERITY()*0.08f) * (1f+ ModifyMovementSpeed) / (1f + ModifyMovementSlow);
    }
    public float GetCurrentSpeed()
    {
        if (!isMoving.Value || !CanFunction()) return 0;
        return GetSpeed();
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
    public int GetFaction()
    {
        return Faction.Value;
    }
    public bool CanBeTargeted(SPACE space)
    {
        if (space != Space) return false;
        return !isDead();
    } 
    // Returns true if there is a clear line of sight to the target position (no obstacles in between)
    public bool HasLineOfSight(Vector3 target)
    {
        Vector3 origin = transform.position;
        Vector2 direction = (target - origin).normalized;
        float distance = (target - origin).magnitude;

        // Raycast to check for obstacles
        RaycastHit2D[] hit = Physics2D.RaycastAll(origin, direction, distance);
        foreach (RaycastHit2D h in hit)
        {
            if (h.collider.gameObject.tag.Equals("LOSBlocker"))
            {
                // Hit something that is not self and not a trigger, so line of sight is blocked
                return false;
            }
        }
        return true;
    }
}
