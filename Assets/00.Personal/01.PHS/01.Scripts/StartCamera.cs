using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartCamera : MonoBehaviour
{
    public float rotationAngle = 0f;
    public float startRotationAngle = 180f;
    public float rotationTime = 10;

    float rotationVelocity;

    public SurviverController surviverController;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, startRotationAngle, 0);
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0, Mathf.SmoothDamp(startRotationAngle, rotationAngle, ref rotationVelocity, 5), 0);
        if (transform.rotation == Quaternion.Euler(0, rotationAngle, 0))
        {
            surviverController.enabled = true;
            this.enabled = false;
        }
    }
}
