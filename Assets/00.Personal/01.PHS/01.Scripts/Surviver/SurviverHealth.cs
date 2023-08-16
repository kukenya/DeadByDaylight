using JetBrains.Annotations;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SurviverHealth : MonoBehaviourPun
{
    public SurviverController controller;
    SurviverAnimation surviverAnimation;
    SurviverLookAt surviverLookAt;
    SurvivorInteraction interaction;
    SurviverSound surviverSound;
    Animator anim;

    public Transform rootCameraPosition;

    public enum HealthState
    {
        Healthy,
        Injured,
        Down,
        Carrying,
        Hook,
    }

    HealthState state = HealthState.Healthy;

    public HealthState State { get { return state; } set { 
            photonView.RPC(nameof(SetHealthState), RpcTarget.All, value);
        }
    }

    [PunRPC]
    void SetHealthState(HealthState value)
    {
        state = value;
        if(state == HealthState.Healthy)
        {
            surviverAnimation.Injuerd = false;
        }
        else if(state == HealthState.Injured)
        {
            surviverAnimation.Injuerd = true;
        }
        else if(state == HealthState.Hook)
        {
            Prograss = 0;
        }
        surviverAnimation.AnimationChange();
    }

    // 플레이어 갈고리 걸린 횟수 관련 변수
    [Range(0, 2)]
    public int hook = 0;

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<SurviverController>();
        surviverAnimation = GetComponent<SurviverAnimation>();
        surviverLookAt = GetComponent<SurviverLookAt>();
        surviverSound = GetComponent<SurviverSound>();
        interaction = GetComponent<SurvivorInteraction>();
    }

    private void Update()
    {
        if (photonView.IsMine == false) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            NormalHit();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeCarring();
        }

        HookEscape();
    }

    public void NormalHit()
    {
        if(state == HealthState.Healthy) {
            ChangeInjuerd();
        }
        else if(state == HealthState.Injured){
            ChangeDown();
        }
    }

    void ChangeInjuerd()
    {
        state = HealthState.Injured;
        surviverAnimation.anim.CrossFadeInFixedTime("Hit", 0.25f, 2);
        surviverSound.PlayInjSound();
        StartCoroutine(HitSpeed());
    }

    IEnumerator HitSpeed()
    {
        float currentTime = 0;
        controller.isHit = true;
        while(true)
        {
            currentTime += Time.deltaTime;
            if (currentTime > 1.5f) break;
            yield return null;
        }
        controller.isHit = false;
    }

    void ChangeDown()
    {
        surviverLookAt.LookAt = false;
        state = HealthState.Down;
        controller.Crawl = true;
        surviverLookAt.isLookAt = false;
        surviverSound.PlayDownSound();
        surviverAnimation.PlayStandToCrawl();
    }

    public void ChangeCarring()
    {
        surviverLookAt.LookAt = false;
        if (state == HealthState.Carrying)
        {
            StartCoroutine(WaitAnimEnd());
        }
        else
        {
            controller.BanMove = true;
            state = HealthState.Carrying;
            surviverAnimation.Play("PickUp_IN");
        }
    }

    public float yOffset = 2;

    IEnumerator WaitAnimEnd()
    {
        surviverAnimation.Play("Hook_IN");
        while (true)
        {
            if (surviverAnimation.IsAnimEnd("Hook_IN")) break;
            yield return null;
        }
        hook++;
        rootCameraPosition.position += new Vector3(0, yOffset, 0);
        surviverAnimation.Play("Hook_OUT");
        while (true)
        {
            if (surviverAnimation.IsAnimEnd("Hook_OUT")) break;
            yield return null;
        }
        state = HealthState.Hook;
        surviverAnimation.Play("Hook_Idle");
    }

    [Header("탈출")]
    bool escaping = false;
    public bool Escape { get { return escaping; } set {  escaping = value; } }
    float prograss;
    public float maxPrograssTime = 2f;
    public float Prograss { get { return prograss; } set { prograss = Mathf.Clamp(value, 0, maxPrograssTime); } }

    float animationPrograss;
    public float AnimationPrograss { get { return animationPrograss; } set { animationPrograss = Mathf.Clamp(value, 0, 1f); } }

    public float hookAnimationChangeRate = 2;

    void HookEscape()
    {
        if (state != HealthState.Hook) return;
        anim.SetLayerWeight(3, animationPrograss);

        if(Prograss >= maxPrograssTime)
        {
            StartCoroutine(WaitHook());
        }
        if(escaping) { Prograss += Time.deltaTime; AnimationPrograss += Time.deltaTime * hookAnimationChangeRate; }
        else { Prograss -= Time.deltaTime; AnimationPrograss -= Time.deltaTime * hookAnimationChangeRate; }
        
        SurviverUI.instance.prograssBar.fillAmount = Prograss / maxPrograssTime;
    }

    IEnumerator WaitHook()
    {
        State = HealthState.Injured;
        rootCameraPosition.position -= new Vector3(0, yOffset, 0);
        surviverAnimation.Play("Hook_Free");
        anim.SetLayerWeight(3, 0);
        while (true)
        {
            if (surviverAnimation.IsAnimEnd("Hook_Free")) break;
            yield return null;
        }
        controller.BanMove = false;
    }
}
