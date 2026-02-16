
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CinematicScene", order = 1)]
public class ScriptableScene : ScriptableObject
{
    public List<ScriptableSceneObject> Objects;
}