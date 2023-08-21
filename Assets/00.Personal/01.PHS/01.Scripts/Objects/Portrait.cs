using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Portrait : MonoBehaviour
{
    public enum State
    {
        Healthy,
        Injuerd,
        Down,
        Carry,
        Hook
    }
    State state = State.Healthy;
    public State PortraitState { get { return state; } set { 
            state = value;
            foreach (GameObject go in stateImage)
            {
                go.SetActive(false);
            }
            stateImage[(int)state].SetActive(true);
        } 
    }

    public TextMeshProUGUI survivorName;
    public GameObject[] stateImage;
}
