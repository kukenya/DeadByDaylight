using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class AnnaMove : MonoBehaviourPun, IPunObservable
{
    #region ����
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

    #region ����
    // ĳ���� ���� ����
    CharacterController cc;                     // ĳ���� ��Ʈ�ѷ�
    Animator anim;                              // ���θ� �ִϸ�����
    public AnimatorOverrideController animOC;   // ���θ� �ִϸ����� (��Ʈ��ũ)
    public Camera cineCam;                      // �ó׸ӽ� ī�޶�
    public Transform playCamera;                // �÷��� ī�޶�
    public Light redlight;                      // ���θ� �տ� �ִ� ����
    public LayerMask layerMask;                 // OverlapSphere ���� �� LayerMask
    public LayerMask suvivorLayerMask;          // ������ ���̾� ����ũ(���̿�)
    public Transform foward;                    // �չ��� Ȯ���ϱ� ���� ����
    public Transform animCam;
    public Transform trSkeleton;
    public Transform cameraAnimOffset;

    // �̵� �ӵ�
    float currentSpeed;                     // ���� �̵��ӵ�
    float normalSpeed = 4.4f;               // �⺻ �̵��ӵ�
    float chargingSpeed = 3.08f;            // ��¡ �� �̵��ӵ�
    float delaySpeed = 3.74f;               // ���� �� �̵��ӵ�

    // �߷�
    public float yVelocity;                 // y �ӵ�
    public float gravity = -0.5f;           // �߷¼ӵ�

    // ȸ��
    float rotX;                             // X ȸ����
    float rotY;                             // Y ȸ����
    public float rotSpeed = 100;            // ȸ���ӵ�
    public Transform cam;                   // ī�޶� Transform
    public GameObject go;
    public Vector3 cameraOffset;

    // �ð�
    public float gameStartTime;             // ���ӽ��۱��� �ɸ��� �ð�
    float currentTime;                      // ���� �ð�
    float currentChargingTime;              // ���� ��¡ �ð�
    // float minimumChargingTime = 1.25f;   // �ּ� ��¡ �ð�
    float maximumChargingTime = 3;          // �ִ� ��¡ �ð�
    // float axeRechargingTime = 4;         // ���� ���� �ð�

    // �μ� ����
    public BoxCollider bigAxeCollider;      // �μյ��� �ݶ��̴�
    public Transform rayPos;                // ���� ������

    // �Ѽ� ����
    public GameObject smallAxe;             // �Ѽյ��� GameObject
    public GameObject smallAxeFactory;      // ��ô�� �Ѽյ��� ������
    public Transform throwingSpot;          // �Ѽյ��� ���ư� ��ġ
    public float chargingForce;             // �Ѽյ��� ������ ��
    public float minAxePower = 20;          // �Ѽյ��� ������ �� �ּҰ�
    public float maxAxePower = 40;          // �Ѽյ��� ������ �� �ִ밪

    // �Ѽյ��� ����
    float maxAxeCount = 5;                  // �ִ� ���� ������ �Ѽյ��� ����
    public float currentAxeCount;           // ���� ������ �ִ� �Ѽյ��� ����

    // ������ 
    float maxGenCount = 5;                  // �ִ� ���� ������ ����
    float currentGenCount;                  // ���� ���� ������ ����

    // bool
    public bool isNormalAttack;             // �Ϲݰ��� ���ΰ�?
    bool isCharging;                        // ��¡ ���ΰ�?
    bool isCanceled;                        // ��¡�� ����ߴ°�?
    bool isThrowing;                        // �յ����� �����°�?
    bool isStunned;                         // ���� ���ߴ°�?
    bool isDrivingForce;                    // �Ϲݰ����� �� �������� �޾Ҵ°�?
    bool isDestroying;                      // �����⸦ �μ��� ���ΰ�?
    public bool isanimation;                // �ִϸ��̼��� ���� ���ΰ�?
    public bool isCloset;                   // ĳ����ΰ�?
    public bool canCarry;                   // �����ڸ� �� �� �ִ°�?
    public bool canDestroyGenerator;        // �����⸦ �μ� �� �ִ°�?
    public bool canHook;                    // ������ �� �� �ִ°�?
    public bool canOpenDoor;                // ĳ��� ���� �� �� �ִ°�?
    public bool canReLoad;                  // �� �� ������ ������ �� �� �ִ°�?
    public bool canDestroyPallet;           // ������ ���ڸ� �μ� �� �ִ°�?
    public bool canRotate;

    // ��ȣ�ۿ� �������� ���� ����
    public Transform leftArm;                   // ����
    public GameObject survivor;                 // ������ ���ӿ�����Ʈ
    Animator closetAnim;                        // Ŭ���� �ִϸ�����

    // �ִϸ��̼� ���� ��ġ
    GameObject goHook;                      // ���� ������Ʈ ������ �׿���
    Vector3 hookSpot;                       // ���� �ִϸ��̼� ���� ��ġ

    public GameObject goCloset;             // ĳ��� ������Ʈ ������ �׿���
    Vector3 closetSpot;                     // ĳ��� �ִϸ��̼� ���� ��ġ

    GameObject goPallet;                    // ���� ������Ʈ ������ �׿���
    public Vector3 palletSpot;              // ���� �ִϸ��̼� ���� ��ġ
    Transform palletPos;

    // Photon
    Vector3 receivePos;                     // ���޹��� ������ ��
    Quaternion receiveRot;                  // ���޹��� �����̼� ��
    float lerpSpeed = 50;                   // ���� �ӷ�
    float h;                                // �¿� �Է°�
    float v;                                // �յ� �Է°�

    public Collider[] hitcolliders = new Collider[10]; // �迭�� [] �ȿ� ����������� �迭 �󸶳� �� ����

    public bool HaxMode = false;            // ���� 999 ���
    #endregion

    #region Start
    void Start()
    {
        if (HaxMode) maxAxeCount = 999;

        // ����
        anim = GetComponent<Animator>();    // �ȳ� Animator ������Ʈ �����´�.
        smallAxe.SetActive(false);          // �޼տ� ��� �ִ� �Ѽյ��� ������ ��Ȱ��ȭ�Ѵ�.
        bigAxeCollider.enabled = false;     // ��յ��� �ݶ��̴��� ��Ȱ��ȭ �Ѵ�.

        // ���� ���� ���θ��� ��� ( ���θ� ���� )
        if (photonView.IsMine == true)
        {
            cc = GetComponent<CharacterController>();   // �ȳ� Character Controller ������Ʈ �����´�.
            anim.SetLayerWeight(1, 0);                  // ������ �ִϸ��̼� ���̾ �����Ѵ�.
            playCamera.gameObject.SetActive(true);      // �ȳ� MainCamera Ȱ��ȭ
            redlight.enabled = false;                   // �ȳ� redlight �����´�.
            cineCam.depth = 5;                          // �ó׸ӽ� ī�޶� ���̰� �Ѵ�.


            currentAxeCount = maxAxeCount;                                          // ������ �� ���� ������ �ִ�(5��)�� �����Ѵ�.
            UIManager.instance.axeCount.text = Convert.ToString(currentAxeCount);   // UI �� ��Ÿ����.

            currentGenCount = maxGenCount;                                          // ������ �� ���ư��� �� ������ ������ �ִ�(5��)�� �����Ѵ�.
            UIManager.instance.genCount.text = Convert.ToString(currentGenCount);   // UI �� ��Ÿ����.
        }

        // ���� ���� ���θ��� �ƴ϶�� ( ������ ���� )
        else
        {
            redlight.enabled = true;                                                // ���θ� ���� Ȱ��ȭ
            anim.SetLayerWeight(1, 0);                                              // ����� �� �ִϸ��̼� ���̾ �����Ѵ�.
            cineCam.gameObject.SetActive(false);                                    // �ó׸ӽ� ī�޶� ����
            anim.runtimeAnimatorController = animOC;                                // �ִϸ��̼� ��Ʈ�ѷ� �������̵� ����
            playCamera.gameObject.GetComponent<Camera>().enabled = false;           // �÷���ī�޶� ����
            playCamera.gameObject.GetComponent<AudioListener>().enabled = false;    // �÷���ī�޶� ����� ������ ����

        }
    }
    #endregion

    #region ī�޶� ȸ�� ����
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

    #region Update : ����ġ / OverlapSphere & UI / ȸ�� / �̵� / ��ȣ�ۿ�
    void Update()
    {
        #region ����ġ
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

        // photonView.IsMine �̶�� OverlapSphere Radius 2 �ƴϸ� 2.5
        float a = photonView.IsMine ? 2f : 2.5f;

        hitcolliders = Physics.OverlapSphere(transform.position, a, layerMask);

        for (int i = 0; i < hitcolliders.Length; i++)
        {
            // print(hitcolliders[i].transform.gameObject.name);

            #region Closet ĳ���
            if (hitcolliders[i].transform.gameObject.name.Contains("Closet") && isanimation == false)
            {
                // ó���� ������ null �̱� ������ else ���� hitcollider[0] �� goCloset �� ��
                // �� ������ ������ �� hitcolliders[1]�� �Ǹ鼭 �Ѱ� �� ������ �Ÿ��� �缭,
                // ���� hitcollider[0] �� �� ������ �װ� ���� goCloset �� ��
                if (goCloset != null)
                {
                    float ab = Vector3.Distance(hitcolliders[i].transform.position, transform.position);
                    float bc = Vector3.Distance(goCloset.transform.position, transform.position);
                    if (ab < bc) goCloset = hitcolliders[i].gameObject;

                    canOpenDoor = true;

                    UICloset();

                    // �־����� ���� �� �� ����.
                    if (Vector3.Distance(transform.position, goCloset.transform.position) > 1.7f)
                    {
                        canOpenDoor = false;
                        UICloset();
                    }
                }

                else
                {
                    goCloset = hitcolliders[i].gameObject;

                    canOpenDoor = true;

                    UICloset();

                    if (Vector3.Distance(transform.position, goCloset.transform.position) > 1.7f)
                    {
                        canOpenDoor = false;
                        UICloset();
                    }
                }

                closetAnim = goCloset.GetComponentInParent<Animator>();
                closetSpot = goCloset.GetComponent<Closet>().trCloset.position;
            }
            #endregion

            #region Pallet ����
            if (hitcolliders[i].transform.gameObject.name.Contains("Pallet") && isanimation == false)
            {
                goPallet = hitcolliders[i].gameObject;
                palletPos = goPallet.GetComponent<Pallet>().GetAnimPosition(transform.position);

                Pallet pallet = goPallet.GetComponent<Pallet>();

                // ���� ������ ���°� '������' �̶��
                if (pallet.State == Pallet.PalletState.Ground)
                {
                    canDestroyPallet = true;

                    UIPallet();

                    if (Vector3.Distance(transform.position, pallet.transform.position) > 1.7f)
                    {
                        canDestroyPallet = false;
                        UIPallet();
                    }
                }
            }
            #endregion

            #region Survivor ������
            if (hitcolliders[i].transform.gameObject.name.Contains("Survivor"))
            {
                // ���°� Down �̶�� canCarry �� true �� �ٲ۴�.
                if (hitcolliders[i].GetComponent<SurviverHealth>().State == SurviverHealth.HealthState.Down)
                {
                    survivor = hitcolliders[i].gameObject;      // ������ ���ӿ�����Ʈ

                    canCarry = true;                            // �� �� ����

                    UICarry();

                    if (Vector3.Distance(transform.position, survivor.transform.position) > 1.7f)
                    {
                        canCarry = false;

                        UICarry();
                    }
                }
            }
            #endregion

            #region Hook ���� // ���� �� ���°� Carry���
            if (hitcolliders[i].transform.gameObject.name.Contains("Hook") && state == State.Carry && isanimation == false)
            {
                // canHook -> true
                canHook = true;
                UIHook();

                // ���� ���ӿ�����Ʈ
                goHook = hitcolliders[i].gameObject;
                hookSpot = goHook.GetComponent<Hook>().hookPos.position;

                if (Vector3.Distance(transform.position, hookSpot) > 1.7f)
                {
                    canHook = false;
                    UIHook();
                }
            }
            #endregion

            #region Generator ������ <- ����
            //if (hitcolliders[i].transform.gameObject.name == "EnableCollider" && isanimation == false)
            //{
            //    canDestroyGenerator = true;

            //    if (Vector3.Distance(transform.position, hitcolliders[i].transform.position) > 1.7f)
            //    {
            //        canDestroyGenerator = false;

            //        // UI                 
            //        UIGenerator();
            //    }

            // ���� �������� ���� ���α׷����� 0���� ũ�ٸ�
            // if(������ ���� ���α׷��� > 0)
            // {
            //      canDestroyGenerator �� true ��
            // }
            //}
            #endregion
        }
        #endregion

        #region ȸ�� / �̵� / ��ȣ�ۿ�(������, ����, ĳ���, ����)
        if (photonView.IsMine == true)
        {
            if (canRotate == true)
            {
                trSkeleton.transform.parent = Camera.main.transform;

                #region ȸ��
                // Idle ���¸� �ƴϸ� ȸ�� ����
                if (state != State.Idle)
                {
                    // ȸ������ �޾ƿ´�.
                    float mx = Input.GetAxis("Mouse X");
                    float my = Input.GetAxis("Mouse Y");

                    // ȸ������ ����
                    rotX += mx * rotSpeed * Time.deltaTime;
                    rotY += my * rotSpeed * Time.deltaTime;

                    // ȸ������ ����
                    transform.eulerAngles = new Vector3(0, rotX, 0);                      // Horizontal
                    cam.transform.eulerAngles = new Vector3(-rotY, rotX, 0);              // Vertical
                    // Camera.main.transform.rotation = Quaternion.Euler(-rotY, rotX, 0);
                    // cam.transform.localEulerAngles = new Vector3(rotY, -90, -180);
                    // rotY = Mathf.Clamp(rotY, 45, -35);
                }
                #endregion

                #region �̵�
                // �̵��� �޾ƿ´�
                h = Input.GetAxis("Horizontal");
                v = Input.GetAxis("Vertical");

                // �̵����� �ִϸ��̼ǰ� ����
                anim.SetFloat("h", h);
                anim.SetFloat("v", v);

                // ������ ���Ѵ�
                Vector3 dirH = transform.right * h;
                Vector3 dirV = transform.forward * v;
                Vector3 dir = dirH + dirV;
                dir.Normalize();

                // ��¡ �ϰ� ���� ��
                if (isCharging == true)
                {
                    // �����ӵ�(3.08)�� �̵�
                    currentSpeed = chargingSpeed;
                }
                // ���� ������ ���� ������ �ð�����
                else if (state == State.CoolTime)
                {
                    currentSpeed = delaySpeed;
                }
                // �Ϲ� ���� �� ��
                else if (isDrivingForce == true)
                {
                    currentSpeed = 6.5f;
                }
                // �Ϲ� ���� �� ������ �ð�����
                else if (isNormalAttack == true)
                {
                    currentSpeed = 0.6f;
                }
                else
                {
                    // �� �ܴ̿� �Ϲݼӵ�(4.4)�� �̵�
                    currentSpeed = normalSpeed;
                }

                // ���� �ٴڿ� ���� ������
                if (cc.isGrounded == false)
                {
                    yVelocity += gravity;
                }
                else
                {
                    yVelocity = 0;
                }

                // velocity ����
                Vector3 velocity = dir * currentSpeed;

                // yVelocity �����ϱ�
                velocity.y = yVelocity;

                // �̵��Ѵ�
                cc.Move(velocity * Time.deltaTime);
                #endregion
            }
            else
            {
                trSkeleton.transform.parent = this.transform;
                Camera.main.transform.position = cameraAnimOffset.position;
                Camera.main.transform.eulerAngles = animCam.eulerAngles;
            }

            #region ĳ��� ����
            // ĳ��� �տ��� �����̽��ٸ� ������ ĳ����� ����
            if (canOpenDoor == true && Input.GetKeyDown(KeyCode.Space))
            {
                canOpenDoor = false;
                isanimation = true;

                UICloset();

                UIManager.instance.throwUI.SetActive(false);

                if (0 <= currentAxeCount && currentAxeCount < maxAxeCount)
                {
                    // canOpenDoor = false;

                    // �ִϸ��̼� ���� ��ҷ� �̵��Ѵ�.
                    transform.position = closetSpot;                                            // Closet ��ġ�� �̵�
                    transform.forward = goCloset.GetComponent<Closet>().trCloset.forward;       // ���� �չ����� Closet �չ�������

                    OffCC();                                                                    // ������ ����
                    OffRotate();                                                                // ȸ�� �Ұ���

                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Reload");             // ������ ������ �ִϸ��̼� ����
                    photonView.RPC(nameof(SetClosetTriggerRPC), RpcTarget.All, "OpenDoor");     // ĳ��� ������ ������ �ִϸ��̼� ����
                }

                // ���� ���� ������ �ִ��� �׳� �����ٰ� �ݴ´�
                else
                {
                    // canOpenDoor = false;

                    OffCC();
                    OffRotate();

                    // photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "");                // �� ���� �ִϸ��̼�
                    photonView.RPC(nameof(SetClosetTriggerRPC), RpcTarget.All, "OpenDoor");     // �� ������ �ִϸ��̼�

                    Invoke("OnCC", 3f);         // ������ ����
                    Invoke("OnRotate", 3f);     // ȸ�� ����
                }
            }
            #endregion

            #region ���� �μ���
            if (canDestroyPallet == true)
            {
                UIPallet();

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    canDestroyPallet = false;   // ���� �μ��� �Ұ���
                    isanimation = true;         // �ִϸ��̼� ��

                    UIPallet();     // UI

                    OffCC();        // ������ ����   
                    OffRotate();    // ȸ�� �Ұ���
                    //if (palletPos.position == )
                    //{

                    //}
                    Vector3 pospos = new Vector3(palletPos.position.x, transform.position.y, palletPos.position.z);  // Y�� ����
                    transform.position = pospos;                                        // ��ġ�� �̵�
                    transform.forward = palletPos.forward * -1;                         // �չ����� ����

                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "DestroyP");   // ���ڸ� �μ��� �ִϸ��̼� ����

                    Pallet pallet = goPallet.GetComponent<Pallet>();                    // ���� ������Ʈ
                    pallet.State = Pallet.PalletState.Destroy;                          // ���ڰ� �μ����� ���·� ��ȯ
                }
            }
            #endregion

            #region ���� �ɱ�
            if (state == State.Carry && canHook)
            {
                // �����̽��ٸ� ������ 
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    canHook = false;
                    isanimation = true;

                    UIHook();     // UI

                    // ������ ä���

                    Hook hook = goHook.GetComponent<Hook>();

                    transform.forward = hook.hookPos.forward;

                    // StartCoroutine("LerptoHook", hookSpot);                          // �ִϸ��̼� ��ҷ� Lerp

                    cc.enabled = false;                                                 // �������� �����.

                    photonView.RPC(nameof(Hook), RpcTarget.All);                        // �����ڰ� ������ �ɸ��� �ִϸ��̼� �����Ѵ�.

                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Hook");       // �����ڸ� ������ �Ŵ� �ִϸ��̼� �����Ѵ�.

                    // �ڷ�ƾ �Լ� : ��Ȧ ���ʷ���Ʈ
                    StartCoroutine(BlackHole(hook));
                }
            }
            #endregion

            #region ������ UI ���� ==================================================================================================================
            UIManager.instance.genCount.text = Convert.ToString(currentGenCount);

            // ���� �����Ⱑ �ϳ��� ���� �ʰ� �ȴٸ�
            if (currentGenCount == 0)
            {
                // ������ �׸��� ��Ȱ��ȭ �ϰ�



                // Ż�� �׸��� Ȱ��ȭ �Ѵ�.

            }
            #endregion

            #region ������ �μ��� <- ����
            //if (canDestroyGenerator == true)
            //{
            //    UIGenerator();

            //    // �����̽� �ٸ� ��� ������ ������ �����⸦ �μ���.
            //    if (Input.GetKey(KeyCode.Space))
            //    {
            //        isDestroying = true;                                                        // �μ��� ��
            //        isanimation = true;                                                         // �ִϸ��̼� true

            //        OffCC();                                                                    // ������ ���߱�
            //        OffRotate();                                                                // ȸ�� ����

            //        UIManager.instance.GageUI(true, false, "�ջ�");                             // ������ UI true, �ջ�
            //        // UIManager.instance.FillGage(t);                                          // ������ ä��� �ڷ�ƾ ����

            //        photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "DestroyG", true);        // ������ �μ��� �ִϸ��̼� ����

            //    }
            //    // �߰��� �����̽� �ٿ��� ���� ���� ��ҵȴ�.
            //    if (Input.GetKeyUp(KeyCode.Space))
            //    {
            //        isDestroying = true;                                                        // �μ��� ���� �ƴ�
            //        isanimation = false;                                                        // �ִϸ��̼� false

            //        OnCC();                                                                     // ������ ���߱�
            //        OnRotate();                                                                 // ȸ�� ����

            //        UIManager.instance.GageUI(false, false, null);                              // ������ UI false, null
            //                                                                                    // UIManager.instance.FillGage(t);                                          // ������ ä��� �ڷ�ƾ ����
            //        UIManager.instance.EmptyGage();                                            // ������ �ʱ�ȭ

            //        photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "DestroyG", false);       // ��� �ִϸ��̼�
            //        photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "DestroyCancel");
            //    }
            //}
            #endregion
        }
        // ���� Anna �� �ƴ� ��
        else
        {
            // ��ġ �� ȸ�� ���� ���� <- �� �Ʒ� ȸ�� ���� �ʹ�.
            transform.position = Vector3.Lerp(transform.position, receivePos, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, receiveRot, lerpSpeed * Time.deltaTime);

            // Parameter ���� ����
            anim.SetFloat("h", h);
            anim.SetFloat("v", v);
        }
        #endregion

    }
    #endregion

    #region ��Ȧ
    IEnumerator BlackHole(Hook hook)
    {
        yield return new WaitForSeconds(1.3f);
        hook.photonView.RPC(nameof(hook.GenerateHookBlackHoleEffect), RpcTarget.All);
    }
    #endregion

    #region UI
    #region UI - ĳ���
    public void UICloset()
    {
        if (photonView.IsMine)
        {
            // �� �� �� ���� ��
            if (canOpenDoor == true)
            {
                UIManager.instance.throwUI.SetActive(false);                          // �߾� ������ UI ��Ȱ��

                if (currentAxeCount > 0) UIManager.instance.DuoUI(true, "ã��");      // ���� O : ������UI + �����̽��� * ã��

                else UIManager.instance.SoloUI(true, "ã��");                         // ���� X : �����̽��� + ã��
            }

            // �� �� �� ���� ��
            else
            {
                if (currentAxeCount > 0)
                {
                    // �ִϸ��̼��� �����ϰ� ���� ������  �߾� ������ UI Ȱ��
                    if (isanimation == false) UIManager.instance.throwUI.SetActive(true);

                    UIManager.instance.DuoUI(false, null);
                }

                else UIManager.instance.SoloUI(false, null);
            }
        }
    }
    #endregion

    #region UI - ��  ��
    public void UIPallet()
    {
        if (photonView.IsMine)
        {
            // ���� ������ ���� UI Ȱ��
            if (canDestroyPallet == true)
            {
                UIManager.instance.throwUI.SetActive(false);                          // ������ UI ��Ȱ��

                if (currentAxeCount > 0) UIManager.instance.DuoUI(true, "�ı�");      // ���� O : ������UI + �����̽��� * �ı�

                else UIManager.instance.SoloUI(true, "�ı�");                         // ���� X : �����̽��� + �ı�
            }

            // ���ڿ��� �������� UI ��Ȱ��
            else
            {
                if (currentAxeCount > 0)
                {
                    // �ִϸ��̼��� �����ϰ� ���� ������ ������ UI Ȱ��
                    if (isanimation == false) UIManager.instance.throwUI.SetActive(true);

                    UIManager.instance.DuoUI(false, null);
                }

                else UIManager.instance.SoloUI(false, null);
            }
        }
    }
    #endregion

    #region UI - ������ ��� & ����
    public void UICarry()
    {
        if (photonView.IsMine)
        {
            // ������ ������ ���� UI Ȱ��
            if (canCarry == true)
            {
                UIManager.instance.throwUI.SetActive(false);                          // ������ UI ��Ȱ��

                if (currentAxeCount > 0) UIManager.instance.DuoUI(true, "���");      // ���� O : ������UI + �����̽��� * ���

                else UIManager.instance.SoloUI(true, "���");                         // ���� X : �����̽��� + ���
            }

            // �����ڷκ��� �������� UI ��Ȱ��
            else
            {
                if (currentAxeCount > 0)
                {
                    // �ִϸ��̼��� �����ϰ� ���� ������ ������ UI Ȱ��
                    if (isanimation == false) UIManager.instance.throwUI.SetActive(true);

                    UIManager.instance.DuoUI(false, null);
                }

                else UIManager.instance.SoloUI(false, null);
            }
        }
    }

    public void UIHook()
    {
        if (photonView.IsMine)
        {
            // ������ �� �� ����
            if (canHook == true)
            {
                UIManager.instance.throwUI.SetActive(false);       // ������ UI ��Ȱ��
                UIManager.instance.SoloUI(true, "�Ŵޱ�");         // �����̽��� + �Ŵޱ�
                //UIManager.instance.GageUI(true, true, "�Ŵޱ�");   // ������ UI
            }

            // ������ �� �� ����
            else
            {
                // �ִϸ��̼��� �����߱� �����̶��
                if (isanimation == true)
                {
                    //UIManager.instance.GageUI(true, true, "�Ŵޱ�");   // ������ UI Ȱ��ȭ
                    UIManager.instance.SoloUI(false, null);            // (�����̽��� + �Ŵޱ�) ��Ȱ��ȭ
                }
                // �׳� �־��� ��Ȳ�̶��
                else UIManager.instance.SoloUI(true, "�Ŵޱ�");        // �����̽��� + �Ŵޱ�
            }
        }
    }
    #endregion

    #region UI - ������ < - ����
    //// ������ UI
    //public void UIGenerator()
    //{
    //    if (photonView.IsMine)
    //    {
    //        if (isDestroying == true)
    //        {

    //        }
    //        else
    //        {
    //            // �����⸦ �μ� �� ���� ��
    //            if (canDestroyGenerator == true)
    //            {
    //                UIManager.instance.throwUI.SetActive(false);                                // �߾� ������ UI ��Ȱ��

    //                if (currentAxeCount > 0) UIManager.instance.DuoUI(true, "�ջ�");        // ���� O : ������UI + �����̽��� * �ջ�
    //                else UIManager.instance.SoloUI(true, "�ջ�");                           // ���� X : �����̽��� + �ջ�        
    //            }

    //            // �����⸦ �μ� �� ���� ��
    //            else
    //            {
    //                if (currentAxeCount > 0)
    //                {
    //                    // �ִϸ��̼��� �����ϰ� ���� ������ ������ UI Ȱ��
    //                    if (isanimation != true) UIManager.instance.throwUI.SetActive(true);

    //                    UIManager.instance.DuoUI(false, null);
    //                }

    //                else UIManager.instance.SoloUI(false, null);
    //            }
    //        }
    //    }
    //}

    ////// ������ ������ UI
    ////public void GenGageUI()
    ////{
    ////    if (isDestroying == true) UIManager.instance.GageUI(true, false, "�ջ�");       // UI Ȱ��

    ////    else UIManager.instance.GageUI(false, false, null);                             // UI ��Ȱ��
    ////}
    #endregion
    #endregion

    #region ����
    public void Stunned()
    {
        SoundManager.instance.PlayHitSounds(5);

        OffCC();
        OffRotate();

        UIManager.instance.throwUI.SetActive(false);

        if (state == State.Carry)
        {
            // ĳ�� ���� �ִϸ��̼��� ����
            photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "CarryStunned");
        }
        else
        {
            // ���� �ִϸ��̼��� ����
            photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Stunned");
            OffSmallAxe();
        }
    }
    #endregion

    #region Lerp
    // ����
    IEnumerator LerptoHook(Vector3 animPos)
    {
        while (Vector3.Distance(transform.position, animPos) > 0.3f)
        {
            // Vector3 hookdir = hookSpot.position - transform.position;   // hookSpot ���� ���� ���⺤��
            transform.position = Vector3.Lerp(transform.position, animPos, 0.1f);
            // transform.forward = hookSpot.forward;

            if (Vector3.Distance(transform.position, animPos) <= 0.3f)
            {
                break;
            }
            yield return null;
        }

    }

    // ������ ���
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

    // ������

    // ����

    // ĳ���

    #endregion

    #region ���
    private void Idle()
    {
        if (photonView.IsMine)
        {
            cc.enabled = false;
            canRotate = false;

            // ������ �����ϰ� 5�� �ڿ� ���¸� Move �� �ٲ۴�.
            currentTime += Time.deltaTime;
            if (currentTime >= gameStartTime)
            {
                // UI Ȱ��ȭ
                UIManager.instance.PerksUI.SetActive(true);
                UIManager.instance.leftDownUI.SetActive(true);
                UIManager.instance.throwUI.SetActive(true);
                UIManager.instance.OffTitleUI();

                // Material
                MaterialManager materialManager = GetComponent<MaterialManager>();
                materialManager.ChangeMaterials();

                // ī�޶� �� �̵�, ȸ��
                cineCam.enabled = false;
                cc.enabled = true;
                canRotate = true;

                // �ȳ� ���� ��ȯ �� �ִϸ��̼�
                AnnaState = State.Move;
                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Move", true);

                // ����ð� �ʱ�ȭ
                currentTime = 0;
            }
        }
    }
    #endregion

    #region �̵� �� ����
    bool playingthrowsound;

    private void UpdateMove()
    {
        if (photonView.IsMine == true)
        {
            #region  �Ϲ� ����
            // ���콺 ���� ��ư�� ������ �Ϲݰ����� �Ѵ�.
            if (Input.GetButtonDown("Fire1") && isCharging == false && isThrowing == false && isCanceled == false && state != State.CoolTime)
            {
                SoundManager.instance.PlayHitSounds(0);                             // ������ �ԼҸ�

                AnnaState = State.NormalAttack;                                     // ���¸� NormalAttack �� �ٲ�

                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Attack");     // �Ϲݰ��� �ִϸ��̼� ����

                isNormalAttack = true;                                              // ������ ���� ������ ���� ��ư �������� �ϱ�

                UIManager.instance.throwUI.SetActive(false);
            }
            #endregion

            #region ������ ���� ����
            // ���콺 ������ ��ư�� ������ �Ѽյ����� ��¡�ϱ� �����Ѵ�.
            if (state != State.NormalAttack && isNormalAttack == false && isCanceled == false && currentAxeCount > 0 && Input.GetButton("Fire2"))
            {
                Charging();                                                             // ��¡ �Լ��� ����

                isCharging = true;                                                      // ��¡ �߿� ���콺 ���� ��ư�� ������ �Ϲݰ����� ���ϵ���

                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", true);    // ���� ��� �ִϸ��̼�  ����

                UIManager.instance.throwUI.SetActive(false);
            }

            // ���콺 ������ ��ư�� ���� ������ ������.
            if (isCanceled == false && currentAxeCount != 0 && Input.GetButtonUp("Fire2"))
            {
                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Throw");              // ������ ������ �ִϸ��̼� ����
                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", false);       // ���� ��� �ִϸ��̼� ���

                currentChargingTime = 0;                                                    // ���� ��¡ �ð��� �ʱ�ȭ
            }

            // ��¡ �߿� ���콺 ���� ��ư�� ������ ������ ����Ѵ�.
            if (isCharging == true && isNormalAttack == false && isThrowing == false && Input.GetButtonDown("Fire1"))
            {
                isCanceled = true;                                                      // ĵ������

                isCharging = false;                                                     // ��¡ ���� �ƴ�

                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", false);   // ��¡ �ִϸ��̼� ���

                currentChargingTime = 0;                                                // ���� ��¡ �ð��� �ʱ�ȭ

                SoundManager.instance.PlaySmallAxeSounds(2);                            // ����ϴ� ���� ���

                playingchargingsound = false;                                           // ��¡�ϴ� ���� ��� �ȵ�

                playingfullchargingsound = false;

                UIManager.instance.throwUI.SetActive(true);
            }
            #endregion

            #region ������ ���
            // ���� �����Ÿ� �ȿ� �����ڰ� �������ִٸ�
            if (canCarry && state != State.Carry)
            {
                if (photonView.IsMine)
                {
                    // ���ø��� UI �� ȭ�鿡 ���� �� �����̽��ٸ� ������
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        canCarry = false;                                                   // �� �� �ִ� ���°� �ƴ�
                        UICarry();
                        isanimation = true;

                        StartCoroutine("LerptoSurvivor");                                   // �ִϸ��̼� ���� ��ҷ� �̵��ϴ� �ڷ�ƾ �����Ѵ�.

                        photonView.RPC(nameof(SurvivorCarry), RpcTarget.All);               // �������� ���¸� �ٲ۴�.

                        photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Pickup");     // �����ڸ� ���ø���.

                        AnnaState = State.Carry;                                            // ���¸� Carry �� �ٲ۴�.

                        survivor.transform.forward = transform.forward;                     // �������� �Ӹ��� �� �������� ���ϰ� �Ѵ�.
                    }
                }
            }
            #endregion
        }
    }
    #endregion

    #region �Ϲݰ���
    private void NormalAttack()
    {
        if (photonView.IsMine)
        {
            UIManager.instance.throwUI.SetActive(false);
            //anim.SetLayerWeight(3, 0f);
            // ������ ������ ���¸� Move �� �ٲ۴�. -> �ִϸ��̼� �̺�Ʈ
        }
    }
    #endregion

    #region �Ѽ� ���� ��¡ / ������ / ��Ÿ�� 
    bool playingchargingsound;
    bool playingfullchargingsound;
    void Charging()
    {
        if (photonView.IsMine)
        {
            if (playingchargingsound == false)
            {
                // ��¡ ���� ���
                SoundManager.instance.PlaySmallAxeSounds(0);
                playingchargingsound = true;
            }

            #region �ð��� ���� �����Ǵ� ����ü ������ ��
            // �ð��� ������Ų��.
            currentChargingTime += Time.deltaTime;

            // �ð��� �����ʿ� ���� chargingForce �� ������Ų��.
            chargingForce = Mathf.Lerp(minAxePower, maxAxePower, (currentChargingTime - 1.25f) / 1.75f);
            // print("aaa : " + chargingForce);
            // 1.25�� �������� ������ ���� ���� �ʴ´�.
            // if (currentChargingTime < minimumChargingTime) return;

            // �ִ� ���� �ð����� �����ð��� �������
            if (currentChargingTime > maximumChargingTime && playingfullchargingsound == false)
            {
                playingfullchargingsound = true;

                // �ִ� ��¡ ���带 ���
                SoundManager.instance.PlaySmallAxeSounds(1);

                //  �ִ������� �����Ѵ�.
                chargingForce = maxAxePower;
            }
            #endregion
        }
    }

    // ���� ����
    private void ThrowingAttack()
    {
        if (photonView.IsMine)
        {
            if (isThrowing == true) return;                                 // ���� isThrowing ��� ����

            isThrowing = true;
            isCharging = false;                                             // ��¡ ���� �ƴ�
            playingchargingsound = false;                                   // ��¡�ϴ� ���� ����� �� �ִ� ����
            playingfullchargingsound = false;

            Vector3 pos = throwingSpot.position;                                // ���� ��ġ
            Vector3 forward = Camera.main.transform.forward;                    // ���� �չ���

            photonView.RPC(nameof(ThrowAxeRPC), RpcTarget.All, pos, forward, chargingForce);    // ���� ������ �ִϸ��̼� ����

            currentAxeCount--;                                                                  // ���� ������ ���δ�

            UIManager.instance.axeCount.text = Convert.ToString(currentAxeCount);               // UI �����Ѵ�
            AnnaState = State.CoolTime;                                                         // ���¸� CoolTime ���� �ٲ۴�
        }
    }

    // 2�� ��Ÿ��
    void CoolTime()
    {
        if (photonView.IsMine)
        {
            isThrowing = true;

            playingchargingsound = false;       // ��¡�ϴ� ���� ����� �� �ִ� ����
            playingfullchargingsound = false;

            currentTime += Time.deltaTime;      // ���� �ð��� �帣�� �Ѵ�
            if (currentTime >= 2)               // ���� �ð��� 2�ʰ� �Ǹ�
            {
                AnnaState = State.Move;         // ���¸� Move �� �ٲ۴�

                isCharging = false;             // ��¡ ����
                isThrowing = false;             // ������ ����
                isCanceled = false;

                if (currentAxeCount != 0)       // ���� ���� ������ 0�� �ƴ϶��
                {
                    UIManager.instance.throwUI.SetActive(true);    // ���� ���� UI ���̰� �ϱ� ( ���� 0 �̶�� ��� �Ⱥ��̰���)
                }

                currentTime = 0;                // ���� �ð��� �ʱ�ȭ
            }
        }
    }
    #endregion

    #region ������ ��� / �� ���¿��� ����
    // �̵�
    private void UpdateCarry()
    {
        if (photonView.IsMine)
        {
            UIManager.instance.throwUI.SetActive(false);

            #region ��������
            //if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook == false)
            //{
            //    // anim.SetTrigger("Drop");
            //    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Drop");
            //    cc.enabled = false;
            //    survivor.transform.parent = null;
            //    survivor.GetComponent<SurviverHealth>().State = SurviverHealth.HealthState.Down;
            //}
            #endregion

            #region �� ���¿��� ����
            if (Input.GetButtonDown("Fire1"))
            {
                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "CarryAttack");
                AnnaState = State.CarryAttack;
            }
            #endregion
        }
        else
        {
            // ������ ���� ���θ� �ٸ� ����
            // h �� v �Է°��� ������
            if (h > 0 || v > 0)
            {
                // ��� �ִϸ����� ���̾� ���� ����
                anim.SetLayerWeight(1, 0.5f);
            }
            // �������� ���� ��
            else
            {
                anim.SetLayerWeight(1, 0);
            }
        }
    }

    // ����
    public void CarryAttack()
    {
        if (photonView.IsMine)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= 2.5f)
            {
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

    public void Throwing()      // State �� ThrowingAttack �� �ٲٴ� �Լ�
    {
        if (photonView.IsMine == false) return;
        AnnaState = State.ThrowingAttack;
    }

    public void OnCoolTime()    // State �� CoolTime ���� �ٲٴ� �Լ�
    {
        if (photonView.IsMine == false) return;
        AnnaState = State.CoolTime;
    }

    public void OnSmallAxe()    // ���� Ȱ��ȭ
    {
        // smallAxe.SetActive(true);
        photonView.RPC(nameof(AxeRPC), RpcTarget.All, true);
    }

    public void OffSmallAxe()   // ���� ��Ȱ��ȭ
    {
        // smallAxe.SetActive(false);
        photonView.RPC(nameof(AxeRPC), RpcTarget.All, false);
    }

    public void EndReload()     // ���� ������ ���� �� ���� ��, UI �����ϴ� �Լ�
    {
        if (photonView.IsMine)
        {
            currentAxeCount = maxAxeCount;                                              // ���� �ִ� ���� ������ �ִ�� ä���

            UIManager.instance.axeCount.text = Convert.ToString(currentAxeCount);       // UI �����Ѵ�
        }
    }

    public void EndAnimation()  // �ִϸ��̼� ���� ��
    {
        print("dfgljasdrhlfkjsadhfljash");
        isanimation = false;
    }

    // �ʱ�ȭ �Լ�
    void OnMyReset()
    {
        if (photonView.IsMine)
        {
            AnnaState = State.Move;

            OnCC();
            OffAxe();

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

            if (currentAxeCount > 0 && state != State.Carry)
            {
                UIManager.instance.throwUI.SetActive(true);
            }
        }
    }
    #endregion

    #region PunRPC
    [PunRPC]    // Closet �ִϸ����� trigger
    void SetClosetTriggerRPC(string parameter)
    {
        closetAnim.SetTrigger(parameter);
    }

    [PunRPC]    // Anna �ִϸ����� trigger
    void SetTriggerRPC(string parameter)
    {
        anim.SetTrigger(parameter);
    }

    [PunRPC]    // Anna �ִϸ����� Bool
    void SetBoolRPC(string parameter, bool boolean)
    {
        anim.SetBool(parameter, boolean);
    }

    [PunRPC]    // ���� ������
    void ThrowAxeRPC(Vector3 throwPos, Vector3 throwForward, float force)
    {
        GameObject axe = Instantiate(smallAxeFactory);          // ���� ����
        axe.transform.position = throwPos;                      // ���� ������ ��ġ
        axe.transform.forward = throwForward;                   // ���� �� ����
        axe.GetComponent<Axe>().flying(force);                  // ���� ������ ��
        axe.GetComponent<Axe>().photonView2 = this.photonView;
    }

    public GameObject bloodEffectFactory;

    [PunRPC]
    void SmallAxeBlood(Vector3 point, Vector3 normal)
    {
        GameObject smallAxeBlood = Instantiate(bloodEffectFactory);
        smallAxeBlood.transform.position = point;
        smallAxeBlood.transform.forward = normal;
    }

    [PunRPC]    // ���� �޽������� Ȱ������
    public void AxeRPC(Boolean axeBool)
    {
        smallAxe.SetActive(axeBool);
    }

    [PunRPC]    // ������ ���
    void SurvivorCarry()
    {
        survivor.GetComponent<SurviverHealth>().ChangeCarring();        // �������� ���¸� �ٲٴ� �Լ��� ȣ�� 

        survivor.transform.parent = leftArm.transform;                 // �����ڸ� ���� �ڽ� ������Ʈ�� �����Ѵ�.


        //survivor.transform.localPosition = new Vector3(-0.331999868f, 0.976001263f, -0.19100064f);
        survivor.transform.localPosition = new Vector3(-0.0379999988f, 0.976001143f, 0.202000007f);
        //survivor.transform.localRotation = new Quaternion(2.60770321e-08f, -0.1253708f, 2.09547579e-09f, 0.992109954f);
        survivor.transform.localRotation = new Quaternion(0, -0.999083936f, 0, -0.0427953899f);
    }

    [PunRPC]    // ���� �ɱ�
    void Hook()
    {
        survivor.GetComponent<SurviverHealth>().ChangeCarring();

        survivor.transform.parent = null;   // �����ڿ��� �θ��ڽ� ���踦 ���´�.
    }
    #endregion

    #region OnPhotonSerializeView
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // ���� Anna ���
        if (stream.IsWriting)
        {
            // ���� ��ġ ���� ������.
            stream.SendNext(transform.position);

            // Y ȸ�� ���� ������.
            stream.SendNext(new Vector3(0, transform.eulerAngles.y, 0));

            // h ���� ������
            stream.SendNext(h);

            // v ���� ������
            stream.SendNext(v);
        }

        // ���� Anna �� �ƴ϶��
        else
        {
            // ���� ��ġ ���� �޴´�.
            receivePos = (Vector3)stream.ReceiveNext();

            // Y ȸ�� ���� �޴´�.
            receiveRot = Quaternion.Euler((Vector3)stream.ReceiveNext());

            // h ���� �޴´�
            h = (float)stream.ReceiveNext();

            // v ���� �޴´�
            v = (float)stream.ReceiveNext();
        }
    }
    #endregion
}