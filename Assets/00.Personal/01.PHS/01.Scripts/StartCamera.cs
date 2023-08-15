using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartCamera : MonoBehaviourPun
{
    public float rotationAngle = 0f;
    public float startRotationAngle = 180f;
    public float rotationTime = 10;
    public SurviverController surviverController;

    Transform targetTrans;

    public Ease ease;
    public Ease textEase;

    private void Start()
    {
        targetTrans = transform.GetChild(0); 
        if (photonView.IsMine)
        {
            targetTrans.GetChild(0).gameObject.SetActive(true);
        }
        targetTrans.rotation = Quaternion.Euler(0, startRotationAngle, 0);
        targetTrans.DORotate(new Vector3(0, 0, 0), rotationTime).SetEase(ease);
    }

    private void Update()
    {
        
        //transform.Rotate(0, 10 * Time.deltaTime, 0);
        //transform.rotation = Quaternion.Euler(0, Mathf.SmoothDamp(transform.rotation.eulerAngles.y, rotationAngle, ref rotationVelocity, 5), 0);
        if (targetTrans.rotation == Quaternion.Euler(0, rotationAngle, 0))
        {
            surviverController.enabled = true;
            this.enabled = false;
        }
    }
}
