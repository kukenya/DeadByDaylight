using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverAutoMove : MonoBehaviour
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

    private void Update()
    {

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
            Vector3 moveDirection = (new Vector3(targetTrans.position.x, 0, targetTrans.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetTrans.eulerAngles.y, transform.eulerAngles.z);
            if (Vector3.Distance(new Vector3(targetTrans.position.x, 0, targetTrans.position.z), new Vector3(transform.position.x, 0, transform.position.z)) > autoMoveStopDist)
            {
                transform.position += moveDirection * autoMoveSpeed * Time.deltaTime;
            }
            else
            {
                break;
            }

           
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
            Vector3 moveDirection = (new Vector3(targetTrans.position.x, 0, targetTrans.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
            float targetAngle = reverse == false ? targetTrans.eulerAngles.y : -targetTrans.eulerAngles.y;
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z);

            if (Vector3.Distance(new Vector3(targetTrans.position.x, 0, targetTrans.position.z), new Vector3(transform.position.x, 0, transform.position.z)) > autoMoveStopDist)
            {
                transform.position += moveDirection * autoMoveSpeed * Time.deltaTime;
            }
            else
            {
                break;
            }
            
            yield return null;
        }
        action?.Invoke();
    }
}
