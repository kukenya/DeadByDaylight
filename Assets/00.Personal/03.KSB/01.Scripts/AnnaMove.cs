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
        Idle, Move, NormalAttack, ThrowingAttack, CoolTime, Carry, CarryAttack, Stunned
    }

    public State state;
    #endregion

    #region 변수
    Animator anim;                          // 애니메이터
    CharacterController cc;                 // 캐릭터 컨트롤러

    // 이동 속도
    float currentSpeed;                     // 현재 이동속도
    float normalSpeed = 4.4f;               // 기본 이동속도
    float readySpeed = 3.08f;               // 차징 시 이동속도
    float delaySpeed = 3.74f;               // 경직 시 이동속도

    // 회전
    float rotX;                             // X 회전값
    float rotY;                             // Y 회전값
    public float rotSpeed = 100;            // 회전속도
    public Transform cam;                   // 카메라 Transform

    // 시간
    public float gameStartTime;             // 게임시작까지 걸리는 시간
    float currentTime;                      // 현재 시간
    float currentChargingTime;              // 현재 차징 시간
    float minimumChargingTime = 1.25f;      // 최소 차징 시간
    float maximumChargingTime = 3;          // 최대 차징 시간
    float axeRechargingTime = 4;            // 도끼 충전 시간

    // 카운트
    float currentAxeCount;                  // 현재 가지고 있는 한손도끼 개수
    float maxAxeCount = 5;                  // 최대 소유 가능한 한손도끼 개수
    float notMoveCurrentTime;

    // 한손 도끼
    public GameObject smallAxe;             // 한손도끼 GameObject
    public GameObject smallAxeFactory;      // 투척용 한손도끼 프리팹
    public Transform throwingSpot;          // 한손도끼 날아갈 위치
    public float chargingForce;             // 한손도끼 던지는 힘
    public float minAxePower = 20;          // 한손도끼 던지는 힘 최소값
    public float maxAxePower = 40;          // 한손도끼 던지는 힘 최대값

    // bool
    bool isNormalAttack;                    // 일반공격 중인가?
    bool isCharging;                        // 차징 중인가?
    bool isCanceled;                        // 차징을 취소했는가?
    bool isThrowing;                        // 손도끼를 던졌는가?
    #endregion

    #region Start & Update
    void Start()
    {
        anim = GetComponent<Animator>();            // 안나 Animator 컴포넌트
        cc = GetComponent<CharacterController>();   // 안나 Rigidbody 컴포넌트
        anim.SetLayerWeight(1, 0);                  // 애니메이션 레이어
        anim.SetLayerWeight(2, 0);                  // 애니메이션 레이어
        smallAxe.SetActive(false);                  // 왼손에 들고 있는 한손도끼 렌더러 비활성화
        notMoveCurrentTime = 0f;
    }

    void Update()
    {
       print(currentSpeed);

        switch (state)
        {
            case State.Idle: Idle(); break;
            case State.Move: UpdateMove(); break;
            case State.NormalAttack: NormalAttack(); break;
            case State.ThrowingAttack: ThrowingAttack(); break;
            case State.CoolTime: CoolTime(); break;
            case State.Carry: UpdateCarry(); break;
            case State.CarryAttack: CarryAttack(); break;
            case State.Stunned: break;
        }
    }
    #endregion

    #region 대기
    private void Idle()
    {
        // 게임이 시작하고 5초 뒤에 상태를 Move 로 바꾼다.
        currentTime += Time.deltaTime;
        if (currentTime >= gameStartTime)
        {
            state = State.Move;
            anim.SetTrigger("Move");
        }
    }
    #endregion

    #region 이동 및 공격
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
        transform.eulerAngles = new Vector3(0, rotX, 0);
        cam.eulerAngles = new Vector3(-rotY, rotX, 0);

        if (rotY >= 36)
        {
            rotY = 36;
        }
        if (rotY <= -36)
        {
            rotY = -36;
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
        if (isCharging == false)
        {
            currentSpeed = normalSpeed;
        }
        else
        {
            currentSpeed = readySpeed;
        }
        cc.Move(dir * currentSpeed * Time.deltaTime);
        #endregion

        #region 공격
        // 마우스 왼쪽 버튼을 누르면 일반공격을 한다.
        if (Input.GetButtonDown("Fire1") && isCharging == false)
        {
            state = State.NormalAttack;
            anim.SetTrigger("Attack");
            isNormalAttack = true;
        }

        // 마우스 오른쪽 버튼을 누르면 한손도끼를 차징하기 시작한다.
        if (Input.GetButton("Fire2") && state != State.NormalAttack && isCanceled == false)
        {

            Charging();
            isCharging = true;                  // 차징 중에 마우스 왼쪽 버튼을 눌러도 일반공격을 못하도록
            anim.SetBool("Throwing", true);     // 왼손을 든 채로 돌아다닐 수 있다.
        }

        // 마우스 오른쪽 버튼을 떼면 도끼를 던진다.
        if (isCanceled == false && Input.GetButtonUp("Fire2"))
        {
            anim.SetTrigger("Throw");
            anim.SetBool("Throwing", false);
            currentChargingTime = 0;
        }

        // 차징 중에 마우스 왼쪽 버튼을 누르면 공격을 취소한다.
        if (Input.GetButtonDown("Fire1"))
        {
            isCanceled = true;
            anim.SetBool("Throwing", false);
            currentChargingTime = 0;

            Invoke("DoCancel", 0.8f);
        }
        #endregion

        #region 생존자 들기
        // 만약 사정거리 안에 생존자가 쓰러져있다면
        // 들어올리기 UI 가 화면에 보일 때 스페이스바를 누르면
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 생존자를 들어올린다.
            anim.SetTrigger("Pickup");
            cc.enabled = false;
        }
        #endregion
    }

    // 도끼 차징 캔슬
    void DoCancel()
    {
        isCanceled = false;
        OffSmallAxe();
    }
    #endregion

    #region 일반공격
    private void NormalAttack()
    {
        // 회전속도와 이동속도 줄어들게 한다.
        #region 회전
        // 회전값을 받아온다.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // 회전값을 누적
        rotX += mx * 1 * Time.deltaTime;
        rotY += my * 1 * Time.deltaTime;

        // 회전값을 적용
        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);
        #endregion

        #region 이동
        // 이동값 받아온다
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 이동값을 애니메이션과 연결
        anim.SetFloat("h", h);
        anim.SetFloat("v", v);

        // 방향을 구한다
        Vector3 dirH = transform.right * h;
        Vector3 dirV = transform.forward * v;
        Vector3 dir = dirH + dirV;
        dir.Normalize();

        // 이동한다
        cc.Move(dir * 0.1f * Time.deltaTime);
        #endregion

        // 공격이 끝나면 상태를 Move 로 바꾼다. -> 애니메이션 이벤트
    }
    #endregion

    #region 한손 도끼 던지기
    // 도끼 차징
    void Charging()
    {
        #region 시간에 따라 누적되는 투사체 던지는 힘
        // 시간을 누적시킨다.
        currentChargingTime += Time.deltaTime;

        // 시간이 누적됨에 따라 chargingForce 를 증가시킨다.
        chargingForce = Mathf.Lerp(minAxePower, maxAxePower, (currentChargingTime - 1.25f) / 1.75f);

        // 1.25초 전까지는 던지는 힘을 주지 않는다.
        // if (currentChargingTime < minimumChargingTime) return;

        // 최대 차지 시간보다 차지시간이 길어지면 최대힘으로 유지한다.
        if (currentChargingTime > maximumChargingTime)
        {
            chargingForce = maxAxePower;
        }
        #endregion

        #region 이동할 때 안할 때 레이어값 조정
        if (cc.velocity == Vector3.zero)
        {
            anim.SetLayerWeight(1, 0);
        }
        else
        {
            anim.SetLayerWeight(1, 1);
        }
        #endregion
    }

    // 도끼 던짐
    private void ThrowingAttack()
    {
        if (isThrowing == true) return;

        // 오른손에 한손도끼를 만든다.
        GameObject smallaxe = Instantiate(smallAxeFactory);
        smallaxe.transform.position = throwingSpot.position;
        smallaxe.transform.forward = Camera.main.transform.forward;

        state = State.CoolTime;
        isThrowing = true;
    }

    // 2초 쿨타임
    void CoolTime()
    {
        #region 회전
        // 회전값을 받아온다.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // 회전값을 누적
        rotX += mx * 50 * Time.deltaTime;
        rotY += my * 50 * Time.deltaTime;

        // 회전값을 적용
        transform.eulerAngles = new Vector3(0, rotX, 0);
        cam.eulerAngles = new Vector3(-rotY, rotX, 0);

        if (rotY >= 20)
        {
            rotY = 20;
        }
        if (rotY <= -20)
        {
            rotY = -20;
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
        currentSpeed = delaySpeed;
        cc.Move(dir * 1 * Time.deltaTime);
        #endregion

        currentTime += Time.deltaTime;
        if (currentTime >= 2)
        {
            state = State.Move;
            isThrowing = false;
            isCharging = false;
        }
    }
    #endregion

    #region 생존자 들기
    private void UpdateCarry()
    {
        #region 회전
        // 회전값을 받아온다.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // 회전값을 누적
        rotX += mx * rotSpeed * Time.deltaTime;
        rotY += my * rotSpeed * Time.deltaTime;

        // 회전값을 적용
        transform.eulerAngles = new Vector3(0, rotX, 0);
        cam.eulerAngles = new Vector3(-rotY, rotX, 0);

        if (rotY >= 36)
        {
            rotY = 36;
        }
        if (rotY <= -36)
        {
            rotY = -36;
        }
        #endregion

        #region 이동
        // 이동값 받아온다
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 이동값을 애니메이션과 연결
        anim.SetFloat("h", h);
        anim.SetFloat("v", v);

        // 방향을 구한다
        Vector3 dirH = transform.right * h;
        Vector3 dirV = transform.forward * v;
        Vector3 dir = dirH + dirV;
        dir.Normalize();

        // 이동한다
        currentSpeed = normalSpeed;
        cc.Move(dir * normalSpeed * Time.deltaTime);
        #endregion

        // 갈고리 근처에서 UI 가 떴을 때
        // 스페이스바를 계속 누르고 있으면 게이지가 찬다. -> 
        if (Input.GetKey(KeyCode.V))
        {
            DoHook();
            cc.enabled = false;
        }
        // 만약 게이지가 다 차기 전에 스페이스바에서 떨어지면 걸기가 캔슬된다.

        // 만약 게이지가 다 차면 생존자를 갈고리에 건다.

        // 생존자를 들고 있는 상태에서 스페이스바를 다시 누르면 내려놓는다. -> Vector3.zero
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Drop");
            cc.enabled = false;
        }

        // 생존자를 들고 있는 상태에서 마우스 왼쪽 버튼을 누르면 공격한다. -> Vector3.zero
        if (Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("CarryAttack");
            state = State.CarryAttack;
        }

        #region 이동할 때 안할 때 레이어값 조정
        if (cc.velocity == Vector3.zero)
        {
            anim.SetLayerWeight(2, 0);
        }
        else
        {
            anim.SetLayerWeight(2, 1);
        }
        #endregion
    }

    public void DoHook()
    {
        currentTime += Time.deltaTime;
    }

    public void CarryAttack()
    {

    }
    #endregion

    #region Events
    public void OnCC()
    {
        cc.enabled = true;
    }

    public void OffCC()
    {
        cc.enabled = false;

    }
    void OnMyReset()            // State 를 Move 로 초기화하는 함수
    {
        state = State.Move;
        anim.SetBool("Throwing", false);
        isCharging = false;
        isNormalAttack = false;
    }

    public void Throwing()      // State 를 ThrowingAttack 로 바꾸는 함수
    {
        state = State.ThrowingAttack;
    }

    public void OnCoolTime()    // State 를 CoolTime 으로 바꾸는 함수
    {
        state = State.CoolTime;
    }

    void OnMyCarry()            // State 를 Carry 로 바꾸는 함수
    {
        // 생존자 들기 상태로 바꾼다.
        state = State.Carry;
    }

    public void OnSmallAxe()    // 도끼 활성화
    {
        smallAxe.SetActive(true);
    }

    public void OffSmallAxe()   // 도끼 비활성화
    {
        smallAxe.SetActive(false);
    }
    #endregion
}