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

    public void OnAutoMove(Transform targetTrans, System.Action<float> action, float targetAngle)
    {
        this.targetTrans = targetTrans;
        StartCoroutine(AutoMoveCor(action, targetAngle));
    }

    public void OnAutoMove(Transform targetTrans, System.Action action)
    {
        this.targetTrans = targetTrans;
        StartCoroutine(AutoMoveCor(action));
    }

    IEnumerator AutoMoveCor(System.Action<float> action, float targetAngle)
    {
        surviverController.banMove = true;
        while (true)
        {
            Vector3 moveDirection = (targetTrans.position - transform.position).normalized;

            if (Vector3.Distance(transform.position, targetTrans.position) > autoMoveStopDist)
            {
                controller.Move(moveDirection * autoMoveSpeed * Time.deltaTime);
            }
            else
            {
                break;
            }

            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetTrans.eulerAngles.y, transform.eulerAngles.z);
            yield return null;
        }
        action?.Invoke(targetAngle);
    }

    IEnumerator AutoMoveCor(System.Action action)
    {
        surviverController.banMove = true;
        while (true)
        {
            Vector3 moveDirection = (targetTrans.position - transform.position).normalized;

            if (Vector3.Distance(transform.position, targetTrans.position) > autoMoveStopDist)
            {
                controller.Move(moveDirection * autoMoveSpeed * Time.deltaTime);
            }
            else
            {
                break;
            }

            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetTrans.eulerAngles.y, transform.eulerAngles.z);
            yield return null;
        }
        action?.Invoke;
    }
}
