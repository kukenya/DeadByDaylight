using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class AnnaMove : MonoBehaviourPun, IPunObservable
{
    #region �̱��� �� ����
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

    #region ����
    Animator anim;                   // ���θ� �ִϸ�����
    // public Animator anim2;                  // ������ �� ���θ� �ִϸ�����
    CharacterController cc;                 // ĳ���� ��Ʈ�ѷ�
    public Camera cineCam;                  // �ó׸ӽ� ī�޶� 
    public Transform neck;

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

    // �ð�
    public float gameStartTime;             // ���ӽ��۱��� �ɸ��� �ð�
    float currentTime;                      // ���� �ð�
    float currentChargingTime;              // ���� ��¡ �ð�
    // float minimumChargingTime = 1.25f;      // �ּ� ��¡ �ð�
    float maximumChargingTime = 3;          // �ִ� ��¡ �ð�
    // float axeRechargingTime = 4;            // ���� ���� �ð�

    // ī��Ʈ
    float maxAxeCount = 5;                  // �ִ� ���� ������ �Ѽյ��� ����
    float currentAxeCount;                  // ���� ������ �ִ� �Ѽյ��� ����

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
    public bool isNormalAttack;                     // �Ϲݰ��� ���ΰ�?
    bool isCharging;                                // ��¡ ���ΰ�?
    bool isCanceled;                                // ��¡�� ����ߴ°�?
    bool isThrowing;                                // �յ����� �����°�?
    bool isStunned;                                 // ���� ���ߴ°�?
    public bool canCarry;                           // �����ڸ� �� �� �ִ°�?
    public bool canDestroyGenerator;                // �����⸦ �μ� �� �ִ°�?
    public bool canHook;                            // ������ �� �� �ִ°�?
    public bool canReLoad;                          // �� �� ������ ������ �� �� �ִ°�?
    public bool canDestroyPallet;                   // ������ ���ڸ� �μ� �� �ִ°�?
    public bool canRotate;

    // UI
    public GameObject goGeneratorCount;             // ������ ���� UI GameObject
    TextMeshProUGUI generatorCount;                 // ������ ���� UI

    public GameObject goAxeCount;                   // ���� ���� UI GameObject
    TextMeshProUGUI axeCount;                       // ���� ���� UI

    public GameObject throwUI;                      // ���� ������ UI


    // ��Ÿ
    public GameObject survivor;                 // ������ ���ӿ�����Ʈ
    public Transform leftArm;                   // ����
    public Light redlight;                      // ���θ� �տ� �ִ� ����

    // �ִϸ��̼� ���� ��ġ
    public Transform hookSpot;                  // ����

    float h;
    float v;

    #endregion

    #region Start & Update
    void Start()
    {
        if (photonView.IsMine == true)
        {
            redlight.enabled = false;
            anim = GetComponent<Animator>();                    // �ȳ� Animator ������Ʈ
                                                                //anim.runtimeAnimatorController =
            cc = GetComponent<CharacterController>();           // �ȳ� Rigidbody ������Ʈ

            anim.SetLayerWeight(1, 0);                          // ������ �ִϸ��̼� ���̾�
            anim.SetLayerWeight(2, 0);                          // ��� �ִϸ��̼� ���̾�
            anim.SetLayerWeight(3, 0);                          // �Ϲ� �ִϸ��̼� ���̾�

            goGeneratorCount = GameObject.Find("TextGenerator");    // TextGenerator GameObject ã�´�.
            TextMeshProUGUI generatorCount =                        // ã�� ���ӿ�����Ʈ�� TextMeshProUGUI ������Ʈ�� �ҷ��´�.
                goGeneratorCount.GetComponent<TextMeshProUGUI>();

            goAxeCount = GameObject.Find("TextAxe");            // TextAxe GameObject  �� ã�´�.
            TextMeshProUGUI axeCount =                          // ã�� ���ӿ�����Ʈ�� TextMeshProUGUI ������Ʈ�� �ҷ��´�.
                goAxeCount.GetComponent<TextMeshProUGUI>();

            currentAxeCount = maxAxeCount;                      // ������ �� ���� ������ �ִ�(5��)�� �����Ѵ�.

            axeCount.text = Convert.ToString(maxAxeCount);      // UI �� ��Ÿ����.

            throwUI = GameObject.Find("ThrowUI");               // ���� ������ UI ã�´�.
            throwUI.SetActive(false);

            smallAxe.SetActive(false);                          // �޼տ� ��� �ִ� �Ѽյ��� ������ ��Ȱ��ȭ

            bigAxeCollider.enabled = false;                     // ��յ��� �ݶ��̴��� �����´�.

            cineCam.depth = 5;                                  // �ó׸ӽ� ī�޶� ���̰� �Ѵ�.
        }
    }

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
            #region ȸ��
            if (state != State.Idle)
            {
                // ȸ������ �޾ƿ´�.
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");

                // ȸ������ ����
                rotX += mx * rotSpeed * Time.deltaTime;
                rotY += my * rotSpeed * Time.deltaTime;

                // ȸ������ ����
                transform.eulerAngles = new Vector3(0, rotX, 0);        // Horizontal

                if (cc.enabled == true)
                {
                    transform.eulerAngles = new Vector3(-rotY, rotX, 0);    // Vertical

                    rotY = Mathf.Clamp(rotY, -35, 35);
                }
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
                // �� �ܴ̿� �Ϲݼӵ�(4.4)�� �̵�
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

            // �̵��Ѵ�
            Vector3 velocity = dir * currentSpeed;
            velocity.y = yVelocity;
            cc.Move(velocity * Time.deltaTime);
            //}

            // ���� Anna �� �ƴ� ��
            //else
            //{
            // ��ġ ����
            // ȸ�� ����
            // Animator2 �� Parameter ���� ����
            // anim2.SetFloat("h", h);
            // anim2.SetFloat("v", v);
            //}
            #endregion

            // ��ȣ�ۿ��� �� ���� ��ȣ�ۿ� �ִϸ��̼��� �����ϴ� ��ġ�� �̵��� �ؾ��Ѵ�.

            #region ���� ������
            // ������ ������ 5���� �۰� ������ ������ ������ ��, �����̽� �ٸ� ������
            if (currentAxeCount < maxAxeCount && canReLoad && Input.GetKeyDown(KeyCode.Space))
            {
                // �ִϸ��̼� ���� ��ҷ� �̵��Ѵ�.

                OffCC();                                                        // ������ ����

                anim.SetTrigger("Reload");                                      // ������ ������ �ִϸ��̼� ����

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

            #region ������ �μ���
            if (canDestroyGenerator == true)
            {
                // �����̽� �ٸ� ��� ������ ������ �����⸦ �μ���.
                if (Input.GetKey(KeyCode.Space))
                {
                    cc.enabled = false;
                    anim.SetBool("DestroyG", true);
                }
                // �߰��� �����̽� �ٿ��� ���� ���� ��ҵȴ�.
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    anim.SetTrigger("DestroyCancel");
                    anim.SetBool("DestroyG", false);
                    cc.enabled = true;
                }
            }
            #endregion

            #region ���� �μ���
            if (canDestroyPallet == true)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    cc.enabled = false;             // ������ ����
                    anim.SetTrigger("DestroyP");    // ���ڸ� �μ��� �ִϸ��̼� ����
                                                    // ���ڰ� �μ����� �ִϸ��̼� ����
                }
            }
            #endregion

            #region ���� �ɱ�
            if (state == State.Carry && canHook)
            {
                // �����̽��ٸ� ������ 
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine("LerptoHook");

                    cc.enabled = false;                 // �������� �����.

                    anim.SetTrigger("Hook");            // �����ڸ� ������ �Ŵ� �ִϸ��̼� �����Ѵ�.

                    // ������ �Ŵ� UI �������� ����.

                    // �� ���� UI ����.

                    survivor.transform.parent = null;   // �����ڿ��� �θ��ڽ� ���踦 ���´�.
                }
            }
            #endregion

            // UI �����״� �ϱ�
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
                // Surviver ������
                if (hitcolliders[i].transform.gameObject.name.Contains("Surviver"))
                {
                    // ���°� Healthy �� �����ڰ� 1�� �̻��̶�� chase BG�� ������ �Ѵ�.


                    // ���°� Down �̶�� canCarry �� true �� �ٲ۴�.
                    if (survivor.GetComponent<SurviverHealth>().State == SurviverHealth.HealthState.Down)
                    {
                        canCarry = true;
                    }
                }
                else
                {
                    // ���࿡ �����ڰ� ������ Lullaby BG �� ������ �Ѵ�.


                    // canCarry -> false

                }

                // Generator ������
                if (hitcolliders[i].transform.gameObject.name.Contains("Generator"))
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
                if (hitcolliders[i].transform.gameObject.name.Contains("Hook") && survivor.GetComponent<SurviverHealth>().State == SurviverHealth.HealthState.Carrying)
                {
                    // canHook -> true
                    canHook = true;

                    // ���� �ִϸ��̼� ���� ���  
                }
            }
            #endregion
        }

        #region ����
        if (Input.GetKeyDown(KeyCode.G))
        {
            // G ��ư = ���� �Լ��� �ҷ��ٰ� �����ϰ� 
            Stunned();
        }
    }

    // ����
    private void Stunned()
    {
        SoundManager.instance.PlayHitSounds(4);

        cc.enabled = false;

        if (state == State.Carry)
        {
            // ĳ�� ���� �ִϸ��̼��� ����
            anim.SetTrigger("CarryStunned");
        }
        else
        {
            // ���� �ִϸ��̼��� ����
            anim.SetTrigger("Stunned");
            OffSmallAxe();
        }
    }
    #endregion
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
        }

        yield return null;
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
        }
        yield return null;
    }

    // ������

    // ����

    // ĳ���

    #endregion

    #region ���
    private void Idle()
    {
        cc.enabled = false;
        // ������ �����ϰ� 5�� �ڿ� ���¸� Move �� �ٲ۴�.
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

    #region �̵� �� ����

    bool playingthrowsound;
    private void UpdateMove()
    {
        #region  �Ϲ� ����
        // ���콺 ���� ��ư�� ������ �Ϲݰ����� �Ѵ�.
        if (Input.GetButtonDown("Fire1") && isCharging == false)
        {
            SoundManager.instance.PlayHitSounds(0);         // ������ �ԼҸ�

            state = State.NormalAttack;                     // ���¸� NormalAttack �� �ٲ�

            anim.SetTrigger("Attack");                      // �Ϲݰ��� �ִϸ��̼� ����

            isNormalAttack = true;                          // ������ ���� ������ ���� ��ư �������� �ϱ�
        }
        #endregion

        #region ������ ���� ����
        // ���콺 ������ ��ư�� ������ �Ѽյ����� ��¡�ϱ� �����Ѵ�.
        if (Input.GetButton("Fire2") && state != State.NormalAttack && isCanceled == false && currentAxeCount != 0)
        {
            Charging();                         // ��¡ �Լ��� ����
            isCharging = true;                  // ��¡ �߿� ���콺 ���� ��ư�� ������ �Ϲݰ����� ���ϵ���
            anim.SetBool("Throwing", true);     // �޼��� �� ä�� ���ƴٴ� �� �ִ�.
            throwUI.SetActive(false);
        }

        // ���콺 ������ ��ư�� ���� ������ ������.
        if (isCanceled == false && Input.GetButtonUp("Fire2") && currentAxeCount != 0)
        {
            anim.SetTrigger("Throw");                       // �ִϸ��̼� ����
            anim.SetBool("Throwing", false);                // ��¡ �ִϸ��̼� ���
            currentChargingTime = 0;                        // ���� ��¡ �ð��� �ʱ�ȭ
        }

        // ��¡ �߿� ���콺 ���� ��ư�� ������ ������ ����Ѵ�.
        if (Input.GetButtonDown("Fire1") && isCharging == true)
        {
            isCanceled = true;                              // ĵ������
            isCharging = false;                             // ��¡ ���� �ƴ�
            anim.SetBool("Throwing", false);                // ��¡ �ִϸ��̼� ���
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
            // ���ø��� UI �� ȭ�鿡 ���� �� �����̽��ٸ� ������
            if (Input.GetKeyDown(KeyCode.Space))
            {
                survivor.GetComponent<SurviverHealth>().ChangeCarring();    // �������� ���¸� �ٲٴ� �Լ��� ȣ��

                anim.SetTrigger("Pickup");                                  // �����ڸ� ���ø���.

                state = State.Carry;                                        // ���¸� Carry �� �ٲ۴�.

                canCarry = false;                                           // �� �� �ִ� ���°� �ƴ�

                transform.forward = survivor.transform.forward;             // �� �չ����� �������� �չ������� �����Ѵ�.

                StartCoroutine("LerptoSurvivor");                           // �ִϸ��̼� ���� ��ҷ� �̵��ϴ� �ڷ�ƾ �����Ѵ�.

                survivor.transform.parent = leftArm.transform;                 // �����ڸ� ���� �ڽ� ������Ʈ�� �����Ѵ�.


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

    #region �Ϲݰ���
    private void NormalAttack()
    {
        throwUI.SetActive(false);
        //anim.SetLayerWeight(3, 0f);
        // ������ ������ ���¸� Move �� �ٲ۴�. -> �ִϸ��̼� �̺�Ʈ
    }
    #endregion

    #region �Ѽ� ���� ��¡ / ��� / ���� / ��Ÿ��
    bool playingchargingsound;
    bool playingfullchargingsound;
    // ���� ��¡
    void Charging()
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
        if (isThrowing == true) return;                                 // ���� isThrowing ��� ����

        isThrowing = true;
        isCharging = false;                                             // ��¡ ���� �ƴ�
        playingchargingsound = false;                                   // ��¡�ϴ� ���� ����� �� �ִ� ����
        playingfullchargingsound = false;

        GameObject smallaxe = Instantiate(smallAxeFactory);             // �Ѽյ����� �����.
        smallaxe.transform.position = throwingSpot.position;            // ���� �Ѽյ����� ��ġ�� �޼տ� ��ġ�Ѵ�.
        smallaxe.transform.forward = Camera.main.transform.forward;     // ���� �Ѽյ����� �չ����� ī�޶��� �չ������� �Ѵ�

        currentAxeCount--;                                              // ���� ������ ���δ�
        axeCount.text = Convert.ToString(currentAxeCount);              // UI �����Ѵ�
        print(currentAxeCount);

        state = State.CoolTime;                                         // ���¸� CoolTime ���� �ٲ۴�
    }

    // 2�� ��Ÿ��
    void CoolTime()
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
    #endregion

    #region ������ ��� / �� ���¿��� ����
    // �̵�
    private void UpdateCarry()
    {
        throwUI.SetActive(false);

        #region ��������
        if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook == false)
        {
            anim.SetTrigger("Drop");
            cc.enabled = false;
            survivor.transform.parent = null;
            survivor.GetComponent<SurviverHealth>().State = SurviverHealth.HealthState.Down;
        }
        #endregion

        #region �� ���¿��� ����
        if (Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("CarryAttack");
            state = State.CarryAttack;
        }
        #endregion
    }

    // ����
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

    void OnMyReset()            // State �� Move �� �ʱ�ȭ�ϴ� �Լ�
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


    public void Throwing()      // State �� ThrowingAttack �� �ٲٴ� �Լ�
    {
        state = State.ThrowingAttack;
    }

    public void OnCoolTime()    // State �� CoolTime ���� �ٲٴ� �Լ�
    {
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



    #region Pun
    [PunRPC]
    void SetTriggerRpc(string parameter)        // �ִϸ��̼� SetTrigger
    {
        // anim2.SetTrigger(parameter);
    }

    //[PunRPC]
    //void SetBoolTrueRpc(string parameter)       // �ִϸ��̼� SetBool -> true
    //{
    //    anim2.SetBool(parameter, true);
    //}

    //[PunRPC]
    //void SetBoolFalseRpc(string parameter)      // �ִϸ��̼� SetBool -> false
    //{
    //    anim2.SetBool(parameter, false);
    //}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // ���� Anna ���


        // ���� Anna �� �ƴ϶��

    }
    #endregion
}