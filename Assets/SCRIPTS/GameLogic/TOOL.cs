using System.Collections.Generic;
using UnityEngine;

public class TOOL : MonoBehaviour
{
    public float localX;
    public float localY;
    public float localRot;

    public Transform Tool2;
    public List<Transform> handPoints;
    public List<Transform> strikePoints;
    public List<BlockAttacks> Blockers;
    public ToolAI AI;
    public bool isConsumable = false;
    public bool DashResetsCooldown = true;
    public ToolAI GetUsageAI()
    {
        return AI;
    }
    public enum ToolAI
    {
        MELEE,
        RANGED,
        MELEE_AND_SHIELD,
        TARGET_ALLIES,
        HEAL_ALLIES
    }

    [Header("CROSSHAIR")]
    public Sprite CrosshairSprite;
    public float CrosshairMaxRange = 999;
    public float CrosshairMinRange = -1;
    public bool RotateCrosshair = false;

    [Header("ACTION 1")]
    public ToolActionType ActionUse1;
    public List<ANIM.AnimationState> attackAnimations1;
    public float UsageStamina1 = 2f;
    public AudioClip[] Action1_SFX;
    public AudioClip[] Action1_SFX_Hit;

    public float attackDamage1 = 10f;
    public PROJ RangedPrefab1;
    public UniqueSpell UniqueSpell1;
    public float Reload1 = 0f;
    public float ExtendedCooldown1 = 0f;
    public ScriptableBuff ApplyBuff;

    [Header("ACTION 2")]
    public ToolActionType ActionUse2;
    public List<ANIM.AnimationState> attackAnimations2;
    public float UsageStamina2 = 2f;
    public AudioClip[] Action2_SFX;

    public float attackDamage2 = 10f;
    public PROJ RangedPrefab2;
    public UniqueSpell UniqueSpell2;
    public float Reload2 = 0f;
    public float ExtendedCooldown2 = 0f;

    private CREW Crew;

    public CREW GetCrew()
    {
        return Crew;
    }
    public enum ToolActionType
    {
        NONE,
        MELEE_ATTACK,
        RANGED_ATTACK,
        BLOCK,
        REPAIR,
        HEAL_OTHERS,
        HEAL_SELF,
        SPELL_ATTACK,
        MELEE_AND_BLOCK,
        UNIQUE_SPELL
    }


    public void Init(CREW crew)
    {
        Crew = crew;
    }
}
