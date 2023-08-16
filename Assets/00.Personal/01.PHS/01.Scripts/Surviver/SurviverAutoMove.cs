using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverAutoMove : MonoBehaviourPun
{
    public float autoMoveStopDist = 0.2f;
    public float autoMoveSpeed = 8;

    Transform targetTrans;
    SurviverController surviverController;
    CharacterController controller;

    private void Start()
    {
        surviverController = GetComponent<SurviverController>();
        controller = GetComponent<CharacterController>();
    }

    Coroutine cor;

    public void StopCoroutine()
    {
        if(cor != null) StopCoroutine(cor);
    }

    public void OnAutoMove(Transform targetTrans, System.Action<float> action, float targetAngle)
    {
        this.targetTrans = targetTrans;
        StartCoroutine(AutoMoveCor(action, targetAngle));
    }

    public void OnAutoMove(Transform targetTrans, System.Action action, bool reverse = false)
    {
        this.targetTrans = targetTrans;
        cor = StartCoroutine(AutoMoveCor(action, reverse));
    }

    IEnumerator AutoMoveCor(System.Action<float> action, float targetAngle)
    {
        controller.enabled = false;
        surviverController.BanMove = true;
        while (true)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetTrans.eulerAngles.y, transform.eulerAngles.z);
            if (Vector3.Distance(
                new Vector3(targetTrans.position.x, 0, targetTrans.position.z),
                new Vector3(transform.position.x, 0, transform.position.z)) < autoMoveStopDist) break;

            transform.position = Vector3.MoveTowards(
                   transform.position,
                   new Vector3(targetTrans.position.x, transform.position.y, targetTrans.position.z),
                   Time.deltaTime * autoMoveSpeed);
            yield return null;
        }
        action?.Invoke(targetAngle);
    }

    IEnumerator AutoMoveCor(System.Action action, bool reverse)
    {
        controller.enabled = false;
        surviverController.BanMove = true;
        while (true)
        {
            float targetAngle = reverse == false ? targetTrans.eulerAngles.y : targetTrans.eulerAngles.y - 180;
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z);
            if (Vector3.Distance(
                new Vector3(targetTrans.position.x, 0, targetTrans.position.z),
                new Vector3(transform.position.x, 0, transform.position.z)) < autoMoveStopDist) break;

            transform.position = Vector3.MoveTowards(
                   transform.position,
                   new Vector3(targetTrans.position.x, transform.position.y, targetTrans.position.z),
                   Time.deltaTime * autoMoveSpeed);

            yield return null;
        }
        action?.Invoke();
    }
}
