using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PrerequisiteRandom", order = 1)]
public class ScriptablePrerequisiteRandom : ScriptablePrerequisite
{
    public float Chance;
    public override bool IsTrue()
    {
        return UnityEngine.Random.Range(0f, 1f) < Chance;
    }
}
