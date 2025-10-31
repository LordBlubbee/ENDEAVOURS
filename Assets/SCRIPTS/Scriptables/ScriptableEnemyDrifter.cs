
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableEnemyDrifter", order = 1)]
public class ScriptableEnemyDrifter : ScriptableObject
{
    //This spawns a group of enemies
    public DRIFTER SpawnDrifter;
    public int Weight;
    public float BaseWeaponBudgetMod = 1f;
    public float BaseModuleBudgetMod = 1f;

    public List<ScriptableEquippableModule> EquippableWeapons;
    public List<ScriptableEquippableModule> EquippableModules;
    public float HullIncreasePerLevelup;
}