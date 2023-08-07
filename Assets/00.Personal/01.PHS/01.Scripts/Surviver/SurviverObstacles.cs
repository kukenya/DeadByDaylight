using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using static SurviverAnimation;

public class SurviverObstacles : MonoBehaviour
{
    public float possibleAngle = 120f;

    public float autoMoverStopDistance = 0.2f;
    public float autoMoverSpeed = 4.0f;
    public Transform currentObstacleTrans;

    public Transform Obstacle { get { return currentObstacleTrans; } set { currentObstacleTrans = value; } }

    SurviverAnimation surviverAnimation;
    CharacterController controller;
    SurviverController surviverController;

    private void Start()
    {
        surviverAnimation = SurviverAnimation.instance;
        controller = GetComponent<CharacterController>();
        surviverController = GetComponent<SurviverController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 targetDir = currentObstacleTrans.position - transform.position;
            float targetAngle = Vector3.Angle(transform.forward, new Vector3(targetDir.x, 0, targetDir.z));
            if(targetAngle < midJumpAngle)
            {
                StartCoroutine(AutoMove(targetAngle));
            }
        }
    }

    IEnumerator AutoMove(float targetAngle)
    {
        surviverController.banMove = true;
        while (true)
        {
            Vector3 moveDirection = (currentObstacleTrans.position - transform.position).normalized;

            if (Vector3.Distance(transform.position, currentObstacleTrans.position) > autoMoverStopDistance)
            {
                controller.Move(moveDirection * autoMoverSpeed * Time.deltaTime);
            }
            else
            {
                break;
            }

            transform.rotation = Quaternion.Euler(transform.rotation.x, Mathf.MoveTowardsAngle(transform.rotation.y, currentObstacleTrans.rotation.y, Time.deltaTime), transform.rotation.z);
            yield return null;
        }
        JumpWindow(targetAngle);
    }

    [Header("창문 점프 각도")]
    public float fastJumpAngle = 50f;
    public float midJumpAngle = 90f;

    void JumpWindow(float targetAngle)
    {
        if(surviverController.Sprint == false)
        {
            surviverAnimation.Play("WindowIn");
            StartCoroutine(WaitAnimEnd("WindowJump"));
            return;
        }

        if (targetAngle < fastJumpAngle)
        {
            surviverAnimation.Play("WindowFast");
            StartCoroutine(WaitAnimFast());
        }
        else if(targetAngle < midJumpAngle)
        {
            surviverAnimation.Play("WindowMid");
            StartCoroutine(WaitAnimEnd("WindowMid"));
        }
        
    }

    public float a;

    IEnumerator WaitAnimFast()
    {
        surviverController.banMove = true;
        float currentTime = 0;
        while (true)
        {
            currentTime += Time.deltaTime;
            print(currentTime);
            if (currentTime > a)
            {
                controller.Move(transform.forward * 4.0f * Time.deltaTime);
            }
            if (surviverAnimation.IsAnimEnd("WindowFast")) break;
            yield return null;
        }
        surviverController.banMove = false;
        surviverAnimation.AnimationChange();
    }

    IEnumerator WaitAnimEnd(string animName)
    {
        surviverController.banMove = true;
        while (true)
        {
            if (surviverAnimation.IsAnimEnd(animName)) break;
            yield return null;
        }
        surviverController.banMove = false;
        surviverAnimation.AnimationChange();
    }

    void JumpPannel()
    {

    }

    void DownPannel()
    {

    }
}
