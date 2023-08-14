using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnnaMove : MonoBehaviour
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
    Animator anim;                          // �ִϸ�����
    CharacterController cc;                 // ĳ���� ��Ʈ�ѷ�
    public Camera cineCam;                  // �ó׸ӽ� ī�޶� 

    // �̵� �ӵ�
    float currentSpeed;                     // ���� �̵��ӵ�
    float normalSpeed = 4.4f;               // �⺻ �̵��ӵ�
    float chargingSpeed = 3.08f;            // ��¡ �� �̵��ӵ�
    float delaySpeed = 3.74f;               // ���� �� �̵��ӵ�

    // �߷�
    public float yVelocity;
    public float gravity = -9.8f;


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
    float currentAxeCount;                  // ���� ������ �ִ� �Ѽյ��� ����
    float maxAxeCount = 5;                  // �ִ� ���� ������ �Ѽյ��� ����

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

    // UI
    public TextMeshProUGUI generatorCount;          // ������ ���� UI
    public TextMeshProUGUI axeCount;                // ���� ���� UI



    // ��Ÿ
    public GameObject survivor;                 // ������ ���ӿ�����Ʈ
    public Transform leftArm;                   // ����
    public Light redlight;                      // ���θ� �տ� �ִ� ����
    public GameObject throwUI;                  // ���� ������ UI
    #endregion

    #region Start & Update
    void Start()
    {
        anim = GetComponent<Animator>();                // �ȳ� Animator ������Ʈ
        cc = GetComponent<CharacterController>();       // �ȳ� Rigidbody ������Ʈ
        anim.SetLayerWeight(1, 0);                      // ������ �ִϸ��̼� ���̾�
        anim.SetLayerWeight(2, 0);                      // ��� �ִϸ��̼� ���̾�
        anim.SetLayerWeight(3, 0);                      // �Ϲ� �ִϸ��̼� ���̾�
        smallAxe.SetActive(false);                      // �޼տ� ��� �ִ� �Ѽյ��� ������ ��Ȱ��ȭ
        currentAxeCount = maxAxeCount;                  // ������ �� �������� �ִ�� ����
        axeCount.text = Convert.ToString(maxAxeCount);  // UI �� ��Ÿ����
        // survivor = GameObject.Find("Survivor");
        bigAxeCollider.enabled = false;
        // redlight.enabled = false;                    // ���θ� �տ� �ִ� ������ ��
        cineCam.depth = 5;                              // �ó׸ӽ� ī�޶� ���̰� �ϱ�
        throwUI.SetActive(false);                        // ���� ������  UI ��Ȱ��ȭ
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

        #region ȸ��
        if (state == State.Move || state == State.CoolTime || state == State.Carry && state == State.CarryAttack || state == State.Carry)
        {
            // ȸ������ �޾ƿ´�.
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            // ȸ������ ����
            rotX += mx * rotSpeed * Time.deltaTime;
            rotY += my * rotSpeed * Time.deltaTime;

            // ȸ������ ����
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

        #region �̵�
        // �̵��� �޾ƿ´�
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

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
        #endregion

        #region ���� ������
        if (Input.GetKeyDown(KeyCode.Space) && canReLoad && currentAxeCount < maxAxeCount)
        {
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
            // ���� ��ó���� UI �� ���� ��
            // �����̽��ٸ� ������ 
            if (Input.GetKeyDown(KeyCode.Space))
            {
                cc.enabled = false;             // ������ ����
                anim.SetBool("Hook", true);     // �����ڸ� ������ �Ŵ� �ִϸ��̼� ����
                                                // ������ �Ŵ� UI �������� ����.
                                                // �� ���� UI ��
            }
        }
        #endregion

        #region Ray �� ��ȣ�ۿ� ���� Ȯ���ϱ�
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        RaycastHit hitinfo;
        if (Physics.Raycast(ray, out hitinfo, 1f))
        {
            print(hitinfo.collider.transform.name);

            // Generator ������
            if (hitinfo.transform.name.Contains("Generator"))
            {
                // �μ� �� �ִ� ���·� �����.
                canDestroyGenerator = true;
            }
            else
            {
                canDestroyGenerator = false;
            }

            // Closet ĳ���
            if (hitinfo.transform.name.Contains("Closet"))
            {
                // ������ �� �� �ִ� ���·� �����.
                canReLoad = true;
            }
            else
            {
                canReLoad = false;
            }

            // Pallet ���� 
            if (hitinfo.transform.name.Contains("Pallet"))
            {
                // �μ� �� �ִ� ���·� �����.
                canDestroyPallet = true;
            }
            else
            {
                canDestroyPallet = false;
            }

            // ������ ������
            if (hitinfo.transform.name.Contains("Surviver") && survivor.GetComponent<SurviverHealth>().state == SurviverHealth.HealthState.Down)
            {
                canCarry = true;
            }
            else
            {
                canCarry = false;
            }

            // ����
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

        // ����
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
                survivor.GetComponent<SurviverHealth>().ChangeCarring();
                anim.SetTrigger("Pickup");  // �����ڸ� ���ø���.
                state = State.Carry;        // ���¸� Carry �� �ٲ۴�.
                canCarry = false;           // �� �� �ִ� ���°� �ƴ�

                // �������� ���� �� ���� �ڽ����� ���� ��� �ٴѴ�.
                survivor.transform.SetParent(leftArm);
                
                // survivor.transform.localPosition = new Vector3(-0.4f,0.961f,-0.125f);
                // survivor.transform.localPosition = new Vector3(0,0,0);
                // survivor.transform.localRotation = new Quaternion(0,0,0,0);
            }
        }
        #endregion

        #region �̵��� �� ���� �� ���̾ ����
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

        #region �̵��� �� ���� �� ���̾ ����
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

        isCharging = false;                                             // ��¡ ���� �ƴ�
        playingchargingsound = false;                                   // ��¡�ϴ� ���� ����� �� �ִ� ����
        playingfullchargingsound = false;
        GameObject smallaxe = Instantiate(smallAxeFactory);             // �Ѽյ����� �����.
        smallaxe.transform.position = throwingSpot.position;            // ���� �Ѽյ����� ��ġ�� �޼տ� ��
        smallaxe.transform.forward = Camera.main.transform.forward;     // ���� �Ѽյ����� �չ����� ī�޶��� �չ������� �Ѵ�

        currentAxeCount--;                                              // ���� ������ ���δ�
        axeCount.text = Convert.ToString(currentAxeCount);              // UI �����Ѵ�

        state = State.CoolTime;                                         // ���¸� CoolTime ���� �ٲ۴�
    }

    // 2�� ��Ÿ��
    void CoolTime()
    {
        isThrowing = true;

        playingchargingsound = false;   // ��¡�ϴ� ���� ����� �� �ִ� ����
        playingfullchargingsound = false;

        currentTime += Time.deltaTime;  // ���� �ð��� �帣�� �Ѵ�
        if (currentTime >= 2)           // ���� �ð��� 2�ʰ� �Ǹ�
        {
            state = State.Move;         // ���¸� Move �� �ٲ۴�
            isCharging = false;         // ��¡ ����
            isThrowing = false;         // ������ ����
            throwUI.SetActive(true);
            currentTime = 0;            // ���� �ð��� �ʱ�ȭ
        }
    }
    #endregion

    #region ������ ���
    // �̵�
    private void UpdateCarry()
    {
        throwUI.SetActive(false);


        #region ��������
        if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook == false)
        {
            anim.SetTrigger("Drop");
            cc.enabled = false;
        }
        #endregion

        #region �� ���¿��� ����
        if (Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("CarryAttack");
            state = State.CarryAttack;
        }
        #endregion

        #region �̵��� �� ���� �� ���̾ ����
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
}