using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableModule", order = 1)]
public class ScriptableEquippableModule : ScriptableEquippable
{
    public Module PrefabModule;
    public EquipTypes EquipType;
    public enum EquipTypes
    {
        CORE, //Cannot be removed
        WEAPON,
        SYSTEM,
        LOON
    }
}
