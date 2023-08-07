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
        Idle, Move, NormalAttack, ThrowingAttack, Stunned
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
    float currentTime;                      // ���� �ð�
    float currentChargingTime;              // ���� ��¡ �ð�
    float minimumChargingTime = 1.25f;      // �ּ� ��¡ �ð�
    float maximumChargingTime = 3;          // �ִ� ��¡ �ð�
    float axeRechargingTime = 4;            // ���� ���� �ð�

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
    bool isCharging;                        // ��¡ ���ΰ�?
    bool isCanceled;                        // ��¡�� ����ߴ°�?
    #endregion

    #region Start & Update
    void Start()
    {
        anim = GetComponent<Animator>();            // �ȳ� Animator ������Ʈ
        cc = GetComponent<CharacterController>();   // �ȳ� Rigidbody ������Ʈ
        // anim.SetLayerWeight(1, 1);                  // Idle  �ִϸ��̼� ���̾�
        // anim.SetLayerWeight(2, 0);                  // Carry �ִϸ��̼� ���̾�
        smallAxe.SetActive(false);                  // �޼տ� ��� �ִ� �Ѽյ��� ������ ��Ȱ��ȭ
    }

    void Update()
    {
        #region switch
        switch (state)
        {
            case State.Idle: Idle(); break;
            case State.Move: UpdateMove(); break;
            case State.NormalAttack: NormalAttack(); break;
            case State.ThrowingAttack: ThrowingAttack(); break;
            case State.Stunned: break;
        }
        #endregion
    }
    #endregion

    #region ���� �ó׸���
    // ������ �� ī�޶� ���� ���� �� ���� Idle ���·� �ִٰ� Move ���·� �ٲ۴�.
    public void Idle()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= 1)
        {
            state = State.Move;
            anim.SetBool("Move", true);
        }
    }
    #endregion

    #region �̵�
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

        if (rotY >= 40)
        {
            rotY = 40;
        }
        if (rotY <= -40)
        {
            rotY = -40;
        }
        #endregion

        if (isCharging == false)
        {
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
            currentSpeed = normalSpeed;
            cc.Move(dir * currentSpeed * Time.deltaTime);
            #endregion
        }

        print(currentSpeed);
        // ���콺 ���� ��ư�� ������ �Ϲݰ����� �Ѵ�.
        if (Input.GetButtonDown("Fire1") && isCharging == false)
        {
            state = State.NormalAttack;
            anim.SetTrigger("Attack");
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

            Invoke("DoCancel", 0.7f);
        }
    }
    #endregion

    void DoCancel()
    {
        isCanceled = false;
        OffSmallAxe();
    }

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
    void Charging()
    {
        #region ȸ��
        // ȸ������ �޾ƿ´�.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // ȸ������ ����
        rotX += mx * rotSpeed * Time.deltaTime;
        rotY += my * rotSpeed * Time.deltaTime;

        // ȸ������ ����
        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);

        // ������ �ִ밢
        if (rotY >= 40)
        {
            rotY = 40;
        }

        // �Ʒ����� �ִ밢
        else if (rotY <= -40)
        {
            rotY = -40;
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
        currentSpeed = readySpeed;
        cc.Move(dir * currentSpeed * Time.deltaTime);
        #endregion

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
    }

    private void ThrowingAttack()
    {
        // �����տ� �Ѽյ����� �����.
        GameObject smallaxe = Instantiate(smallAxeFactory);
        smallaxe.transform.position = throwingSpot.position;
        smallaxe.transform.forward = throwingSpot.up;
    }
    #endregion

    #region Events
    public void Throwing()                  // State �� ThrowingAttack �� �ٲٴ� �Լ�
    {
        state = State.ThrowingAttack;
    }

    void OnMyReset()                        // State �� Move �� �ʱ�ȭ�ϴ� �Լ�
    {
        state = State.Move;
        anim.SetBool("Throwing", false);
        isCharging = false;
    }

    public void OnSmallAxe()                // ���� Ȱ��ȭ
    {
        smallAxe.SetActive(true);
    }

    public void OffSmallAxe()               // ���� ��Ȱ��ȭ
    {
        smallAxe.SetActive(false);
    }
    #endregion
}
