using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverLookAt : MonoBehaviourPun
{
    public Transform rootCamTrans;

    public bool isLookAt = true;

    public bool LookAt { get { return isLookAt; }
        set
        {
            SetLookAt(value);
        } 
    }

    [PunRPC]
    void SetLookAt(bool value)
    {
        if (value == true)
        {
            anim.SetLayerWeight(1, 0);
            isLookAt = value;
        }
        else
        {
            anim.SetLayerWeight(1, 0);
            isLookAt = value;
        }
    }

    string currentState = null;
    Animator anim;
    SurviverAnimation surviverAnimation;

    private void Start()
    {
        anim = GetComponent<Animator>();
        surviverAnimation = GetComponent<SurviverAnimation>();
    }

    private void Update()
    {
        UpdateLookAt();
    }

    void UpdateLookAt()
    {
        if (isLookAt == false) return;

        if(photonView.IsMine == false) { return; }

        float angle = rootCamTrans.localRotation.eulerAngles.y;

        photonView.RPC(nameof(LookAtLayer), RpcTarget.All, angle);


        if (angle < 170)
        {
            if (surviverAnimation.Pose == SurviverAnimation.PoseState.Standing)
            {
                if (surviverAnimation.Injuerd == false) Play("StandRight");
                else Play("Inj_StandRight");
            }
            else if (surviverAnimation.Pose == SurviverAnimation.PoseState.Crouching)
            {
                if (surviverAnimation.Injuerd == false) Play("CrouchRight");
                else Play("Inj_CrouchRight");
            }
        }
        else if (angle >= 190)
        {
            if (surviverAnimation.Pose == SurviverAnimation.PoseState.Standing)
            {
                if (surviverAnimation.Injuerd == false) Play("StandLeft");
                else Play("Inj_StandLeft");
            }
            else if (surviverAnimation.Pose == SurviverAnimation.PoseState.Crouching)
            {
                if (surviverAnimation.Injuerd == false) Play("CrouchLeft");
                else Play("Inj_CrouchLeft");
            }
        }
    }


    [PunRPC]
    void LookAtLayer(float angle)
    {
        if(angle > 180)
        {
            angle = 360 - angle;
        }

        if(anim == null) { return; }
        anim.SetLayerWeight(1, angle / 100);
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
        photonView.RPC(nameof(AnimPlay), RpcTarget.All, state, time);

        if (overplay) currentState = "";
        else currentState = state;
    }

    [PunRPC]
    void AnimPlay(string state, float time)
    {
        if(anim == null) { return; }
        anim.CrossFadeInFixedTime(state, time, 1);
    }

    public bool CheckAnimExists(string animName)
    {
        if (animName.Length <= 0 || animName == null)
        {
            return false;
        }

        return anim.HasState(1, Animator.StringToHash(animName));
    }

}
