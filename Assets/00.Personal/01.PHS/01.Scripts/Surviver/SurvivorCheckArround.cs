using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SurvivorCheckArround : MonoBehaviourPun
{
    public float checkRadius;
    public Collider[] checkColliders = new Collider[10];

    public LayerMask checkLayer;
    public GameObject childCollider;

    SurvivorInteraction interaction;
    SurviverHealth health;
    SurviverController controller;

    private void Start()
    {
        interaction = GetComponent<SurvivorInteraction>();
        health = GetComponent<SurviverHealth>();
        controller = GetComponent<SurviverController>();
    }

    private void Update()
    {
        CheckArround();
    }

    void CheckArround() 
    {
        if (photonView.IsMine == false) return;

        Collider[] checkColliders = Physics.OverlapSphere(transform.position, checkRadius, checkLayer);
        this.checkColliders = checkColliders;

        for (int i = 0; i < checkColliders.Length; i++)
        {
            if (checkColliders[i] == null) continue;

            if (checkColliders[i].gameObject == childCollider)
            {
                checkColliders[i] = null;
                continue;
            }

            if (checkColliders[i].GetComponentInParent<SurviverHealth>().State != SurviverHealth.HealthState.Injured)
            {
                checkColliders[i] = null; 
                continue;
            }

            if (health.State == SurviverHealth.HealthState.Hook)
            {
                checkColliders[i] = null;
                continue;
            }

            if (Physics.Raycast(transform.position, checkColliders[i].transform.position - transform.position, 3f, Physics.AllLayers) == false)
            {
                checkColliders[i] = null;
            }
        }

        //checkColliders.Sort(ColliderListSortComparer);
        System.Array.Sort(checkColliders, ColliderListSortComparer);

        if (checkColliders[0] == null) 
        {
            if(health.State == SurviverHealth.HealthState.Injured && controller.Moving == false)
            {
                interaction.Type = SurvivorInteraction.InteractiveType.SelfHeal;
            }
            else if(health.State == SurviverHealth.HealthState.Hook)
            {
                interaction.Type = SurvivorInteraction.InteractiveType.HookEscape;
            }
            else
            {
                interaction.Type = SurvivorInteraction.InteractiveType.None;
            }
        }
        else
        {
            if (checkColliders[0] == null) { return; }
            InteractiveObject obj = checkColliders[0].GetComponent<InteractiveObject>();
            switch (obj.type)
            {
                case InteractiveObject.Type.Window:
                    interaction.Window = obj.window;
                    break;
                case InteractiveObject.Type.Pallet:
                    interaction.Pallet = obj.pallet;
                    break;
                case InteractiveObject.Type.Generator:
                    interaction.Generator = obj.generator;
                    break;
                case InteractiveObject.Type.Exit:
                    interaction.Exit = obj.exit;
                    break;
                case InteractiveObject.Type.Survivor:            
                    interaction.CamperHealing = obj.healing;
                    break;
            }
        }
    }






    // ETC
    int ColliderListSortComparer(Collider A, Collider B)
    {
        if (A == null && B != null)
        {
            return 1;
        }
        else if (A != null && B == null)
        {
            return -1;
        }
        else if (A == null && B == null)
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(transform.position, A.transform.position).CompareTo(Vector3.Distance(transform.position, B.transform.position));
        }
    }
}
