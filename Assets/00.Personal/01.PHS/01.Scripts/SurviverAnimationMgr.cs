using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverAnimationMgr : MonoBehaviour
{
    public static SurviverAnimationMgr instance;

    public void Awake()
    {
        instance = this;
    }

    public Animator anim;

    string currentState;

    private void Start()
    {
        anim = GetComponent<Animator>();
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
