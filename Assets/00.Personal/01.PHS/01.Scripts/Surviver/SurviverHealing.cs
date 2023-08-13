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

    float prograss;
    public float Prograss { get { return prograss; } set { prograss = value; } }
    public float maxPrograssTime;
    public bool healing = false;

    public bool healed = false;

    private void Start()
    {
        surviverController = GetComponent<SurviverController>();
        surviverAnimation = GetComponent<SurviverAnimation>();
        health = GetComponent<SurviverHealth>();
        ui = SurviverUI.instance;
    }

    private void Update()
    {
        Healing();
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
        healing = true;
    }

    public void OffSelfHeal()
    {
        surviverController.BanMove = false;
        healing = false;
    }
}
