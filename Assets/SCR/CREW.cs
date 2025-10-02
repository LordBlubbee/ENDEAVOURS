using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class CREW : NetworkBehaviour, iDamageable
{
    private Rigidbody2D Rigid;
    private Collider2D Col;
    public SpriteRenderer Spr;

    private ANIM AnimationController;
    private List<AnimTransform> AnimTransforms = new List<AnimTransform>();
    private ANIM.AnimationState animDefaultIdle = ANIM.AnimationState.MI_IDLE;
    private ANIM.AnimationState animDefaultMove = ANIM.AnimationState.MI_MOVE;

    private bool UseItem1_Input = false;
    private bool UseItem2_Input = false;
    private float NextDashCost = 30;

    [NonSerialized] public NetworkVariable<int> PlayerController = new(); //0 = No Player Controller
    public int Faction = 0;

    [Header("STATS")]
    public float MaxHealth = 100f;
    public float NaturalHealthRegen = 0.1f;
    public float NaturalStaminaRegen = 25f;
    public float MovementSpeed = 7;
    public float RotationDistanceFactor = 4;
    public float RotationBaseSpeed = 90;
    public float GrappleCooldown = 10f;

    [Header("ATTRIBUTES")]
    [NonSerialized] public NetworkVariable<int> SkillPoints = new NetworkVariable<int>(0); //Not used in initial character creation
    public NetworkVariable<int> ATT_PHYSIQUE = new NetworkVariable<int>(2); //Buffs maximum health, melee damage
    public NetworkVariable<int> ATT_ARMS = new NetworkVariable<int>(2); //Buffs ranged damage, reload (+gunnery)
    public NetworkVariable<int> ATT_DEXTERITY = new NetworkVariable<int>(2);//Buffs movement speed, stamina
    public NetworkVariable<int> ATT_COMMUNOPATHY = new NetworkVariable<int>(2);//Buffs magic damage, camera range, senses
    public NetworkVariable<int> ATT_PILOTING = new NetworkVariable<int>(2); //Buffs piloting maneuverability, dodge chance
    public NetworkVariable<int> ATT_ENGINEERING = new NetworkVariable<int>(2); //Buffs repair speed
    public NetworkVariable<int> ATT_GUNNERY = new NetworkVariable<int>(2); //Buffs usage of large heavy weapons
    public NetworkVariable<int> ATT_MEDICAL = new NetworkVariable<int>(2); //Buffs healing abilities (+regeneration)
    [NonSerialized] public ScriptableBackground CharacterBackground;
    public ScriptableEquippableWeapon[] EquippedWeapons = new ScriptableEquippableWeapon[3];
    public ScriptableEquippableArtifact EquippedArmor = null;
    public ScriptableEquippableArtifact[] EquippedArtifacts = new ScriptableEquippableArtifact[3];

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
        ATT_PILOTING.Value = pil; //Buffs piloting maneuverability, dodge chance
        ATT_ENGINEERING.Value = eng; //Buffs repair speed
        ATT_GUNNERY.Value = gun; //Buffs usage of large heavy weapons
        ATT_MEDICAL.Value = med; //Buffs healing abilities (+regeneration)
        CurHealth.Value = GetMaxHealth();
    }
    // ==========================
    // GET FUNCTIONS
    // ==========================
    public void SetCharacterBackground(ScriptableBackground back)
    {
        CharacterBackground = back;
    }
    public int GetATT_PHYSIQUE()
    {
        if (CharacterBackground) return ATT_PHYSIQUE.Value + CharacterBackground.Background_ATT_BONUS[0];
        return ATT_PHYSIQUE.Value;
    }

    public int GetATT_ARMS()
    {
        if (CharacterBackground) return ATT_ARMS.Value + CharacterBackground.Background_ATT_BONUS[1];
        return ATT_ARMS.Value;
    }

    public int GetATT_DEXTERITY()
    {
        if (CharacterBackground) return ATT_DEXTERITY.Value + CharacterBackground.Background_ATT_BONUS[2];
        return ATT_DEXTERITY.Value;
    }

    public int GetATT_COMMUNOPATHY()
    {
        if (CharacterBackground) return ATT_COMMUNOPATHY.Value + CharacterBackground.Background_ATT_BONUS[3];
        return ATT_COMMUNOPATHY.Value;
    }

    public int GetATT_PILOTING()
    {
        if (CharacterBackground) return ATT_PILOTING.Value + CharacterBackground.Background_ATT_BONUS[4];
        return ATT_PILOTING.Value;
    }

    public int GetATT_ENGINEERING()
    {
        if (CharacterBackground) return ATT_ENGINEERING.Value + CharacterBackground.Background_ATT_BONUS[5];
        return ATT_ENGINEERING.Value;
    }

    public int GetATT_GUNNERY()
    {
        if (CharacterBackground) return ATT_GUNNERY.Value + CharacterBackground.Background_ATT_BONUS[6];
        return ATT_GUNNERY.Value;
    }

    public int GetATT_MEDICAL()
    {
        if (CharacterBackground) return ATT_MEDICAL.Value + CharacterBackground.Background_ATT_BONUS[7];
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
            case 4: ATT = ATT_PILOTING; break;
            case 5: ATT = ATT_ENGINEERING; break;
            case 6: ATT = ATT_GUNNERY; break;
            case 7: ATT = ATT_MEDICAL; break;
            default: return 0; // Invalid ID, do nothing
        }
        return ATT.Value;
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
            case 4: ATT = ATT_PILOTING; break;
            case 5: ATT = ATT_ENGINEERING; break;
            case 6: ATT = ATT_GUNNERY; break;
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
            case 7:
                LevelNeed = 6;
                break;
            case 8:
                LevelNeed = 7;
                break;
            case 9:
                LevelNeed = 8;
                break;
            default:
                LevelNeed = 999;
                break;
        }
        if (SkillPoints.Value < LevelNeed) return;
        SkillPoints.Value--;
        ATT.Value++;
        CO.co.UpdateATTUIRpc();
    }


    [Header("ITEMS")]
    public CO_SPAWNER.ToolType StartingTool;

    [NonSerialized] public NetworkVariable<CO_SPAWNER.ToolType> EquippedTool = new NetworkVariable<CO_SPAWNER.ToolType>(
        CO_SPAWNER.ToolType.NONE, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [NonSerialized] public NetworkVariable<FixedString64Bytes> CharacterName = new();
    [NonSerialized] public NetworkVariable<Vector3> CharacterNameColor = new();

    private GamerTag CharacterNameTag;

    public TOOL EquippedToolObject { protected set; get; }

    private NetworkVariable<float> CurHealth = new();
    private NetworkVariable<float> CurStamina = new();
    private NetworkVariable<float> CurGrappleCooldown = new();

    private NetworkVariable<bool> Alive = new();

    public bool isDead()
    {
        return !Alive.Value;
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
    public SPACE Space { get { return CO.co.GetSpace(SpaceID.Value); } set { } }

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
        CharacterNameTag.SetPlayerAndName(transform, CharacterName.Value.ToString(), new Color(CharacterNameColor.Value.x, CharacterNameColor.Value.y, CharacterNameColor.Value.z));
        //
        StartCoroutine(PeriodicUpdates());
        if (IsServer) {

            Alive.Value = true;
            CurHealth.Value = GetMaxHealth();
            CurStamina.Value = 100;

            EquipTool(StartingTool);
        } else
        {
            LocallyEquip(EquippedTool.Value);
        }
    }

    IEnumerator PeriodicUpdates()
    {
        while (true)
        {
            CharacterNameTag.SetPlayerAndName(transform, CharacterName.Value.ToString(), new Color(CharacterNameColor.Value.x, CharacterNameColor.Value.y, CharacterNameColor.Value.z));
            yield return new WaitForSeconds(2);
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
        CO.co.GetLOCALCO(PlayerController.Value).SetPlayerObject(this);
    }
    public void DespawnAndUnregister()
    {
        CO.co.UnregisterCrew(this);
        NetworkObject.Despawn();
    }

    /*Decide Inputs*/

    private Vector3 MoveInput;
    private Vector3 LookTowards;
    private bool isMoving = false;
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
        isMoving = mov != Vector3.zero;
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
        UseItem1_Input = true;
    }
    [Rpc(SendTo.Server)]
    public void StopItem1Rpc()
    {
        UseItem1_Input = false;
    }
    [Rpc(SendTo.Server)]
    public void StopItem2Rpc()
    {
        UseItem1_Input = false;
    }
    [Rpc(SendTo.Server)]
    public void UseGrappleRpc(Vector3 trt)
    {
        UseGrapple(trt);
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

    IEnumerator GrappleNum(Transform trt)
    {
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
        while (CurGrappleCooldown.Value > 0f)
        {
            CurGrappleCooldown.Value -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
    }

    public void AddToSpace(SPACE newSpace)
    {
        newSpace.AddCrew(this);
        AddToSpaceRpc(newSpace.SpaceID.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddToSpaceRpc(int spaceID)
    {
        CO.co.GetSpace(spaceID).AddCrew(this);
    }
    public void UseGrapple(SPACE newSpace)
    {
        if (GetGrappleCooldown() > 0f) return;
        if (Space) Space.RemoveCrew(this);
        AnimationComboWeapon1 = 0;
        AnimationComboWeapon2 = 0;
        AddToSpace(newSpace);
        CurGrappleCooldown.Value = GrappleCooldown / 0.8f + (GetATT_ARMS() * 0.1f);
        StartCoroutine(GrappleNum(newSpace.GetNearestBoardingGridTransformToPoint(transform.position).transform));
    }
    public void UseGrapple(Vector3 trt)
    {
        if (GetGrappleCooldown() > 0f) return;
        foreach (Collider2D col in Physics2D.OverlapCircleAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.1f))
        {
            WalkableTile tile = col.GetComponent<WalkableTile>();
            if (tile && tile.Space != Space)
            {
                UseGrapple(tile.Space);
                return;
            }
        }
    }
    /*Use Inputs*/
    public void Dash()
    {
        if (!isMoving) return;
        if (isDashing) return;
        if (!CanFunction()) return;
        if (!ConsumeStamina(NextDashCost)) return;
        NextDashCost = GetDashCost();
        AnimationComboWeapon1 = 0;
        AnimationComboWeapon2 = 0;
        StartCoroutine(DashNumerator());
        setAnimationRpc(ANIM.AnimationState.MI_DASH);
    }

    bool isDashing = false;
    IEnumerator DashNumerator()
    {
        isDashing = true;
        float Speed = 1f;
        Vector3 dir = MoveInput;
        while (Speed > 0f && UsePhysics())
        {
            float mov = (5f+ GetATT_DEXTERITY() * 0.3f) * MovementSpeed * Speed * CO.co.GetWorldSpeedDeltaFixed();

            transform.position += mov * dir;
            Rigid.MovePosition(transform.position);

            Speed -= CO.co.GetWorldSpeedDeltaFixed() * 3f;
            yield return new WaitForFixedUpdate();
        }
        isDashing = false;
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
        SelectedWeaponAbility = 0;
        if (AnimationComboWeapon1 >= EquippedToolObject.attackAnimations1.Count)
        {
            AnimationComboWeapon1 = 0;
        }
        if (EquippedToolObject.ActionUse1 != TOOL.ToolActionType.MELEE_ATTACK && !canStrike) return;
        if (GetStamina() < 2) return;
        AnimationComboWeapon2 = 0;
        setAnimationToClientsOnlyRpc(EquippedToolObject.attackAnimations1[AnimationComboWeapon1]);
        if (!setAnimationLocally(EquippedToolObject.attackAnimations1[AnimationComboWeapon1], 3)) return;
        canStrikeMelee = true;
        ConsumeStamina(2f);
        AnimationComboWeapon1++;
    }
    public void UseItem2()
    {
        if (!CanFunction()) return;
        if (EquippedToolObject == null) return;
        SelectedWeaponAbility = 1;
        if (AnimationComboWeapon2 >= EquippedToolObject.attackAnimations2.Count)
        {
            AnimationComboWeapon2 = 0;
        }
        if (EquippedToolObject.ActionUse2 != TOOL.ToolActionType.MELEE_ATTACK && !canStrike) return;
        if (GetStamina() < 2) return;
        AnimationComboWeapon1 = 0;
        setAnimationToClientsOnlyRpc(EquippedToolObject.attackAnimations2[AnimationComboWeapon2]);
        if (setAnimationLocally(EquippedToolObject.attackAnimations2[AnimationComboWeapon2],3)) return;
        canStrikeMelee = true;
        ConsumeStamina(2f);
        AnimationComboWeapon2++;
    }

    private float LastStaminaUsed = 0f;
    private void Update()
    {
        AnimationUpdate();
        if (!IsServer) return;
        StrikeUpdate();
        if (GetStamina() < 100)
        {
            float StaminaFactor = isMoving ? 1 : 2;
            LastStaminaUsed += CO.co.GetWorldSpeedDelta() * StaminaFactor;
           
            if (LastStaminaUsed > 2f)
            {
                AddStamina(CO.co.GetWorldSpeedDelta() * GetStaminaRegen() * StaminaFactor);
            }
        }
        if (UseItem1_Input) UseItem1();
        if (UseItem2_Input) UseItem2();

        DamageHealingUpdate();
    }
    private void StrikeUpdate()
    {
        if (EquippedToolObject == null) return;
        if (EquippedToolObject.strikePoints.Count == 0) return;
        if (!canStrikeMelee) return;
        if (AnimationController.isCurrentlyStriking())
        {
            foreach (Transform hitTrans in EquippedToolObject.strikePoints)
            {
                Vector3 checkHit = hitTrans.position;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(checkHit, 0.3f))
                {
                    iDamageable crew = col.GetComponent<iDamageable>();
                    if (crew != null)
                    {
                        if (crew.GetFaction() == Faction) return;
                        if (crew.Space != Space) return; 
                        if (!crew.CanBeTargeted()) return;
                        float dmg = SelectedWeaponAbility == 0 ? EquippedToolObject.attackDamage1 : EquippedToolObject.attackDamage2;
                        dmg *= AnimationController.CurrentStrikePower();
                        crew.TakeDamage(dmg, checkHit);
                        canStrikeMelee = false;
                        return;
                    }
                }
            }
            
        }
    }

    bool canStrikeMelee = true;
    bool canStrike = true;
    IEnumerator AttackCooldown(float col)
    {
        canStrike = false;
        while (col > 0f)
        {
            col -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        canStrike = true;
    }
    private void AnimationUpdate()
    {
        AnimationController.animationFrame();
        if (IsServer)
        {
            if (AnimationController.getAnimationMoveForward() != 0)
            {
                transform.position += AnimationController.getAnimationMoveForward() * getLookVector() * CO.co.GetWorldSpeedDelta();
            }
        }
    }
    public bool setAnimationLocally(ANIM.AnimationState stat, int pr = 99)
    {
        return AnimationController.setAnimation(stat, pr);
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
    private void FixedUpdate()
    {
        //if (!IsServer) return;
        if (!CanFunction()) return;
        if (isLooking)
        {
            float ang = AngleToTurnTarget();
            if (ang > 1f)
            {
                transform.Rotate(Vector3.forward, (Mathf.Abs(ang) * RotationDistanceFactor + RotationBaseSpeed) * CO.co.GetWorldSpeedDeltaFixed());
                ang = AngleToTurnTarget();
                if (AngleToTurnTarget() < 0f)
                {
                    transform.Rotate(Vector3.forward, ang);
                    isLooking = false;
                }
            }
            else if (ang < -1f)
            {
                transform.Rotate(Vector3.forward, -(Mathf.Abs(ang) * RotationDistanceFactor + RotationBaseSpeed) * CO.co.GetWorldSpeedDeltaFixed());
                ang = AngleToTurnTarget();
                if (AngleToTurnTarget() > 0f)
                {
                    transform.Rotate(Vector3.forward, ang);
                    isLooking = false;
                }
            } else
            {
                isLooking = false;
            }
        }
        if (!UsePhysics()) return;
        if (isMoving)
        {
            if (IsServer) setAnimationRpc(animDefaultMove, 1);
            float towardsang = Mathf.Abs(AngleTowards(MoveInput));
            float towardsfactor = 1.1f - Mathf.Clamp((towardsang-70f)*0.005f,0,0.5f); //The more you look in the correct direction, the faster you move!
            transform.position += MoveInput * GetSpeed() * towardsfactor * CO.co.GetWorldSpeedDeltaFixed();
            Rigid.MovePosition(transform.position);
            //Rigid.MovePosition(transform.position + MoveInput * GetSpeed() * towardsfactor * CO.co.GetWorldSpeedDelta());
        } else
        {
            if (IsServer) setAnimationRpc(animDefaultIdle, 1);
        }
    }

    /*WEAPONS*/
    public void EquipWeaponPrefab(int ID)
    {
        //Works on Server
        if (EquippedWeapons[ID]) EquipTool(EquippedWeapons[ID].ToolPrefab);
        else EquipTool(CO_SPAWNER.ToolType.NONE);
        EquipWeaponUpdateUIRpc(ID);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EquipWeaponUpdateUIRpc(int ID)
    {
        //Works on Owner
        if (LOCALCO.local.GetPlayer() != this) return;
        UI.ui.MainGameplayUI.EquipWeaponUI(ID);
    }
    public void EquipTool(CO_SPAWNER.ToolType tol)
    {
        //Works on Server
        EquippedTool.Value = tol;

        EquipToolLocallyRpc(tol); //Send to Clients
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EquipToolLocallyRpc(CO_SPAWNER.ToolType tol)
    {
        LocallyEquip(tol);
    }

    private void LocallyEquip(CO_SPAWNER.ToolType tol)
    {
        //Works on Client
        if (!hasInitialized)
        {
            Init();
            return;
        }
        if (EquippedToolObject)
        {
            Destroy(EquippedToolObject.gameObject);
        }
        if (tol == CO_SPAWNER.ToolType.NONE)
        {
            EquippedToolObject = null;
            return;
        }
        EquippedToolObject = Instantiate(CO_SPAWNER.co.ToolPrefabs[tol], transform);
        EquippedToolObject.transform.localPosition = new Vector3(EquippedToolObject.localX, EquippedToolObject.localY, -0.0002f);
        EquippedToolObject.transform.Rotate(Vector3.forward, EquippedToolObject.localRot);
        AnimTransforms[1].setTransform(EquippedToolObject.transform);
        Debug.Log(EquippedToolObject);
    }

    /*GETTERS AND SETTERS*/
    public float GetHealth()
    {
        return CurHealth.Value;
    }
    public float GetMaxHealth()
    {
        return MaxHealth * (0.8f + 0.1f * GetATT_PHYSIQUE());
    }
    public float GetHealthRegen()
    {
        return NaturalHealthRegen * (0.8f + 0.1f * GetATT_MEDICAL());
    }
    public float GetStaminaRegen()
    {
        return NaturalStaminaRegen * (0.8f + 0.1f * GetATT_DEXTERITY());
    }
    public float GetDashCost()
    {
        return UnityEngine.Random.Range(27f, 37f)-GetATT_DEXTERITY();
    }
    public float GetHealthRelative()
    {
        return GetHealth() / GetMaxHealth();
    }
    public float GetStamina()
    {
        return CurStamina.Value;
    }
    public bool ConsumeStamina(float fl)
    {
        if (fl > CurStamina.Value) return false;
        LastStaminaUsed = 0f;
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
        if (GetHealth() < GetMaxHealth())
        {
            Heal(GetHealthRegen() * CO.co.GetWorldSpeedDelta());
        }
    }
    public void Heal(float fl)
    {
        CurHealth.Value = Mathf.Min(GetMaxHealth(), CurHealth.Value + fl);
    }
    public void TakeDamage(float fl, Vector3 src)
    {
        if (isDashing) return;
        lastDamageTime = 7f;
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            CurHealth.Value = 0f;
            Die();
            //Death
        }
        CO_SPAWNER.co.SpawnDMGRpc(fl, src);
    }

    public void EquipWeapon(int slot, ScriptableEquippableWeapon wep)
    {
        EquippedWeapons[slot] = wep;
        if (IsServer)
        {
            if (wep == null)
            {
                EquipWeaponLocallyRpc(slot, ""); 
                EquipTool(CO_SPAWNER.ToolType.NONE);
            }
            else
            {
                EquipWeaponLocallyRpc(slot, wep.ItemResourceID);
                EquipTool(wep.ToolPrefab);
            }
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void EquipWeaponLocallyRpc(int slot, string wep)
    {
        if (IsServer) return;
        if (wep == null) EquipWeapon(slot, null);
        else EquipWeapon(slot, Resources.Load<ScriptableEquippableWeapon>(wep));
    }
    [Rpc(SendTo.Server)]
    public void EquipWeaponRpc(int slot, string wep)
    {
        if (wep == null) EquipWeapon(slot, null);
        EquipWeapon(slot, Resources.Load<ScriptableEquippableWeapon>(wep));
    }
    public void EquipArmor(ScriptableEquippableArtifact wep)
    {
        EquippedArmor = wep;
        if (IsServer)
        {
            if (wep == null) EquipArmorLocallyRpc("");
            else EquipArmorLocallyRpc(wep.ItemResourceID);
        }
    }

    [Rpc(SendTo.Server)]
    public void EquipArmorRpc(string wep)
    {
        if (wep == null) EquipArmor(null);
        EquipArmor(Resources.Load<ScriptableEquippableArtifact>(wep));
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void EquipArmorLocallyRpc(string wep)
    {
        if (IsServer) return;
        if (wep == null) EquipArmor(null);
        else EquipArmor(Resources.Load<ScriptableEquippableArtifact>(wep));
    }
    public void EquipArtifact(int slot, ScriptableEquippableArtifact wep)
    {
        EquippedArtifacts[slot] = wep;
        if (IsServer)
        {
            if (wep == null) EquipArtifactLocallyRpc(slot, "");
            else EquipArtifactLocallyRpc(slot, wep.ItemResourceID);
        }
    }
    [Rpc(SendTo.Server)]
    public void EquipArtifactRpc(int slot, string wep)
    {
        if (wep == null) EquipArtifact(slot, null);
        EquipArtifact(slot, Resources.Load<ScriptableEquippableArtifact>(wep));
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void EquipArtifactLocallyRpc(int slot, string wep)
    {
        if (IsServer) return;
        if (wep == null) EquipArtifact(slot, null);
        else EquipArtifact(slot, Resources.Load<ScriptableEquippableArtifact>(wep));
    }
    public void Die()
    {
        Alive.Value = false;
        setAnimationRpc(ANIM.AnimationState.MI_DEAD1, 1);
    }
    public void AddStamina(float fl)
    {
        CurStamina.Value = Mathf.Min(100, CurStamina.Value + fl);
    }
    private bool IsPlayerControlled()
    {
        return PlayerController.Value > 0;
    }
    public float GetSpeed()
    {
        return MovementSpeed * (0.7f+GetATT_DEXTERITY()*0.1f);
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
        return Faction;
    }
    public bool CanBeTargeted()
    {
        return !isDead();
    }
}
