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
    public bool canDestroyBoard;                   // ������ ���ڸ� �μ� �� �ִ°�?
    
    // ��Ÿ
    GameObject survivor;                    // ������ ���ӿ�����Ʈ
    public Transform leftArm;               // ����
    // public Light redlight;               // ���θ� �տ� �ִ� ����
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
        survivor = GameObject.Find("Survivor");
        // redlight.enabled = false;                // ���θ� �տ� �ִ� ������ ��
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
                anim.SetBool("Destroy", true);
            }
            // �߰��� �����̽� �ٿ��� ���� ���� ��ҵȴ�.
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.SetTrigger("DestroyCancel");
                anim.SetBool("Destroy", false);
                cc.enabled = true;
            }
        }        
        #endregion

        #region ���� �μ���
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

        if(Physics.Raycast(ray, out hitinfo))
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
        // ������ �����ϰ� 5�� �ڿ� ���¸� Move �� �ٲ۴�.
        currentTime += Time.deltaTime;
        if (currentTime >= gameStartTime)
        {
            state = State.Move;
            anim.SetBool("Move", true);
        }
    }
    #endregion

    #region �̵� �� ����
    private void UpdateMove()
    {
        #region ȸ��
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
        else if (isCharging == false || state != State.CoolTime)
        {
            // �� �ܴ̿� �Ϲݼӵ�(4.4)�� �̵�
            currentSpeed = normalSpeed;
        }

        // �̵��Ѵ�
        cc.Move(dir * currentSpeed * Time.deltaTime);
        #endregion

        #region  �Ϲ� ����
        // ���콺 ���� ��ư�� ������ �Ϲݰ����� �Ѵ�.
        if (Input.GetButtonDown("Fire1") && isCharging == false)
        {
            // ���¸� NormalAttack �� �ٲ�
            state = State.NormalAttack;

            // �Ϲݰ��� �ִϸ��̼� ����
            anim.SetTrigger("Attack");

            isNormalAttack = true;

            // �����ִ� ���¿��� ������
            //if (cc.velocity == Vector3.zero)
            //{
            //    // �׳� ����
            //    // ���¸� NormalAttack �� �ٲ�
            //    state = State.NormalAttack;
            //}
            // �����̴� ���¶��
            //else
            //{
            //    ������ ���鼭 ����
            //    transform.position = transform.position + transform.forward * 2;
            //    
            //    Vector3 attackPos = transform.position + transform.forward * 2;
            //    transform.position = Vector3.Lerp(transform.position, attackPos, 0.8f);
            //}
        }
        #endregion

        #region ������ ���� ����
        // ���콺 ������ ��ư�� ������ �Ѽյ����� ��¡�ϱ� �����Ѵ�.
        if (Input.GetButton("Fire2") && state != State.NormalAttack && isCanceled == false && currentAxeCount != 0)
        {
            Charging();
            isCharging = true;                  // ��¡ �߿� ���콺 ���� ��ư�� ������ �Ϲݰ����� ���ϵ���
            anim.SetBool("Throwing", true);     // �޼��� �� ä�� ���ƴٴ� �� �ִ�.
        }

        // ���콺 ������ ��ư�� ���� ������ ������.
        if (isCanceled == false && Input.GetButtonUp("Fire2") && currentAxeCount != 0)
        {
            anim.SetTrigger("Throw");
            anim.SetBool("Throwing", false);
            currentChargingTime = 0;
        }

        // ��¡ �߿� ���콺 ���� ��ư�� ������ ������ ����Ѵ�.
        if (Input.GetButtonDown("Fire1"))
        {
            isCanceled = true;
            anim.SetBool("Throwing", false);
            currentChargingTime = 0;
            isCharging = false;

            Invoke("DoCancel", 0.4f);
        }
        #endregion

        #region ������ ���
        // ���� �����Ÿ� �ȿ� �����ڰ� �������ִٸ�
        if (canCarry && state != State.Carry)
        {
            // ���ø��� UI �� ȭ�鿡 ���� �� �����̽��ٸ� ������
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // �����ڸ� ���ø���.
                anim.SetTrigger("Pickup");
                state = State.Carry;
                canCarry = false;

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
        //anim.SetLayerWeight(3, 0f);
        // ������ ������ ���¸� Move �� �ٲ۴�. -> �ִϸ��̼� �̺�Ʈ
    }
    #endregion

    #region �Ѽ� ���� ��¡ / ��� / ���� / ��Ÿ��
    // ���� ��¡
    void Charging()
    {
        #region �ð��� ���� �����Ǵ� ����ü ������ ��
        // �ð��� ������Ų��.
        currentChargingTime += Time.deltaTime;

        // �ð��� �����ʿ� ���� chargingForce �� ������Ų��.
        chargingForce = Mathf.Lerp(minAxePower, maxAxePower, (currentChargingTime - 1.25f) / 1.75f);

        // 1.25�� �������� ������ ���� ���� �ʴ´�.
        // if (currentChargingTime < minimumChargingTime) return;

        // �ִ� ���� �ð����� �����ð��� ������� �ִ������� �����Ѵ�.
        if (currentChargingTime > maximumChargingTime)
        {
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
        if (isThrowing == true) return;

        // �����տ� �Ѽյ����� �����.
        GameObject smallaxe = Instantiate(smallAxeFactory);
        smallaxe.transform.position = throwingSpot.position;
        smallaxe.transform.forward = Camera.main.transform.forward;

        // ���� ������ ���δ�
        currentAxeCount--;

        state = State.CoolTime;
    }

    // 2�� ��Ÿ��
    void CoolTime()
    {
        isThrowing = true;

        #region ȸ��
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

        // �̵��Ѵ�
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

    #region ������ ���
    // �̵�
    private void UpdateCarry()
    {
        #region ȸ��
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
        #endregion

        #region �̵�
        // �̵��� �޾ƿ´�
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // �̵����� �ִϸ��̼ǰ� ����
        anim.SetFloat("h", h);
        anim.SetFloat("v", v);

        // ������ ���Ѵ�
        Vector3 dirH = transform.right * h;
        Vector3 dirV = transform.forward * v;
        Vector3 dir = dirH + dirV;
        dir.Normalize();

        // �̵��Ѵ�
        currentSpeed = normalSpeed;
        cc.Move(dir * normalSpeed * Time.deltaTime);
        #endregion

        #region ��������
        // �����ڸ� ��� �ִ� ���¿��� �����̽��ٸ� �ٽ� ������ �������´�. -> Vector3.zero
        if (Input.GetKeyDown(KeyCode.Space) && canCarry == false && canHook ==false)
        {
            anim.SetTrigger("Drop");
            cc.enabled = false;
        }
        #endregion

        #region �� ���¿��� ����
        // �����ڸ� ��� �ִ� ���¿��� ���콺 ���� ��ư�� ������ �����Ѵ�. -> Vector3.zero
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
        currentTime += Time.deltaTime;
        if(currentTime >= 2.5f)
        {
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
        if (other.gameObject.layer == 23)
        {
            canDestroyGenerator = true;
        }

        // ĳ���(����������)
        //if(other.gameObject.layer == 24)
        //{
        //    canReLoad = true;
        //}

        // ������ ����
        if(other.gameObject.layer == 25)
        {
            canDestroyBoard = true;
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
        //if (other.gameObject.layer == 23)
        //{
        //    canDestroyGenerator = false;
        //}

        // ĳ���(����������)
        if (other.gameObject.layer == 24)
        {
            canReLoad = false;
        }

        // ������ ����
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

    void OnMyReset()            // State �� Move �� �ʱ�ȭ�ϴ� �Լ�
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