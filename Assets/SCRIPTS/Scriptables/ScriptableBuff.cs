
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableBuff", order = 1)]
public class ScriptableBuff : ScriptableObject
{
    public int MaxStacks;
    public float Duration;

    public float ModifyHealthMax;
    public float ModifyHealthRegen;
    public float ModifyStaminaMax;
    public float ModifyStaminaRegen;
    public float ModifyMovementSpeed;
    public float ModifyMovementSlow;
    public float ModifyAnimationSpeed;
    public float ModifyAnimationSlow;

    public float ModifyMeleeDamage;
    public float ModifyRangedDamage;
    public float ModifySpellDamage;
}