using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class AnnaMove : MonoBehaviourPun, IPunObservable
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
    Animator anim;                   // 살인마 애니메이터
    // public Animator anim2;                  // 생존자 용 살인마 애니메이터
    CharacterController cc;                 // 캐릭터 컨트롤러
    public Camera cineCam;                  // 시네머신 카메라 
    public Transform neck;

    // 이동 속도
    float currentSpeed;                     // 현재 이동속도
    float normalSpeed = 4.4f;               // 기본 이동속도
    float chargingSpeed = 3.08f;            // 차징 시 이동속도
    float delaySpeed = 3.74f;               // 경직 시 이동속도

    // 중력
    public float yVelocity;                 // y 속도
    public float gravity = -9.8f;           // 중력속도


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
    float maxAxeCount = 5;                  // 최대 소유 가능한 한손도끼 개수
    float currentAxeCount;                  // 현재 가지고 있는 한손도끼 개수

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
    public bool canRotate;

    // UI
    public GameObject goGeneratorCount;             // 발전기 갯수 UI GameObject
    TextMeshProUGUI generatorCount;                 // 발전기 갯수 UI

    public GameObject goAxeCount;                   // 도끼 갯수 UI GameObject
    TextMeshProUGUI axeCount;                       // 도끼 갯수 UI

    public GameObject throwUI;                      // 도끼 던지기 UI


    // 기타
    public GameObject survivor;                 // 생존자 게임오브젝트
    public Transform leftArm;                   // 왼팔
    public Light redlight;                      // 살인마 앞에 있는 조명

    // 애니메이션 실행 위치
    public Transform hookSpot;                  // 갈고리

    float h;
    float v;

    #endregion

    #region Start & Update
    void Start()
    {
        if (photonView.IsMine == true)
        {
            redlight.enabled = false;
            anim = GetComponent<Animator>();                    // 안나 Animator 컴포넌트
                                                                //anim.runtimeAnimatorController =
            cc = GetComponent<CharacterController>();           // 안나 Rigidbody 컴포넌트

            anim.SetLayerWeight(1, 0);                          // 던지는 애니메이션 레이어
            anim.SetLayerWeight(2, 0);                          // 들기 애니메이션 레이어
            anim.SetLayerWeight(3, 0);                          // 일반 애니메이션 레이어

            goGeneratorCount = GameObject.Find("TextGenerator");    // TextGenerator GameObject 찾는다.
            TextMeshProUGUI generatorCount =                        // 찾은 게임오브젝트의 TextMeshProUGUI 컴포넌트를 불러온다.
                goGeneratorCount.GetComponent<TextMeshProUGUI>();

            goAxeCount = GameObject.Find("TextAxe");            // TextAxe GameObject  를 찾는다.
            TextMeshProUGUI axeCount =                          // 찾은 게임오브젝트의 TextMeshProUGUI 컴포넌트를 불러온다.
                goAxeCount.GetComponent<TextMeshProUGUI>();

            currentAxeCount = maxAxeCount;                      // 시작할 때 도끼 개수를 최대(5개)로 소지한다.

            axeCount.text = Convert.ToString(maxAxeCount);      // UI 로 나타낸다.

            throwUI = GameObject.Find("ThrowUI");               // 도끼 던지기 UI 찾는다.
            throwUI.SetActive(false);

            smallAxe.SetActive(false);                          // 왼손에 들고 있는 한손도끼 렌더러 비활성화

            bigAxeCollider.enabled = false;                     // 양손도끼 콜라이더를 꺼놓는다.

            cineCam.depth = 5;                                  // 시네머신 카메라 보이게 한다.
        }
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

        if (photonView.IsMine == true)
        {
            #region 회전
            if (state != State.Idle)
            {
                // 회전값을 받아온다.
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");

                // 회전값을 누적
                rotX += mx * rotSpeed * Time.deltaTime;
                rotY += my * rotSpeed * Time.deltaTime;

                // 회전값을 적용
                transform.eulerAngles = new Vector3(0, rotX, 0);        // Horizontal

                if (cc.enabled == true)
                {
                    transform.eulerAngles = new Vector3(-rotY, rotX, 0);    // Vertical

                    rotY = Mathf.Clamp(rotY, -35, 35);
                }
            }
            #endregion

            #region 이동
            // 이동값 받아온다
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

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
            //}

            // 나의 Anna 가 아닐 때
            //else
            //{
            // 위치 보정
            // 회전 보정
            // Animator2 에 Parameter 값을 전달
            // anim2.SetFloat("h", h);
            // anim2.SetFloat("v", v);
            //}
            #endregion

            // 상호작용을 할 때는 상호작용 애니메이션이 시작하는 위치로 이동을 해야한다.

            #region 도끼 재충전
            // 도끼의 개수가 5보다 작고 재충전 가능한 상태일 때, 스페이스 바를 누르면
            if (currentAxeCount < maxAxeCount && canReLoad && Input.GetKeyDown(KeyCode.Space))
            {
                // 애니메이션 시작 장소로 이동한다.

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
                // 스페이스바를 누르면 
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine("LerptoHook");

                    cc.enabled = false;                 // 움직임을 멈춘다.

                    anim.SetTrigger("Hook");            // 생존자를 갈고리에 거는 애니메이션 실행한다.

                    // 갈고리에 거는 UI 게이지가 찬다.

                    // 다 차면 UI 끈다.

                    survivor.transform.parent = null;   // 생존자와의 부모자식 관계를 끊는다.
                }
            }
            #endregion

            // UI 껐다켰다 하기
            #region OverlapSphere & UI
            canHook = false;
            canReLoad = false;
            canDestroyPallet = false;
            canDestroyGenerator = false;
            canCarry = false;
            Collider[] hitcolliders = Physics.OverlapSphere(transform.position, 2);
            // for 문 돌리기
            for (int i = 0; i < hitcolliders.Length; i++)
            {
                print(hitcolliders[i].transform.gameObject.name);
                // Surviver 생존자
                if (hitcolliders[i].transform.gameObject.name.Contains("Surviver"))
                {
                    // 상태가 Healthy 인 생존자가 1명 이상이라면 chase BG이 나오게 한다.


                    // 상태가 Down 이라면 canCarry 을 true 로 바꾼다.
                    if (survivor.GetComponent<SurviverHealth>().State == SurviverHealth.HealthState.Down)
                    {
                        canCarry = true;
                    }
                }
                else
                {
                    // 만약에 생존자가 없으면 Lullaby BG 이 나오게 한다.


                    // canCarry -> false

                }

                // Generator 발전기
                if (hitcolliders[i].transform.gameObject.name.Contains("Generator"))
                {
                    canDestroyGenerator = true;

                    // 만약 발전기의 수리 프로그래스가 0보다 크다면
                    // if(발전기 수리 프로그래스 > 0)
                    // {
                    // canDestroyGenerator 를 true 로
                    // }
                }

                // Pallet 판자
                if (hitcolliders[i].transform.gameObject.name.Contains("Pallet"))
                {
                    // 만약 판자의 상태가 '내려감' 이라면
                    //if ()
                    //{
                    //    // canDestroyPallet 을 true 로
                    canDestroyPallet = true;
                    //}
                }
                else { }

                // Closet 캐비넷
                if (hitcolliders[i].transform.gameObject.name.Contains("Closet"))
                {
                    // 만약 들고 있는 도끼의 개수가 최대개수라면
                    if (currentAxeCount == maxAxeCount)
                    {
                        // 그냥 문 열기 -> [SPACE] 찾기 UI

                        // 만약 생존자가 있으면 바로 들기 / 아니면 걍 문닫기

                    }

                    // 만약 들고 있는 도끼의 개수가 1~4 일때
                    else if (currentAxeCount > 0 && currentAxeCount < maxAxeCount)
                    {
                        // canReload -> true & [SPACE] 찾기 + 손도끼 투척
                        canReLoad = true;
                    }
                }

                // Hook 갈고리 // 만약 내 상태가 Carry라면
                if (hitcolliders[i].transform.gameObject.name.Contains("Hook") && survivor.GetComponent<SurviverHealth>().State == SurviverHealth.HealthState.Carrying)
                {
                    // canHook -> true
                    canHook = true;

                    // 갈고리 애니메이션 시작 장소  
                }
            }
            #endregion
        }

        #region 스턴
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
    #endregion

    #region Lerp
    // 갈고리
    IEnumerator LerptoHook()
    {
        while (Vector3.Distance(transform.position, hookSpot.position) > 0.3f)
        {
            // Vector3 hookdir = hookSpot.position - transform.position;   // hookSpot 에서 나의 방향벡터
            transform.position = Vector3.Lerp(transform.position, hookSpot.position, 0.1f);
            transform.forward = hookSpot.forward;

            if (Vector3.Distance(transform.position, hookSpot.position) <= 0.3f)
            {
                break;
            }
        }

        yield return null;
    }

    // 생존자 들기
    IEnumerator LerptoSurvivor()
    {
        while (Vector3.Distance(transform.position, survivor.transform.position) > 0.3f)
        {
            transform.position = Vector3.Lerp(transform.position, survivor.transform.position, 0.1f);

            if (Vector3.Distance(transform.position, survivor.transform.position) <= 0.8f)
            {
                break;
            }
        }
        yield return null;
    }

    // 발전기

    // 판자

    // 캐비넷

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
            currentTime = 0;
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
                survivor.GetComponent<SurviverHealth>().ChangeCarring();    // 생존자의 상태를 바꾸는 함수를 호출

                anim.SetTrigger("Pickup");                                  // 생존자를 들어올린다.

                state = State.Carry;                                        // 상태를 Carry 로 바꾼다.

                canCarry = false;                                           // 들 수 있는 상태가 아님

                transform.forward = survivor.transform.forward;             // 내 앞방향을 생존자의 앞방향으로 설정한다.

                StartCoroutine("LerptoSurvivor");                           // 애니메이션 시작 장소로 이동하는 코루틴 실행한다.

                survivor.transform.parent = leftArm.transform;                 // 생존자를 나의 자식 오브젝트로 설정한다.


                // survivor.transform.localPosition = new Vector3(-0.350566328f, 1.01032352f, 0.0430793613f);
                // survivor.transform.localPosition = new Vector3(-0.367999434f, 1.02999997f, -0.191000223f);
                survivor.transform.localPosition = new Vector3(-0.331999868f, 0.976001263f, -0.19100064f);
                survivor.transform.localRotation = new Quaternion(2.60770321e-08f, -0.1253708f, 2.09547579e-09f, 0.992109954f);
                // survivor.transform.localPosition = new Vector3(-0.4f,0.961f,-0.125f);
                // survivor.transform.localPosition = new Vector3(0,0,0);
                // survivor.transform.localRotation = new Quaternion(0,0,0,0);
            }
        }
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

        isThrowing = true;
        isCharging = false;                                             // 차징 중이 아님
        playingchargingsound = false;                                   // 차징하는 사운드 재생할 수 있는 상태
        playingfullchargingsound = false;

        GameObject smallaxe = Instantiate(smallAxeFactory);             // 한손도끼를 만든다.
        smallaxe.transform.position = throwingSpot.position;            // 만든 한손도끼의 위치를 왼손에 배치한다.
        smallaxe.transform.forward = Camera.main.transform.forward;     // 만든 한손도끼의 앞방향을 카메라의 앞방향으로 한다

        currentAxeCount--;                                              // 도끼 갯수를 줄인다
        axeCount.text = Convert.ToString(currentAxeCount);              // UI 갱신한다
        print(currentAxeCount);

        state = State.CoolTime;                                         // 상태를 CoolTime 으로 바꾼다
    }

    // 2초 쿨타임
    void CoolTime()
    {
        isThrowing = true;

        playingchargingsound = false;       // 차징하는 사운드 재생할 수 있는 상태
        playingfullchargingsound = false;

        currentTime += Time.deltaTime;      // 현재 시간을 흐르게 한다
        if (currentTime >= 2)               // 현재 시간이 2초가 되면
        {
            state = State.Move;             // 상태를 Move 로 바꾼다
            isCharging = false;             // 차징 가능
            isThrowing = false;             // 던지기 가능
            if (currentAxeCount != 0)       // 현재 도끼 개수가 0이 아니라면
            {
                throwUI.SetActive(true);    // 도끼 개수 UI 보이게 하기 ( 만약 0 이라면 계속 안보이겠지)
            }
            currentTime = 0;                // 현재 시간을 초기화
        }
    }
    #endregion

    #region 생존자 들기 / 든 상태에서 공격
    // 이동
    private void UpdateCarry()
    {
        throwUI.SetActive(false);

        #region 내려놓기
        if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook == false)
        {
            anim.SetTrigger("Drop");
            cc.enabled = false;
            survivor.transform.parent = null;
            survivor.GetComponent<SurviverHealth>().State = SurviverHealth.HealthState.Down;
        }
        #endregion

        #region 든 상태에서 공격
        if (Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("CarryAttack");
            state = State.CarryAttack;
        }
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

    public void OnRotate()
    {
        canRotate = true;
    }

    public void OffRotate()
    {
        canRotate = false;
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



    #region Pun
    [PunRPC]
    void SetTriggerRpc(string parameter)        // 애니메이션 SetTrigger
    {
        // anim2.SetTrigger(parameter);
    }

    //[PunRPC]
    //void SetBoolTrueRpc(string parameter)       // 애니메이션 SetBool -> true
    //{
    //    anim2.SetBool(parameter, true);
    //}

    //[PunRPC]
    //void SetBoolFalseRpc(string parameter)      // 애니메이션 SetBool -> false
    //{
    //    anim2.SetBool(parameter, false);
    //}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 나의 Anna 라면


        // 나의 Anna 가 아니라면

    }
    #endregion
}