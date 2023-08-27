using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeController : MonoBehaviour
{
    Volume volume;

    private void Start()
    {
        volume = GetComponent<Volume>();
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
