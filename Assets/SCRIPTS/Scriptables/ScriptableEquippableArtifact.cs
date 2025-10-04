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
}
