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

    public enum State
    {
        Move, TwoHandAttack, OneHandAttack, Stunned
    }

    public State state;
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
    public GameObject smallAxe;             // 한손도끼 GameObject
    public GameObject smallAxeFactory;      // 투척용 한손도끼 프리팹
                                            // 한손도끼 Rigidbody 컴포넌트
    public float axeSpeed = 25;             // 투사체 속도
    #endregion


    void Start()
    {
        anim = GetComponent<Animator>();            // 안나 Animator 컴포넌트
        cc = GetComponent<CharacterController>();   // 안나 Rigidbody 컴포넌트
        anim.SetLayerWeight(1, 0);                  // 
        smallAxe.SetActive(false);
    }

    void Update()
    {
        #region switch
        switch (state)
        {
            case State.Move: UpdateMove(); break;
            case State.TwoHandAttack: TwoHandAttack(); break;
            case State.OneHandAttack: OneHandAttack(); break;
            case State.Stunned: break;
            default: UpdateMove(); break;
        }
        #endregion
    }

    private void UpdateMove()
    {
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
        if (rotY >= 20)
        {
            rotY = 20;
        }

        // 아래방향 최대각
        else if (rotY <= -15)
        {
            rotY = -15;
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
        cc.Move(dir * speed * Time.deltaTime);
        #endregion

        // 마우스 왼쪽 버튼을 누르면 두손 공격을 한다.
        if (Input.GetButtonDown("Fire1") && state != State.OneHandAttack)
        {
            state = State.TwoHandAttack;
            anim.SetTrigger("Attack");
        }

        // 마우스 오른쪽 버튼을 누르면 한손 공격을 시작한다.
        if (Input.GetButton("Fire2") && state != State.TwoHandAttack)
        {
            anim.SetBool("Throwing", true);
        }
    }

    private void TwoHandAttack()
    {
        #region 회전
        // 회전값을 받아온다.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // 회전값을 누적
        rotX += mx * 10 * Time.deltaTime;
        rotY += my * 10 * Time.deltaTime;

        // 회전값을 적용
        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);

        // 윗방향 최대각
        if (rotY >= 10)
        {
            rotY = 10;
        }

        // 아래방향 최대각
        else if (rotY <= -10)
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
        cc.Move(dir * 0.5f * Time.deltaTime);
        #endregion
    }

    private void OneHandAttack()
    {
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
        if (rotY >= 20)
        {
            rotY = 20;
        }

        // 아래방향 최대각
        else if (rotY <= -15)
        {
            rotY = -15;
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
        cc.Move(dir * readySpeed * Time.deltaTime);
        #endregion

        if (Input.GetButton("Fire2"))
        {
            // 현재시간을 누적한다.
            currentTime += Time.deltaTime;

            print(currentTime);
            // 투사체 속도는 최소 25m/s , 최대 40m/s
            // 최소차징시간은 1.25초 최대차징시간은 3초

            // 마우스 오른쪽 버튼에서 손을 떼면
            if (Input.GetButtonDown("Fire1"))
            {
                // 동작을 취소한다.
                state = State.Move;
                anim.SetBool("Throwing", false);
                currentTime = 0;
            }
        }

        if (Input.GetButtonUp("Fire2"))
        {
            anim.SetTrigger("Throw");
            // 한손 도끼를 던진다.
            GameObject sa = Instantiate(smallAxeFactory);
            sa.transform.position = this.transform.position;
            sa.transform.forward = this.transform.forward;

            currentTime = 0;
        }
    }


    #region Events

    public void OnmyOneHandAttack()
    {
        state = State.OneHandAttack;
    }

    void OnMyReset()                // State 를 UpdateMove 로 초기화하는 함수
    {
        state = State.Move;
        anim.SetBool("Throwing", false);
    }

    public void Finish_Action()     // CharacterController 활성화 함수
    {
        cc.enabled = true;
    }

    public void OnSmallAxe()        // 도끼 활성화
    {
        smallAxe.SetActive(true);
    }

    public void OffSmallAxe()       // 도끼 비활성화
    {
        smallAxe.SetActive(false);
    }
    #endregion
}
