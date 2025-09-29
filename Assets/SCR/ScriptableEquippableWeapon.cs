using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableWeapon", order = 1)]
public class ScriptableEquippableWeapon : ScriptableEquippable
{
    public CO_SPAWNER.ToolType ToolPrefab;
}
