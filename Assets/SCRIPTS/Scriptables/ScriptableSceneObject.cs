
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CinematicSceneObject", order = 1)]
public class ScriptableSceneObject : ScriptableObject
{
    public List<Sprite> SpriteList; 
    public List<Keyframe> ScaleKeyframes;
    public List<Keyframe> PositionKeyframes;
}
[Serializable]
public class Keyframe
{
    public Vector3 Vector;
    public float Time;
}