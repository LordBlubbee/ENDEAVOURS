
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableEnemyCrew", order = 1)]
public class ScriptableEnemyCrew : ScriptableObject
{
    //This spawns a group of enemies
    public CREW SpawnCrew;

    public int Worth;

    public float HealthIncreasePerLevelup;
    public float[] PointIncreasePerLevelup;
}