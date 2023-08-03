using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnaMove : MonoBehaviour
{
    #region �̱���
    public static AnnaMove instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region ����
    // �̵� �ӵ�
    float speed = 4.4f;         // �⺻    
    float readySpeed = 3.08f;   // �غ�
    float delaySpeed = 3.74f;   // ����

    // ȸ��
    float rotX;                     // X ȸ����
    float rotY;                     // Y ȸ����
    public float rotSpeed = 150;    // ȸ���ӵ�
    public Transform cam;           // ī�޶� Transform
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        #region ȸ��
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        rotX += mx * rotSpeed * Time.deltaTime;
        rotY += my * rotSpeed * Time.deltaTime;

        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);
        #endregion

        #region �̵�
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dirH = transform.right * h;
        Vector3 dirV = transform.forward * v;
        Vector3 dir = dirH + dirV;
        dir.Normalize();

        transform.position += dir * speed * Time.deltaTime;
        #endregion
    }
}
