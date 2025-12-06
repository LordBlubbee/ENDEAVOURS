using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PrerequisiteResource", order = 1)]
public class ScriptablePrerequisiteResource : ScriptablePrerequisite
{
    public int MinimumRequired = 25;
    public ResourceType Resource;
    public enum ResourceType
    {
        MATERIALS,
        SUPPLIES,
        AMMO,
        TECH
    }
    public override bool IsTrue()
    {
        switch (Resource)
        {
            case ResourceType.MATERIALS:
                return CO.co.Resource_Materials.Value >= MinimumRequired;
            case ResourceType.SUPPLIES:
                return CO.co.Resource_Supplies.Value >= MinimumRequired;
            case ResourceType.AMMO:
                return CO.co.Resource_Ammo.Value >= MinimumRequired;
            case ResourceType.TECH:
                return CO.co.Resource_Tech.Value >= MinimumRequired;
        }
        return false;
    }
}
