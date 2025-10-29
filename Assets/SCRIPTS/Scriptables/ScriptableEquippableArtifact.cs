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
