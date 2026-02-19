using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SceneObject : MonoBehaviour
{
    private Image img;
    int CurrentImageFrame;
    private ScriptableSceneObject ObjectType;
    private List<Sprite> Sprites = new();
    float AnimationTime = 0f;
    float SwitchTime = 0f;
    float WantScale = 1f;
    Vector3 CurrentMovement = Vector3.zero;
    float CurrentScaling = 0;
    float CurrentRotation =0;
    Vector3 CurrentMovementAccel = Vector3.zero;
    float CurrentScalingAccel = 0;
    float CurrentRotationAccel = 0;
    int CurrentScaleKeyframe = 0;
    int CurrentMovementKeyframe = 0;
    int CurrentSpriteKeyframe = 0;
    int CurrentRotKeyframe = 0;
    public void AddScale(float amn)
    {
        SetScale(WantScale + amn);
    }
    public void SetScale(float scale)
    {
        WantScale = scale;
        // Size of one pixel in scale space
        float pixelStep = 1f / (320 * 6);

        // Snap scale to nearest pixel step
        float snappedScale = Mathf.Round(WantScale / pixelStep) * pixelStep;

        transform.localScale = new Vector3(snappedScale, snappedScale, 1f);
    }
    public float GetAnimationSpeed()
    {
        return 0.2f;
    }
    public void Init(ScriptableSceneObject ob) {
        ObjectType = ob;
        Sprites = ob.SpriteList;
        img = GetComponent<Image>();
        img.sprite = ObjectType.SpriteList[CurrentImageFrame];
        if (img.sprite != null) img.rectTransform.sizeDelta = img.sprite.rect.size * 6f;
        else img.rectTransform.sizeDelta = Vector2.zero;

        if (ObjectType.ScaleKeyframes.Count > CurrentScaleKeyframe)
            SetScale(ObjectType.ScaleKeyframes[CurrentMovementKeyframe].Scale);
        else SetScale(1);
        if (ObjectType.PositionKeyframes.Count > CurrentMovementKeyframe)
            transform.position = GetPos(ObjectType.PositionKeyframes[CurrentMovementKeyframe].Vector);
        else transform.position = GetPos(0, 0);
        if (ObjectType.RotationKeyframes.Count > CurrentRotKeyframe)
        {
            float zRot = ObjectType.RotationKeyframes[CurrentRotKeyframe].Rotation;
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y,
                zRot
            );
        }
    }
    private void Update()
    {
        
        AnimationTime += Time.deltaTime;
        if (ObjectType.ScaleKeyframes.Count > CurrentScaleKeyframe)
        {
            if (AnimationTime > ObjectType.ScaleKeyframes[CurrentScaleKeyframe].Time)
            {
                CurrentScaleKeyframe++;
                if (ObjectType.ScaleKeyframes.Count > CurrentScaleKeyframe)
                {
                    Vector3 Difference = new Vector3(ObjectType.ScaleKeyframes[CurrentScaleKeyframe].Scale, ObjectType.ScaleKeyframes[CurrentScaleKeyframe].Scale,1) - transform.localScale;
                    float TimeDiff = ObjectType.ScaleKeyframes[CurrentScaleKeyframe].Time - AnimationTime;
                    CurrentScaling = Difference.x / TimeDiff;
                }
            }
        }
        if (ObjectType.PositionKeyframes.Count > CurrentMovementKeyframe)
        {
            if (AnimationTime > ObjectType.PositionKeyframes[CurrentMovementKeyframe].Time)
            {
                CurrentMovementKeyframe++;
                if (ObjectType.PositionKeyframes.Count > CurrentMovementKeyframe)
                {
                    Vector3 Difference = GetPos(ObjectType.PositionKeyframes[CurrentMovementKeyframe].Vector) - transform.position;
                    float TimeDiff = ObjectType.PositionKeyframes[CurrentMovementKeyframe].Time - AnimationTime;
                    CurrentMovement = Difference / TimeDiff;
                }
            }
        }
        if (ObjectType.RotationKeyframes.Count > CurrentRotKeyframe)
        {
            if (AnimationTime > ObjectType.RotationKeyframes[CurrentRotKeyframe].Time)
            {
                CurrentRotKeyframe++;
                if (ObjectType.RotationKeyframes.Count > CurrentRotKeyframe)
                {
                    float Difference = ObjectType.RotationKeyframes[CurrentRotKeyframe].Rotation - transform.rotation.eulerAngles.z;
                    float TimeDiff = ObjectType.RotationKeyframes[CurrentRotKeyframe].Time - AnimationTime;
                    CurrentRotation = Difference / TimeDiff;
                }
            }
        }
        if (ObjectType.SpriteKeyframes.Count > CurrentSpriteKeyframe)
        {
            if (AnimationTime > ObjectType.SpriteKeyframes[CurrentSpriteKeyframe].Time)
            {
                Sprites = ObjectType.SpriteKeyframes[CurrentSpriteKeyframe].List;
                CurrentImageFrame = -1;
                SwitchTime = 99;
                CurrentSpriteKeyframe++;
            }
        }
        AddScale(CurrentScaling * Time.deltaTime);
        transform.Rotate(Vector3.forward, CurrentRotation * Time.deltaTime);
        transform.position += CurrentMovement * Time.deltaTime;

        SwitchTime += Time.deltaTime;
        if (SwitchTime > GetAnimationSpeed())
        {
            SwitchTime -= GetAnimationSpeed();
            CurrentImageFrame++;
            if (CurrentImageFrame >= Sprites.Count) CurrentImageFrame = 0;
            img.sprite = Sprites[CurrentImageFrame];
            if (img.sprite != null) img.rectTransform.sizeDelta = img.sprite.rect.size * 6f;
            else img.rectTransform.sizeDelta = Vector2.zero;
        }
    }

    private Vector3 GetPos(Vector3 vec)
    {
        return GetPos(vec.x, vec.y);
    }
    private Vector3 GetPos(float x, float y)
    {
        //Takes a value from -1 to 1
        //x = (x / 320f) * 2f - 1f;
       // y = (y / 180f) * 2f - 1f;
        Vector3 vec = Camera.main.ScreenToWorldPoint(new Vector3((x * Screen.width + Screen.width * 0.5f), (y * Screen.height + Screen.height * 0.5f)));
        //My Canvas projects in World coordinates
        return new Vector3(vec.x * 0.5f, vec.y * 0.5f, 0);
    }
    public static Vector2 ConvertToScreenSpace(float x, float y)
    {
        float screenX = (x + 1f) * 0.5f * 320f;
        float screenY = (y + 1f) * 0.5f * 180f;

        return new Vector2(screenX, screenY);
    }
}
