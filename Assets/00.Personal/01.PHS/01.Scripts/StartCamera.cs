using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StartCamera : MonoBehaviourPun
{
    public float rotationAngle = 0f;
    public float startRotationAngle = 180f;
    public float rotationTime = 10;
    public SurviverController surviverController;
    SurviverLookAt lookAt;

    Transform targetTrans;

    public Ease ease;
    public Ease textEase;

    IEnumerator Start()
    {
        lookAt = GetComponent<SurviverLookAt>();
        lookAt.LookAt = false;
        targetTrans = transform.GetChild(0); 
        if (photonView.IsMine)
        {
            targetTrans.GetChild(0).gameObject.SetActive(true);
        }
        targetTrans.rotation = Quaternion.Euler(0, startRotationAngle, 0);

        yield return new WaitForSeconds(1f);
        targetTrans.DORotate(new Vector3(0, 0, 0), rotationTime).SetEase(ease);
        yield return new WaitForSeconds(rotationTime);
        lookAt.LookAt = true;
    }

    private void Update()
    {
        if (targetTrans.rotation == Quaternion.Euler(0, rotationAngle, 0))
        {
            surviverController.enabled = true;
            this.enabled = false;
        }
    }
}
