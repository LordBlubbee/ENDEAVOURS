using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PrerequisiteReputation", order = 1)]
public class ScriptablePrerequisiteReputation : ScriptablePrerequisite
{
    public int MinReputation = -999;
    public int MaxReputation = 999;
    public CO.Faction FactionID;
    public override bool IsTrue()
    {
        return CO.co.Resource_Reputation.GetValueOrDefault(FactionID) > MinReputation && CO.co.Resource_Reputation.GetValueOrDefault(FactionID) < MaxReputation;
    }
}
