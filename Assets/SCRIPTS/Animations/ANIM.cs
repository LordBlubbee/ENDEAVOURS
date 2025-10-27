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
        return CurrentStrikePower() > 0;
    }
    public float CurrentStrikePower()
    {
        if (state == null) return 0;
        if (currentFrame < 0) return 0;
        if (currentFrame >= state.Frames.Count) return 0;
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

        ATTACK_HALBERD1,
        ATTACK_HALBERD2,
        ATTACK_HALBERD3,
        ATTACK_HALBERD4,
        ATTACK_CROSSBOW1,
        ATTACK_CROSSBOW2,
        ATTACK_CROSSBOW3,

        USE_MEDKIT_OTHER,
        USE_MEDKIT_SELF,
        USE_WRENCH,
        USE_GRAPPLE,

        ATTACK_LOONCRAB1,
        ATTACK_LOONCRAB2,

        BLOCK_HALBERD1,
        BLOCK_LOONCRAB1,

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
                curFrame = new AnimFrame(1, 0.05f);
                curFrame.AnimationMoveFactor = 1.1f;
                curFrame.addMovement(new Vector3(0, 0), new Vector3(0.1f, 0.1f)); //Body
                curFrame.addMovement(new Vector3(0.05f, 0), new Vector3(0.1f, 0.1f)); //Tool Right
                curFrame.addMovement(new Vector3(0.05f, 0), new Vector3(0.1f, 0.1f)); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.ATTACK_HALBERD1:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.3f);
                curFrame.moveForward = 6f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(-0.1f, -0.1f), -40); //Body
                curFrame.addMovement(new Vector3(-1.8f, -2.6f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f),-15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.1f);
                curFrame.moveForward = 6f;
                curFrame.AnimationMoveFactor = 0.6f; //STRIKE forward
                curFrame.isStriking = 0.8f;
                curFrame.addMovement(new Vector3(0.4f, -0.1f), new Vector3(0.05f, 0.05f),-25); //Body
                curFrame.addMovement(new Vector3(-0.6f, -2.6f),77); //Tool Right
                curFrame.addMovement(new Vector3(0.2f, -0.05f),-14); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.1f);
                curFrame.moveForward = 6f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = 0.8f;
                curFrame.addMovement(new Vector3(0.1f, 0f), new Vector3(-0.05f, -0.05f),-30); //Body
                curFrame.addMovement(new Vector3(-2f, -2.2f),57); //Tool Right
                curFrame.addMovement(new Vector3(0.15f, 0f),-12); //Tool Left
                Frames.Add(curFrame);

                /*curFrame = new AnimFrame(4, 0.1f);
                curFrame.moveForward = 0f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.06f, 0f), new Vector3(0.05f, 0.05f), 7); //Body
                curFrame.addMovement(new Vector3(0.15f, 0.02f)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);*/
                break;
            case AnimationState.ATTACK_HALBERD2:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.3f);
                curFrame.moveForward = 3f;
                curFrame.AnimationMoveFactor = 0.2f;
                curFrame.addMovement(new Vector3(-0.1f, 0.1f), -40); //Body
                curFrame.addMovement(new Vector3(0.45f, -0.7f), -10); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.2f);
                curFrame.moveForward = 3f;
                curFrame.AnimationMoveFactor = 0.2f; //STRIKE forward
                curFrame.isStriking = 1.25f;
                curFrame.addMovement(new Vector3(0.3f, 0.2f), new Vector3(0.05f, 0.05f), 35); //Body
                curFrame.addMovement(new Vector3(0.2f, -1.4f), 122); //Tool Right
                curFrame.addMovement(new Vector3(0.2f, -0.05f), -14); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.1f);
                curFrame.moveForward = -4f;
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(0.1f, 0.1f), new Vector3(-0.05f, -0.05f),55); //Body
                curFrame.addMovement(new Vector3(-0.8f, -1f), new Vector3(0,-2), 102); //Tool Right
                curFrame.addMovement(new Vector3(0.15f, 0f), -12); //Tool Left
                Frames.Add(curFrame);
                /*curFrame = new AnimFrame(4, 0.1f);
                curFrame.moveForward = 0f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.06f, 0f), new Vector3(0.05f, 0.05f), 7); //Body
                curFrame.addMovement(new Vector3(2.25f, -1.52f), -36); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);*/
                break;
            case AnimationState.ATTACK_HALBERD3:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.15f);
                curFrame.moveForward = -3f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(0.2f, -0.2f), 75); //Body
                curFrame.addMovement(new Vector3(-0.55f, -1.05f), new Vector3(0, -2), 117); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.25f);
                curFrame.moveForward = 1f;
                curFrame.AnimationMoveFactor = 0.6f; //STRIKE forward
                curFrame.isStriking = 1.25f;
                curFrame.addMovement(new Vector3(0f, -0.2f), new Vector3(0.05f, 0.05f), -45); //Body
                curFrame.addMovement(new Vector3(0.12f, -1.6f), new Vector3(0, -2), -29); //Tool Right
                curFrame.addMovement(new Vector3(0.2f, -0.05f), -14); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.2f);
                curFrame.moveForward = 2f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(0.1f, 0f), new Vector3(-0.05f, -0.05f),-62); //Body
                curFrame.addMovement(new Vector3(-2.6f, -1.4f), 57); //Tool Right
                curFrame.addMovement(new Vector3(0.15f, 0f), -12); //Tool Left
                Frames.Add(curFrame);
                /*curFrame = new AnimFrame(4, 0.1f);
                curFrame.moveForward = 0f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.06f, 0f), new Vector3(0.05f, 0.05f), 7); //Body
                curFrame.addMovement(new Vector3(1.65f, 1.72f), 41); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame); */
                break;
            case AnimationState.ATTACK_HALBERD4:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.4f);
                curFrame.moveForward = -4f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(-0.1f, 0f), new Vector3(-0.05f, -0.05f),-70); //Body
                curFrame.addMovement(new Vector3(-2.4f, -1.5f), 62); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.1f);
                curFrame.moveForward = -8f;
                curFrame.AnimationMoveFactor = 0.6f; //STRIKE forward
                curFrame.isStriking = 1.0f;
                curFrame.addMovement(new Vector3(0.9f, 0), new Vector3(0.05f, 0.05f), -8); //Body
                curFrame.addMovement(new Vector3(1.95f, -2.7f), 76); //Tool Right
                curFrame.addMovement(new Vector3(0.2f, -0.05f), -14); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.1f);
                curFrame.moveForward = -8f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.3f, 0f), new Vector3(-0.05f, -0.05f), 12); //Body
                curFrame.addMovement(new Vector3(0.4f, -2.95f), 87); //Tool Right
                curFrame.addMovement(new Vector3(0.15f, 0f), -12); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(4, 0.8f);
                curFrame.moveForward = -3f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.06f, 0f), new Vector3(0.05f, 0.05f), 3); //Body
                curFrame.addMovement(new Vector3(0.15f, 0.02f)); //Tool Right
                curFrame.addMovement(new Vector3(0, 0)); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.BLOCK_HALBERD1:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.1f);
                curFrame.moveForward = -6f;
                curFrame.isStriking = 1f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(-0.3f, 0f), new Vector3(-0.05f, -0.05f)); //Body
                curFrame.addMovement(new Vector3(0.7f, 0.1f), -17); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.3f);
                curFrame.moveForward = 1f;
                curFrame.isStriking = 1f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(-0.3f, 0f)); //Body
                curFrame.addMovement(new Vector3(0.5f, 0.1f), -19); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.ATTACK_CROSSBOW1:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.1f);
                curFrame.moveForward = -8f;
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.3f, 0), new Vector3(0, 0), -15); //Body
                curFrame.addMovement(new Vector3(-0.6f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.1f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(-0.4f, 0), new Vector3(0.05f, 0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.9f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.05f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.3f, 0), new Vector3(0, 0), -12); //Body
                curFrame.addMovement(new Vector3(-0.6f, -1.1f), 70); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(4, 0.5f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(-0.05f, -0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.45f, -1.3f), 62); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.ATTACK_CROSSBOW2:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.1f);
                curFrame.moveForward = -8f;
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.3f, 0), new Vector3(0, 0), -15); //Body
                curFrame.addMovement(new Vector3(-0.6f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.1f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(-0.4f, 0), new Vector3(0.05f, 0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.9f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.05f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.3f, 0), new Vector3(0, 0), -12); //Body
                curFrame.addMovement(new Vector3(-0.6f, -1.1f), 70); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(4, 0.5f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(-0.05f, -0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.45f, -1.3f), 62); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.ATTACK_CROSSBOW3:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.1f);
                curFrame.moveForward = -8f;
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.3f, 0), new Vector3(0, 0), -15); //Body
                curFrame.addMovement(new Vector3(-0.6f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.1f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(-0.4f, 0), new Vector3(0.05f, 0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.9f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.05f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.3f, 0), new Vector3(0, 0), -12); //Body
                curFrame.addMovement(new Vector3(-0.6f, -1.1f), 70); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(4, 0.5f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(-0.05f, -0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.45f, -1.3f), 62); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.USE_WRENCH:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.7f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0), -32); //Body
                curFrame.addMovement(new Vector3(-0.25f, -1.2f), 96); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.7f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0.05f, 0.05f), -24); //Body
                curFrame.addMovement(new Vector3(0.1f, -1.6f), 126); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.USE_GRAPPLE:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.1f);
                curFrame.moveForward = -8f;
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.3f, 0), new Vector3(0, 0), -15); //Body
                curFrame.addMovement(new Vector3(-0.6f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 1.1f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.4f, 0), new Vector3(0.05f, 0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.9f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.1f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(-0.4f, 0), new Vector3(0.05f, 0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.9f, -1.1f), 72); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(4, 0.05f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.3f, 0), new Vector3(0, 0), -12); //Body
                curFrame.addMovement(new Vector3(-0.6f, -1.1f), 70); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(5, 0.8f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(-0.05f, -0.05f), -18); //Body
                curFrame.addMovement(new Vector3(-0.45f, -1.3f), 62); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.USE_MEDKIT_OTHER:
                Priority = 3;
                for (int i = 0; i < 7; i+= 2) //Goes towards 8, lasts 3.6 seconds
                {
                    curFrame = new AnimFrame(i+1, 0.5f);
                    curFrame.AnimationMoveFactor = 0.3f;
                    curFrame.isStriking = 0.01f;
                    curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0), -30); //Body
                    curFrame.addMovement(new Vector3(-0.55f, -0.9f), 22); //Tool Right
                    curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                    Frames.Add(curFrame);
                    curFrame = new AnimFrame(i+2, 0.6f);
                    curFrame.AnimationMoveFactor = 0.4f;
                    curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0.05f, 0.05f), -38); //Body
                    curFrame.addMovement(new Vector3(-0.75f, -1.1f), 32); //Tool Right
                    curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                    Frames.Add(curFrame);
                } //Healing lasts 1 second
                curFrame = new AnimFrame(9, 0.3f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(0.1f, 0), new Vector3(0, 0), 0); //Body
                curFrame.addMovement(new Vector3(0.6f, 0f), new Vector3(0.05f, 0.05f), 0); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(10, 0.4f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(0.1f, 0), new Vector3(0, 0), 0); //Body
                curFrame.addMovement(new Vector3(0.6f, 0f), new Vector3(0f, 0f), 0); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(11, 0.3f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(0.1f, 0), new Vector3(0, 0), 0); //Body
                curFrame.addMovement(new Vector3(0.6f, 0f), new Vector3(0.05f, 0.05f), 0); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.USE_MEDKIT_SELF:
                Priority = 3;
                for (int i = 0; i < 7; i += 2) //Goes towards 8, lasts 3.6 seconds
                {
                    curFrame = new AnimFrame(i + 1, 0.5f);
                    curFrame.AnimationMoveFactor = 0.3f;
                    curFrame.isStriking = 0.01f;
                    curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0), -30); //Body
                    curFrame.addMovement(new Vector3(-0.35f, -0.9f), 22); //Tool Right
                    curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                    Frames.Add(curFrame);
                    curFrame = new AnimFrame(i + 2, 0.6f);
                    curFrame.AnimationMoveFactor = 0.4f;
                    curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0.05f, 0.05f), -38); //Body
                    curFrame.addMovement(new Vector3(-0.55f, -1.1f), 32); //Tool Right
                    curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                    Frames.Add(curFrame);
                } //Healing lasts 1 second
                curFrame = new AnimFrame(9, 0.3f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0), -40); //Body
                curFrame.addMovement(new Vector3(-0.4f, -1f), new Vector3(0.05f, 0.05f), -49); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(10, 0.4f);
                curFrame.AnimationMoveFactor = 0.3f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0), -45); //Body
                curFrame.addMovement(new Vector3(-0.4f, -1.2f), new Vector3(0f, 0f), -47); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(11, 0.3f);
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(-0.1f, 0), new Vector3(0, 0), -36); //Body
                curFrame.addMovement(new Vector3(-0.4f, -0.6f), new Vector3(0.05f, 0.05f), -43); //Tool Right
                curFrame.addMovement(new Vector3(-0.03f, -0.05f), -15); //Tool Left
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
            case AnimationState.ATTACK_LOONCRAB1:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.5f);
                curFrame.moveForward = -1f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(-0.3f, 0f), 0); //Body
                curFrame.addMovement(new Vector3(-0.3f, -0.1f), -76); //Tool Right
                curFrame.addMovement(new Vector3(-0.3f, 0.1f), 76); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.15f);
                curFrame.moveForward = 12f;
                curFrame.AnimationMoveFactor = 0.6f; //STRIKE forward
                curFrame.isStriking = 1.5f;
                curFrame.addMovement(new Vector3(0.4f, 0f), new Vector3(0.05f, 0.05f)); //Body
                curFrame.addMovement(new Vector3(0.4f, 0f), 18); //Tool Right
                curFrame.addMovement(new Vector3(0.4f, 0f), -18); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.6f);
                curFrame.moveForward = 6f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(0f, 0f), new Vector3(-0.05f, -0.05f)); //Body
                curFrame.addMovement(new Vector3(-0.3f, -0.1f), 0); //Tool Right
                curFrame.addMovement(new Vector3(-0.3f, 0.1f), 0); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.ATTACK_LOONCRAB2:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.5f);
                curFrame.moveForward = -1f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.addMovement(new Vector3(-0.3f, 0f), 0); //Body
                curFrame.addMovement(new Vector3(-0.3f, -0.1f), -76); //Tool Right
                curFrame.addMovement(new Vector3(-0.3f, 0.1f), 76); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.15f);
                curFrame.moveForward = 12f;
                curFrame.AnimationMoveFactor = 0.6f; //STRIKE forward
                curFrame.isStriking = 1.5f;
                curFrame.addMovement(new Vector3(0.4f, 0f), new Vector3(0.05f, 0.05f)); //Body
                curFrame.addMovement(new Vector3(0.4f, 0f), 18); //Tool Right
                curFrame.addMovement(new Vector3(0.4f, 0f), -18); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(3, 0.6f);
                curFrame.moveForward = 6f;
                curFrame.AnimationMoveFactor = 0.4f;
                curFrame.addMovement(new Vector3(0f, 0f), new Vector3(-0.05f, -0.05f)); //Body
                curFrame.addMovement(new Vector3(-0.3f, -0.1f), 0); //Tool Right
                curFrame.addMovement(new Vector3(-0.3f, 0.1f), 0); //Tool Left
                Frames.Add(curFrame);
                break;
            case AnimationState.BLOCK_LOONCRAB1:
                Priority = 3;
                curFrame = new AnimFrame(1, 0.3f);
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(-0.4f, 0f), new Vector3(0.05f, 0.05f)); //Body
                curFrame.addMovement(new Vector3(-0.7f, -0.2f), 18); //Tool Right
                curFrame.addMovement(new Vector3(-0.7f, 0.2f), -18); //Tool Left
                Frames.Add(curFrame);
                curFrame = new AnimFrame(2, 0.4f);
                curFrame.moveForward = -4f;
                curFrame.AnimationMoveFactor = 0.6f;
                curFrame.isStriking = 1f;
                curFrame.addMovement(new Vector3(-0.4f, 0f), new Vector3(0f, 0f)); //Body
                curFrame.addMovement(new Vector3(-0.4f, 0f), 22); //Tool Right
                curFrame.addMovement(new Vector3(-0.4f, 0f), -22); //Tool Left
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
    public float isStriking;
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