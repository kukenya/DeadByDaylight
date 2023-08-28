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

    public float maxRandomRange = 5f;
    public float sphereSize = 1f;

    private void Update()
    {
        if (controller.Sprint)
        {
            if (controller.BanMove) return;
            currentTime += Time.deltaTime;
            if(currentTime >= generateTime)
            {
                currentTime = 0;
                for (int i = 0; i < Random.Range(0, maxRandomRange); i++)
                {
                    GameObject go = Instantiate(footStepDecal, generatePos.position + Random.insideUnitSphere * sphereSize, Quaternion.Euler(0, transform.rotation.y, transform.rotation.z)) ;
                    Destroy(go, decalRemainTime);
                }
            }
        }
    }
}
