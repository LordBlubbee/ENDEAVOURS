using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableArtifact", order = 1)]
public class ScriptableEquippableArtifact : ScriptableEquippable
{
    public EquipTypes EquipType;
    public enum EquipTypes
    {
        ARMOR,
        ARTIFACT
    }
    public ArtifactAbilityTypes Ability = ArtifactAbilityTypes.NONE;
    public GameObject AbilityPrefab1;
    public enum ArtifactAbilityTypes
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

    public bool CanWear(CREW crew)
    {
        int i = 0;
        foreach (int num in MinimumAttributes)
        {
            if (crew.GetATT(i) >= num) return true;
            i++;
        }
        return false;
    }

    [Header("PHYS, ARM, DEX, COM, CMD, ENG, ALC, MED")]
    public int[] ModifyAttributes;
    public float ModifyHealthMax;
    public float ModifyHealthRegen;
    public float ModifyStaminaMax;
    public float ModifyStaminaRegen;
    public float ModifyMovementSpeed;

    public float ModifyMeleeDamage;
    public float ModifyRangedDamage;
    public float ModifySpellDamage;
}
