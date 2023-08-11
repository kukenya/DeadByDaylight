using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public enum State
    {
        Closed,
        Open
    }

    public State state = State.Closed;
    public Transform animPos;
    public Animator anim;

    float prograss;
    public float Prograss { get { return prograss; } set { prograss = Mathf.Clamp(value, 0, maxPrograssTime); } }
    public float maxPrograssTime = 10;

    SurviverUI ui;
    SurvivorInteraction interaction;


    public void OnSwitch()
    {
        activate = true;
        Play("SwitchActivate");
    }

    public void OffSwitch()
    {
        activate = false;
        if (state == State.Closed) Play("Closed");
    }

    string currentState;

    public void Play(string state, float time = 0.1f)
    {
        if (state == currentState) return;


        anim.enabled = true;
        anim.CrossFadeInFixedTime(state, time, 0);

        currentState = state;
    }

    private void Start()
    {
        ui = SurviverUI.instance;
    }

    private void Update()
    {
        ExitActivate();
    }

    public bool activate = false;

    public void ExitActivate()
    {
        if (activate == false) return;
        if (prograss >= maxPrograssTime)
        {
            Play("Opening");
            state = State.Open;
            ui.UnFocusProgressUI();
            interaction.EndInteract(SurvivorInteraction.InteractiveType.ExitLever);
        }

        ui.OnProgressUI();
        Prograss += Time.deltaTime;
        ui.prograssBar.fillAmount = Prograss / maxPrograssTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state == State.Open) return;

        ui.FocusProgressUI("√‚±∏");
        ui.prograssBar.fillAmount = Prograss / maxPrograssTime;
        interaction = other.GetComponent<SurvivorInteraction>();
        interaction.ChangeInteract(SurvivorInteraction.InteractiveType.ExitLever, this, animPos);
    }

    private void OnTriggerExit(Collider other)
    {
        if (state == State.Open) return;

        ui.UnFocusProgressUI();
        interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
    }
}
