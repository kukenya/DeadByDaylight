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
    float chargingSpeed = 3.08f;            // 차징 시 이동속도
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
    // float minimumChargingTime = 1.25f;      // 최소 차징 시간
    float maximumChargingTime = 3;          // 최대 차징 시간
    // float axeRechargingTime = 4;            // 도끼 충전 시간

    // 카운트
    float currentAxeCount;                  // 현재 가지고 있는 한손도끼 개수
    float maxAxeCount = 5;                  // 최대 소유 가능한 한손도끼 개수

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
    bool isStunned;                         // 스턴 당했는가?
    public bool canCarry;                   // 생존자를 들 수 있는가?
    public bool canDestroyGenerator;               // 발전기를 부술 수 있는가?
    public bool canHook;                           // 갈고리에 걸 수 있는가?
    public bool canReLoad;                         // 한 손 도끼를 재충전 할 수 있는가?
    public bool canDestroyBoard;                   // 내려간 판자를 부술 수 있는가?
    
    // 기타
    GameObject survivor;                    // 생존자 게임오브젝트
    public Transform leftArm;               // 왼팔
    // public Light redlight;               // 살인마 앞에 있는 조명
    #endregion

    #region Start & Update
    void Start()
    {
        anim = GetComponent<Animator>();            // 안나 Animator 컴포넌트
        cc = GetComponent<CharacterController>();   // 안나 Rigidbody 컴포넌트
        anim.SetLayerWeight(1, 0);                  // 던지는 애니메이션 레이어
        anim.SetLayerWeight(2, 0);                  // 들기 애니메이션 레이어
        anim.SetLayerWeight(3, 0);                  // 일반 애니메이션 레이어
        smallAxe.SetActive(false);                  // 왼손에 들고 있는 한손도끼 렌더러 비활성화
        currentAxeCount = maxAxeCount;              // 시작할 때 도끼갯수 최대로 소지
        survivor = GameObject.Find("Survivor");
        // redlight.enabled = false;                // 살인마 앞에 있는 조명을 끔
    }

    void Update()
    {
        #region 스위치
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
        #endregion
      
        #region 도끼 재충전
        if (Input.GetKeyDown(KeyCode.Space) && canReLoad && currentAxeCount < maxAxeCount)
        {
            OffCC();
            anim.SetTrigger("Reload");
            currentAxeCount = maxAxeCount;
        }

        // 그냥 열기 -> 할지 말지
        #endregion

        #region 발전기 부수기
        if (canDestroyGenerator == true)
        {
            // 스페이스 바를 계속 누르고 있으면 발전기를 부순다.
            if (Input.GetKey(KeyCode.Space))
            {
                cc.enabled = false;
                anim.SetBool("Destroy", true);
            }
            // 중간에 스페이스 바에서 손을 떼면 취소된다.
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.SetTrigger("DestroyCancel");
                anim.SetBool("Destroy", false);
                cc.enabled = true;
            }
        }        
        #endregion

        #region 판자 부수기
        if(canDestroyBoard == true)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                cc.enabled = false;
                anim.SetBool("Destroy", true);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.SetTrigger("DestroyBoardCancel");
                anim.SetBool("Destroy", false);
                cc.enabled = true;
            }
        }        
        #endregion

        #region 갈고리 걸기
        if (state == State.Carry && canHook)
        {
            // 갈고리 근처에서 UI 가 떴을 때
            // 스페이스바를 계속 누르고 있으면 
            if (Input.GetKey(KeyCode.Space))
            {
                // 게이지가 찬다.
                // 다 차면 UI 끔
                anim.SetBool("Hook", true);
                cc.enabled = false;
            }
            // 만약 게이지가 다 차기 전에 스페이스바에서 떨어지면 걸기가 캔슬된다.
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.SetTrigger("HookCancel");
                anim.SetBool("Hook", false);
                cc.enabled = true;

                // 게이지 초기화
                // UI 끄기
            }
        }
        #endregion

        // 스턴
        if (Input.GetKeyDown(KeyCode.G))
        {
            // G 버튼 = 스턴 함수를 불렀다고 가정하고 
            Stunned();
        }

        // 레이로 
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        RaycastHit hitinfo;

        if(Physics.Raycast(ray, out hitinfo))
        {
            //만약 이름에 "Generator"를 포함하고 있다면
            if (hitinfo.transform.name.Contains("Generator"))
            {
                // 부술 수 있는 상태로 만든다.
                canDestroyGenerator = true;
            }
            else
            {
                canDestroyGenerator = false;
            }         
        }
      
    }

    // 스턴
    private void Stunned()
    {
        cc.enabled = false;

        if (state == State.Carry)
        {
            // 캐리 스턴 애니메이션을 실행
            anim.SetTrigger("CarryStunned");
        }
        else
        {
            // 스턴 애니메이션을 실행
            anim.SetTrigger("Stunned");
            OffSmallAxe();
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
            anim.SetBool("Move", true);
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

        if (rotY >= 30)
        {
            rotY = 30;
        }
        if (rotY <= -30)
        {
            rotY = -30;
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

        // 만약 차징 중이라면
        if (isCharging == true)
        {
            // 차지속도(3.08)로 이동
            currentSpeed = chargingSpeed;
        }
        else if (isCharging == false || state != State.CoolTime)
        {
            // 그 이외는 일반속도(4.4)로 이동
            currentSpeed = normalSpeed;
        }

        // 이동한다
        cc.Move(dir * currentSpeed * Time.deltaTime);
        #endregion

        #region  일반 공격
        // 마우스 왼쪽 버튼을 누르면 일반공격을 한다.
        if (Input.GetButtonDown("Fire1") && isCharging == false)
        {
            // 상태를 NormalAttack 로 바꿈
            state = State.NormalAttack;

            // 일반공격 애니메이션 실행
            anim.SetTrigger("Attack");

            isNormalAttack = true;

            // 멈춰있는 상태에서 때리면
            //if (cc.velocity == Vector3.zero)
            //{
            //    // 그냥 때림
            //    // 상태를 NormalAttack 로 바꿈
            //    state = State.NormalAttack;
            //}
            // 움직이는 상태라면
            //else
            //{
            //    앞으로 가면서 때림
            //    transform.position = transform.position + transform.forward * 2;
            //    
            //    Vector3 attackPos = transform.position + transform.forward * 2;
            //    transform.position = Vector3.Lerp(transform.position, attackPos, 0.8f);
            //}
        }
        #endregion

        #region 던지는 도끼 공격
        // 마우스 오른쪽 버튼을 누르면 한손도끼를 차징하기 시작한다.
        if (Input.GetButton("Fire2") && state != State.NormalAttack && isCanceled == false && currentAxeCount != 0)
        {
            Charging();
            isCharging = true;                  // 차징 중에 마우스 왼쪽 버튼을 눌러도 일반공격을 못하도록
            anim.SetBool("Throwing", true);     // 왼손을 든 채로 돌아다닐 수 있다.
        }

        // 마우스 오른쪽 버튼을 떼면 도끼를 던진다.
        if (isCanceled == false && Input.GetButtonUp("Fire2") && currentAxeCount != 0)
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
            isCharging = false;

            Invoke("DoCancel", 0.4f);
        }
        #endregion

        #region 생존자 들기
        // 만약 사정거리 안에 생존자가 쓰러져있다면
        if (canCarry && state != State.Carry)
        {
            // 들어올리기 UI 가 화면에 보일 때 스페이스바를 누르면
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 생존자를 들어올린다.
                anim.SetTrigger("Pickup");
                state = State.Carry;
                canCarry = false;

                // 생존자의 몸을 내 팔의 자식으로 만들어서 들고 다닌다.
                // survivor.transform.SetParent(leftArm);
            }
        }
        #endregion

        #region 이동할 때 안할 때 레이어값 조정
        //if (cc.velocity == Vector3.zero)
        //{
        //    anim.SetLayerWeight(3, 0f);
        //}
        //else
        //{
        //    anim.SetLayerWeight(3, 0);
        //}
        #endregion
    }
    #endregion

    #region 일반공격
    private void NormalAttack()
    {
        //anim.SetLayerWeight(3, 0f);
        // 공격이 끝나면 상태를 Move 로 바꾼다. -> 애니메이션 이벤트
    }
    #endregion

    #region 한손 도끼 차징 / 취소 / 던짐 / 쿨타임
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
        //if (cc.velocity == Vector3.zero)
        //{
        //    anim.SetLayerWeight(1, 0);
        //}
        //else
        //{
        //    anim.SetLayerWeight(1, 1);
        //}
        #endregion
    }

    // 도끼 차징 캔슬
    void DoCancel()
    {
        isCanceled = false;
        OffSmallAxe();
    }

    // 도끼 던짐
    private void ThrowingAttack()
    {
        if (isThrowing == true) return;

        // 오른손에 한손도끼를 만든다.
        GameObject smallaxe = Instantiate(smallAxeFactory);
        smallaxe.transform.position = throwingSpot.position;
        smallaxe.transform.forward = Camera.main.transform.forward;

        // 도끼 갯수를 줄인다
        currentAxeCount--;

        state = State.CoolTime;
    }

    // 2초 쿨타임
    void CoolTime()
    {
        isThrowing = true;

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

        if (rotY >= 30)
        {
            rotY = 30;
        }
        if (rotY <= -30)
        {
            rotY = -30;
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
        cc.Move(dir * delaySpeed * Time.deltaTime);
        #endregion

        currentTime += Time.deltaTime;
        if (currentTime >= 2)
        {
            state = State.Move;
            isThrowing = false;
            isCharging = false;
            currentTime = 0;
        }
    }
    #endregion

    #region 생존자 들기
    // 이동
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

        if (rotY >= 30)
        {
            rotY = 30;
        }
        if (rotY <= -30)
        {
            rotY = -30;
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

        #region 내려놓기
        // 생존자를 들고 있는 상태에서 스페이스바를 다시 누르면 내려놓는다. -> Vector3.zero
        if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook ==false)
        {
            anim.SetTrigger("Drop");
            cc.enabled = false;
        }
        #endregion

        #region 든 상태에서 공격
        // 생존자를 들고 있는 상태에서 마우스 왼쪽 버튼을 누르면 공격한다. -> Vector3.zero
        if (Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("CarryAttack");
            state = State.CarryAttack;
        }
        #endregion

        #region 이동할 때 안할 때 레이어값 조정
        //if (cc.velocity == Vector3.zero)
        //{
        //    anim.SetLayerWeight(2, 0);
        //}
        //else
        //{
        //    anim.SetLayerWeight(2, 1);
        //}
        #endregion
    }

    // 공격
    public void CarryAttack()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= 2.5f)
        {
            state = State.Carry;
            currentTime = 0;
        }
    }
    #endregion

    #region 트리거 확인
    private void OnTriggerEnter(Collider other)
    {
        // 생존자
        if (other.gameObject.layer == 6)
        {
            canCarry = true;
        }

        // 갈고리
        if (other.gameObject.layer == 22)
        {
            canHook = true;
        }

        // 발전기
        if (other.gameObject.layer == 23)
        {
            canDestroyGenerator = true;
        }

        // 캐비넷(도끼재충전)
        //if(other.gameObject.layer == 24)
        //{
        //    canReLoad = true;
        //}

        // 내려간 판자
        if(other.gameObject.layer == 25)
        {
            canDestroyBoard = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 생존자
        if (other.gameObject.layer == 6)
        {
            canCarry = false;
        }
        // 갈고리
        if (other.gameObject.layer == 22)
        {
            canHook = false;
        }

        // 발전기
        //if (other.gameObject.layer == 23)
        //{
        //    canDestroyGenerator = false;
        //}

        // 캐비넷(도끼재충전)
        if (other.gameObject.layer == 24)
        {
            canReLoad = false;
        }

        // 내려간 판자
        if (other.gameObject.layer == 25)
        {
            canDestroyBoard = false;
        }
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
        cc.enabled = true;
        state = State.Move;
        anim.SetBool("Throwing", false);
        anim.SetBool("Hook", false);
        anim.SetBool("Destroy", false);
        isCharging = false;
        isNormalAttack = false;
        isStunned = false;
        canDestroyGenerator = false;
    }

    public void Throwing()      // State 를 ThrowingAttack 로 바꾸는 함수
    {
        state = State.ThrowingAttack;
    }

    public void OnCoolTime()    // State 를 CoolTime 으로 바꾸는 함수
    {
        state = State.CoolTime;
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