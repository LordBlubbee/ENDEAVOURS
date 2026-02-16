using UnityEngine;
using UnityEngine.UI;

public class SceneObject : MonoBehaviour
{
    private Image img;
    int CurrentImageFrame;
    private ScriptableSceneObject ObjectType;
    float AnimationTime = 0f;
    float SwitchTime = 0f;
    Vector3 CurrentMovement = Vector3.zero;
    Vector3 CurrentScaling = Vector3.zero;
    int CurrentScaleKeyframe = 0;
    int CurrentMovementKeyframe = 0;
    public float GetAnimationSpeed()
    {
        return 0.1f;
    }
    public void Init(ScriptableSceneObject ob) {
        ObjectType = ob;
        img = GetComponent<Image>();

        if (ObjectType.ScaleKeyframes.Count > CurrentScaleKeyframe)
            transform.localScale = ObjectType.PositionKeyframes[CurrentMovementKeyframe].Vector;
        if (ObjectType.PositionKeyframes.Count > CurrentMovementKeyframe)
            transform.position = GetPos(ObjectType.PositionKeyframes[CurrentMovementKeyframe].Vector);
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
                    CurrentScaling = Difference / TimeDiff;
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
        transform.localScale += CurrentScaling * Time.deltaTime;
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
