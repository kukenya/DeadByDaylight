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
    float readySpeed = 3.08f;               // ��¡ �� �̵��ӵ�
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
    float minimumChargingTime = 1.25f;      // �ּ� ��¡ �ð�
    float maximumChargingTime = 3;          // �ִ� ��¡ �ð�
    float axeRechargingTime = 4;            // ���� ���� �ð�

    // ī��Ʈ
    float currentAxeCount;                  // ���� ������ �ִ� �Ѽյ��� ����
    float maxAxeCount = 5;                  // �ִ� ���� ������ �Ѽյ��� ����
    float notMoveCurrentTime;

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
    #endregion

    #region Start & Update
    void Start()
    {
        anim = GetComponent<Animator>();            // �ȳ� Animator ������Ʈ
        cc = GetComponent<CharacterController>();   // �ȳ� Rigidbody ������Ʈ
        anim.SetLayerWeight(1, 0);                  // �ִϸ��̼� ���̾�
        anim.SetLayerWeight(2, 0);                  // �ִϸ��̼� ���̾�
        smallAxe.SetActive(false);                  // �޼տ� ��� �ִ� �Ѽյ��� ������ ��Ȱ��ȭ
        notMoveCurrentTime = 0f;
    }

    void Update()
    {
       print(currentSpeed);

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
            anim.SetTrigger("Move");
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

        if (rotY >= 36)
        {
            rotY = 36;
        }
        if (rotY <= -36)
        {
            rotY = -36;
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
        if (isCharging == false)
        {
            currentSpeed = normalSpeed;
        }
        else
        {
            currentSpeed = readySpeed;
        }
        cc.Move(dir * currentSpeed * Time.deltaTime);
        #endregion

        #region ����
        // ���콺 ���� ��ư�� ������ �Ϲݰ����� �Ѵ�.
        if (Input.GetButtonDown("Fire1") && isCharging == false)
        {
            state = State.NormalAttack;
            anim.SetTrigger("Attack");
            isNormalAttack = true;
        }

        // ���콺 ������ ��ư�� ������ �Ѽյ����� ��¡�ϱ� �����Ѵ�.
        if (Input.GetButton("Fire2") && state != State.NormalAttack && isCanceled == false)
        {

            Charging();
            isCharging = true;                  // ��¡ �߿� ���콺 ���� ��ư�� ������ �Ϲݰ����� ���ϵ���
            anim.SetBool("Throwing", true);     // �޼��� �� ä�� ���ƴٴ� �� �ִ�.
        }

        // ���콺 ������ ��ư�� ���� ������ ������.
        if (isCanceled == false && Input.GetButtonUp("Fire2"))
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

            Invoke("DoCancel", 0.8f);
        }
        #endregion

        #region ������ ���
        // ���� �����Ÿ� �ȿ� �����ڰ� �������ִٸ�
        // ���ø��� UI �� ȭ�鿡 ���� �� �����̽��ٸ� ������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // �����ڸ� ���ø���.
            anim.SetTrigger("Pickup");
            cc.enabled = false;
        }
        #endregion
    }

    // ���� ��¡ ĵ��
    void DoCancel()
    {
        isCanceled = false;
        OffSmallAxe();
    }
    #endregion

    #region �Ϲݰ���
    private void NormalAttack()
    {
        // ȸ���ӵ��� �̵��ӵ� �پ��� �Ѵ�.
        #region ȸ��
        // ȸ������ �޾ƿ´�.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // ȸ������ ����
        rotX += mx * 1 * Time.deltaTime;
        rotY += my * 1 * Time.deltaTime;

        // ȸ������ ����
        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);
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
        cc.Move(dir * 0.1f * Time.deltaTime);
        #endregion

        // ������ ������ ���¸� Move �� �ٲ۴�. -> �ִϸ��̼� �̺�Ʈ
    }
    #endregion

    #region �Ѽ� ���� ������
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
        if (cc.velocity == Vector3.zero)
        {
            anim.SetLayerWeight(1, 0);
        }
        else
        {
            anim.SetLayerWeight(1, 1);
        }
        #endregion
    }

    // ���� ����
    private void ThrowingAttack()
    {
        if (isThrowing == true) return;

        // �����տ� �Ѽյ����� �����.
        GameObject smallaxe = Instantiate(smallAxeFactory);
        smallaxe.transform.position = throwingSpot.position;
        smallaxe.transform.forward = Camera.main.transform.forward;

        state = State.CoolTime;
        isThrowing = true;
    }

    // 2�� ��Ÿ��
    void CoolTime()
    {
        #region ȸ��
        // ȸ������ �޾ƿ´�.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // ȸ������ ����
        rotX += mx * 50 * Time.deltaTime;
        rotY += my * 50 * Time.deltaTime;

        // ȸ������ ����
        transform.eulerAngles = new Vector3(0, rotX, 0);
        cam.eulerAngles = new Vector3(-rotY, rotX, 0);

        if (rotY >= 20)
        {
            rotY = 20;
        }
        if (rotY <= -20)
        {
            rotY = -20;
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
        cc.Move(dir * 1 * Time.deltaTime);
        #endregion

        currentTime += Time.deltaTime;
        if (currentTime >= 2)
        {
            state = State.Move;
            isThrowing = false;
            isCharging = false;
        }
    }
    #endregion

    #region ������ ���
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

        if (rotY >= 36)
        {
            rotY = 36;
        }
        if (rotY <= -36)
        {
            rotY = -36;
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

        // ���� ��ó���� UI �� ���� ��
        // �����̽��ٸ� ��� ������ ������ �������� ����. -> 
        if (Input.GetKey(KeyCode.V))
        {
            DoHook();
            cc.enabled = false;
        }
        // ���� �������� �� ���� ���� �����̽��ٿ��� �������� �ɱⰡ ĵ���ȴ�.

        // ���� �������� �� ���� �����ڸ� ������ �Ǵ�.

        // �����ڸ� ��� �ִ� ���¿��� �����̽��ٸ� �ٽ� ������ �������´�. -> Vector3.zero
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Drop");
            cc.enabled = false;
        }

        // �����ڸ� ��� �ִ� ���¿��� ���콺 ���� ��ư�� ������ �����Ѵ�. -> Vector3.zero
        if (Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("CarryAttack");
            state = State.CarryAttack;
        }

        #region �̵��� �� ���� �� ���̾ ����
        if (cc.velocity == Vector3.zero)
        {
            anim.SetLayerWeight(2, 0);
        }
        else
        {
            anim.SetLayerWeight(2, 1);
        }
        #endregion
    }

    public void DoHook()
    {
        currentTime += Time.deltaTime;
    }

    public void CarryAttack()
    {

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
        state = State.Move;
        anim.SetBool("Throwing", false);
        isCharging = false;
        isNormalAttack = false;
    }

    public void Throwing()      // State �� ThrowingAttack �� �ٲٴ� �Լ�
    {
        state = State.ThrowingAttack;
    }

    public void OnCoolTime()    // State �� CoolTime ���� �ٲٴ� �Լ�
    {
        state = State.CoolTime;
    }

    void OnMyCarry()            // State �� Carry �� �ٲٴ� �Լ�
    {
        // ������ ��� ���·� �ٲ۴�.
        state = State.Carry;
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