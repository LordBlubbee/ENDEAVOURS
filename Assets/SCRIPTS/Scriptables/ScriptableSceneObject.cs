
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CinematicSceneObject", order = 1)]
public class ScriptableSceneObject : ScriptableObject
{
    public List<Sprite> SpriteList;
    public List<SpriteKeyframe> SpriteKeyframes;
    public List<ScaleKeyframe> ScaleKeyframes;
    public List<PositionKeyframe> PositionKeyframes;
    public List<RotationKeyframe> RotationKeyframes;
}
[Serializable]
public class SpriteKeyframe
{
    public List<Sprite> List;
    public float Time;
}
[Serializable]
public class ScaleKeyframe
{
    public float Scale;
    public bool StartIntense = false;
    public bool EndIntense = false;
    public float Time;
}
[Serializable]
public class RotationKeyframe
{
    public float Rotation;
    public bool StartIntense = false;
    public bool EndIntense = false;
    public float Time;
}
[Serializable]
public class PositionKeyframe
{
    public Vector3 Vector;
    public bool StartIntense = false;
    public bool EndIntense = false;
    public float Time;
}