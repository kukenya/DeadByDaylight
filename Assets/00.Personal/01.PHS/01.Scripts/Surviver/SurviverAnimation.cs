using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SurviverAnimation : MonoBehaviourPun, IPunObservable
{
    public Animator anim;
    SurviverController controller;
    SurviverHealing healing;
    public string currentState;

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
            photonView.RPC(nameof(SetMoveStateRPC), RpcTarget.All, value);
        }
    }

    [PunRPC]
    void SetMoveStateRPC(MoveState value)
    {
        if (value != moveState)
        {
            moveState = value;
            AnimationChange();
        }
    }

    public PoseState Pose { get { return poseState; } set {
            photonView.RPC(nameof(SetPoseRPC), RpcTarget.All, value);       
        }
    }

    [PunRPC]
    void SetPoseRPC(PoseState value)
    {
        if (value != poseState)
        {
            poseState = value;
            AnimationChange();
        }
    }

    bool isInjuerd = false;

    public bool Injuerd { get { return isInjuerd; } set { photonView.RPC(nameof(SetInjuerdRPC), RpcTarget.All, value); } }

    [PunRPC]
    void SetInjuerdRPC(bool value)
    {
        if (isInjuerd == value) return;
        isInjuerd = value;
        AnimationChange();
        healing.Prograss = 0;
        healing.healed = false;
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<SurviverController>();
        healing = GetComponent<SurviverHealing>();
        AnimationChange();
    }

    [PunRPC]
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
        if(anim.speed != 1) anim.speed = 1;

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
        if (anim.speed != 1) anim.speed = 1;

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
                anim.speed = 0;
                break;
            case MoveState.Walking:
                anim.speed = 1;
                break;
            case MoveState.Sprinting:
                anim.speed = 1;
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

    public void Play(string state, float time = 0.1f, int layerIdx = 0, bool overplay = false)
    {
        if (photonView.IsMine)
        {
            if (state == currentState) return;

            anim.enabled = true;
            photonView.RPC(nameof(PlayAnimationRPC), RpcTarget.All, state, time, layerIdx);

            if (overplay) currentState = "";
            else currentState = state;
        }
    }

    [PunRPC]
    public void ForcePlay(string state)
    {
        if (state == currentState) return;

        anim.enabled = true;
        photonView.RPC(nameof(PlayAnimationRPC), RpcTarget.All, state, 0.1f, 0);
    }

    [PunRPC]
    void PlayAnimationRPC(string state, float time, int layerIdx)
    {
        anim.CrossFadeInFixedTime(state, time, layerIdx);
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(anim.speed);
        }
        else
        {
            anim.speed = (float)stream.ReceiveNext();   
        }
    }
}
