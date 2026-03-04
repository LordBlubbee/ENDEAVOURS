
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CinematicScene", order = 1)]
public class ScriptableScene : ScriptableObject
{
    public List<ScriptableSceneObject> Objects;

    public List<SceneSFXKeyframe> AudioKeyframes;
    public List<SceneShakeKeyframe> ShakeKeyframes;
}
[Serializable]
public class SceneShakeKeyframe
{
    public float Intensity;
    public float Time;
}
[Serializable]
public class SceneSFXKeyframe
{
    public AudioClip Clip;
    public float Time;
}