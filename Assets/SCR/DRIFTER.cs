using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DRIFTER : NetworkBehaviour
{
    [Header("REFERENCES")]
    public SPACE Interior;

    private Rigidbody2D Rigid;
    public SpriteRenderer Spr;
    public int Faction;

    [Header("STATS")]
    public float MaxHealth = 100f;
    public float MovementSpeed = 5;
    public float AccelerationSpeedMod = 0.25f;
    public float RotationBaseSpeed = 30f;

    private Vector3 CurrentMovement;
    private float CurrentRotation;

    private NetworkVariable<float> CurHealth = new();

    private void Start()
    {
        Init();
    }

    private bool hasInitialized = false;

    public void Init()
    {
        if (hasInitialized) return;

        hasInitialized = true;

        Rigid = GetComponent<Rigidbody2D>();
        Interior.Init();

        if (IsServer)
        {
            CurHealth.Value = MaxHealth;
        }


    }

    float PilotingEfficiency = 1f;
    private Vector3 MoveInput;
    private Vector3 LookTowards = new Vector3(1,0);

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
        MoveInput = mov;
        PilotingEfficiency = eff;
    }
    public void SetLookTowards(Vector3 mov)
    {
        LookTowards = (mov-transform.position).normalized;
    }

    public float GetRotation()
    {
        return RotationBaseSpeed * PilotingEfficiency;
    }

    public float GetMovementSpeed()
    {
        return MovementSpeed;
    }

    public float GetMovementAccel()
    {
        return AccelerationSpeedMod * PilotingEfficiency * 1.1f;
    }
    void UpdateTurn()
    {
        float ang = AngleToTurnTarget();
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
        if (rotGoal > CurrentRotation)
        {
            CurrentRotation += accelSpeed * CO.co.GetWorldSpeedDeltaFixed();
        }
        else if (rotGoal < CurrentRotation)
        {
            CurrentRotation -= accelSpeed * CO.co.GetWorldSpeedDeltaFixed();
        }
        float maxRot = Mathf.Min(GetRotation(), Mathf.Abs(ang)*2f);
        CurrentRotation = Mathf.Clamp(CurrentRotation, -maxRot, maxRot);
        transform.Rotate(Vector3.forward,CurrentRotation * CO.co.GetWorldSpeedDeltaFixed());
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        
        UpdateTurn();

        if (MoveInput == Vector3.zero)
        {
            bool XPOS = CurrentMovement.x > 0;
            bool YPOS = CurrentMovement.y > 0;
            CurrentMovement -= CurrentMovement.normalized * GetMovementAccel() * MovementSpeed * CO.co.GetWorldSpeedDeltaFixed();
            if (XPOS != CurrentMovement.x > 0 || YPOS != CurrentMovement.y > 0 || CurrentMovement.magnitude < 0.1f) CurrentMovement = Vector3.zero;
        } else
        {
            CurrentMovement += MoveInput * GetMovementAccel() * MovementSpeed * CO.co.GetWorldSpeedDeltaFixed();
            if (CurrentMovement.magnitude > MovementSpeed) CurrentMovement = CurrentMovement.normalized * MovementSpeed;
        }

        float towardsang = Mathf.Abs(AngleTowards(CurrentMovement));
        float towardsfactor = 1.2f - Mathf.Clamp((towardsang - 60f) * 0.006f, 0, 0.4f); //The more you look in the correct direction, the faster you move!
        transform.position += CurrentMovement * towardsfactor * CO.co.GetWorldSpeedDeltaFixed();
        Rigid.MovePosition(transform.position);
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
        Module nearest = Interior.NearestModule(ImpactArea);
        if (nearest != null)
        {
            if ((nearest.transform.position-ImpactArea).magnitude < nearest.HitboxRadius)
            {
                nearest.TakeDamage(fl);
            }
        }
    }
}
