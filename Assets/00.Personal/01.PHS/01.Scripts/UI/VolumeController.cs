using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class VolumeController : MonoBehaviour
{
    Volume volume;
    ScreenSpaceReflections screenSpaceReflections;

    private void Start()
    {
        volume = GetComponent<Volume>();
        //screenSpaceReflections = volume.GetComponent<ScreenSpaceReflections>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (volume.enabled == true)
            {
                OffVolume();
            }
            else
            {
                OnVolume();
            }
        }
        //else if (Input.GetKeyDown(KeyCode.Alpha8))
        //{
        //    if(screenSpaceReflections.active == true)
        //    {
        //        screenSpaceReflections.active = false;
        //    }
        //    else
        //    {
        //        screenSpaceReflections.active = true;
        //    }
        //}
    }
    public void OnVolume()
    {
        volume.enabled = true;
    }

    public void OffVolume()
    {
        volume.enabled = false;
    }
}
