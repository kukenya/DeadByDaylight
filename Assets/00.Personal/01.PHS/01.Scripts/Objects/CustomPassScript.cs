using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class CustomPassScript : MonoBehaviour
{
    public CustomPassVolume CustomPassVolume;
    CustomPass hacksPass;

    private void Start()
    {
        CustomPassVolume = GetComponent<CustomPassVolume>();
        foreach (var pass in CustomPassVolume.customPasses)
        {
            if (pass.name == "HacksOn") hacksPass = pass;
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if(hacksPass.enabled == true)
            {
                hacksPass.enabled = false;
            }
            else
            {
                hacksPass.enabled = true;
            }
        }
    }
}
