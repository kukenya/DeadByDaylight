using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorFootStepDecal : MonoBehaviour
{
    public GameObject footStepDecal;
    public Transform generatePos;

    SurviverController controller;

    float currentTime = 0;
    public float generateTime = 1;
    public float decalRemainTime = 3;

    private void Start()
    {
        controller = GetComponent<SurviverController>();
    }

    private void Update()
    {
        if (controller.Sprint)
        {
            currentTime += Time.deltaTime;
            if(currentTime >= generateTime)
            {
                currentTime = 0;
                GameObject go = Instantiate(footStepDecal, generatePos.position, Quaternion.Euler(0, transform.rotation.y, transform.rotation.z));
                Destroy(go, decalRemainTime);
            }
        }
    }
}
