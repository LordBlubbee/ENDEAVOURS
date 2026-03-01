using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
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
        if (TargetScene == CurrentScene) CurrentScene = null;
        TargetScene = scene;
        TransitionSpeed = Transition;
        if (TransitionSpeed <= 0)
        {
            ChangeScene();
            FadeScreen.color = new Color(0, 0, 0, 0);
        }
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

    Vector3 CameraShake;
    bool isShaking = false;
    float ShakePower = 0f;
    public void ShakeCamera(float power)
    {
        ShakePower = power;
        if (!isShaking) StartCoroutine(ShakingCamera());
    }
    IEnumerator ShakingCamera()
    {
        while (ShakePower > 0f)
        {
            ShakePower -= Time.deltaTime * 1.5f * Mathf.Max((ShakePower - 1f), 1f);
            float Shake = Mathf.Clamp(ShakePower, 0, 4);
            if (ShakePower > 4f) Shake += (ShakePower - 4f) * 0.2f;
            CameraShake = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-1f, 1f)) * Shake * 0.7f;
            transform.position = CameraShake;
            yield return null;
        }
        CameraShake = Vector3.zero;
        transform.position = Vector3.zero;
    }
}
