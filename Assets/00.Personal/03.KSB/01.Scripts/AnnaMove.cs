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
        Move, TwoHandAttack, OneHandAttack, Stunned
    }

    public State state;
    #endregion

    #region ����
    Animator anim;                          // �ִϸ�����
    CharacterController cc;                 // ĳ���� ��Ʈ�ѷ�

    // �̵� �ӵ�
    float speed = 4.4f;                     // �⺻ �̵��ӵ�
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
                                            // �Ѽյ��� Rigidbody ������Ʈ
    public float axeSpeed = 25;             // ����ü �ӵ�
    #endregion


    void Start()
    {
        anim = GetComponent<Animator>();            // �ȳ� Animator ������Ʈ
        cc = GetComponent<CharacterController>();   // �ȳ� Rigidbody ������Ʈ
        anim.SetLayerWeight(1, 0);                  // 
        smallAxe.SetActive(false);
    }

    void Update()
    {
        #region switch
        switch (state)
        {
            case State.Move: UpdateMove(); break;
            case State.TwoHandAttack: TwoHandAttack(); break;
            case State.OneHandAttack: OneHandAttack(); break;
            case State.Stunned: break;
            default: UpdateMove(); break;
        }
        #endregion
    }

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
        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);

        // ������ �ִ밢
        if (rotY >= 20)
        {
            rotY = 20;
        }

        // �Ʒ����� �ִ밢
        else if (rotY <= -15)
        {
            rotY = -15;
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
        cc.Move(dir * speed * Time.deltaTime);
        #endregion

        // ���콺 ���� ��ư�� ������ �μ� ������ �Ѵ�.
        if (Input.GetButtonDown("Fire1") && state != State.OneHandAttack)
        {
            state = State.TwoHandAttack;
            anim.SetTrigger("Attack");
        }

        // ���콺 ������ ��ư�� ������ �Ѽ� ������ �����Ѵ�.
        if (Input.GetButton("Fire2") && state != State.TwoHandAttack)
        {
            anim.SetBool("Throwing", true);
        }
    }

    private void TwoHandAttack()
    {
        #region ȸ��
        // ȸ������ �޾ƿ´�.
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // ȸ������ ����
        rotX += mx * 10 * Time.deltaTime;
        rotY += my * 10 * Time.deltaTime;

        // ȸ������ ����
        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);

        // ������ �ִ밢
        if (rotY >= 10)
        {
            rotY = 10;
        }

        // �Ʒ����� �ִ밢
        else if (rotY <= -10)
        {
            rotY = -10;
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
        cc.Move(dir * 0.5f * Time.deltaTime);
        #endregion
    }

    private void OneHandAttack()
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
        if (rotY >= 20)
        {
            rotY = 20;
        }

        // �Ʒ����� �ִ밢
        else if (rotY <= -15)
        {
            rotY = -15;
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
        cc.Move(dir * readySpeed * Time.deltaTime);
        #endregion

        if (Input.GetButton("Fire2"))
        {
            // ����ð��� �����Ѵ�.
            currentTime += Time.deltaTime;

            print(currentTime);
            // ����ü �ӵ��� �ּ� 25m/s , �ִ� 40m/s
            // �ּ���¡�ð��� 1.25�� �ִ���¡�ð��� 3��

            // ���콺 ������ ��ư���� ���� ����
            if (Input.GetButtonDown("Fire1"))
            {
                // ������ ����Ѵ�.
                state = State.Move;
                anim.SetBool("Throwing", false);
                currentTime = 0;
            }
        }

        if (Input.GetButtonUp("Fire2"))
        {
            anim.SetTrigger("Throw");
            // �Ѽ� ������ ������.
            GameObject sa = Instantiate(smallAxeFactory);
            sa.transform.position = this.transform.position;
            sa.transform.forward = this.transform.forward;

            currentTime = 0;
        }
    }


    #region Events

    public void OnmyOneHandAttack()
    {
        state = State.OneHandAttack;
    }

    void OnMyReset()                // State �� UpdateMove �� �ʱ�ȭ�ϴ� �Լ�
    {
        state = State.Move;
        anim.SetBool("Throwing", false);
    }

    public void Finish_Action()     // CharacterController Ȱ��ȭ �Լ�
    {
        cc.enabled = true;
    }

    public void OnSmallAxe()        // ���� Ȱ��ȭ
    {
        smallAxe.SetActive(true);
    }

    public void OffSmallAxe()       // ���� ��Ȱ��ȭ
    {
        smallAxe.SetActive(false);
    }
    #endregion
}
