using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PrerequisiteBackground", order = 1)]
public class ScriptablePrerequisiteBackground : ScriptablePrerequisite
{
    public List<ScriptableBackground> Backgrounds;
    public override bool IsTrue()
    {
        foreach (CREW crew in CO.co.GetAlliedCrew())
        {
            if (Backgrounds.Contains(crew.CharacterBackground)) return true;
        }
        return false;
    }
}
