using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnaMove : MonoBehaviour
{
    #region 싱글톤
    public static AnnaMove instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region 변수
    // 이동 속도
    float speed = 4.4f;         // 기본    
    float readySpeed = 3.08f;   // 준비
    float delaySpeed = 3.74f;   // 경직

    // 회전
    float rotX;                     // X 회전값
    float rotY;                     // Y 회전값
    public float rotSpeed = 150;    // 회전속도
    public Transform cam;           // 카메라 Transform
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        #region 회전
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        rotX += mx * rotSpeed * Time.deltaTime;
        rotY += my * rotSpeed * Time.deltaTime;

        transform.localEulerAngles = new Vector3(0, rotX, 0);
        cam.localEulerAngles = new Vector3(-rotY, 0, 0);
        #endregion

        #region 이동
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
