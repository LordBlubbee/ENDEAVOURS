using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANIM //Monobehavior that uses AnimTransforms and acts as a controller
{
    protected List<AnimTransform> AnimTransforms;
    protected AnimState state;
    protected AnimationState currentAnimID;
    protected int currentFrame;
    protected float currentTimer;
    protected float currentFrameProgress;
    protected float currentMaxTimer;
    public ANIM(List<AnimTransform> trans)
    {
        AnimTransforms = trans;
    }

    public bool isCurrentlyStriking()
    {
        if (state == null) return false;
        if (currentFrame < 0) return false;
        if (currentFrame >= state.Frames.Count) return false;
        return state.Frames[currentFrame].isStriking;
    }
    public float getAnimationMoveFactor()
    {
        if (state == null) return 1f;
        if (currentFrame < 0) return 1f;
        if (currentFrame >= state.Frames.Count) return 1f;
        return state.Frames[currentFrame].AnimationMoveFactor;
    }
    public float getAnimationMoveForward()
    {
        if (state == null) return 0f;
        if (currentFrame < 0) return 0f;
        if (currentFrame >= state.Frames.Count) return 0f;
        return state.Frames[currentFrame].moveForward;
    }
    public int getCurrentPriority()
    {
        if (state == null) return 0;
        return state.AnimPriority;
    }
    public bool setAnimation(AnimationState sta, int priority = 99)
    {
        if (getCurrentPriority() >= priority) return false;
        if (sta == currentAnimID) return false;
        animationNewState(sta);
        animationNewFrameStart(0);
        return true;
    }

    public AnimationState getAnimationState()
    {
        return currentAnimID;
    }
    public void animationFrame(float speedFactor = 1) {
        if (state == null) return;
        if (currentTimer > 95) return;
        currentTimer -= CO.co.GetWorldSpeedDelta() * speedFactor;
        currentFrameProgress += (CO.co.GetWorldSpeedDelta() / currentMaxTimer) * speedFactor;
        AnimFrame frame = state.Frames[currentFrame];
        if (currentTimer < 0f)
        {
            for (int i = 0; i < AnimTransforms.Count; i++)
            {
                AnimTransform animtransform = AnimTransforms[i];

                Transform trans = animtransform.transform;
                if (trans == null) continue;
                trans.localPosition = animtransform.targetPos;
                trans.localScale = animtransform.targetScale;
                trans.localEulerAngles = new Vector3(0, 0, animtransform.targetRot);
                animtransform.currentMovePos = Vector3.zero;
                animtransform.currentMoveScale = Vector3.zero;
                animtransform.currentMoveRot = 0;
            }
            animationNewFrameStart(frame.nextFrame);
        }
        float Mod = 1f * speedFactor;
        if (frame.isCurve)
        {
            Mod = (0.5f-Mathf.Abs(0.5f-currentFrameProgress))*4f * speedFactor;
        }
        for (int i = 0; i < AnimTransforms.Count; i++)
        {
            AnimTransform animtransform = AnimTransforms[i];
          
            Transform trans = animtransform.transform;
            if (trans == null) continue;
            trans.localPosition += animtransform.currentMovePos * CO.co.GetWorldSpeedDelta() * Mod;
            trans.localScale += animtransform.currentMoveScale * CO.co.GetWorldSpeedDelta() * Mod;
            trans.localEulerAngles += new Vector3(0, 0, animtransform.currentMoveRot * CO.co.GetWorldSpeedDelta()) * Mod;
        }
    }
    public void animationNewFrameStart(int Frame)
    {
        if (Frame >= state.Frames.Count) //If the animation is done...
        {
            currentFrame = 0;
            state = null;
            return;
        }
        currentFrame = Frame;
        AnimFrame frame = state.Frames[currentFrame];

        currentMaxTimer = Random.Range(frame.minDuration,frame.maxDuration);
        currentTimer = currentMaxTimer;
        currentFrameProgress = 0f;

        float averageSpeedMod = 1f / currentTimer;
        for (int i = 0; i < Mathf.Min(AnimTransforms.Count, frame.Frames.Count); i++)
        {
            AnimTransform animtransform = AnimTransforms[i];

            Transform trans = animtransform.transform;
            if (trans == null) continue;
            animtransform.targetPos = animtransform.defaultPos + frame.Frames[i].Positions;
            animtransform.targetScale = animtransform.defaultScale + frame.Frames[i].Scales;
            if (frame.Frames[i].RotationsMax > 0) animtransform.targetRot = animtransform.defaultRot + Random.Range(frame.Frames[i].Rotations, frame.Frames[i].RotationsMax);
            else animtransform.targetRot = animtransform.defaultRot + frame.Frames[i].Rotations;

            //0 degrees + 30 degrees = target rot = 30 degrees
            //start rot = how we are rotated now (-10 degrees)
            //move rotations (executed per frame) = targetrot - startrot = 40 degrees = move rotations
            if (animtransform.targetRot > 180f) animtransform.targetRot -= 360f;
            else if (animtransform.targetRot < -180f) animtransform.targetRot += 360f;
          
            //Fix rotations here

            animtransform.startPos = trans.localPosition;
            animtransform.startScale = trans.localScale;
            animtransform.startRot = trans.localEulerAngles.z;

            if (animtransform.startRot > 180f) animtransform.startRot -= 360f;
            else if (animtransform.startRot < -180f) animtransform.startRot += 360f;

            animtransform.currentMovePos = (animtransform.targetPos - animtransform.startPos) * averageSpeedMod;
            animtransform.currentMoveScale = (animtransform.targetScale - animtransform.startScale) * averageSpeedMod;
            animtransform.currentMoveRot = (animtransform.targetRot - animtransform.startRot) * averageSpeedMod;

        }
    }
    public enum AnimationState
    {
        MI_IDLE,
        MI_MOVE,
        MI_DASH,

        ATTACK_SPEAR1,
        ATTACK_SPEAR2,
        ATTACK_SPEAR3,
        ATTACK_BOW1,
        ATTACK_BOW2,
        ATTACK_BOW3,

        ATTACK_AXE1,
        ATTACK_AXE2,

        MI_DEAD1,
        MI_DEAD2,
        MI_DEAD3
    }
    public void animationNewState(AnimationState sta)
    {
        currentAnimID = sta;
        AnimState newState;
        AnimFrame curFrame;

        int Priority = 0;

        List<AnimFrame> Frames = new List<AnimFrame>();
        switch (currentAnimID)
        {
            default: //MI_IDLE
                curFrame = new AnimFrame(1, 0.2f, 0.5f);
                curFrame.addMovement(new Vector3(0,0), new Vector3(0, 0)); //Body
                curFrame.addMovement(new Vector3(0, 0)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 1.6f, 1.9f);
                curFrame.addMovement(new Vector3(0, 0), new Vector3(0.02f, 0.02f)); //Body
                curFrame.addMovement(new Vector3(0, 0)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(1, 1.2f);
                curFrame.addMovement(new Vector3(0, 0), new Vector3(-0.02f, -0.02f)); //Body
                curFrame.addMovement(new Vector3(0, 0)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.MI_MOVE:
                curFrame = new AnimFrame(1, 0.2f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(0, 0)); //Body
                curFrame.addMovement(new Vector3(0, 0)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.3f);
                curFrame.AnimationMoveFactor = 1.1f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(0.03f, 0.03f)); //Body
                curFrame.addMovement(new Vector3(0.03f, 0), new Vector3(0.03f, 0.03f)); //Tool Right
                curFrame.addMovement(new Vector3(0.03f, 0), new Vector3(0.03f, 0.03f)); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(1, 0.2f);
                curFrame.AnimationMoveFactor = 0.8f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(-0.03f, -0.03f)); //Body
                curFrame.addMovement(new Vector3(-0.02f, -0.02f), new Vector3(-0.03f, -0.03f)); //Tool Right
                curFrame.addMovement(new Vector3(-0.02f, 0.02f), new Vector3(-0.03f, -0.03f)); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.MI_DASH:
                Priority = 4;
                curFrame = new AnimFrame(1, 0.1f);
                curFrame.AnimationMoveFactor = 1.1f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(0.1f, 0.1f)); //Body
                curFrame.addMovement(new Vector3(0.05f, 0), new Vector3(0.1f, 0.1f)); //Tool Right
                curFrame.addMovement(new Vector3(0.05f, 0), new Vector3(0.1f, 0.1f)); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.2f);
                curFrame.AnimationMoveFactor = 0.8f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(-0.03f, -0.03f)); //Body
                curFrame.addMovement(new Vector3(-0.02f, -0.02f), new Vector3(-0.03f, -0.03f)); //Tool Right
                curFrame.addMovement(new Vector3(-0.02f, -0.02f), new Vector3(-0.03f, -0.03f)); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.ATTACK_SPEAR1:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.1f);
                curFrame.moveForward = 6f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0)); //Body
                curFrame.addMovement(new Vector3(-0.15f, 0), 7); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f),-15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.1f);
                curFrame.moveForward = 6f;
                curFrame.AnimationMoveFactor = 0.6f; //STRIKE forward
                curFrame.isStriking = true;
                curFrame.addMovement(new Vector3(0.3f, 0), new Vector3(0.05f, 0.05f),8); //Body
                curFrame.addMovement(new Vector3(3.2f, 0.1f),-4); //Tool Right
                curFrame.addMovement(new Vector3(0.2f, -0.05f),-14); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.1f);
                curFrame.moveForward = 6f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = true;
                curFrame.addMovement(new Vector3(0.1f, 0f), new Vector3(-0.05f, -0.05f)); //Body
                curFrame.addMovement(new Vector3(0.3f, 0.1f),8); //Tool Right
                curFrame.addMovement(new Vector3(0.15f, 0f),-12); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(4, 0.2f);
                curFrame.moveForward = 0f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.06f, 0f), new Vector3(0.05f, 0.05f), 7); //Body
                curFrame.addMovement(new Vector3(0.15f, 0.02f)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.ATTACK_BOW1:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.2f, 0.6f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0)); //Body
                curFrame.addMovement(new Vector3(-0.15f, 0), 7); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.9f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(0.3f, 0), new Vector3(0.05f, 0.05f), 8); //Body
                curFrame.addMovement(new Vector3(0.6f, 0.1f), 35,25); //Tool Right
                curFrame.addMovement(new Vector3(0.1f, -0.05f), -14); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.9f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.isStriking = true;
                curFrame.addMovement(new Vector3(0.1f, 0f), new Vector3(-0.05f, -0.05f)); //Body
                curFrame.addMovement(new Vector3(0.5f, 0.1f), 35,25); //Tool Right
                curFrame.addMovement(new Vector3(0.05f, 0f), -12); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(4, 0.7f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.06f, 0f), new Vector3(0.05f, 0.05f), 7); //Body
                curFrame.addMovement(new Vector3(0.15f, 0.02f),-8); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(5, 1f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(-0.05f, -0.05f), 3); //Body
                curFrame.addMovement(new Vector3(0.12f, 0f)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.ATTACK_AXE1:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.2f, 0.4f);
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0)); //Body
                curFrame.addMovement(new Vector3(-0.15f, 0), -20); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.2f);
                curFrame.AnimationMoveFactor = 0.7f;
                curFrame.addMovement(new Vector3(0.3f, 0), new Vector3(0.05f, 0.05f), 8); //Body
                curFrame.addMovement(new Vector3(0.3f, -0.6f), -68,-60); //Tool Right
                curFrame.addMovement(new Vector3(0.2f, -0.05f), -14); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.3f);
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.isStriking = true;
                curFrame.addMovement(new Vector3(0.1f, 0f), new Vector3(-0.05f, -0.05f)); //Body
                curFrame.addMovement(new Vector3(0.8f, 0.2f), 32,36); //Tool Right
                curFrame.addMovement(new Vector3(0.15f, 0f), -12); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(4, 0.2f);
                curFrame.isStriking = true;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.06f, 0f), new Vector3(0.05f, 0.05f), 7); //Body
                curFrame.addMovement(new Vector3(0.15f, -0.22f),-26); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(5, 0.7f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(-0.05f, -0.05f), 3); //Body
                curFrame.addMovement(new Vector3(0.12f, 0f),-18); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.MI_DEAD1:
                curFrame = new AnimFrame(1, 0.2f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(0, 0)); //Body
                curFrame.addMovement(new Vector3(0, 0)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(99, 99);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(0, 0)); //Body
                curFrame.addMovement(new Vector3(0, 0)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                break;
        }
        newState = new AnimState(Priority, Frames);
        state = newState;
    }
}
public class AnimState //A full animation that carries every frame in it. Each animation has a priority.
{
    public List<AnimFrame> Frames = new List<AnimFrame>();
    public int AnimPriority;
    public AnimState(int pr, List<AnimFrame> frames)
    {
        AnimPriority = pr;
        Frames = frames;
    }
}
public class AnimFrame //A single frame in an animation which tells each AnimTransform where they should move
{
    public float minDuration;
    public float maxDuration;
    public float moveForward;
    public int nextFrame;
    public bool isStriking;
    public bool isCurve;
    public float AnimationMoveFactor = 1f;
    public List<AnimFramePart> Frames = new List<AnimFramePart>();
    public AnimFrame(int next, float minDur, float maxDur, bool curve = true)
    {
        nextFrame = next;
        minDuration = minDur;
        maxDuration = maxDur;
        isCurve = curve;
    }
    public AnimFrame(int next, float Dur, bool curve = true)
    {
        nextFrame = next;
        minDuration = Dur;
        maxDuration = Dur;
        isCurve = curve;
    }
    public void addMovement(Vector3 pos, Vector3 scal, float fl, float flmax)
    {
        Frames.Add(new AnimFramePart(fl, flmax, pos, scal));
    }
    public void addMovement(Vector3 pos, Vector3 scal, float fl)
    {
        Frames.Add(new AnimFramePart(fl, pos, scal));
    }
    public void addMovement(Vector3 pos, Vector3 scal)
    {
        Frames.Add(new AnimFramePart(0, pos, scal));
    }
    public void addMovement(Vector3 pos, float fl)
    {
        Frames.Add(new AnimFramePart(fl, pos, Vector3.zero));
    }
    public void addMovement(Vector3 pos, float fl, float flmax)
    {
        Frames.Add(new AnimFramePart(fl, flmax, pos, Vector3.zero));
    }
    public void addMovement(Vector3 pos)
    {
        Frames.Add(new AnimFramePart(0, pos, Vector3.zero));
    }
}
public class AnimFramePart
{
    public float Rotations;
    public float RotationsMax;
    public Vector3 Scales;
    public Vector3 Positions;

