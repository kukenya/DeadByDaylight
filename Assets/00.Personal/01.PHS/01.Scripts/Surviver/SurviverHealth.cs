using JetBrains.Annotations;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class SurviverHealth : MonoBehaviourPun
{
    SurviverController controller;
    SurviverAnimation surviverAnimation;
    SurviverLookAt surviverLookAt;
    SurvivorInteraction interaction;
    SurviverSound surviverSound;
    Animator anim;
    SurvivorShader shader;
    SurvivorListManager listManager;
    SurviverHealing healing;
    SurvivorHookEscape hookEscape;

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
        if(State == HealthState.Healthy)
        {
            surviverAnimation.Injuerd = false;
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Healthy;
        }
        else if(State == HealthState.Injured)
        {
            surviverAnimation.Injuerd = true;
            healing.Prograss = 0;
            healing.healed = false;
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Injuerd;
        }
        else if(State == HealthState.Down)
        {
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Down;
        }
        else if(State == HealthState.Carrying)
        {
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Carry;
        }
        else if(State == HealthState.Hook)
        {
            Prograss = 0;
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Hook;
        }
        
        surviverAnimation.AnimationChange();
    }

    public HealthState StateNA { get { return state; } set {
            photonView.RPC(nameof(SetHealthStateNA), RpcTarget.All, value);
        }
    }

    [PunRPC]
    void SetHealthStateNA(HealthState value)
    {
        state = value;
        if (State == HealthState.Healthy)
        {
            surviverAnimation.Injuerd = false;
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Healthy;
        }
        else if (State == HealthState.Injured)
        {
            surviverAnimation.Injuerd = true;
            healing.Prograss = 0;
            healing.healed = false;
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Injuerd;
        }
        else if (State == HealthState.Down)
        {
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Down;
        }
        else if (State == HealthState.Carrying)
        {
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Carry;
        }
        else if (State == HealthState.Hook)
        {
            Prograss = 0;
            WorldShaderManager.Instance.SurvivorShader = WorldShaderManager.Survivor.Hooked;
            listManager.portraits[survivorRoomIdx].GetComponent<Portrait>().PortraitState = Portrait.State.Hook;
        }
    }

    // 플레이어 갈고리 걸린 횟수 관련 변수
    [Range(0, 2)]
    public int hook = 0;

    public int survivorRoomIdx;

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<SurviverController>();
        surviverAnimation = GetComponent<SurviverAnimation>();
        surviverLookAt = GetComponent<SurviverLookAt>();
        surviverSound = GetComponent<SurviverSound>();
        interaction = GetComponent<SurvivorInteraction>();
        shader = GetComponent<SurvivorShader>();
        listManager = SurvivorListManager.instance;
        healing = GetComponent<SurviverHealing>();
        hookEscape = GetComponent<SurvivorHookEscape>();
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
        if (State == HealthState.Healthy)
        {
            photonView.RPC(nameof(ChangeInjuerd), RpcTarget.All);
        }
        else if (State == HealthState.Injured)
        {
            photonView.RPC(nameof(ChangeDown), RpcTarget.All);
        }
    }

    [PunRPC]
    void ChangeInjuerd()
    {
        if(photonView.IsMine == false) return;
        State = HealthState.Injured;
        photonView.RPC("PlayAnimationRPC", RpcTarget.All, "Hit", 0.25f, 2);
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

    [PunRPC]
    void ChangeDown()
    {
        if (photonView.IsMine == false) return;
        surviverLookAt.LookAt = false;
        shader.RedXray = true;
        WorldShaderManager.Instance.SurvivorShader = WorldShaderManager.Survivor.OwnerDown;
        State = HealthState.Down;
        healing.healed = false;
        controller.Crawl = true;
        surviverSound.PlayDownSound();
        surviverAnimation.PlayStandToCrawl();
    }

    public void ChangeCarring()
    {
        surviverLookAt.LookAt = false;
        anim.speed = 1;
        if (State == HealthState.Carrying)
        {
            StartCoroutine(WaitAnimEnd());
        }
        else if(State == HealthState.Down)
        {
            controller.BanMove = true;
            StateNA = HealthState.Carrying;
            surviverAnimation.Play("PickUp_IN");
        }
    }

    public float yOffset = 2;

    IEnumerator WaitAnimEnd()
    {
        StateNA = HealthState.Hook;
        surviverAnimation.Play("Hook_IN");
        while (true)
        {
            if (surviverAnimation.IsAnimEnd("Hook_IN")) break;
            yield return null;
        }
        rootCameraPosition.position += new Vector3(0, yOffset, 0);
        hook++;
        surviverAnimation.Play("Hook_OUT");
        while (true)
        {
            if (surviverAnimation.IsAnimEnd("Hook_OUT")) break;
            yield return null;
        }
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

    Coroutine hookCor;

    void HookEscape()
    {
        if (State != HealthState.Hook) return;

        if (hookEscape.escaped || hookEscape.Escape) return;

        if(Prograss >= maxPrograssTime)
        {
            if(hookCor == null)
            {
                hookCor = StartCoroutine(WaitHook());
            }
            return;
        }
        photonView.RPC(nameof(AnimationWeight), RpcTarget.All, AnimationPrograss);

        if(escaping) { Prograss += Time.deltaTime; AnimationPrograss += Time.deltaTime * hookAnimationChangeRate; }
        else { Prograss -= Time.deltaTime; AnimationPrograss -= Time.deltaTime * hookAnimationChangeRate; }
        
        SurviverUI.instance.prograssBar.fillAmount = Prograss / maxPrograssTime;
    }

    [PunRPC]
    void AnimationWeight(float value)
    {
        anim.SetLayerWeight(3, value);
    }

    IEnumerator WaitHook()
    {
        rootCameraPosition.position -= new Vector3(0, yOffset, 0);
        surviverAnimation.Play("Hook_Free");
        photonView.RPC(nameof(AnimationWeight), RpcTarget.All, (float)0);
        while (true)
        {
            if (surviverAnimation.IsAnimEnd("Hook_Free")) break;
            yield return null;
        }
        controller.BanMove = false;
        WorldShaderManager.Instance.SurvivorShader = WorldShaderManager.Survivor.None;
        State = HealthState.Injured;
        controller.Crawl = false;
        photonView.RPC(nameof(ChangePose), RpcTarget.All);
        hookCor = null;
        shader.RedXray = false;
    }

    [PunRPC]
    void ChangePose()
    {
        surviverAnimation.Pose = SurviverAnimation.PoseState.Standing;
        surviverAnimation.anim.enabled = true;
    }
}
