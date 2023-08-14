using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public Camera cineCam;                  // 시네머신 카메라 

    // 이동 속도
    float currentSpeed;                     // 현재 이동속도
    float normalSpeed = 4.4f;               // 기본 이동속도
    float chargingSpeed = 3.08f;            // 차징 시 이동속도
    float delaySpeed = 3.74f;               // 경직 시 이동속도

    // 중력
    public float yVelocity;
    public float gravity = -9.8f;


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

    // 두손 도끼
    public BoxCollider bigAxeCollider;      // 두손도끼 콜라이더

    // 한손 도끼
    public GameObject smallAxe;             // 한손도끼 GameObject
    public GameObject smallAxeFactory;      // 투척용 한손도끼 프리팹
    public Transform throwingSpot;          // 한손도끼 날아갈 위치
    public float chargingForce;             // 한손도끼 던지는 힘
    public float minAxePower = 20;          // 한손도끼 던지는 힘 최소값
    public float maxAxePower = 40;          // 한손도끼 던지는 힘 최대값

    // bool
    public bool isNormalAttack;                     // 일반공격 중인가?
    bool isCharging;                                // 차징 중인가?
    bool isCanceled;                                // 차징을 취소했는가?
    bool isThrowing;                                // 손도끼를 던졌는가?
    bool isStunned;                                 // 스턴 당했는가?
    public bool canCarry;                           // 생존자를 들 수 있는가?
    public bool canDestroyGenerator;                // 발전기를 부술 수 있는가?
    public bool canHook;                            // 갈고리에 걸 수 있는가?
    public bool canReLoad;                          // 한 손 도끼를 재충전 할 수 있는가?
    public bool canDestroyPallet;                   // 내려간 판자를 부술 수 있는가?

    // UI
    public TextMeshProUGUI generatorCount;          // 발전기 갯수 UI
    public TextMeshProUGUI axeCount;                // 도끼 갯수 UI



    // 기타
    public GameObject survivor;                 // 생존자 게임오브젝트
    public Transform leftArm;                   // 왼팔
    public Light redlight;                      // 살인마 앞에 있는 조명
    public GameObject throwUI;                  // 도끼 던지기 UI
    #endregion

    #region Start & Update
    void Start()
    {
        anim = GetComponent<Animator>();                // 안나 Animator 컴포넌트
        cc = GetComponent<CharacterController>();       // 안나 Rigidbody 컴포넌트
        anim.SetLayerWeight(1, 0);                      // 던지는 애니메이션 레이어
        anim.SetLayerWeight(2, 0);                      // 들기 애니메이션 레이어
        anim.SetLayerWeight(3, 0);                      // 일반 애니메이션 레이어
        smallAxe.SetActive(false);                      // 왼손에 들고 있는 한손도끼 렌더러 비활성화
        currentAxeCount = maxAxeCount;                  // 시작할 때 도끼갯수 최대로 소지
        axeCount.text = Convert.ToString(maxAxeCount);  // UI 로 나타낸다
        // survivor = GameObject.Find("Survivor");
        bigAxeCollider.enabled = false;
        // redlight.enabled = false;                    // 살인마 앞에 있는 조명을 끔
        cineCam.depth = 5;                              // 시네머신 카메라 보이게 하기
        throwUI.SetActive(false);                        // 도끼 던지기  UI 비활성화
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

        #region 회전
        if (state == State.Move || state == State.CoolTime || state == State.Carry && state == State.CarryAttack || state == State.Carry)
        {
            // 회전값을 받아온다.
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            // 회전값을 누적
            rotX += mx * rotSpeed * Time.deltaTime;
            rotY += my * rotSpeed * Time.deltaTime;

            // 회전값을 적용
            transform.eulerAngles = new Vector3(0, rotX, 0);
            if (cc.enabled == true)
            {
                transform.eulerAngles = new Vector3(-rotY, rotX, 0);

                if (rotY >= 35)
                {
                    rotY = 35;
                }
                if (rotY <= -35)
                {
                    rotY = -35;
                }
            }
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
        else if (state == State.CoolTime)
        {
            currentSpeed = delaySpeed;
        }
        else if (state == State.NormalAttack)
        {
            currentSpeed = 6;
        }
        else if (isCharging == false || state != State.CoolTime || isThrowing == false)
        {
            // 그 이외는 일반속도(4.4)로 이동
            currentSpeed = normalSpeed;
        }

        if (cc.isGrounded == false)
        {
            yVelocity += gravity;
        }
        else
        {
            yVelocity = 0;
        }

        // 이동한다
        Vector3 velocity = dir * currentSpeed;
        velocity.y = yVelocity;
        cc.Move(velocity * Time.deltaTime);
        #endregion

        #region 도끼 재충전
        if (Input.GetKeyDown(KeyCode.Space) && canReLoad && currentAxeCount < maxAxeCount)
        {
            OffCC();                                                        // 움직임 멈춤
            anim.SetTrigger("Reload");                                      // 도끼를 집어드는 애니메이션 실행
                                                                            // 캐비넷 열리고 닫히는 애니메이션 실행
            currentAxeCount = maxAxeCount;                                  // 도끼 최대 소지 갯수를 최대로 채운다
            axeCount.text = Convert.ToString(currentAxeCount);              // UI 갱신한다
        }
        #endregion

        #region 캐비넷 확인
        // 캐비넷 앞에서 스페이스바를 누르면 캐비넷을 연다
        // 만약 안에 사람이 없고 도끼 소지 개수가 최대라면
            // 그냥 열었다가 닫는다
        // 만약 안에 사람이 있다면
            // 그 사람을 들어버린다
            // 상태를 Carry 로 바꾼다
        #endregion

        #region 발전기 부수기
        if (canDestroyGenerator == true)
        {
            // 스페이스 바를 계속 누르고 있으면 발전기를 부순다.
            if (Input.GetKey(KeyCode.Space))
            {
                cc.enabled = false;
                anim.SetBool("DestroyG", true);
            }
            // 중간에 스페이스 바에서 손을 떼면 취소된다.
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.SetTrigger("DestroyCancel");
                anim.SetBool("DestroyG", false);
                cc.enabled = true;
            }
        }
        #endregion

        #region 판자 부수기
        if (canDestroyPallet == true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                cc.enabled = false;             // 움직임 멈춤
                anim.SetTrigger("DestroyP");    // 판자를 부수는 애니메이션 실행
                                                // 판자가 부서지는 애니메이션 실행
            }
        }
        #endregion

        #region 갈고리 걸기
        if (state == State.Carry && canHook)
        {
            // 갈고리 근처에서 UI 가 떴을 때
            // 스페이스바를 누르면 
            if (Input.GetKeyDown(KeyCode.Space))
            {
                cc.enabled = false;             // 움직임 멈춤
                anim.SetBool("Hook", true);     // 생존자를 갈고리에 거는 애니메이션 실행
                                                // 갈고리에 거는 UI 게이지가 찬다.
                                                // 다 차면 UI 끔
            }
        }
        #endregion

        #region Ray 로 상호작용 여부 확인하기
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        RaycastHit hitinfo;
        if (Physics.Raycast(ray, out hitinfo, 1f))
        {
            print(hitinfo.collider.transform.name);

            // Generator 발전기
            if (hitinfo.transform.name.Contains("Generator"))
            {
                // 부술 수 있는 상태로 만든다.
                canDestroyGenerator = true;
            }
            else
            {
                canDestroyGenerator = false;
            }

            // Closet 캐비넷
            if (hitinfo.transform.name.Contains("Closet"))
            {
                // 재충전 할 수 있는 상태로 만든다.
                canReLoad = true;
            }
            else
            {
                canReLoad = false;
            }

            // Pallet 판자 
            if (hitinfo.transform.name.Contains("Pallet"))
            {
                // 부술 수 있는 상태로 만든다.
                canDestroyPallet = true;
            }
            else
            {
                canDestroyPallet = false;
            }

            // 쓰러진 생존자
            if (hitinfo.transform.name.Contains("Surviver") && survivor.GetComponent<SurviverHealth>().state == SurviverHealth.HealthState.Down)
            {
                canCarry = true;
            }
            else
            {
                canCarry = false;
            }

            // 갈고리
            if(hitinfo.transform.name.Contains("Hook") && survivor.GetComponent<SurviverHealth>().state == SurviverHealth.HealthState.Carrying)
            {
                canHook = true;
            }
            else
            {
                canHook = false;
            }
        }

        //if(Physics.Raycast(ray, out hitinfo, 3))
        //{
        //    if (hitinfo.transform.name.Contains("Surviver") && ChaseorLullaby.instance.isChasing == false)
        //    {
        //        ChaseorLullaby.instance.PlayChaseBG();
        //    }
        //    else if(hitinfo.transform.name.Contains("Surviver") && ChaseorLullaby.instance.isLullaby == false)
        //    {
        //        ChaseorLullaby.instance.PlayLullaby();
        //    }
        //}
        #endregion

        // 스턴
        if (Input.GetKeyDown(KeyCode.G))
        {
            // G 버튼 = 스턴 함수를 불렀다고 가정하고 
            Stunned();
        }
    }

    // 스턴
    private void Stunned()
    {
        SoundManager.instance.PlayHitSounds(4);

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
        cc.enabled = false;
        // 게임이 시작하고 5초 뒤에 상태를 Move 로 바꾼다.
        currentTime += Time.deltaTime;
        if (currentTime >= gameStartTime)
        {
            throwUI.SetActive(true);
            cineCam.enabled = false;
            cc.enabled = true;
            state = State.Move;
            anim.SetBool("Move", true);
        }
    }
    #endregion

    #region 이동 및 공격

    bool playingthrowsound;
    private void UpdateMove()
    {
        #region  일반 공격
        // 마우스 왼쪽 버튼을 누르면 일반공격을 한다.
        if (Input.GetButtonDown("Fire1") && isCharging == false)
        {
            SoundManager.instance.PlayHitSounds(0);         // 때리는 입소리

            state = State.NormalAttack;                     // 상태를 NormalAttack 로 바꿈

            anim.SetTrigger("Attack");                      // 일반공격 애니메이션 실행

            isNormalAttack = true;                          // 공격이 끝날 때까지 왼쪽 버튼 못누르게 하기
        }
        #endregion

        #region 던지는 도끼 공격
        // 마우스 오른쪽 버튼을 누르면 한손도끼를 차징하기 시작한다.
        if (Input.GetButton("Fire2") && state != State.NormalAttack && isCanceled == false && currentAxeCount != 0)
        {
            Charging();                         // 차징 함수를 실행
            isCharging = true;                  // 차징 중에 마우스 왼쪽 버튼을 눌러도 일반공격을 못하도록
            anim.SetBool("Throwing", true);     // 왼손을 든 채로 돌아다닐 수 있다.
            throwUI.SetActive(false);
        }

        // 마우스 오른쪽 버튼을 떼면 도끼를 던진다.
        if (isCanceled == false && Input.GetButtonUp("Fire2") && currentAxeCount != 0)
        {
            anim.SetTrigger("Throw");                       // 애니메이션 실행
            anim.SetBool("Throwing", false);                // 차징 애니메이션 취소
            currentChargingTime = 0;                        // 현재 차징 시간을 초기화
        }

        // 차징 중에 마우스 왼쪽 버튼을 누르면 공격을 취소한다.
        if (Input.GetButtonDown("Fire1") && isCharging == true)
        {
            isCanceled = true;                              // 캔슬했음
            isCharging = false;                             // 차징 중이 아님
            anim.SetBool("Throwing", false);                // 차징 애니메이션 취소
            currentChargingTime = 0;                        // 현재 차징 시간을 초기화
            SoundManager.instance.PlaySmallAxeSounds(2);    // 취소하는 사운드 재생
            playingchargingsound = false;                   // 차징하는 사운드 재생 안됨
            playingfullchargingsound = false;
            throwUI.SetActive(true);

            Invoke("DoCancel", 0.4f);                       // 0.4초 후에 도끼 안보이게 함
        }
        #endregion

        #region 생존자 들기
        // 만약 사정거리 안에 생존자가 쓰러져있다면
        if (canCarry && state != State.Carry)
        {
            // 들어올리기 UI 가 화면에 보일 때 스페이스바를 누르면
            if (Input.GetKeyDown(KeyCode.Space))
            {
                survivor.GetComponent<SurviverHealth>().ChangeCarring();
                anim.SetTrigger("Pickup");  // 생존자를 들어올린다.
                state = State.Carry;        // 상태를 Carry 로 바꾼다.
                canCarry = false;           // 들 수 있는 상태가 아님

                // 생존자의 몸을 내 팔의 자식으로 만들어서 들고 다닌다.
                survivor.transform.SetParent(leftArm);
                
                // survivor.transform.localPosition = new Vector3(-0.4f,0.961f,-0.125f);
                // survivor.transform.localPosition = new Vector3(0,0,0);
                // survivor.transform.localRotation = new Quaternion(0,0,0,0);
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
        throwUI.SetActive(false);
        //anim.SetLayerWeight(3, 0f);
        // 공격이 끝나면 상태를 Move 로 바꾼다. -> 애니메이션 이벤트
    }
    #endregion

    #region 한손 도끼 차징 / 취소 / 던짐 / 쿨타임
    bool playingchargingsound;
    bool playingfullchargingsound;
    // 도끼 차징
    void Charging()
    {
        if (playingchargingsound == false)
        {
            // 차징 사운드 재생
            SoundManager.instance.PlaySmallAxeSounds(0);
            playingchargingsound = true;
        }

        #region 시간에 따라 누적되는 투사체 던지는 힘
        // 시간을 누적시킨다.
        currentChargingTime += Time.deltaTime;

        // 시간이 누적됨에 따라 chargingForce 를 증가시킨다.
        chargingForce = Mathf.Lerp(minAxePower, maxAxePower, (currentChargingTime - 1.25f) / 1.75f);

        // 1.25초 전까지는 던지는 힘을 주지 않는다.
        // if (currentChargingTime < minimumChargingTime) return;

        // 최대 차지 시간보다 차지시간이 길어지면
        if (currentChargingTime > maximumChargingTime && playingfullchargingsound == false)
        {
            playingfullchargingsound = true;

            // 최대 차징 사운드를 재생
            SoundManager.instance.PlaySmallAxeSounds(1);

            //  최대힘으로 유지한다.
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
        throwUI.SetActive(true);

    }

    // 도끼 던짐
    private void ThrowingAttack()
    {
        if (isThrowing == true) return;                                 // 만약 isThrowing 라면 리턴

        isCharging = false;                                             // 차징 중이 아님
        playingchargingsound = false;                                   // 차징하는 사운드 재생할 수 있는 상태
        playingfullchargingsound = false;
        GameObject smallaxe = Instantiate(smallAxeFactory);             // 한손도끼를 만든다.
        smallaxe.transform.position = throwingSpot.position;            // 만든 한손도끼의 위치를 왼손에 배
        smallaxe.transform.forward = Camera.main.transform.forward;     // 만든 한손도끼의 앞방향을 카메라의 앞방향으로 한다

        currentAxeCount--;                                              // 도끼 갯수를 줄인다
        axeCount.text = Convert.ToString(currentAxeCount);              // UI 갱신한다

        state = State.CoolTime;                                         // 상태를 CoolTime 으로 바꾼다
    }

    // 2초 쿨타임
    void CoolTime()
    {
        isThrowing = true;

        playingchargingsound = false;   // 차징하는 사운드 재생할 수 있는 상태
        playingfullchargingsound = false;

        currentTime += Time.deltaTime;  // 현재 시간을 흐르게 한다
        if (currentTime >= 2)           // 현재 시간이 2초가 되면
        {
            state = State.Move;         // 상태를 Move 로 바꾼다
            isCharging = false;         // 차징 가능
            isThrowing = false;         // 던지기 가능
            throwUI.SetActive(true);
            currentTime = 0;            // 현재 시간을 초기화
        }
    }
    #endregion

    #region 생존자 들기
    // 이동
    private void UpdateCarry()
    {
        throwUI.SetActive(false);


        #region 내려놓기
        if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook == false)
        {
            anim.SetTrigger("Drop");
            cc.enabled = false;
        }
        #endregion

        #region 든 상태에서 공격
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
        OnAxe();

        currentTime += Time.deltaTime;
        if (currentTime >= 2.5f)
        {
            OffAxe();
            state = State.Carry;
            currentTime = 0;

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

    public void OnAxe()
    {
        bigAxeCollider.enabled = true;
    }
    public void OffAxe()
    {
        bigAxeCollider.enabled = false;
    }

    void OnMyReset()            // State 를 Move 로 초기화하는 함수
    {
        state = State.Move;

        OnCC();
        OffAxe();

        anim.SetBool("Throwing", false);
        anim.SetBool("DestroyG", false);

        isStunned = false;
        isCharging = false;
        isNormalAttack = false;
        playingthrowsound = false;
        canDestroyGenerator = false;
        canDestroyPallet = false;
        canReLoad = false;
        throwUI.SetActive(true);
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