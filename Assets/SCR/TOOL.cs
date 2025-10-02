using System.Collections.Generic;
using UnityEngine;

public class TOOL : MonoBehaviour
{
    public float localX;
    public float localY;
    public float localRot;

    public List<Transform> handPoints;
    public List<Transform> strikePoints;
    public List<ANIM.AnimationState> attackAnimations1;
    public List<ANIM.AnimationState> attackAnimations2;
    public ToolActionType ActionUse1;
    public ToolActionType ActionUse2;

    public float attackDamage1 = 10f;
    public float attackDamage2 = 10f;
    public enum ToolActionType
    {
        MELEE_ATTACK,
        RANGED_ATTACK,
        BLOCK,
        REPAIR
    }
}