    public AnimFramePart(float rot, Vector3 pos, Vector3 scal) {
        Rotations = rot;
        Positions = pos;
        Scales = scal;
    }
    public AnimFramePart(float rot, float rotmax, Vector3 pos, Vector3 scal)
    {
        Rotations = rot;
        RotationsMax = rotmax;
        Positions = pos;
        Scales = scal;
    }
}

public class AnimTransform //This is a transform with its default state stored plus some other variables
{
    public Transform transform;
    public float defaultRot;
    public Vector3 defaultScale;
    public Vector3 defaultPos;
    public float startRot;
    public Vector3 startScale;
    public Vector3 startPos;
    public float targetRot = 0f;
    public Vector3 targetScale = Vector3.zero;
    public Vector3 targetPos = Vector3.zero;
    public float currentMoveRot = 0f;
    public Vector3 currentMoveScale = Vector3.zero;
    public Vector3 currentMovePos = Vector3.zero;
    public AnimTransform()
    {
    }
    public AnimTransform(Transform trans)
    {
        transform = trans;
        defaultRot = transform.localEulerAngles.z;
        defaultScale = transform.localScale;
        defaultPos = transform.localPosition;

        startRot = defaultRot;
        startScale = defaultScale;
        startPos = defaultPos;
    }
    public void setTransform(Transform trans)
    {
        transform = trans;
        if (trans == null) return;
        defaultRot = transform.localEulerAngles.z;
        defaultScale = transform.localScale;
        defaultPos = transform.localPosition;

        startRot = defaultRot;
        startScale = defaultScale;
        startPos = defaultPos;
    }
}