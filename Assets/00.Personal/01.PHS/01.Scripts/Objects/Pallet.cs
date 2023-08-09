using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pallet : MonoBehaviour
{
    public enum State
    {
        Stand,
        Ground
    }

    public State state;
    public LayerMask layerMask;

    public Animator anim;
    string currentState;

    public Transform animPos1;
    public Transform animPos2;

    SurvivorInteraction interaction;

    public void Play(string state, float time = 0.1f)
    {
        if (state == currentState) return;


        anim.enabled = true;
        anim.CrossFadeInFixedTime(state, time, 0);

        currentState = state;
    }

    private void OnTriggerStay(Collider other)
    {
        interaction = other.GetComponent<SurvivorInteraction>();
        if (Physics.Raycast(other.transform.position, transform.position - other.transform.position, out RaycastHit hitInfo, 3, layerMask))
        {
            if(hitInfo.transform.gameObject.layer == 7)
            {
                SurviverUI.instance.FocusSpaceBarUI();
                float dist = Vector3.Distance(animPos1.position, other.transform.position);
                float dist2 = Vector3.Distance(animPos2.position, other.transform.position);

                interaction.ChangeInteract(SurvivorInteraction.InteractiveType.Pallet, this, dist < dist2 ? animPos1 : animPos2);
            }
            else
            {
                SurviverUI.instance.UnFocusSpaceBarUI();
                interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SurviverUI.instance.UnFocusSpaceBarUI();
        interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
    }
}
