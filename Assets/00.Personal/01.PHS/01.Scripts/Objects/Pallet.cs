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


    void ChangeState()
    {
        state = State.Ground;
    }
}
