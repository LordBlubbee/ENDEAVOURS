using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MasteryItem", order = 1)]
public class ScriptableMasteryItem : ScriptableObject
{
    [TextArea(2,2)]
    public string ItemName;
    [TextArea(3, 10)]
    public string ItemDesc;
    public string GetItemResourceIDFull()
    {
        return $"OBJ/SCRIPTABLES/MASTERY/{name}";
    }
    public string GetItemResourceIDShort()
    {
        return name;
    }
    
    public string AbilityID;
    public GameObject AbilityPrefab1;

    [Header("PHYS, ARM, DEX, COM, CMD, ENG, ALC, MED")]
    public int[] ModifyAttributes;
    public float ModifyHealthMax;
    public float ModifyHealthRegen;
    public float ModifyStaminaMax;
    public float ModifyStaminaRegen;
    public float ModifyMovementSpeed;

    public float ModifyHealingDone;
    public float ModifyRepairDone;

    public float ModifyMeleeDamage;
    public float ModifyRangedDamage;
    public float ModifySpellDamage;

    public float ModifyMeleeRes;
    public float ModifyRangedRes;
    public float ModifySpellRes;
    public float ModifyFireRes;
    public float ModifyMistRes;
}
