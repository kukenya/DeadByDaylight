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

    private void Start()
    {
        surviverController = GetComponent<SurviverController>();
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
        surviverController.BanMove = true;
        while (true)
        {
            print(1);
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

    public float friendHealAutoStopDist = 0.5f;

    public IEnumerator FriendHealingAutoMoveCor(System.Action<SurvivorInteraction> action, System.Action action2, SurvivorInteraction interaction, Transform moveTrans)
    {
        surviverController.BanMove = true;
        while (true)
        {
            transform.rotation = Quaternion.LookRotation(moveTrans.position - transform.position);
            if (Vector3.Distance(
                new Vector3(moveTrans.position.x, 0, moveTrans.position.z),
                new Vector3(transform.position.x, 0, transform.position.z)) < friendHealAutoStopDist) break;

            transform.position = Vector3.MoveTowards(
                   transform.position,
                   new Vector3(moveTrans.position.x, transform.position.y, moveTrans.position.z),
                   Time.deltaTime * autoMoveSpeed);

            yield return null;
        }
        action?.Invoke(interaction);
        action2?.Invoke();
    }
}
