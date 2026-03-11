using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MasteryItem", order = 1)]
public class ScriptableMasteryItem : ScriptableObject
{
    public string ItemName;
    [TextArea(3, 10)]
    public string ItemDesc;
    public Sprite ItemIcon;
    public string GetItemResourceIDFull()
    {
        return $"OBJ/SCRIPTABLES/MASTERY/{name}";
    }
    public string GetItemResourceIDShort()
    {
        return name;
    }

    public BUFF AssociatedBuff;
    public MasteryAbilityTypes Ability = MasteryAbilityTypes.NONE;
    public GameObject AbilityPrefab1;
    public enum MasteryAbilityTypes
    {
        NONE,
        KNUCKLE_RAGE,
        RED_POWDER,
        CANDLE_BLAST,
        SHAMANIC_HEAL,
        GUARDIAN_ARMOR,
        TOKEN_OF_VENGEANCE,
        STIPULATION_OF_RIGHTS,
        ENFORCEMENT_ORDER,
        SKIRMISH_ORDER,
        STEEL_INSCRIPTION
    }
}
