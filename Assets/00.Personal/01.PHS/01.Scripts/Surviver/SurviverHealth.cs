using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverHealth : MonoBehaviour
{
    public SurviverController controller;
    SurviverAnimation surviverAnimation;
    SurviverLookAt surviverLookAt;
    SurviverSound surviverSound;

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
            state = value;
            surviverAnimation.Injuerd = false;
            surviverAnimation.AnimationChange();
        } }

    // 플레이어 갈고리 걸린 횟수 관련 변수
    [Range(0, 2)]
    public int hook = 0;

    private void Start()
    {
        controller = GetComponent<SurviverController>();
        surviverAnimation = GetComponent<SurviverAnimation>();
        surviverLookAt = GetComponent<SurviverLookAt>();
        surviverSound = GetComponent<SurviverSound>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            NormalHit();
        }else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeCarring();
        }
    }

    void NormalHit()
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
        surviverAnimation.Injuerd = true;
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

    void ChangeCarring()
    {
        controller.BanMove = true;
        state = HealthState.Carrying;
        surviverAnimation.Play("PickUp_IN");
    }
}
