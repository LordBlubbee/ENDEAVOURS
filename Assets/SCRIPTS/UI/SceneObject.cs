using UnityEngine;
using UnityEngine.UI;

public class SceneObject : MonoBehaviour
{
    private Image img;
    int CurrentImageFrame;
    private ScriptableSceneObject ObjectType;
    float AnimationTime = 0f;
    float SwitchTime = 0f;
    float WantScale = 1f;
    Vector3 CurrentMovement = Vector3.zero;
    float CurrentScaling = 0;
    int CurrentScaleKeyframe = 0;
    int CurrentMovementKeyframe = 0;

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
        return 0.1f;
    }
    public void Init(ScriptableSceneObject ob) {
        ObjectType = ob;
        img = GetComponent<Image>();
        img.sprite = ObjectType.SpriteList[CurrentImageFrame];

        img.rectTransform.sizeDelta = img.sprite.rect.size * 6f;

        if (ObjectType.ScaleKeyframes.Count > CurrentScaleKeyframe)
            SetScale(ObjectType.ScaleKeyframes[CurrentMovementKeyframe].Vector.x);
        else SetScale(1);
        if (ObjectType.PositionKeyframes.Count > CurrentMovementKeyframe)
            transform.position = GetPos(ObjectType.PositionKeyframes[CurrentMovementKeyframe].Vector);
        else transform.position = GetPos(0, 0);
    }
    private void Update()
    {
        SwitchTime += Time.deltaTime;
        if (SwitchTime > GetAnimationSpeed())
        {
            SwitchTime -= GetAnimationSpeed();
            CurrentImageFrame++;
            if (CurrentImageFrame >= ObjectType.SpriteList.Count) CurrentImageFrame = 0;
            img.sprite = ObjectType.SpriteList[CurrentImageFrame];
        }
        AnimationTime += Time.deltaTime;
        if (ObjectType.ScaleKeyframes.Count > CurrentScaleKeyframe)
        {
            if (AnimationTime > ObjectType.ScaleKeyframes[CurrentScaleKeyframe].Time)
            {
                CurrentScaleKeyframe++;
                if (ObjectType.ScaleKeyframes.Count > CurrentScaleKeyframe)
                {
                    Vector3 Difference = ObjectType.ScaleKeyframes[CurrentScaleKeyframe].Vector - transform.localScale;
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
                    Vector3 Difference = ObjectType.PositionKeyframes[CurrentMovementKeyframe].Vector - transform.position;
                    float TimeDiff = ObjectType.PositionKeyframes[CurrentMovementKeyframe].Time - AnimationTime;
                    CurrentMovement = Difference / TimeDiff;
                }
            }
        }
        AddScale(CurrentScaling * Time.deltaTime);
        transform.position += CurrentMovement * Time.deltaTime;
    }

    private Vector3 GetPos(Vector3 vec)
    {
        return GetPos(vec.x, vec.y);
    }
    private Vector3 GetPos(float x, float y)
    {
        //Takes a value from -1 to 1
        Vector3 vec = Camera.main.ScreenToWorldPoint(new Vector3((x * Screen.width + Screen.width * 0.5f), (y * Screen.height + Screen.height * 0.5f)));
        //My Canvas projects in World coordinates
        return new Vector3(vec.x * 0.5f, vec.y * 0.5f, 0);
    }
}
