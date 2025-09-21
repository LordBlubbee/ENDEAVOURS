using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CREW : NetworkBehaviour
{
    private Rigidbody2D Rigid;
    public SpriteRenderer Spr;

    private ANIM AnimationController;
    private List<AnimTransform> AnimTransforms = new List<AnimTransform>();
    private ANIM.AnimationState animDefaultIdle = ANIM.AnimationState.MI_IDLE;
    private ANIM.AnimationState animDefaultMove = ANIM.AnimationState.MI_MOVE;

    [NonSerialized] public NetworkVariable<int> PlayerController = new(); //0 = No Player Controller
    public int Faction = 0;

    [Header("STATS")]
    public float MaxHealth = 100f;
    public float NaturalHealthRegen = 0.1f;
    public float NaturalStaminaRegen = 25f;
    public float MovementSpeed = 7;
    public float RotationDistanceFactor = 4;
    public float RotationBaseSpeed = 90;

    [Header("ITEMS")]
    public CO_SPAWNER.ToolType StartingTool;

    [NonSerialized] public NetworkVariable<CO_SPAWNER.ToolType> EquippedTool = new NetworkVariable<CO_SPAWNER.ToolType>(
        CO_SPAWNER.ToolType.NONE, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [NonSerialized] public NetworkVariable<FixedString64Bytes> CharacterName = new();
    [NonSerialized] public NetworkVariable<Vector3> CharacterNameColor = new();

    private GamerTag CharacterNameTag;

    protected TOOL EquippedToolObject;

    private NetworkVariable<float> CurHealth = new();
    private NetworkVariable<float> CurStamina = new();

    public SPACE space { get; set; }

    bool hasInitialized = false;

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

        AnimTransforms.Add(new AnimTransform(Spr.transform));
        AnimTransforms.Add(new AnimTransform());
        AnimTransforms.Add(new AnimTransform());

        AnimationController = new ANIM(AnimTransforms);

        CharacterNameTag = Instantiate(CO_SPAWNER.co.PrefabGamerTag);
        CharacterNameTag.SetPlayerAndName(transform, CharacterName.Value.ToString(), new Color(CharacterNameColor.Value.x, CharacterNameColor.Value.y, CharacterNameColor.Value.z));
        //
        if (IsServer) {

            CurHealth.Value = MaxHealth;
            CurStamina.Value = 100;

            EquipTool(StartingTool);
        } else
        {
            LocallyEquip(EquippedTool.Value);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RegisterPlayerOnLOCALCORpc()
    {
        if (!IsPlayerControlled()) Debug.Log("Error: Is not player controlled");
        else CO.co.GetLOCALCO(PlayerController.Value).SetPlayerObject(this);
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
        UseItem1();
    }
    [Rpc(SendTo.Server)]
    public void UseItem2Rpc()
    {
        UseItem2();
    }
    /*Use Inputs*/
    public void Dash()
    {
        if (!isMoving) return;
        if (isDashing) return;
        if (!ConsumeStamina(30f)) return;
        StartCoroutine(DashNumerator());
        setAnimationRpc(ANIM.AnimationState.MI_DASH);
    }

    bool isDashing = false;
    IEnumerator DashNumerator()
    {
        isDashing = true;
        float Speed = 1f;
        Vector3 dir = MoveInput;
        while (Speed > 0f)
        {
            float mov = 6f * MovementSpeed * Speed * CO.co.GetWorldSpeedDeltaFixed();

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
        while (Speed > 0f && !isDashing)
        {
            transform.position += 2f * Power * Speed * CO.co.GetWorldSpeedDeltaFixed() * dir;
            Rigid.MovePosition(transform.position);
            Speed -= CO.co.GetWorldSpeedDeltaFixed() / Duration;
            yield return new WaitForFixedUpdate();
        }
    }

    int AnimationComboWeapon1 = 0;
    int AnimationComboWeapon2 = 0;
    public void UseItem1()
    {
        if (EquippedToolObject == null) return;
        if (AnimationComboWeapon1 >= EquippedToolObject.attackAnimations1.Count)
        {
            AnimationComboWeapon1 = 0;
        }
        AnimationComboWeapon2 = 0;
        setAnimationToClientsOnlyRpc(EquippedToolObject.attackAnimations1[AnimationComboWeapon1]);
        if (!setAnimationLocally(EquippedToolObject.attackAnimations1[AnimationComboWeapon1])) return;
        AnimationComboWeapon1++;
    }
    public void UseItem2()
    {
        if (EquippedToolObject == null) return;
        if (AnimationComboWeapon2 >= EquippedToolObject.attackAnimations2.Count)
        {
            AnimationComboWeapon2 = 0;
        }
        AnimationComboWeapon1 = 0;
        setAnimationToClientsOnlyRpc(EquippedToolObject.attackAnimations2[AnimationComboWeapon2]);
        if (setAnimationLocally(EquippedToolObject.attackAnimations2[AnimationComboWeapon2])) return;
        AnimationComboWeapon2++;
    }

    private float LastStaminaUsed = 0f;
    private void Update()
    {
        AnimationUpdate();
        if (!IsServer) return;
        if (GetStamina() < 100)
        {
            float StaminaFactor = isMoving ? 1 : 2;
            LastStaminaUsed += CO.co.GetWorldSpeedDelta() * StaminaFactor;
           
            if (LastStaminaUsed > 2f)
            {
                AddStamina(CO.co.GetWorldSpeedDelta() * NaturalStaminaRegen * StaminaFactor);
            }
        }
        if (GetHealth() < MaxHealth)
        {
            Heal(CO.co.GetWorldSpeedDelta() * NaturalHealthRegen);
        }
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
        AnimationController.setAnimation(stat, pr);
    }
    [Rpc(SendTo.NotServer)]
    public void setAnimationToClientsOnlyRpc(ANIM.AnimationState stat, int pr = 99)
    {
        AnimationController.setAnimation(stat, pr);
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;
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
        if (isMoving)
        {
            setAnimationRpc(animDefaultMove, 1);
            float towardsang = Mathf.Abs(AngleTowards(MoveInput));
            float towardsfactor = 1.1f - Mathf.Clamp((towardsang-70f)*0.005f,0,0.5f); //The more you look in the correct direction, the faster you move!
            transform.position += MoveInput * GetSpeed() * towardsfactor * CO.co.GetWorldSpeedDeltaFixed();
            Rigid.MovePosition(transform.position);
            //Rigid.MovePosition(transform.position + MoveInput * GetSpeed() * towardsfactor * CO.co.GetWorldSpeedDelta());
        } else
        {
            setAnimationRpc(animDefaultIdle, 1);
        }
    }

    /*WEAPONS*/

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
        if (EquippedToolObject)
        {
            Destroy(EquippedToolObject);
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
    }

    /*GETTERS AND SETTERS*/

    public float GetHealth()
    {
        return CurHealth.Value;
    }
    public float GetHealthRelative()
    {
        return CurHealth.Value / MaxHealth;
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
    public void Heal(float fl)
    {
        CurHealth.Value = Mathf.Min(MaxHealth, CurHealth.Value + fl);
    }
    public void TakeDamage(float fl)
    {
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            CurHealth.Value = 0f;
            //Death
        }
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
        return MovementSpeed;
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
}
