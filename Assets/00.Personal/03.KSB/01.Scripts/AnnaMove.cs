using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // �̵� �ӵ�
    float currentSpeed;                     // ���� �̵��ӵ�
    float normalSpeed = 4.4f;               // �⺻ �̵��ӵ�
    float chargingSpeed = 3.08f;            // ��¡ �� �̵��ӵ�
    float delaySpeed = 3.74f;               // ���� �� �̵��ӵ�

    // �߷�
    public float yVelocity;
    public float gravity = - 9.8f;


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
    bool isNormalAttack;                    // �Ϲݰ��� ���ΰ�?
    bool isCharging;                        // ��¡ ���ΰ�?
    bool isCanceled;                        // ��¡�� ����ߴ°�?
    bool isThrowing;                        // �յ����� �����°�?
    bool isStunned;                         // ���� ���ߴ°�?
    public bool canCarry;                   // �����ڸ� �� �� �ִ°�?
    public bool canDestroyGenerator;               // �����⸦ �μ� �� �ִ°�?
    public bool canHook;                           // ������ �� �� �ִ°�?
    public bool canReLoad;                         // �� �� ������ ������ �� �� �ִ°�?
    public bool canDestroyPallet;                   // ������ ���ڸ� �μ� �� �ִ°�?

    
    // ��Ÿ
    GameObject survivor;                    // ������ ���ӿ�����Ʈ
    // public Transform leftArm;               // ����
    // public Light redlight;               // ���θ� �տ� �ִ� ����
    public Transform groundCheck;           // �ٴ� üũ�� ���� �� ���
    #endregion

    #region Start & Update
    void Start()
    {
        anim = GetComponent<Animator>();            // �ȳ� Animator ������Ʈ
        cc = GetComponent<CharacterController>();   // �ȳ� Rigidbody ������Ʈ
        anim.SetLayerWeight(1, 0);                  // ������ �ִϸ��̼� ���̾�
        anim.SetLayerWeight(2, 0);                  // ��� �ִϸ��̼� ���̾�
        anim.SetLayerWeight(3, 0);                  // �Ϲ� �ִϸ��̼� ���̾�
        smallAxe.SetActive(false);                  // �޼տ� ��� �ִ� �Ѽյ��� ������ ��Ȱ��ȭ
        currentAxeCount = maxAxeCount;              // ������ �� �������� �ִ�� ����
        // survivor = GameObject.Find("Survivor");
        bigAxeCollider.enabled = false;
        // redlight.enabled = false;                // ���θ� �տ� �ִ� ������ ��
    }

    void Update()
    {
        print(currentSpeed);
        print(cc.isGrounded);

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
        if (state == State.Move || state == State.CoolTime || state == State.Carry && state == State.CarryAttack)
        {
            // ȸ������ �޾ƿ´�.
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            // ȸ������ ����
            rotX += mx * rotSpeed * Time.deltaTime;
            rotY += my * rotSpeed * Time.deltaTime;

            // ȸ������ ����
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
            OffCC();
            anim.SetTrigger("Reload");
            currentAxeCount = maxAxeCount;
        }

        // �׳� ���� -> ���� ����
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
            if (Input.GetKey(KeyCode.Space))
            {
                cc.enabled = false;
                anim.SetBool("DestroyP", true);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                SoundManager.instance.StopDestroyPallets();
                anim.SetTrigger("DestroyCancel");
                anim.SetBool("DestroyP", false);
                cc.enabled = true;
            }
        }
        #endregion

        #region ���� �ɱ�
        if (state == State.Carry && canHook)
        {
            // ���� ��ó���� UI �� ���� ��
            // �����̽��ٸ� ��� ������ ������ 
            if (Input.GetKey(KeyCode.Space))
            {
                // �������� ����.
                // �� ���� UI ��
                anim.SetBool("Hook", true);
                cc.enabled = false;
            }
            // ���� �������� �� ���� ���� �����̽��ٿ��� �������� �ɱⰡ ĵ���ȴ�.
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.SetTrigger("HookCancel");
                anim.SetBool("Hook", false);
                cc.enabled = true;

                // ������ �ʱ�ȭ
                // UI ����
            }
        }
        #endregion

        // ����
        if (Input.GetKeyDown(KeyCode.G))
        {
            // G ��ư = ���� �Լ��� �ҷ��ٰ� �����ϰ� 
            Stunned();
        }

        // ���̷� 
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        RaycastHit hitinfo;

        if (Physics.Raycast(ray, out hitinfo))
        {
            //���� �̸��� "Generator"�� �����ϰ� �ִٸ�
            if (hitinfo.transform.name.Contains("Generator"))
            {
                // �μ� �� �ִ� ���·� �����.
                canDestroyGenerator = true;
            }
            else
            {
                canDestroyGenerator = false;
            }
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
            SoundManager.instance.PlayBigAxeSounds(0);      // ������ �ԼҸ�
           
            cc.enabled = false;                             // �̵��Ұ�

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
        }

        // ���콺 ������ ��ư�� ���� ������ ������.
        if (isCanceled == false && Input.GetButtonUp("Fire2") && currentAxeCount != 0)
        {
            anim.SetTrigger("Throw");                       // �ִϸ��̼� ����
            anim.SetBool("Throwing", false);                // ��¡ �ִϸ��̼� ���
            currentChargingTime = 0;                        // ���� ��¡ �ð��� �ʱ�ȭ
            if(playingthrowsound == false)
            {
                SoundManager.instance.PlaySmallAxeSounds(3);    // ������ ���� ���
                playingthrowsound = true;
            }
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
                anim.SetTrigger("Pickup");  // �����ڸ� ���ø���.
                state = State.Carry;        // ���¸� Carry �� �ٲ۴�.
                canCarry = false;           // �� �� �ִ� ���°� �ƴ�

                // �������� ���� �� ���� �ڽ����� ���� ��� �ٴѴ�.
                // survivor.transform.SetParent(leftArm);
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
        OnAxe();    // ���� �ݶ��̴��� Ŵ
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
        if(playingchargingsound == false)
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
            currentTime = 0;            // ���� �ð��� �ʱ�ȭ
        }
    }
    #endregion

    #region ������ ���
    // �̵�
    private void UpdateCarry()
    {        
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

    #region Ʈ���� Ȯ��
    private void OnTriggerEnter(Collider other)
    {
        // ������
        if (other.gameObject.layer == 6)
        {
            canCarry = true;
        }

        // ����
        if (other.gameObject.layer == 22)
        {
            canHook = true;
        }

        // ������
        if (other.transform.name.Contains("Generator"))
        {
            canDestroyGenerator = true;
        }

        // ĳ���(����������)
        //if(other.gameObject.layer == 24)
        //{
        //    canReLoad = true;
        //}

        // ������ ����
        if (other.gameObject.name.Contains("Pallet"))
        {
            canDestroyPallet = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ������
        if (other.gameObject.layer == 6)
        {
            canCarry = false;
        }

        // ����
        if (other.gameObject.layer == 22)
        {
            canHook = false;
        }

        // ������
        if (other.transform.name.Contains("Generator"))
        {
            canDestroyGenerator = false;
        }

        // ĳ���(����������)
        if (other.gameObject.layer == 24)
        {
            canReLoad = false;
        }

        // ������ ����
        if (other.gameObject.name.Contains("Pallet"))
        {
            canDestroyPallet = false;
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
        anim.SetBool("Hook", false);
        anim.SetBool("DestroyG", false);
        anim.SetBool("DestroyP", false);
        
        isStunned = false;
        isCharging = false;
        isNormalAttack = false;
        playingthrowsound = false;
        canDestroyGenerator = false;
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