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
    #endregion

    #region ����
    // ĳ���� ���� ����
    CharacterController cc;                     // ĳ���� ��Ʈ�ѷ�
    Animator anim;                              // ���θ� �ִϸ�����
    public AnimatorOverrideController animOC;   // ���θ� �ִϸ����� (��Ʈ��ũ)
    public Camera cineCam;                      // �ó׸ӽ� ī�޶�
    public Transform playCamera;                // �÷��� ī�޶�
    public Transform foward;


    // �̵� �ӵ�
    float currentSpeed;                     // ���� �̵��ӵ�
    float normalSpeed = 4.4f;               // �⺻ �̵��ӵ�
    float chargingSpeed = 3.08f;            // ��¡ �� �̵��ӵ�
    float delaySpeed = 3.74f;               // ���� �� �̵��ӵ�

    // �߷�
    public float yVelocity;                 // y �ӵ�
    public float gravity = -9.8f;           // �߷¼ӵ�


    // ȸ��
    float rotX;                             // X ȸ����
    float rotY;                             // Y ȸ����
    public float rotSpeed = 100;            // ȸ���ӵ�
    public Transform cam;                   // ī�޶� Transform
    public Transform cameraPos;             // ī�޶� ��ġ�� ��

    // �ð�
    public float gameStartTime;             // ���ӽ��۱��� �ɸ��� �ð�
    float currentTime;                      // ���� �ð�
    float currentChargingTime;              // ���� ��¡ �ð�
    // float minimumChargingTime = 1.25f;   // �ּ� ��¡ �ð�
    float maximumChargingTime = 3;          // �ִ� ��¡ �ð�
    // float axeRechargingTime = 4;         // ���� ���� �ð�

    // ī��Ʈ
    float maxAxeCount = 5;                  // �ִ� ���� ������ �Ѽյ��� ����
    float currentAxeCount;                  // ���� ������ �ִ� �Ѽյ��� ����

    float maxGenCount = 5;                  // �ִ� ���� ������ ����
    float currentGenCount;                  // ���� ���� ������ ����

    // �μ� ����
    public BoxCollider bigAxeCollider;      // �μյ��� �ݶ��̴�

    // �Ѽ� ����
    public GameObject smallAxe;             // �Ѽյ��� GameObject
    public GameObject smallAxeFactory;      // ��ô�� �Ѽյ��� ������
    public Transform throwingSpot;          // �Ѽյ��� ���ư� ��ġ
    public float chargingForce;             // �Ѽյ��� ������ ��
    public float minAxePower = 20;          // �Ѽյ��� ������ �� �ּҰ�
    public float maxAxePower = 40;          // �Ѽյ��� ������ �� �ִ밪

    // bool
    public bool isNormalAttack;             // �Ϲݰ��� ���ΰ�?
    bool isCharging;                        // ��¡ ���ΰ�?
    bool isCanceled;                        // ��¡�� ����ߴ°�?
    bool isThrowing;                        // �յ����� �����°�?
    bool isStunned;                         // ���� ���ߴ°�?

    public bool canCarry;                   // �����ڸ� �� �� �ִ°�?
    public bool canDestroyGenerator;        // �����⸦ �μ� �� �ִ°�?
    public bool canHook;                    // ������ �� �� �ִ°�?
    public bool canReLoad;                  // �� �� ������ ������ �� �� �ִ°�?
    public bool canDestroyPallet;           // ������ ���ڸ� �μ� �� �ִ°�?
    public bool canRotate;

    // �⺻ UI
    GameObject goGenCount;                  // ������ ���� UI GameObject
    TextMeshProUGUI generatorCount;         // ������ ���� UI

    GameObject goAxeCount;                  // ���� ���� UI GameObject
    TextMeshProUGUI axeCount;               // ���� ���� UI

    // ��ȣ�ۿ� UI
    GameObject throwUI;                     // ���� ������ UI



    // ��Ÿ
    public GameObject survivor;             // ������ ���ӿ�����Ʈ
    public Transform leftArm;               // ����
    public Light redlight;                  // ���θ� �տ� �ִ� ����

    // �ִϸ��̼� ���� ��ġ
    public Transform hookSpot;              // ����



    // Photon
    Vector3 receivePos;                     // ���޹��� ������ ��
    Quaternion receiveRot;                  // ���޹��� �����̼� ��
    float lerpSpeed = 50;                   // ���� �ӷ�
    float h;                                // �¿� �Է°�
    float v;                                // �յ� �Է°�
    #endregion

    public bool HaxMode = false;

    public GameObject go;

    public Vector3 cameraOffset;

    #region Start
    void Start()
    {
        if (HaxMode) maxAxeCount = 999;

        // ����
        anim = GetComponent<Animator>();    // �ȳ� Animator ������Ʈ �����´�.
        smallAxe.SetActive(false);          // �޼տ� ��� �ִ� �Ѽյ��� ������ ��Ȱ��ȭ�Ѵ�.
        bigAxeCollider.enabled = false;     // ��յ��� �ݶ��̴��� ��Ȱ��ȭ �Ѵ�.

        // ���� ���� ���θ��� ���
        if (photonView.IsMine == true)
        {
            cc = GetComponent<CharacterController>();   // �ȳ� Character Controller ������Ʈ �����´�.
            anim.SetLayerWeight(1, 0);                  // ������ �ִϸ��̼� ���̾ �����Ѵ�.
            playCamera.gameObject.SetActive(true);      // �ȳ� MainCamera Ȱ��ȭ
            redlight.enabled = false;                   // �ȳ� redlight �����´�.
            cineCam.depth = 5;                          // �ó׸ӽ� ī�޶� ���̰� �Ѵ�.

            goGenCount = GameObject.Find("TextGenerator");                  // TextGenerator GameObject ã�´�.
            generatorCount = goGenCount.GetComponent<TextMeshProUGUI>();    // ã�� ���ӿ�����Ʈ�� TextMeshProUGUI ������Ʈ�� �ҷ��´�.   
            currentGenCount = maxGenCount;                                  // ������ �� ���ư��� �� ������ ������ �ִ�(5��)�� �����Ѵ�.
            generatorCount.text = Convert.ToString(currentGenCount);        // UI �� ��Ÿ����.

            goAxeCount = GameObject.Find("TextAxe");                    // TextAxe GameObject  �� ã�´�.
            axeCount = goAxeCount.GetComponent<TextMeshProUGUI>();      // ã�� ���ӿ�����Ʈ�� TextMeshProUGUI ������Ʈ�� �ҷ��´�.
            currentAxeCount = maxAxeCount;                              // ������ �� ���� ������ �ִ�(5��)�� �����Ѵ�.
            axeCount.text = Convert.ToString(currentAxeCount);          // UI �� ��Ÿ����.

            throwUI = GameObject.Find("ThrowUI");                       // ���� ������ UI ã�´�.
            throwUI.SetActive(false);                                   // ���� ������ UI �� ��Ȱ��ȭ �Ѵ�.
        }
        // ���� ���� ���θ��� �ƴ϶��
        else
        {
            playCamera.gameObject.SetActive(false);     // �÷��̿� ī�޶� ����

            cineCam.gameObject.SetActive(false);        // �ó׸ӽ� ī�޶� ����

            anim.runtimeAnimatorController = animOC;    // �ִϸ��̼� ��Ʈ�ѷ� �������̵� ����

            anim.SetLayerWeight(1, 0);                  // ����� �� �ִϸ��̼� ���̾ �����Ѵ�.

            redlight.enabled = true;                    // ���θ� ���� Ȱ��ȭ
        }
    }
    #endregion

    private void LateUpdate()
    {
        go.transform.localPosition = cameraOffset;
        //Vector3 rot = cam.transform.localEulerAngles;
        //rot.z = rotY;
        //cam.transform.localEulerAngles = rot; // new Vector3(rotY, 0.022f, -2.476f);
        //rotY = Mathf.Clamp(rotY, -35, 35);

        //Camera.main.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, 0);
    }

    [PunRPC]
    void Hook()
    {
        survivor.GetComponent<SurviverHealth>().ChangeCarring();

        survivor.transform.parent = null;   // �����ڿ��� �θ��ڽ� ���踦 ���´�.
    }

    #region Update
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

        if (photonView.IsMine == true)
        {
            if (canRotate == true)
            {
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
                transform.eulerAngles = new Vector3(0, rotX, 0);                        // Horizontal
                cam.transform.eulerAngles = new Vector3(-rotY, rotX, 0);              // Vertical
                // Camera.main.transform.rotation = Quaternion.Euler(-rotY, rotX, 0);
                // cam.transform.localEulerAngles = new Vector3(rotY, -90, -180);
                // Y ȸ���� 35���� ����

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

            // ���� ��¡ ���̶��
            if (isCharging == true)
            {
                // �����ӵ�(3.08)�� �̵�
                currentSpeed = chargingSpeed;
            }
            // ���� ������ ����
            else if (state == State.CoolTime)
            {
                currentSpeed = delaySpeed;
            }
            // �Ϲ� ���� �� ��
            else if (state == State.NormalAttack)
            {
                currentSpeed = 6;
            }
            else if (isCharging == false || state != State.CoolTime || isThrowing == false)
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

            #region ������ �μ���
            if (canDestroyGenerator == true)
            {
                // �����̽� �ٸ� ��� ������ ������ �����⸦ �μ���.
                if (Input.GetKey(KeyCode.Space))
                {
                    cc.enabled = false;                 // ����!
                                                        // anim.SetBool("DestroyG", true);     // �ִϸ��̼� true
                    photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "DestroyG", true);

                    //  UI
                    //  UI
                    // ������ ä���
                }
                // �߰��� �����̽� �ٿ��� ���� ���� ��ҵȴ�.
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    cc.enabled = true;                  // ������!
                                                        // anim.SetBool("DestroyG", false);    // �ִϸ��̼� false
                                                        // anim.SetTrigger("DestroyCancel");   // ��� �ִϸ��̼�
                    photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "DestroyG", false);
                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "DestroyCancel");

                    //  UI
                    //  UI
                    // ������ �ʱ�ȭ
                }
            }
            #endregion

            #region ���� �μ���
            if (canDestroyPallet == true)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    cc.enabled = false;             // ������ ����
                                                    //anim.SetTrigger("DestroyP");    // ���ڸ� �μ��� �ִϸ��̼� ����
                    photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "DestroyP");
                    // ���ڰ� �μ����� �ִϸ��̼� ����
                }
            }
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


        #region ��ȣ�ۿ� �ִϸ��̼�(�����ϴ� ��ġ�� �̵�)
        #region ���� ������
        // ������ ������ 5���� �۰� ������ ������ ������ ��, �����̽� �ٸ� ������
        if (currentAxeCount < maxAxeCount && canReLoad && Input.GetKeyDown(KeyCode.Space))
        {
            // �ִϸ��̼� ���� ��ҷ� �̵��Ѵ�.

            OffCC();                                                        // ������ ����

            // anim.SetTrigger("Reload");                                      // ������ ������ �ִϸ��̼� ����
            photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Reload");
            // ĳ��� ������ ������ �ִϸ��̼� ����

            currentAxeCount = maxAxeCount;                                  // ���� �ִ� ���� ������ �ִ�� ä���

            axeCount.text = Convert.ToString(currentAxeCount);              // UI �����Ѵ�
        }
        #endregion

        #region ĳ��� Ȯ��
        // ĳ��� �տ��� �����̽��ٸ� ������ ĳ����� ����
        // ���� �ȿ� ����� ���� ���� ���� ������ �ִ���
        // �׳� �����ٰ� �ݴ´�
        // ���� �ȿ� ����� �ִٸ�
        // �� ����� ��������
        // ���¸� Carry �� �ٲ۴�
        #endregion

        #region ���� �ɱ�
        if (state == State.Carry && canHook)
        {
            // �����̽��ٸ� ������ 
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine("LerptoHook");

                cc.enabled = false;                 // �������� �����.

                // anim.SetTrigger("Hook");            // �����ڸ� ������ �Ŵ� �ִϸ��̼� �����Ѵ�.


                photonView.RPC(nameof(Hook), RpcTarget.All);
                // ������ �Ŵ� UI �������� ����.

                // �� ���� UI ����.

               
            }
        }
        #endregion
        #endregion

        #region OverlapSphere & UI
        canHook = false;
        canReLoad = false;
        canDestroyPallet = false;
        canDestroyGenerator = false;
        canCarry = false;
        Collider[] hitcolliders = Physics.OverlapSphere(transform.position, 2);
        // for �� ������
        for (int i = 0; i < hitcolliders.Length; i++)
        {
            print(hitcolliders[i].transform.gameObject.name);
            // Survivor ������
            //if (hitcolliders[i].transform.gameObject.name.Contains("Survivor"))
            if (hitcolliders[i].transform.gameObject.name.Contains("Survivor"))
            {
                // ���°� Healthy �� �����ڰ� 1�� �̻��̶�� chase BG�� ������ �Ѵ�.


                // ���°� Down �̶�� canCarry �� true �� �ٲ۴�.
                if (hitcolliders[i].GetComponent<SurviverHealth>().State == SurviverHealth.HealthState.Down)
                {
                    canCarry = true;
                    survivor = hitcolliders[i].gameObject;
                }
            }
            else
            {
                // ���࿡ �����ڰ� ������ Lullaby BG �� ������ �Ѵ�.


                // canCarry -> false

            }

            // Generator ������
            if (hitcolliders[i].transform.gameObject.name == "EnableCollider")
            {
                canDestroyGenerator = true;

                // ���� �������� ���� ���α׷����� 0���� ũ�ٸ�
                // if(������ ���� ���α׷��� > 0)
                // {
                // canDestroyGenerator �� true ��
                // }
            }

            // Pallet ����
            if (hitcolliders[i].transform.gameObject.name.Contains("Pallet"))
            {
                // ���� ������ ���°� '������' �̶��
                //if ()
                //{
                //    // canDestroyPallet �� true ��
                canDestroyPallet = true;
                //}
            }
            else { }

            // Closet ĳ���
            if (hitcolliders[i].transform.gameObject.name.Contains("Closet"))
            {
                // ���� ��� �ִ� ������ ������ �ִ밳�����
                if (currentAxeCount == maxAxeCount)
                {
                    // �׳� �� ���� -> [SPACE] ã�� UI

                    // ���� �����ڰ� ������ �ٷ� ��� / �ƴϸ� �� ���ݱ�

                }

                // ���� ��� �ִ� ������ ������ 1~4 �϶�
                else if (currentAxeCount > 0 && currentAxeCount < maxAxeCount)
                {
                    // canReload -> true & [SPACE] ã�� + �յ��� ��ô
                    canReLoad = true;
                }
            }

            // Hook ���� // ���� �� ���°� Carry���
            if (hitcolliders[i].transform.gameObject.name.Contains("Hook") && state == State.Carry)
            {
                // canHook -> true
                canHook = true;

                // ���� �ִϸ��̼� ���� ���  
            }
        }
        #endregion

        // ���� ���ϸ� ���� �Լ��� ȣ��
        if (Input.GetKeyDown(KeyCode.G))
        {
            // G ��ư = ���� �Լ��� �ҷ��ٰ� �����ϰ� 
            Stunned();
        }


    }
    #endregion

    #region ����
    private void Stunned()
    {
        SoundManager.instance.PlayHitSounds(4);

        cc.enabled = false;

        if (state == State.Carry)
        {
            // ĳ�� ���� �ִϸ��̼��� ����
            // anim.SetTrigger("CarryStunned");
            photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "CarryStunned");
        }
        else
        {
            // ���� �ִϸ��̼��� ����
            // anim.SetTrigger("Stunned");
            photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Stunned");
            OffSmallAxe();
        }
    }
    #endregion

    #region Lerp
    // ����
    IEnumerator LerptoHook()
    {
        while (Vector3.Distance(transform.position, hookSpot.position) > 0.3f)
        {
            // Vector3 hookdir = hookSpot.position - transform.position;   // hookSpot ���� ���� ���⺤��
            transform.position = Vector3.Lerp(transform.position, hookSpot.position, 0.1f);
            transform.forward = hookSpot.forward;

            if (Vector3.Distance(transform.position, hookSpot.position) <= 0.3f)
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
            //if (currentTime >= gameStartTime)
            if (currentTime >= gameStartTime)
            {
                throwUI.SetActive(true);
                cineCam.enabled = false;
                cc.enabled = true;
                canRotate = true;
                state = State.Move;
                //anim.SetBool("Move", true);
                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Move", true);
                currentTime = 0;
            }
        }
        //else
        //{
        //    state = State.Move;
        //}
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
            if (Input.GetButtonDown("Fire1") && isCharging == false)
            {
                SoundManager.instance.PlayHitSounds(0);         // ������ �ԼҸ�

                state = State.NormalAttack;                     // ���¸� NormalAttack �� �ٲ�

                // anim.SetTrigger("Attack");                      // �Ϲݰ��� �ִϸ��̼� ����

                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Attack");

                isNormalAttack = true;                          // ������ ���� ������ ���� ��ư �������� �ϱ�
            }
            #endregion

            #region ������ ���� ����
            // ���콺 ������ ��ư�� ������ �Ѽյ����� ��¡�ϱ� �����Ѵ�.
            if (Input.GetButton("Fire2") && state != State.NormalAttack && isCanceled == false && currentAxeCount != 0)
            {
                Charging();                         // ��¡ �Լ��� ����
                isCharging = true;                  // ��¡ �߿� ���콺 ���� ��ư�� ������ �Ϲݰ����� ���ϵ���
                // anim.SetBool("Throwing", true);     // �޼��� �� ä�� ���ƴٴ� �� �ִ�.
                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", true);
                throwUI.SetActive(false);
            }

            // ���콺 ������ ��ư�� ���� ������ ������.
            if (isCanceled == false && Input.GetButtonUp("Fire2") && currentAxeCount != 0)
            {
                // anim.SetTrigger("Throw");                       // �ִϸ��̼� ����
                // anim.SetBool("Throwing", false);                // ��¡ �ִϸ��̼� ���
                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Throw");
                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", false);
                currentChargingTime = 0;                        // ���� ��¡ �ð��� �ʱ�ȭ
            }

            // ��¡ �߿� ���콺 ���� ��ư�� ������ ������ ����Ѵ�.
            if (Input.GetButtonDown("Fire1") && isCharging == true)
            {
                isCanceled = true;                              // ĵ������
                isCharging = false;                             // ��¡ ���� �ƴ�
                //anim.SetBool("Throwing", false);                // ��¡ �ִϸ��̼� ���
                photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", false);
                currentChargingTime = 0;                        // ���� ��¡ �ð��� �ʱ�ȭ
                SoundManager.instance.PlaySmallAxeSounds(2);    // ����ϴ� ���� ���
                playingchargingsound = false;                   // ��¡�ϴ� ���� ��� �ȵ�
                playingfullchargingsound = false;
                throwUI.SetActive(true);

                Invoke("DoCancel", 0.4f);                       // 0.4�� �Ŀ� ���� �Ⱥ��̰� ��
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
                        photonView.RPC(nameof(SurvivorCarry), RpcTarget.All);

                        photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Pickup"); // �����ڸ� ���ø���.

                        state = State.Carry;                                            // ���¸� Carry �� �ٲ۴�.

                        canCarry = false;                                               // �� �� �ִ� ���°� �ƴ�

                        transform.forward = survivor.transform.forward;                 // �� �չ����� �������� �չ������� �����Ѵ�.

                        StartCoroutine("LerptoSurvivor");                               // �ִϸ��̼� ���� ��ҷ� �̵��ϴ� �ڷ�ƾ �����Ѵ�.

                        // survivor.transform.localPosition = new Vector3(-0.350566328f, 1.01032352f, 0.0430793613f);
                        // survivor.transform.localPosition = new Vector3(-0.367999434f, 1.02999997f, -0.191000223f);
                        
                        // survivor.transform.localPosition = new Vector3(-0.4f,0.961f,-0.125f);
                        // survivor.transform.localPosition = new Vector3(0,0,0);
                        // survivor.transform.localRotation = new Quaternion(0,0,0,0);
                    }
                }
            }
            #endregion
        }
    }
    #endregion

    [PunRPC]
    void SurvivorCarry()
    {
        survivor.GetComponent<SurviverHealth>().ChangeCarring();        // �������� ���¸� �ٲٴ� �Լ��� ȣ�� 

        survivor.transform.parent = leftArm.transform;                 // �����ڸ� ���� �ڽ� ������Ʈ�� �����Ѵ�.

        survivor.transform.localPosition = new Vector3(-0.331999868f, 0.976001263f, -0.19100064f);
        survivor.transform.localRotation = new Quaternion(2.60770321e-08f, -0.1253708f, 2.09547579e-09f, 0.992109954f);
    }

    #region �Ϲݰ���
    private void NormalAttack()
    {
        if (photonView.IsMine)
        {
            throwUI.SetActive(false);
            //anim.SetLayerWeight(3, 0f);
            // ������ ������ ���¸� Move �� �ٲ۴�. -> �ִϸ��̼� �̺�Ʈ
        }
    }
    #endregion

    #region �Ѽ� ���� ��¡ / ��� / ���� / ��Ÿ��
    bool playingchargingsound;
    bool playingfullchargingsound;
    // ���� ��¡
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

    // ���� ��¡ ĵ��
    void DoCancel()
    {
        isCanceled = false;
        OffSmallAxe();
        throwUI.SetActive(true);
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

            //GameObject smallaxe = Instantiate(smallAxeFactory);             // �Ѽյ����� �����.
            //smallaxe.transform.position = throwingSpot.position;            // ���� �Ѽյ����� ��ġ�� �޼տ� ��ġ�Ѵ�.
            //smallaxe.transform.forward = Camera.main.transform.forward;     // ���� �Ѽյ����� �չ����� ī�޶��� �չ������� �Ѵ�

            Vector3 pos = throwingSpot.position;
            Vector3 forward = Camera.main.transform.forward;

            photonView.RPC(nameof(ThrowAxeRPC), RpcTarget.All, pos, forward, chargingForce);

            currentAxeCount--;                                              // ���� ������ ���δ�
            axeCount.text = Convert.ToString(currentAxeCount);              // UI �����Ѵ�
            print(currentAxeCount);

            state = State.CoolTime;                                         // ���¸� CoolTime ���� �ٲ۴�
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
                state = State.Move;             // ���¸� Move �� �ٲ۴�
                isCharging = false;             // ��¡ ����
                isThrowing = false;             // ������ ����
                if (currentAxeCount != 0)       // ���� ���� ������ 0�� �ƴ϶��
                {
                    throwUI.SetActive(true);    // ���� ���� UI ���̰� �ϱ� ( ���� 0 �̶�� ��� �Ⱥ��̰���)
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
            throwUI.SetActive(false);

            #region ��������
            if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook == false)
            {
                // anim.SetTrigger("Drop");
                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "Drop");
                cc.enabled = false;
                survivor.transform.parent = null;
                survivor.GetComponent<SurviverHealth>().State = SurviverHealth.HealthState.Down;
            }
            #endregion

            #region �� ���¿��� ����
            if (Input.GetButtonDown("Fire1"))
            {
                // anim.SetTrigger("CarryAttack");
                photonView.RPC(nameof(SetTriggerRPC), RpcTarget.All, "CarryAttack");
                state = State.CarryAttack;
            }
            #endregion
        }
        else
        {
            // ��� �ִϸ����� ���̾� ���� ����
            anim.SetLayerWeight(1, 0.5f);
        }
    }

    // ����
    public void CarryAttack()
    {
        if (photonView.IsMine)
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
        if (photonView.IsMine == false) return;
        bigAxeCollider.enabled = true;
    }

    public void OffAxe()
    {
        if (photonView.IsMine == false) return;
        bigAxeCollider.enabled = false;
    }

    // �ʱ�ȭ �Լ�
    void OnMyReset()
    {
        if (photonView.IsMine)
        {
            state = State.Move;

            OnCC();
            OffAxe();

            // anim.SetBool("Throwing", false);
            // anim.SetBool("DestroyG", false);
            photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "Throwing", false);
            photonView.RPC(nameof(SetBoolRPC), RpcTarget.All, "DestroyG", false);


            isStunned = false;
            isCharging = false;
            isNormalAttack = false;
            playingthrowsound = false;
            canDestroyGenerator = false;
            canDestroyPallet = false;
            canReLoad = false;
            throwUI.SetActive(true);
        }
    }


    public void Throwing()      // State �� ThrowingAttack �� �ٲٴ� �Լ�
    {
        if (photonView.IsMine == false) return;
        state = State.ThrowingAttack;
    }

    public void OnCoolTime()    // State �� CoolTime ���� �ٲٴ� �Լ�
    {
        if (photonView.IsMine == false) return;
        state = State.CoolTime;
    }

    public void OnSmallAxe()    // ���� Ȱ��ȭ
    {
        smallAxe.SetActive(true);
    }

    public void OffSmallAxe()   // ���� ��Ȱ��ȭ
    {
        smallAxe.SetActive(false);
    }
    #endregion

    #region PunRPC
    [PunRPC]
    void SetTriggerRPC(string parameter)
    {
        anim.SetTrigger(parameter);
    }

    [PunRPC]
    void SetBoolRPC(string parameter, bool boolean)
    {
        anim.SetBool(parameter, boolean);
    }

    [PunRPC]
    void ThrowAxeRPC(Vector3 throwPos, Vector3 throwForward, float force)
    {
        GameObject axe = Instantiate(smallAxeFactory);
        axe.transform.position = throwPos;      // ���� ������ ��ġ
        axe.transform.forward = throwForward;   // ���� �� ����
        axe.GetComponent<Axe>().flying(force);  // ���� ������ ��
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