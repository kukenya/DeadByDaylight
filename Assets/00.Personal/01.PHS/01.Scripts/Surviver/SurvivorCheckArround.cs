using JetBrains.Annotations;
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

    MonoBehaviour interactScript;
    public MonoBehaviour InteractScript { get { return interactScript; } set {
            interactScript = value;
            interaction.NullInteractScript();
            switch (type)
            {
                case InteractiveObject.Type.Window:
                    interaction.Type = SurvivorInteraction.InteractiveType.Window;
                    interaction.window = (Window)InteractScript;
                    break;
                case InteractiveObject.Type.Pallet:
                    interaction.Type = SurvivorInteraction.InteractiveType.Pallet;
                    interaction.pallet = (Pallet)InteractScript;
                    break;
                case InteractiveObject.Type.Generator:
                    interaction.Type = SurvivorInteraction.InteractiveType.Generator;
                    interaction.generator = (Generator)InteractScript;
                    break;
                case InteractiveObject.Type.Exit:
                    interaction.Type = SurvivorInteraction.InteractiveType.ExitLever;
                    interaction.exit = (Exit)InteractScript;
                    break;
                case InteractiveObject.Type.Survivor:
                    interaction.Type = SurvivorInteraction.InteractiveType.HealCamper;
                    interaction.camperHealing = (SurviverHealing)InteractScript;
                    break;
                case InteractiveObject.Type.CamperEscape:
                    interaction.Type = SurvivorInteraction.InteractiveType.EscapeCamper;
                    interaction.camperEscape = (SurvivorHookEscape)InteractScript;
                    break;
            }
        }
    }

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

    [Header("°Å¸®")]
    public float windowDist;
    public float palletDist;
    public float generatorDist;
    public float exitDist;
    public float healingDist;
    public float escapeDist;

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

            if (checkColliders[i].name.Contains("Healing") && checkColliders[i].GetComponentInParent<SurviverHealth>() != null)
            {
                SurviverHealth surviverHealth = checkColliders[i].GetComponentInParent<SurviverHealth>();
                if (surviverHealth.State != SurviverHealth.HealthState.Injured && surviverHealth.State != SurviverHealth.HealthState.Down)
                {
                    checkColliders[i] = null;
                    continue;
                }
            }
            else if (checkColliders[i].name.Contains("Escape") && checkColliders[i].GetComponentInParent<SurviverHealth>() != null)
            {
                SurviverHealth surviverHealth = checkColliders[i].GetComponentInParent<SurviverHealth>();
                if (surviverHealth.State != SurviverHealth.HealthState.Hook)
                {
                    checkColliders[i] = null;
                    continue;
                }
            }

            if (health.State == SurviverHealth.HealthState.Hook)
            {
                checkColliders[i] = null;
                continue;
            }

            //Ray ray = new Ray(transform.position, checkColliders[i].transform.position - transform.position);
            //if(Physics.SphereCast(ray, raySphereRadius, 4f) == false)
            //{
            //    checkColliders[i] = null;
            //    continue;
            //}
            Ray ray = new Ray(transform.position, checkColliders[i].transform.position - transform.position);
            Debug.DrawRay(transform.position, checkColliders[i].transform.position - transform.position, Color.red, 0.1f);
            if (Physics.Raycast(ray, out RaycastHit hit, 3f, layer))
            {
                if(hit.transform.gameObject.layer != 7)
                {
                    print(hit.transform.gameObject.name);
                    checkColliders[i] = null;
                    continue;
                }
            }

            if (health.State == SurviverHealth.HealthState.Down || health.State == SurviverHealth.HealthState.Carrying || health.State == SurviverHealth.HealthState.Hook)
            {
                InteractiveObject obj = checkColliders[i].GetComponent<InteractiveObject>();
                if (obj.type == InteractiveObject.Type.Pallet || obj.type == InteractiveObject.Type.Window)
                {
                    checkColliders[i] = null;
                    continue;
                }
            }

            if (checkColliders[i].GetComponent<InteractiveObject>() != null)
            {
                InteractiveObject ob = checkColliders[i].GetComponent<InteractiveObject>();
                switch (ob.type)
                {
                    case InteractiveObject.Type.Window:
                        if (CompareDist(windowDist, ob.transform.position)) checkColliders[i] = null;
                        break;
                    case InteractiveObject.Type.Pallet:
                        if (CompareDist(palletDist, ob.transform.position)) checkColliders[i] = null;
                        break;
                    case InteractiveObject.Type.Generator:
                        if (CompareDist(generatorDist, ob.transform.position)) checkColliders[i] = null;
                        break;
                    case InteractiveObject.Type.Exit:
                        if (CompareDist(exitDist, ob.transform.position)) checkColliders[i] = null;
                        break;
                    case InteractiveObject.Type.Survivor:
                        if (CompareDist(healingDist, ob.transform.position)) checkColliders[i] = null;
                        break;
                    case InteractiveObject.Type.CamperEscape:
                        if (CompareDist(escapeDist, ob.transform.position)) checkColliders[i] = null;
                        break;
                }
            }

            if(health.State == SurviverHealth.HealthState.Down || health.State == SurviverHealth.HealthState.Carrying || health.State == SurviverHealth.HealthState.Hook)
            {
                InteractiveObject obj = checkColliders[i].GetComponent<InteractiveObject>();
                if (obj.type == InteractiveObject.Type.Pallet || obj.type == InteractiveObject.Type.Window)
                {
                    checkColliders[i] = null;
                }
            }
        }

        //checkColliders.Sort(ColliderListSortComparer);
        System.Array.Sort(checkColliders, ColliderListSortComparer);

        if (checkColliders.Length == 0 || checkColliders[0] == null) 
        {
            interaction.NullInteractScript();
            if (health.State == SurviverHealth.HealthState.Injured && controller.Moving == false || health.State == SurviverHealth.HealthState.Down && controller.Moving == false)
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
            if (checkColliders[0].GetComponent<InteractiveObject>() == null) return;
            InteractiveObject ob = checkColliders[0].GetComponent<InteractiveObject>();
            type = ob.type;
            InteractScript = ob.interactScript;
        }
    }

    public InteractiveObject.Type type;

    public float raySphereRadius = 3f;
    public LayerMask layer;



    public bool CompareDist(float dist, Vector3 targetPos)
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance > dist) return true;
        else return false;
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
