using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; //import
using Photon.Pun;
using System.Linq;

public class SurviverController : MonoBehaviourPun, IPunObservable
{
    [Header("플레이어")]
    public float walkSpeed = 2.26f;
    public float hopeWalkSpeed = 2.42f;
    public float sprintSpeed = 4f;
    public float hopeSprintSpeed = 4.28f;
    public float hitSprintSpeed = 6f;
    public float crouchingSpeed = 1.13f;
    public float crawlSpeed = 0.7f;

    // ī�޶� �����̼� �ӵ�
    public float speedChangeRate = 10.0f;

    // �÷��̾� ���� ó�� ���� ������
    public bool Grounded = true;
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;
    public LayerMask groundLayers;

    public GameObject cinemachineCameraTarget;
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;
    private float cameraAngleOverride = 0.0f;
    public bool lockCameraPosition = false;
    public bool lockPlayerRotation = false;

    // ī�޶� ���� ������
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // �÷��̾�
    private float speed;
    private float targetSpeed;
    private float targetRotation = 0.0f;
    private float verticalVelocity = 0;
    private float moveVelocity = 0;
    private float deltaAngle = 0;

    // �÷��̾� ���콺 ����
    public float mouseSensitivity = 1f;

    bool isMoving = false;
    public bool Moving { get { return isMoving; } set { isMoving = value; } }
    bool isSprint = false;
    //bool isRotating = false;
    bool isCrouching = false;
    bool isCrawl = false;
    public bool isHit = false;

    public bool Sprint { get { return isSprint; } set { isSprint = value; } }
    public bool Crawl { get { return isCrawl; }  set { isCrawl = value; } }

    public float sprintTime;
    public float maxSprintTime = 1;

    // �ִϸ��̼� �Ŵ���
    SurviverAnimation surviverAnimation;
    SurviverLookAt lookAt;

    // ET
    public CharacterController controller;
    Camera mainCamera;

    public Animator playerAnimator;

    void Start()
    {
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        surviverAnimation = GetComponent<SurviverAnimation>();
        lookAt = GetComponent<SurviverLookAt>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();
        CameraStopCheck();
        Move();
    }

    void CameraStopCheck()
    {
        Moving = currentSpeed == 0 ? false : true;
    }

    public float rotationSpeed;
    public float currentSpeed;

    bool banMove = false;

    public bool BanMove { get { return banMove; } set {
            banMove = value; 
            if (value == false)
            {
                lookAt.LookAt = true;
                controller.enabled = true;
                surviverAnimation.photonView.RPC("AnimationChange", RpcTarget.All);
            }
            else
            {
                lookAt.LookAt = false;
                controller.enabled = false;
            }
        }
    }

    Vector3 receivePos;
    Quaternion receiveRot;
    public float lerpSpeed = 5;

    public bool anim = false;
    private void Move()
    {
        if (photonView.IsMine)
        {
            if (banMove) return;

            // ���� �ӷ��� �޸��� ��ư�� ���������� �ȴ������� ������.
            isSprint = Input.GetKey(KeyCode.LeftShift) ? true : false;
            isCrouching = Input.GetKey(KeyCode.LeftControl) ? true : false;

            sprintTime = Mathf.Clamp(isSprint ? sprintTime + Time.deltaTime : 0, 0, maxSprintTime);

            if (surviverAnimation.Pose != SurviverAnimation.PoseState.Crawl)
            {
                surviverAnimation.Pose = isCrouching ? SurviverAnimation.PoseState.Crouching : SurviverAnimation.PoseState.Standing;
            }

            deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.y, targetRotation);

            targetSpeed = GetTargetSpeed();

            float inputHorizontal = Input.GetAxisRaw("Horizontal");
            float inputVertical = Input.GetAxisRaw("Vertical");

            // ��ǲ�� ������ ���� �ӷ��� 0���� �����.
            if (inputHorizontal == 0 && inputVertical == 0)
            {
                targetSpeed = 0.0f;
            }
            //speed = Mathf.MoveTowards(speed, targetSpeed, Time.deltaTime * speedChangeRate);

            float speedOffset = 0.1f;


            // �����϶� ���Ӱ� ��������
            if (currentSpeed < targetSpeed - speedOffset ||
               currentSpeed > targetSpeed + speedOffset)
            {
                speed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);

                // �Ҽ��� 3�ڸ������� �ڸ���
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed;
            }
            speed = Mathf.SmoothDamp(speed, targetSpeed, ref moveVelocity, speedChangeRate);


