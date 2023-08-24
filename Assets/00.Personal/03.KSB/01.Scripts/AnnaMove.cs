using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class AnnaMove : MonoBehaviourPun, IPunObservable
{
    #region 상태
    public enum State
    {
        Idle, Move, NormalAttack, ThrowingAttack, CoolTime, Carry, CarryAttack, Stunned
    }

    public State state;

    public State AnnaState { get { return state; } set { photonView.RPC(nameof(SetAnnaState), RpcTarget.All, value); } }

    [PunRPC]
    void SetAnnaState(State value)
    {
        state = value;
    }
    #endregion

    #region 변수
    // 캐릭터 관련 변수
    CharacterController cc;                     // 캐릭터 컨트롤러
    Animator anim;                              // 살인마 애니메이터
    public AnimatorOverrideController animOC;   // 살인마 애니메이터 (네트워크)
    public Camera cineCam;                      // 시네머신 카메라
    public Transform playCamera;                // 플레이 카메라
    public Light redlight;                      // 살인마 앞에 있는 조명
    public LayerMask layerMask;                 // OverlapSphere 에서 쓸 LayerMask

    public Transform foward;                    // 앞방향 확인하기 위한 변수   ======================== 삭제 =============================

    // 이동 속도
    float currentSpeed;                     // 현재 이동속도
    float normalSpeed = 4.4f;               // 기본 이동속도
    float chargingSpeed = 3.08f;            // 차징 시 이동속도
    float delaySpeed = 3.74f;               // 경직 시 이동속도

    // 중력
    public float yVelocity;                 // y 속도
    public float gravity = -0.5f;           // 중력속도

    // 회전
    float rotX;                             // X 회전값
    float rotY;                             // Y 회전값
    public float rotSpeed = 100;            // 회전속도
    public Transform cam;                   // 카메라 Transform
    public GameObject go;
    public Vector3 cameraOffset;

    // 시간
    public float gameStartTime;             // 게임시작까지 걸리는 시간
    float currentTime;                      // 현재 시간
    float currentChargingTime;              // 현재 차징 시간
    // float minimumChargingTime = 1.25f;   // 최소 차징 시간
    float maximumChargingTime = 3;          // 최대 차징 시간
    // float axeRechargingTime = 4;         // 도끼 충전 시간

    // 두손 도끼
    public BoxCollider bigAxeCollider;      // 두손도끼 콜라이더

    // 한손 도끼
    public GameObject smallAxe;             // 한손도끼 GameObject
    public GameObject smallAxeFactory;      // 투척용 한손도끼 프리팹
    public Transform throwingSpot;          // 한손도끼 날아갈 위치
    public float chargingForce;             // 한손도끼 던지는 힘
    public float minAxePower = 20;          // 한손도끼 던지는 힘 최소값
    public float maxAxePower = 40;          // 한손도끼 던지는 힘 최대값

    // 한손도끼 갯수
    float maxAxeCount = 5;                  // 최대 소유 가능한 한손도끼 개수
    public float currentAxeCount;                  // 현재 가지고 있는 한손도끼 개수

    // 발전기 
    float maxGenCount = 5;                  // 최대 남은 발전기 개수
    float currentGenCount;                  // 현재 남은 발전기 개수

    // bool
    public bool isNormalAttack;             // 일반공격 중인가?
    bool isCharging;                        // 차징 중인가?
    bool isCanceled;                        // 차징을 취소했는가?
    bool isThrowing;                        // 손도끼를 던졌는가?
    bool isStunned;                         // 스턴 당했는가?
    bool isDrivingForce;                    // 일반공격할 때 추진력을 받았는가?
    public bool isCloset;                   // 캐비넷인가?
    public bool canCarry;                   // 생존자를 들 수 있는가?
    public bool canDestroyGenerator;        // 발전기를 부술 수 있는가?
    public bool canHook;                    // 갈고리에 걸 수 있는가?
    public bool canOpenDoor;                // 캐비넷 문을 열 수 있는가?
    public bool canReLoad;                  // 한 손 도끼를 재충전 할 수 있는가?
    public bool canDestroyPallet;           // 내려간 판자를 부술 수 있는가?
    public bool canRotate;

    // 상호작용 오브젝터 관련 변수
    public Transform leftArm;                   // 왼팔
    public GameObject survivor;                 // 생존자 게임오브젝트
    Animator closetAnim;                        // 클로젯 애니메이터

    // 애니메이션 실행 위치
    GameObject goHook;                      // 갈고리 컴포넌트 가져올 겜옵젝
    Vector3 hookSpot;                       // 갈고리 애니메이션 실행 위치

    public GameObject goCloset;             // 캐비넷 컴포넌트 가져올 겜옵젝
    Vector3 closetSpot;                     // 캐비넷 애니메이션 실행 위치

    GameObject goPallet;                    // 판자 컴포넌트 가져올 겜옵젝
    public Vector3 palletSpot;              // 판자 애니메이션 실행 위치
    Transform palletPos;

    // Photon
    Vector3 receivePos;                     // 전달받을 포지션 값
    Quaternion receiveRot;                  // 전달받을 로테이션 값
    float lerpSpeed = 50;                   // 보정 속력
    float h;                                // 좌우 입력값
    float v;                                // 앞뒤 입력값

    public Collider[] hitcolliders = new Collider[10]; // 배열은 [] 안에 선언해줘야함 배열 얼마나 할 건지

    public bool HaxMode = false;            // 도끼 999 모드
    #endregion

    #region Start
    void Start()
    {
        if (HaxMode) maxAxeCount = 999;

        // 공통
        anim = GetComponent<Animator>();    // 안나 Animator 컴포넌트 가져온다.
        smallAxe.SetActive(false);          // 왼손에 들고 있는 한손도끼 렌더러 비활성화한다.
        bigAxeCollider.enabled = false;     // 양손도끼 콜라이더를 비활성화 한다.

        // 내가 만든 살인마일 경우 ( 살인마 시점 )
        if (photonView.IsMine == true)
        {
            cc = GetComponent<CharacterController>();   // 안나 Character Controller 컴포넌트 가져온다.
            anim.SetLayerWeight(1, 0);                  // 던지는 애니메이션 레이어를 설정한다.
            playCamera.gameObject.SetActive(true);      // 안나 MainCamera 활성화
            redlight.enabled = false;                   // 안나 redlight 꺼놓는다.
            cineCam.depth = 5;                          // 시네머신 카메라 보이게 한다.


            currentAxeCount = maxAxeCount;                                          // 시작할 때 도끼 개수를 최대(5개)로 설정한다.
            UIManager.instance.axeCount.text = Convert.ToString(currentAxeCount);   // UI 로 나타낸다.

            currentGenCount = maxGenCount;                                          // 시작할 때 돌아가야 할 발전기 개수를 최대(5개)로 설정한다.
            UIManager.instance.genCount.text = Convert.ToString(currentGenCount);   // UI 로 나타낸다.
        }

        // 내가 만든 살인마가 아니라면 ( 생존자 시점 )
        else
        {
            redlight.enabled = true;                                        // 살인마 조명 활성화
            anim.SetLayerWeight(1, 0);                                      // 들었을 때 애니메이션 레이어를 설정한다.
            cineCam.gameObject.SetActive(false);                            // 시네머신 카메라 끄기
            anim.runtimeAnimatorController = animOC;                        // 애니메이션 컨트롤러 오버라이드 설정
            playCamera.gameObject.GetComponent<Camera>().enabled = false;   // 플레이카메라 끄기'

        }
    }
    #endregion

    #region 카메라 회전 관련
    private void LateUpdate()
    {
        go.transform.localPosition = cameraOffset;
        //Vector3 rot = cam.transform.localEulerAngles;
        //rot.z = rotY;
        //cam.transform.localEulerAngles = rot; // new Vector3(rotY, 0.022f, -2.476f);
        //rotY = Mathf.Clamp(rotY, -35, 35);

        //Camera.main.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, 0);
    }
    #endregion

    #region Update
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

        #region OverlapSphere & UI
        canHook = false;
        canReLoad = false;
        canDestroyPallet = false;
        canDestroyGenerator = false;
        canCarry = false;

        // photonView.IsMine 이라면 OverlapSphere Radius 2 아니면 2.5
        float a = photonView.IsMine ? 2f : 2.5f;

        hitcolliders = Physics.OverlapSphere(transform.position, a, layerMask);

        for (int i = 0; i < hitcolliders.Length; i++)
        {
            print(hitcolliders[i].transform.gameObject.name);

            // Survivor 생존자
            if (hitcolliders[i].transform.gameObject.name.Contains("Survivor"))
            {
                // 상태가 Healthy 인 생존자가 1명 이상이라면 chase BG이 나오게 한다.

                // 상태가 Down 이라면 canCarry 을 true 로 바꾼다.
                if (hitcolliders[i].GetComponent<SurviverHealth>().State == SurviverHealth.HealthState.Down)
                {
                    canCarry = true;                            // 들 수 있음
                    survivor = hitcolliders[i].gameObject;      // 생존자 게임오브젝트

                    //// UI
                    //if (currentAxeCount > 0)
                    //{
                    //    // 도끼 + 상호작용
                    //    UIManager.instance.DuoUI(true, "들기");
                    //}
                    //else
                    //{
                    //    // 도끼 X + 상호작용
                    //    UIManager.instance.SoloUI(false, true, "들기");
                    //}
                }
            }

            // Generator 발전기
            if (hitcolliders[i].transform.gameObject.name == "EnableCollider")
            {
                canDestroyGenerator = true;

                // UI
                //UIManager.instance.toGetherThrowUI.SetActive(true);
                //UIManager.instance.togetherSpace.SetActive(true);
                //UIManager.instance.togetherText.enabled = true;
                //UIManager.instance.togetherText.text = "파괴";

                // UI
                if (currentAxeCount > 0)
                {
                    // 도끼 + 상호작용
                    UIManager.instance.DuoUI(true, "손상");
                    // 게이지
                    UIManager.instance.GageUI(true, false, "손상");
                }
                else
                {
                    // 도끼 X + 상호작용
                    UIManager.instance.SoloUI(false, true, "손상");
                    // 게이지
                    UIManager.instance.GageUI(true, false, "손상");
                }

                // 만약 발전기의 수리 프로그래스가 0보다 크다면
                // if(발전기 수리 프로그래스 > 0)
                // {
                //      canDestroyGenerator 를 true 로
                // }
            }

            // Pallet 판자
            if (hitcolliders[i].transform.gameObject.name.Contains("Pallet"))
            {
                goPallet = hitcolliders[i].gameObject;
                palletPos = goPallet.GetComponent<Pallet>().GetAnimPosition(transform.position);

                Pallet pallet = goPallet.GetComponent<Pallet>();

                // 만약 판자의 상태가 '내려감' 이라면
                if (pallet.State == Pallet.PalletState.Destroy)
                {
                    // UI
                    if (currentAxeCount > 0)
                    {
                        // 도끼 + 상호작용
                        UIManager.instance.DuoUI(true, "파괴");
                        // 게이지
                        UIManager.instance.GageUI(true, false, "파괴");
                    }
                    else
                    {
                        // 도끼 X + 상호작용
                        UIManager.instance.SoloUI(false, true, "파괴");
                        // 게이지
                        UIManager.instance.GageUI(true, false, "파괴");
                    }
                }
            }

            // Closet 캐비넷
            if (hitcolliders[i].transform.gameObject.name.Contains("Closet"))
            {
                // 처음에 들어오면 null 이기 때문에 else 에서 hitcollider[0] 이 goCloset 이 됨
                // 그 다음에 들어오는 게 hitcolliders[1]이 되면서 둘과 나 사이의 거리를 재서,
                // 만약 hitcollider[0] 이 더 가까우면 그게 최종 goCloset 이 됨
                if (goCloset != null)
                {
                    float ab = Vector3.Distance(hitcolliders[i].transform.position, transform.position);
                    float bc = Vector3.Distance(goCloset.transform.position, transform.position);
                    if (ab < bc) goCloset = hitcolliders[i].gameObject;

                    canOpenDoor = true;

                    // 멀어지면 문을 열 수 없다.
                    if (Vector3.Distance(transform.position, goCloset.transform.position) > 1.7f)
                    {
                        canOpenDoor = false;
                    }

                    if (currentAxeCount > 0)
                    {
                        // UI 두개

                    }
                    else
                    {
                        // UI 한개

                    }

                }
                else
                {
                    goCloset = hitcolliders[i].gameObject;

                    canOpenDoor = true;

                    if (Vector3.Distance(transform.position, goCloset.transform.position) > 1.7f)
                    {
                        canOpenDoor = false;
                    }
                }


                closetAnim = goCloset.GetComponentInParent<Animator>();
                closetSpot = goCloset.GetComponent<Closet>().trCloset.position;
            }


            // Hook 갈고리 // 만약 내 상태가 Carry라면
            if (hitcolliders[i].transform.gameObject.name.Contains("Hook") && state == State.Carry)
            {
                // canHook -> true
                canHook = true;

                // 갈고리 게임오브젝트
                goHook = hitcolliders[i].gameObject;
                hookSpot = goHook.GetComponent<Hook>().hookPos.position;

                // UI
                if (currentAxeCount > 0)
                {
                    // 도끼 + 상호작용
                    UIManager.instance.DuoUI(true, "매달기");
                    // 게이지
                    UIManager.instance.GageUI(true, false, "매달기");
                }
                else
                {
                    // 도끼 X + 상호작용
                    UIManager.instance.SoloUI(false, true, "매달기");
                    // 게이지
                    UIManager.instance.GageUI(true, false, "매달기");
                }
            }

            // UIManager.instance.OffAllUI();
        }
        #endregion

        #region 회전 / 이동 / 상호작용(발전기, 판자, 캐비넷, 갈고리)
        if (photonView.IsMine == true)
        {
            if (canRotate == true)
            {
                #region 회전
                // Idle 상태만 아니면 회전 가능
                if (state != State.Idle)
                {
                    // 회전값을 받아온다.
                    float mx = Input.GetAxis("Mouse X");
                    float my = Input.GetAxis("Mouse Y");

                    // 회전값을 누적
                    rotX += mx * rotSpeed * Time.deltaTime;
                    rotY += my * rotSpeed * Time.deltaTime;

                    // 회전값을 적용
                    transform.eulerAngles = new Vector3(0, rotX, 0);                      // Horizontal
                    cam.transform.eulerAngles = new Vector3(-rotY, rotX, 0);              // Vertical
                    // Camera.main.transform.rotation = Quaternion.Euler(-rotY, rotX, 0);
                    // cam.transform.localEulerAngles = new Vector3(rotY, -90, -180);
                    // rotY = Mathf.Clamp(rotY, 45, -35);
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

                // 차징 하고 있을 때
                if (isCharging == true)
                {
                    // 차지속도(3.08)로 이동
                    currentSpeed = chargingSpeed;
                }
                // 도끼 던지고 나서 딜레이 시간동안
                else if (state == State.CoolTime)
                {
                    currentSpeed = delaySpeed;
                }
                // 일반 공격 들어갈 때
                else if (isDrivingForce == true)
                {
                    currentSpeed = 6.5f;
                }
                // 일반 공격 후 딜레이 시간동안
                else if (isNormalAttack == true)
                {
                    currentSpeed = 0.6f;
                }
                else
                {
                    // 그 이외는 일반속도(4.4)로 이동
                    currentSpeed = normalSpeed;
                }

                // 만약 바닥에 있지 않으면
                if (cc.isGrounded == false)
                {
                    yVelocity += gravity;
                }
                else
                {
                    yVelocity = 0;
                }

                // velocity 설정
                Vector3 velocity = dir * currentSpeed;

                // yVelocity 연결하기
                velocity.y = yVelocity;

                // 이동한다
                cc.Move(velocity * Time.deltaTime);
                #endregion
            }

            #region 발전기 부수기
            if (canDestroyGenerator == true)
            {
                // 스페이스 바를 계속 누르고 있으면 발전기를 부순다.
                if (Input.GetKey(KeyCode.Space))
                {
                    cc.enabled = false;

                    // 애니메이션 true                                
                    photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "DestroyG", true);

                    // UI                 
                    UIManager.instance.GageUI(true, false, "손상");

                    // 게이지 채운다
                    // UIManager.instance.FillGage(t);

                    if (currentAxeCount > 0)
                    {
                        // 도끼 + 상호작용
                        UIManager.instance.DuoUI(false, null);
                    }
                    else
                    {
                        // 도끼 X + 상호작용
                        UIManager.instance.SoloUI(false, false, null);
                    }
                }
                // 중간에 스페이스 바에서 손을 떼면 취소된다.
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    cc.enabled = true;

                    // 취소 애니메이션
                    photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "DestroyG", false);
                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "DestroyCancel");

                    // UI                 
                    UIManager.instance.GageUI(false, false, null);

                    // 게이지 초기화
                    UIManager.instance.EmptyGage();

                    if (currentAxeCount > 0)
                    {
                        // 도끼 + 상호작용
                        UIManager.instance.DuoUI(true, "손상");
                    }
                    else
                    {
                        // 도끼 X + 상호작용
                        UIManager.instance.SoloUI(false, true, "손상");
                    }

                }
            }
            #endregion

            #region 판자 부수기
            if (canDestroyPallet == true)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 pospos = new Vector3(palletPos.position.x + 0.8f, transform.position.y, palletPos.position.z);
                    transform.position = pospos;                                        // 위치를 이동
                    transform.forward = palletPos.forward * -1;                         // 앞방향을 설정

                    cc.enabled = false;                                                 // 움직임 멈춤   

                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "DestroyP");   // 판자를 부수는 애니메이션 실행
                    Pallet pallet = goPallet.GetComponent<Pallet>();
                    pallet.State = Pallet.PalletState.Destroy;                          // 판자가 부서지는 상태로 전환

                    // UI                 
                    UIManager.instance.GageUI(true, false, "파괴");

                    // 게이지 채운다
                    // UIManager.instance.FillGage(t);

                    if (currentAxeCount > 0)
                    {
                        // 도끼 + 상호작용
                        UIManager.instance.DuoUI(false, null);
                    }
                    else
                    {
                        // 도끼 X + 상호작용
                        UIManager.instance.SoloUI(false, false, null);
                    }
                }
            }
            #endregion

            #region 캐비넷 열기
            //if (currentAxeCount < maxAxeCount && canReLoad && Input.GetKeyDown(KeyCode.Space)){}

            // 캐비넷 앞에서 스페이스바를 누르면 캐비넷을 연다
            if (canOpenDoor == true && Input.GetKeyDown(KeyCode.Space))
            {
                // UI
                if (currentAxeCount > 0)
                {
                    // 도끼 + 상호작용
                    UIManager.instance.DuoUI(false, null);
                }
                else
                {
                    // 도끼 X + 상호작용
                    UIManager.instance.SoloUI(false, false, null);
                }

                // 만약 안에 사람이 있다면
                // if(          )
                // {
                // 그 사람을 들어버린다
                // photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "ClosetPickup");
                // 상태를 Carry 로 바꾼다
                // AnnaState = State.Carry;
                // canOpenDoor = false;
                // }

                // 만약 
                /*else*/
                if (0 <= currentAxeCount && currentAxeCount < maxAxeCount)
                {
                    canOpenDoor = false;

                    // 애니메이션 시작 장소로 이동한다.
                    transform.position = closetSpot;                                            // Closet 위치로 이동
                    transform.forward = goCloset.transform.forward;                             // 나의 앞방향을 Closet 앞방향으로

                    OffCC();                                                                    // 움직임 멈춤

                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Reload");             // 도끼를 집어드는 애니메이션 실행
                    photonView.RPC(nameof(SetClosetTriggerRPC), RpcTarget.All, "OpenDoor");     // 캐비넷 열리고 닫히는 애니메이션 실행

                    currentAxeCount = maxAxeCount;                                              // 도끼 최대 소지 갯수를 최대로 채운다

                    UIManager.instance.axeCount.text = Convert.ToString(currentAxeCount);       // UI 갱신한다

                }

                // 만약 안에 사람이 없고 도끼 소지 개수가 최대라면 그냥 열었다가 닫는다
                else
                {
                    canOpenDoor = false;
                    OffCC();
                    OffRotate();
                    // photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "");                // 문 여는 애니메이션
                    photonView.RPC(nameof(SetClosetTriggerRPC), RpcTarget.All, "OpenDoor");     // 문 열리는 애니메이션

                    Invoke("OnCC", 3f);
                    Invoke("OnRotate", 3f);
                }
            }
            #endregion

            #region 갈고리 걸기
            if (state == State.Carry && canHook)
            {
                // 스페이스바를 누르면 
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // UI
                    // UIManager.instance.GageUI(true, true, "매달기");

                    // 게이지 채운다
                    // UIManager.instance.FillGage(t);

                    //if (currentAxeCount > 0)
                    //{
                    //    // 도끼 + 상호작용
                    //    UIManager.instance.DuoUI(false, null);
                    //}
                    //else
                    //{
                    //    // 도끼 X + 상호작용
                    //    UIManager.instance.SoloUI(false, false, null);
                    //}

                    //// 다 차면 UI 끈다.
                    //if (UIManager.instance.gageSlider.value == 0)
                    //{
                    //    UIManager.instance.GageUI(false, false, null);
                    //}
                    // Vector3 animspot = hookSpot +
                    transform.position = hookSpot;
                    transform.forward = transform.forward * -1;

                    // StartCoroutine("LerptoHook", hookSpot);                          // 애니메이션 장소로 Lerp

                    cc.enabled = false;                                                 // 움직임을 멈춘다.

                    photonView.RPC(nameof(Hook), RpcTarget.All);                        // 생존자가 갈고리에 걸리는 애니메이션 실행한다.

                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Hook");       // 생존자를 갈고리에 거는 애니메이션 실행한다.
                }
            }
            #endregion
        }
        // 나의 Anna 가 아닐 때
        else
        {
            // 위치 와 회전 값을 전달 <- 위 아래 회전 막고 싶다.
            transform.position = Vector3.Lerp(transform.position, receivePos, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, receiveRot, lerpSpeed * Time.deltaTime);

            // Parameter 값을 전달
            anim.SetFloat("h", h);
            anim.SetFloat("v", v);
        }
        #endregion
    }
    #endregion

    #region 스턴
    private void Stunned()
    {
        // 만약 뭔가 하고 있다면 스턴 안당함
        // if(){} else{}
        SoundManager.instance.PlayHitSounds(4);

        cc.enabled = false;
        OffRotate();

        UIManager.instance.throwUI.SetActive(false);

        if (state == State.Carry)
        {
            // 캐리 스턴 애니메이션을 실행
            photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "CarryStunned");
        }
        else
        {
            // 스턴 애니메이션을 실행
            photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Stunned");
            OffSmallAxe();
        }
    }
    #endregion

    #region Lerp
    // 갈고리
    IEnumerator LerptoHook(Vector3 animPos)
    {
        while (Vector3.Distance(transform.position, animPos) > 0.3f)
        {
            // Vector3 hookdir = hookSpot.position - transform.position;   // hookSpot 에서 나의 방향벡터
            transform.position = Vector3.Lerp(transform.position, animPos, 0.1f);
            // transform.forward = hookSpot.forward;

            if (Vector3.Distance(transform.position, animPos) <= 0.3f)
            {
                break;
            }
            yield return null;
        }

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
            yield return null;
        }
    }

    // 발전기

    // 판자

    // 캐비넷

    #endregion

    #region 대기
    private void Idle()
    {
        if (photonView.IsMine)
        {
            cc.enabled = false;
            canRotate = false;

            // 게임이 시작하고 5초 뒤에 상태를 Move 로 바꾼다.
            currentTime += Time.deltaTime;
            if (currentTime >= gameStartTime)
            {
                // UI 활성화
                UIManager.instance.murdererUI.enabled = true;
                UIManager.instance.throwUI.SetActive(true);

                // Material
                MaterialManager materialManager = GetComponent<MaterialManager>();
                materialManager.ChangeMaterials();

                // 카메라 및 이동, 회전
                cineCam.enabled = false;
                cc.enabled = true;
                canRotate = true;

                // 안나 상태 전환 및 애니메이션
                AnnaState = State.Move;
                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Move", true);

                // 현재시간 초기화
                currentTime = 0;
            }
        }
    }
    #endregion

    #region 이동 및 공격
    bool playingthrowsound;

    private void UpdateMove()
    {
        if (photonView.IsMine == true)
        {
            #region  일반 공격
            // 마우스 왼쪽 버튼을 누르면 일반공격을 한다.
            if (Input.GetButtonDown("Fire1") && isCharging == false && isThrowing == false && isCanceled == false && state != State.CoolTime)
            {
                SoundManager.instance.PlayHitSounds(0);                             // 때리는 입소리

                AnnaState = State.NormalAttack;                                     // 상태를 NormalAttack 로 바꿈

                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Attack");     // 일반공격 애니메이션 실행

                isNormalAttack = true;                                              // 공격이 끝날 때까지 왼쪽 버튼 못누르게 하기

                UIManager.instance.throwUI.SetActive(false);
            }
            #endregion

            #region 던지는 도끼 공격
            // 마우스 오른쪽 버튼을 누르면 한손도끼를 차징하기 시작한다.
            if (state != State.NormalAttack && isNormalAttack == false && isCanceled == false && currentAxeCount != 0 && Input.GetButton("Fire2"))
            {
                Charging();                                                             // 차징 함수를 실행

                isCharging = true;                                                      // 차징 중에 마우스 왼쪽 버튼을 눌러도 일반공격을 못하도록

                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", true);    // 도끼 드는 애니메이션  실행

                UIManager.instance.throwUI.SetActive(false);
            }

            // 마우스 오른쪽 버튼을 떼면 도끼를 던진다.
            if (isCanceled == false && currentAxeCount != 0 && Input.GetButtonUp("Fire2"))
            {
                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Throw");              // 도끼를 던지는 애니메이션 실행
                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", false);       // 도끼 드는 애니메이션 취소

                currentChargingTime = 0;                                                    // 현재 차징 시간을 초기화
            }

            // 차징 중에 마우스 왼쪽 버튼을 누르면 공격을 취소한다.
            if (isCharging == true && isNormalAttack == false && isThrowing == false && Input.GetButtonDown("Fire1"))
            {
                isCanceled = true;                                                      // 캔슬했음

                isCharging = false;                                                     // 차징 중이 아님

                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", false);   // 차징 애니메이션 취소

                currentChargingTime = 0;                                                // 현재 차징 시간을 초기화

                SoundManager.instance.PlaySmallAxeSounds(2);                            // 취소하는 사운드 재생

                playingchargingsound = false;                                           // 차징하는 사운드 재생 안됨

                playingfullchargingsound = false;

                UIManager.instance.throwUI.SetActive(false);
            }
            #endregion

            #region 생존자 들기
            // 만약 사정거리 안에 생존자가 쓰러져있다면
            if (canCarry && state != State.Carry)
            {
                if (photonView.IsMine)
                {
                    // 들어올리기 UI 가 화면에 보일 때 스페이스바를 누르면
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        StartCoroutine("LerptoSurvivor");                                   // 애니메이션 시작 장소로 이동하는 코루틴 실행한다.

                        photonView.RPC(nameof(SurvivorCarry), RpcTarget.All);               // 생존자의 상태를 바꾼다.

                        photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Pickup");     // 생존자를 들어올린다.

                        AnnaState = State.Carry;                                            // 상태를 Carry 로 바꾼다.

                        canCarry = false;                                                   // 들 수 있는 상태가 아님

                        survivor.transform.forward = transform.forward;                     // 생존자의 머리가 내 등쪽으로 향하게 한다.

                        //if (currentAxeCount > 0)
                        //{
                        //    UIManager.instance.DuoUI(false, null);
                        //}
                        //else
                        //{
                        //    UIManager.instance.SoloUI(false, false, null);
                        //}
                    }
                }
            }
            #endregion
        }
    }
    #endregion

    #region 일반공격
    private void NormalAttack()
    {
        if (photonView.IsMine)
        {
            UIManager.instance.throwUI.SetActive(false);
            //anim.SetLayerWeight(3, 0f);
            // 공격이 끝나면 상태를 Move 로 바꾼다. -> 애니메이션 이벤트
        }
    }
    #endregion

    #region 한손 도끼 차징 / 던지기 / 쿨타임 
    bool playingchargingsound;
    bool playingfullchargingsound;
    void Charging()
    {
        if (photonView.IsMine)
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
            // print("aaa : " + chargingForce);
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
    }

    // 도끼 던짐
    private void ThrowingAttack()
    {
        if (photonView.IsMine)
        {
            if (isThrowing == true) return;                                 // 만약 isThrowing 라면 리턴

            isThrowing = true;
            isCharging = false;                                             // 차징 중이 아님
            playingchargingsound = false;                                   // 차징하는 사운드 재생할 수 있는 상태
            playingfullchargingsound = false;

            //GameObject smallaxe = Instantiate(smallAxeFactory);             // 한손도끼를 만든다.
            //smallaxe.transform.position = throwingSpot.position;            // 만든 한손도끼의 위치를 왼손에 배치한다.
            //smallaxe.transform.forward = Camera.main.transform.forward;     // 만든 한손도끼의 앞방향을 카메라의 앞방향으로 한다

            Vector3 pos = throwingSpot.position;
            Vector3 forward = Camera.main.transform.forward;

            photonView.RPC(nameof(ThrowAxeRPC), RpcTarget.All, pos, forward, chargingForce);

            currentAxeCount--;                                              // 도끼 갯수를 줄인다
            UIManager.instance.axeCount.text = Convert.ToString(currentAxeCount);              // UI 갱신한다
            print(currentAxeCount);

            AnnaState = State.CoolTime;                                         // 상태를 CoolTime 으로 바꾼다
        }
    }

    // 2초 쿨타임
    void CoolTime()
    {
        if (photonView.IsMine)
        {
            isThrowing = true;

            playingchargingsound = false;       // 차징하는 사운드 재생할 수 있는 상태
            playingfullchargingsound = false;

            currentTime += Time.deltaTime;      // 현재 시간을 흐르게 한다
            if (currentTime >= 2)               // 현재 시간이 2초가 되면
            {
                AnnaState = State.Move;         // 상태를 Move 로 바꾼다

                isCharging = false;             // 차징 가능
                isThrowing = false;             // 던지기 가능
                isCanceled = false;

                if (currentAxeCount != 0)       // 현재 도끼 개수가 0이 아니라면
                {
                    UIManager.instance.throwUI.SetActive(true);    // 도끼 개수 UI 보이게 하기 ( 만약 0 이라면 계속 안보이겠지)
                }

                currentTime = 0;                // 현재 시간을 초기화
            }
        }
    }
    #endregion

    #region 생존자 들기 / 든 상태에서 공격
    // 이동
    private void UpdateCarry()
    {
        if (photonView.IsMine)
        {
            UIManager.instance.throwUI.SetActive(false);

            #region 내려놓기
            //if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook == false)
            //{
            //    // anim.SetTrigger("Drop");
            //    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Drop");
            //    cc.enabled = false;
            //    survivor.transform.parent = null;
            //    survivor.GetComponent<SurviverHealth>().State = SurviverHealth.HealthState.Down;
            //}
            #endregion

            #region 든 상태에서 공격
            if (Input.GetButtonDown("Fire1"))
            {
                // anim.SetTrigger("CarryAttack");
                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "CarryAttack");
                AnnaState = State.CarryAttack;
            }
            #endregion
        }
        else
        {
            // h 나 v 입력값이 있으면
            if (h > 0 || v > 0)
            {
                // 들기 애니메이터 레이어 값을 변경
                anim.SetLayerWeight(1, 0.5f);
            }
            // 움직이지 않을 때
            else
            {
                anim.SetLayerWeight(1, 0);
            }
        }
    }

    // 공격
    public void CarryAttack()
    {
        if (photonView.IsMine)
        {
            OnAxe();

            currentTime += Time.deltaTime;
            if (currentTime >= 2.5f)
            {
                OffAxe();
                AnnaState = State.Carry;
                currentTime = 0;
            }
        }
    }
    #endregion

    #region Events
    public void OnCC()
    {
        if (photonView.IsMine == false) return;
        cc.enabled = true;
    }

    public void OffCC()
    {
        if (photonView.IsMine == false) return;
        cc.enabled = false;
    }

    public void OnRotate()
    {
        if (photonView.IsMine == false) return;
        canRotate = true;
    }

    public void OffRotate()
    {
        if (photonView.IsMine == false) return;
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

    public void StartDrivingForce()
    {
        isDrivingForce = true;
    }

    public void EndDrivingForce()
    {
        isDrivingForce = false;
    }

    public void Throwing()      // State 를 ThrowingAttack 로 바꾸는 함수
    {
        if (photonView.IsMine == false) return;
        AnnaState = State.ThrowingAttack;
    }

    public void OnCoolTime()    // State 를 CoolTime 으로 바꾸는 함수
    {
        if (photonView.IsMine == false) return;
        AnnaState = State.CoolTime;
    }

    public void OnSmallAxe()    // 도끼 활성화
    {
        // smallAxe.SetActive(true);
        photonView.RPC(nameof(AxeRPC), RpcTarget.All, true);
    }

    public void OffSmallAxe()   // 도끼 비활성화
    {
        // smallAxe.SetActive(false);
        photonView.RPC(nameof(AxeRPC), RpcTarget.All, false);
    }

    // 초기화 함수
    void OnMyReset()
    {
        if (photonView.IsMine)
        {
            AnnaState = State.Move;

            OnCC();
            OffAxe();
            // OffSmallAxe();
            photonView.RPC(nameof(AxeRPC), RpcTarget.All, false);
            photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", false);
            photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "DestroyG", false);


            isStunned = false;
            isCharging = false;
            isCanceled = false;
            isNormalAttack = false;
            playingthrowsound = false;
            canDestroyGenerator = false;
            canDestroyPallet = false;
            canReLoad = false;
            UIManager.instance.throwUI.SetActive(true);
        }
    }
    #endregion

    #region PunRPC
    [PunRPC]    // Closet 애니메이터 trigger
    void SetClosetTriggerRPC(string parameter)
    {
        closetAnim.SetTrigger(parameter);
    }

    [PunRPC]    // Anna 애니메이터 trigger
    void SetTriggerRPC(string parameter)
    {
        anim.SetTrigger(parameter);
    }

    [PunRPC]    // Anna 애니메이터 Bool
    void SetBoolRPC(string parameter, bool boolean)
    {
        anim.SetBool(parameter, boolean);
    }

    [PunRPC]    // 도끼 던지기
    void ThrowAxeRPC(Vector3 throwPos, Vector3 throwForward, float force)
    {
        GameObject axe = Instantiate(smallAxeFactory);      // 도끼 생성
        axe.transform.position = throwPos;                  // 도끼 던지는 위치
        axe.transform.forward = throwForward;               // 도끼 앞 방향
        axe.GetComponent<Axe>().flying(force);              // 도끼 던지는 힘
        axe.GetComponent<Axe>().photonView = this.photonView;
    }

    [PunRPC]    // 도끼 메쉬렌더러 활성여부
    public void AxeRPC(Boolean axeBool)
    {
        smallAxe.SetActive(axeBool);
    }

    [PunRPC]    // 생존자 들기
    void SurvivorCarry()
    {
        survivor.GetComponent<SurviverHealth>().ChangeCarring();        // 생존자의 상태를 바꾸는 함수를 호출 

        survivor.transform.parent = leftArm.transform;                 // 생존자를 나의 자식 오브젝트로 설정한다.


        //survivor.transform.localPosition = new Vector3(-0.331999868f, 0.976001263f, -0.19100064f);
        survivor.transform.localPosition = new Vector3(-0.0379999988f, 0.976001143f, 0.202000007f);
        //survivor.transform.localRotation = new Quaternion(2.60770321e-08f, -0.1253708f, 2.09547579e-09f, 0.992109954f);
        survivor.transform.localRotation = new Quaternion(0, -0.999083936f, 0, -0.0427953899f);
    }

    [PunRPC]    // 갈고리 걸기
    void Hook()
    {
        survivor.GetComponent<SurviverHealth>().ChangeCarring();

        survivor.transform.parent = null;   // 생존자와의 부모자식 관계를 끊는다.
    }
    #endregion

    #region OnPhotonSerializeView
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 나의 Anna 라면
        if (stream.IsWriting)
        {
            // 나의 위치 값을 보낸다.
            stream.SendNext(transform.position);

            // Y 회전 값을 보낸다.
            stream.SendNext(new Vector3(0, transform.eulerAngles.y, 0));

            // h 값을 보낸다
            stream.SendNext(h);

            // v 값을 보낸다
            stream.SendNext(v);
        }

        // 나의 Anna 가 아니라면
        else
        {
            // 그의 위치 값을 받는다.
            receivePos = (Vector3)stream.ReceiveNext();

            // Y 회전 값을 받는다.
            receiveRot = Quaternion.Euler((Vector3)stream.ReceiveNext());

            // h 값을 받는다
            h = (float)stream.ReceiveNext();

            // v 값을 받는다
            v = (float)stream.ReceiveNext();
        }
    }
    #endregion
}