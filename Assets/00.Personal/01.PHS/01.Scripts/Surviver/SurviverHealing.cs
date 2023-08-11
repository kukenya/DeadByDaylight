using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverHealing : MonoBehaviour
{
    SurviverController surviverController;
    SurviverAnimation surviverAnimation;
    public float healingGauge = 0;

    private void Start()
    {
        surviverController = GetComponent<SurviverController>();
        surviverAnimation = GetComponent<SurviverAnimation>();
    }

    public void OnSelfHeal()
    {
        surviverController.BanMove = true;
        surviverAnimation.Play("Healing_Self");
    }

    public void OffSelfHeal()
    {
        surviverController.BanMove = false;
    }
}
