using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnaMove : MonoBehaviour
{
    #region 싱글톤 및 상태
    public static AnnaMove instance;
    private void Awake()
    {
        instance = this;
    }

    //public enum State
    //{
    //    Idle, Move, TwoHandAttack, OneHandAttack, Stunned
    //}

    //public State state;
    #endregion

    #region 변수
    Animator anim;                          // 애니메이터
    CharacterController cc;                 // 캐릭터 컨트롤러

    // 이동 속도
    float speed = 4.4f;                     // 기본 이동속도
    float readySpeed = 3.08f;               // 차징 시 이동속도
    float delaySpeed = 3.74f;               // 경직 시 이동속도

    // 회전
    float rotX;                             // X 회전값
    float rotY;                             // Y 회전값
    public float rotSpeed = 100;            // 회전속도
    public Transform cam;                   // 카메라 Transform

    // 시간
    float currentTime;                      // 현재 시간
    float currentChargingTime;              // 현재 차징 시간
    float minimumChargingTime = 1.25f;      // 최소 차징 시간
    float maximumChargingTime = 3;          // 최대 차징 시간
    float axeRechargingTime = 4;            // 도끼 충전 시간

    // 카운트
    float currentAxeCount;                  // 현재 가지고 있는 한손도끼 개수
    float maxAxeCount = 5;                  // 최대 소유 가능한 한손도끼 개수

    // 한손 도끼
                                            // 한손도끼 SkinnedMeshRenderer 컴포넌트
                                            // 한손도끼 Rigidbody 컴포넌트
    #endregion


    void Start()
    {
        anim = GetComponent<Animator>();            // 안나 Animator 컴포넌트
        cc = GetComponent<CharacterController>();   // 안나 Rigidbody 컴포넌트
    }

    void Update()
    {
        #region switch
        //switch (state)
        //{
        //    case State.Idle:                                break;
        //    case State.Move:                                break;
        //    case State.TwoHandAttack:   TwoHandAttack();    break;
        //    case State.OneHandAttack:                       break;
        //    case State.Stunned:                             break;
        //    default:                                        break;
        //}
        #endregion
        
        #region 회전
        // 회전값을 받아온다.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // 회전값을 누적
        rotX += mx * rotSpeed * Time.deltaTime;
        rotY += my * rotSpeed * Time.deltaTime;

        // 회전값을 적용
        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);

        // 윗방향 최대각
        if(rotY >= 20)
        {
            rotY = 20;
        }

        // 아래방향 최대각
        else if(rotY <= -10)
        {
            rotY = -10;
        }
        #endregion

        #region 이동
        // 이동값 받아온다
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 이동값을 애니메이션과 연결
        anim.SetFloat("h", h);
        anim.SetFloat("v", v);

        // 방향을 구한다
        Vector3 dirH = transform.right * h;
        Vector3 dirV = transform.forward * v;
        Vector3 dir = dirH + dirV;
        dir.Normalize();

        // 이동한다
        // transform.position += dir * speed * Time.deltaTime;
        cc.Move(dir * speed * Time.deltaTime);
        #endregion

        if (Input.GetButtonDown("Fire1"))
        {
            TwoHandAttack();
        }
    }
    private void TwoHandAttack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            cc.enabled = false;
            print(nameof(TwoHandAttack));
            anim.SetTrigger("Attack");
        }
    }
    public void Finish_Action()
    {
        cc.enabled = true;
    }
}
