using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverHealing : MonoBehaviour
{
    SurviverController surviverController;
    SurviverAnimation surviverAnimation;
    SurviverHealth health;
    public SurvivorInteraction interaction;
    SurviverUI ui;
    SkillCheck skillCheck;

    float prograss;


    public float Prograss { get { return prograss; } set { prograss = value; } }
    public float maxPrograssTime;
    public bool healing = false;
    public bool Heal
    {
        get { return healing; }
        set
        {
            healing = value;
            if (healing == false) skillCheck.EndRandomSkillCheck();
            else skillCheck.StartRandomSkillCheck(GetSkillCheckValue);
        }
    }

    public bool healed = false;

    public float normalValue = 2f;
    public float hardValue = 5f;

    private void Start()
    {
        surviverController = GetComponent<SurviverController>();
        surviverAnimation = GetComponent<SurviverAnimation>();
        health = GetComponent<SurviverHealth>();
        ui = SurviverUI.instance;
        skillCheck = SkillCheck.Instance;
    }

    private void Update()
    {
        Healing();
    }

    void GetSkillCheckValue(int value)
    {
        switch (value)
        {
            case 0:
                SkillCheckFail();
                break;
            case 1:
                Prograss += normalValue;
                break;
            case 2:
                Prograss += hardValue;
                break;
        }
    }

    void Healing()
    {
        if (healing == false) return;

        if (Prograss >= maxPrograssTime)
        {
            OffSelfHeal();
            health.State = SurviverHealth.HealthState.Healthy;
            return;
        }

        Prograss += Time.deltaTime;
        ui.prograssBar.fillAmount = Prograss / maxPrograssTime;
    }

    public void OnSelfHeal(SurvivorInteraction interaction)
    {
        this.interaction = interaction;
        surviverController.BanMove = true;
        surviverAnimation.Play("Healing_Self");
        Heal = true;
    }

    public void OffSelfHeal()
    {
        surviverController.BanMove = false;
        Heal = false;
    }

    public void OnFriendHeal(SurvivorInteraction interaction)
    {
        this.interaction = interaction;
        surviverController.BanMove = true;
        surviverAnimation.Play("Being_Heal");
        Heal = true;
    }

    public void OffFriendHeal()
    {
        surviverController.BanMove = false;
        Heal = false;
    }

    public float failValue = 5f;

    void SkillCheckFail()
    {
        Prograss += failValue;
        //failAudio.Play();
        surviverAnimation.Play("Healing_Self_Fail");
    }
}
