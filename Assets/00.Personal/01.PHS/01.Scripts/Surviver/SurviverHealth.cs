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
        Down
    }

    public HealthState state = HealthState.Healthy;

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
        state = HealthState.Down;
        controller.Crawl = true;
        surviverLookAt.isLookAt = false;
        surviverSound.PlayDownSound();
        surviverAnimation.PlayStandToCrawl();
    }
}