            ////�ִϸ��̼ǿ� �ӵ��� �´� �ε巯�� �ֱ�

            // �Էµ� ������ ����ȭ �Ѵ�.
            Vector3 inputDirection = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;

            // �����϶� ������ �ε巴�� �Ѱ��ش�.
            if (inputDirection != Vector3.zero)
            {

                if (isSprint)
                {
                    surviverAnimation.mState = SurviverAnimation.MoveState.Sprinting;
                }
                else
                {
                    surviverAnimation.mState = SurviverAnimation.MoveState.Walking;
                }
                targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                    mainCamera.transform.eulerAngles.y;

                Vector3 rotation = Vector3.MoveTowards(new Vector3(0, transform.eulerAngles.y, 0), new Vector3(0, transform.eulerAngles.y + deltaAngle, 0), rotationSpeed * Time.deltaTime);
                //isRotating = rotation == new Vector3(0, transform.eulerAngles.y + deltaAngle, 0) ? false : true;

                // ���� ī�޶� �����ǿ� �°� ȸ���Ѵ�. (���� ī�޶� �ƴҶ���)
                transform.rotation = Quaternion.Euler(rotation);
            }
            else
            {
                surviverAnimation.mState = SurviverAnimation.MoveState.Idle;
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            currentSpeed = speed;


            verticalVelocity = Grounded ? 0f : -2f;
            // ���������� �÷��̾ �����δ�.
            controller.Move(targetDirection * speed * Time.deltaTime + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        }
        else
        {
            
            ////회전 보정
            //transform.rotation = Quaternion.Lerp(transform.rotation, receiveRot, lerpSpeed * Time.deltaTime);
            ////위치 보정
            //transform.position = Vector3.Lerp(transform.position, receivePos, lerpSpeed * Time.deltaTime);
        }
    }

    float GetTargetSpeed()
    {
        if (isHit)
        {
            return hitSprintSpeed;
        }
        else if(isCrouching)
        {
            return crouchingSpeed;
        }
        else if (isCrawl)
        {
            return crawlSpeed;
        }
        else
        {
            return isSprint ? sprintSpeed : walkSpeed;
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
        if (photonView.IsMine == false)
        {
            //회전 보정
            transform.rotation = receiveRot;
            //위치 보정
            transform.position = receivePos;
        }
    }

    public float cameraDistMultiply = 0.1f;

    private void CameraRotation()
    {
        // ���콺 �Է� �ޱ�
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        cinemachineTargetYaw += mouseX * mouseSensitivity;
        cinemachineTargetPitch += -mouseY * mouseSensitivity;

        // 360���� �����Ѵ�.
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // Ÿ���� ���󰣴�.
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride,
            cinemachineTargetYaw, 0.0f);
    }

    // ���� üũ
    private void GroundedCheck()
    {
        // üũ�� ��ü ��ġ ���ϱ�
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
            transform.position.z);

        // ���� üũ QueryTriggerInteraction.Ignore�� trigger �浹�� �����ϰ� ���ش�.
        Grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
    

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //내 Player 라면
        if (stream.IsWriting)
        {
            //나의 위치값을 보낸다.
            stream.SendNext(transform.position);
            //나의 회전값을 보낸다.
            stream.SendNext(transform.rotation);
            ////h 값 보낸다.
            //stream.SendNext(h);
            ////v 값 보낸다.
            //stream.SendNext(v);
        }
        //내 Player 아니라면
        else
        {
            //위치값을 받자.
            if(anim == false)
            {
                receivePos = (Vector3)stream.ReceiveNext();
            }
            //회전값을 받자.
            receiveRot = (Quaternion)stream.ReceiveNext();
            //h 값 받자.
            //h = (float)stream.ReceiveNext();
            //v 값 받자.
            //v = (float)stream.ReceiveNext();
        }
    }
}
