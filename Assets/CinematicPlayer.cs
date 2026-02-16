using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicPlayer : MonoBehaviour
{
    public SceneObject PrefabSceneObject;
    public Image MainScreen;
    public Image FadeScreen;
    List<SceneObject> SceneObjects;
    ScriptableScene TargetScene;
    ScriptableScene CurrentScene;
    public void SetScene(ScriptableScene scene)
    {
        TargetScene = scene;
    }
    private void Update()
    {
        if (TargetScene != CurrentScene)
        {
            FadeScreen.color = new Color(0,0,0, Mathf.Clamp01(FadeScreen.color.a + Time.deltaTime * 2f));
            if (FadeScreen.color.a >= 1)
            {
                ChangeScene();
            }
        } else
        {
            FadeScreen.color = new Color(0, 0, 0, Mathf.Clamp01(FadeScreen.color.a - Time.deltaTime * 2f));
        }
    }
    private void ChangeScene()
    {
        foreach (SceneObject ob in SceneObjects)
        {
            Destroy(ob.gameObject);
        }
        CurrentScene = TargetScene;
        foreach (ScriptableSceneObject ob in TargetScene.Objects)
        {
            Instantiate(PrefabSceneObject, MainScreen.transform).Init(ob);
        }
    }
}
