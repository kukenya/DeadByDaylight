using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public Image[] hookImage;

    public void SetHookImgae(int hook)
    {
        if(hook == 1)
        {
            hookImage[0].color = Color.white;
        }
        else if(hook == 2)
        {
            hookImage[1].color = Color.white;
        }
    }
}
