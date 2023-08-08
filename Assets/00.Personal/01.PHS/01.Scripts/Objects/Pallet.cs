using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pallet : MonoBehaviour
{
    public enum State{
        Stand,
        Ground
    }

    public State state;

    Animator anim;
    string currentState;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Play(string state, float time = 0.1f)
    {
        if (state == currentState) return;


        anim.enabled = true;
        anim.CrossFadeInFixedTime(state, time, 0);

        currentState = state;
    }
}
