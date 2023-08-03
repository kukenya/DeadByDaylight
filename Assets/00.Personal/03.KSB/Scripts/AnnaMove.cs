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

    //public enum State
    //{
    //    Idle, Move, TwoHandAttack, OneHandAttack, Stunned
    //}

    //public State state;
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
                                            // �Ѽյ��� SkinnedMeshRenderer ������Ʈ
                                            // �Ѽյ��� Rigidbody ������Ʈ
    #endregion


    void Start()
    {
        anim = GetComponent<Animator>();            // �ȳ� Animator ������Ʈ
        cc = GetComponent<CharacterController>();   // �ȳ� Rigidbody ������Ʈ
    }

    void Update()
    {
        #region switch
        //switch (state)
        //{
        //    case State.Idle:                                break;
        //    case State.Move:                                break;
        //    case State.TwoHandAttack:   TwoHandAttack();    break;
        //    case State.OneHandAttack:                       break;
        //    case State.Stunned:                             break;
        //    default:                                        break;
        //}
        #endregion
        
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
        if(rotY >= 20)
        {
            rotY = 20;
        }

        // �Ʒ����� �ִ밢
        else if(rotY <= -10)
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
        // transform.position += dir * speed * Time.deltaTime;
        cc.Move(dir * speed * Time.deltaTime);
        #endregion

        if (Input.GetButtonDown("Fire1"))
        {
            TwoHandAttack();
        }
    }
    private void TwoHandAttack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            cc.enabled = false;
            print(nameof(TwoHandAttack));
            anim.SetTrigger("Attack");
        }
    }
    public void Finish_Action()
    {
        cc.enabled = true;
    }
}
