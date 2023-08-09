using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    public Transform jumpPos1;
    public Transform jumpPos2;

    SurvivorInteraction interaction;

    private void OnTriggerEnter(Collider other)
    {
        SurviverUI.instance.FocusSpaceBarUI();
        interaction = other.GetComponent<SurvivorInteraction>();
        interaction.ChangeInteract(SurvivorInteraction.InteractiveType.Window, this);
    }

    private void OnTriggerStay(Collider other)
    {
        float dist = Vector3.Distance(jumpPos1.position, other.transform.position);
        float dist2 = Vector3.Distance(jumpPos2.position, other.transform.position);

        interaction.Position = dist < dist2 ? jumpPos1 : jumpPos2;

    }

    private void OnTriggerExit(Collider other)
    {
        SurviverUI.instance.UnFocusSpaceBarUI();
        interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
    }
}
