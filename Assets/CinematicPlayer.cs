using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicPlayer : MonoBehaviour
{
    public SceneObject PrefabSceneObject;
    public Image MainScreen;
    public Image FadeScreen;
    float TransitionSpeed;
    List<SceneObject> SceneObjects = new();
    ScriptableScene TargetScene;
    ScriptableScene CurrentScene;
    public void SetScene(ScriptableScene scene, float Transition)
    {
        TargetScene = scene;
        TransitionSpeed = Transition;
        if (TransitionSpeed <= 0) ChangeScene();
    }
    private void Update()
    {
        if (TargetScene != CurrentScene)
        {
            FadeScreen.color = new Color(0,0,0, Mathf.Clamp01(FadeScreen.color.a + Time.deltaTime * TransitionSpeed));
            if (FadeScreen.color.a >= 1)
            {
                ChangeScene();
            }
        } else
        {
            FadeScreen.color = new Color(0, 0, 0, Mathf.Clamp01(FadeScreen.color.a - Time.deltaTime * TransitionSpeed));
        }
    }
    private void ChangeScene()
    {
        foreach (SceneObject ob in SceneObjects)
        {
            Destroy(ob.gameObject);
        }
        SceneObjects = new();
        CurrentScene = TargetScene;
        if (TargetScene != null)
        {
            foreach (ScriptableSceneObject ob in TargetScene.Objects)
            {
                SceneObject newOb = Instantiate(PrefabSceneObject, MainScreen.transform);
                newOb.Init(ob);
                SceneObjects.Add(newOb);
            }
        }
    }
}
