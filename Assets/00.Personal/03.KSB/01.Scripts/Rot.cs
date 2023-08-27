using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rot : MonoBehaviour
{
    public Transform cameraOffset;
    public GameObject torso;

    float my;
    float rotY;
    float rotSpeed = 200;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float my = Input.GetAxis("Mouse Y");
        rotY += my * rotSpeed * Time.deltaTime;

        //torso.transform.eulerAngles = new Vector3(rotY, 0, 0);


        transform.position = cameraOffset.position;
        transform.eulerAngles = cameraOffset.eulerAngles;
        //transform.forward = cameraOffset.forward;
    }
}
