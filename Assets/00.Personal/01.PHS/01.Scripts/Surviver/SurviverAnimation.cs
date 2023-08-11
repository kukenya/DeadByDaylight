using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverAnimation : MonoBehaviour
{
    public Animator anim;
    SurviverController controller;
    string currentState;

    public enum MoveState
    {
        Idle,
        Walking,
        Sprinting,
    }

    public enum PoseState
    {
        Standing,
        Crouching,
        Crawl,
    }

    public enum ObjectInteractState
    {
        Generator
    }

    public MoveState moveState = MoveState.Idle;
    public PoseState poseState = PoseState.Standing;
    public ObjectInteractState interactState = ObjectInteractState.Generator;

    public MoveState mState { get { return moveState; } set {
            if (value != moveState)
            {
                moveState = value;
                AnimationChange();
            }
        } }

    public PoseState Pose { get { return poseState; } set {
            if (value != poseState)
            {
                poseState = value;
                AnimationChange();
            } 
        } }

    bool isInjuerd = false;

    public bool Injuerd { get { return isInjuerd; } set { isInjuerd = value; AnimationChange(); } }

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<SurviverController>();
        AnimationChange();
    }

    public void AnimationChange()
    {
        switch (Pose)
        {
            case PoseState.Standing:
                Standing();
                break;
            case PoseState.Crouching:
                Crouching();
                break;
            case PoseState.Crawl:
                Crawling();
                break;
        }
    }

    void Standing()
    {
        switch (mState)
        {
            case MoveState.Idle:
                if (Injuerd) Play("InjuerdStandIdle");
                else Play("StandIdle");
                break;
            case MoveState.Walking:
                if (Injuerd) Play("InjuerdStandWalk");
                else Play("StandWalk");
                break;
            case MoveState.Sprinting:
                if (Injuerd) Play("InjuerdStandSprint");
                else Play("StandSprint");
                break;
        }
    }

    void Crouching()
    {
        switch (mState)
        {
            case MoveState.Idle:
                if (Injuerd) Play("InjuerdCrouchIdle");
                else Play("CrouchIdle");
                break;
            case MoveState.Walking:
                if (Injuerd) Play("InjuerdCrouchWalk");
                else Play("CrouchWalk");
                break;
            case MoveState.Sprinting:
                if (Injuerd) Play("InjuerdCrouchWalk");
                else Play("CrouchWalk");
                break;
        }
    }

    void Crawling()
    {
        Play("CrawlMove");
        switch (mState)
        {
            case MoveState.Idle:
                anim.enabled = false;
                break;
            case MoveState.Walking:
                anim.enabled = true;
                break;
            case MoveState.Sprinting:
                anim.enabled = true;
                break;
        }
    }

    public void PlayStandToCrawl()
    {
        controller.BanMove = true;
        Play("HitBack");
        StartCoroutine(CheckAnimEnd("HitBack"));
    }

    IEnumerator CheckAnimEnd(string animName)
    {
        while (true)
        {
            if (IsAnimEnd(animName)) break;
            yield return null;
        }
        Play("CrawlMove");
        Pose = PoseState.Crawl;
        controller.BanMove = false;
    }

    public void Play(string state, float time = 0.1f, bool overplay = false)
    {
        if (state == currentState) return;

        if (!CheckAnimExists(state))
        {
            if (state.Length > 0)
            {
                anim.enabled = false;
            }
            else
            {
                anim.enabled = true;
            }
            return;
        }


        anim.enabled = true;
        anim.CrossFadeInFixedTime(state, time, 0);


        if (overplay) currentState = "";
        else currentState = state;
    }


    public bool CheckAnimExists(string animName)
    {
        if (animName.Length <= 0 || animName == null)
        {
            return false;
        }

        return anim.HasState(0, Animator.StringToHash(animName));
    }

    public bool IsAnimEnd(string animName, int layer = 0)
    {
        return (anim.GetCurrentAnimatorStateInfo(layer).IsName(animName) &&
                anim.GetCurrentAnimatorStateInfo(layer).normalizedTime >= 1.0f);
    }

    public void ResetLastState()
    {
        currentState = "";
    }
}
