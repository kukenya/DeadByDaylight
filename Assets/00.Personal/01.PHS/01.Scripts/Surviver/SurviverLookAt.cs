using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SurviverLookAt : MonoBehaviour
{
    public Transform rootCamTrans;

    public bool isLookAt = true;

    public bool LookAt { get { return isLookAt; }
        set
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

        float angle = rootCamTrans.localRotation.eulerAngles.y;

        LookAtLayer(angle);


        if (angle < 170)
        {
            if(surviverAnimation.Pose == SurviverAnimation.PoseState.Standing)
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
        else if(angle >= 190)
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

    void LookAtLayer(float angle)
    {
        if(angle > 180)
        {
            angle = 360 - angle;
        }
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
        anim.CrossFadeInFixedTime(state, time, 1);


        if (overplay) currentState = "";
        else currentState = state;
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
