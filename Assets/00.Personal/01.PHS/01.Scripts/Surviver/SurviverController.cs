using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; //import
using UnityEditor.SceneTemplate;
using System;
using UnityEditor.Experimental.GraphView;

public class SurviverController : MonoBehaviour
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
    private Vector3 surviverVelocity = Vector3.zero;

    // �÷��̾� ���콺 ����
    public float mouseSensitivity = 1f;

    bool isMoving = false;
    bool isSprint = false;
    bool isRotating = false;
    bool isCrouching = false;
    bool isCrawl = false;
    public bool isHit = false;

    public bool Sprint { get { return isSprint; } set { isSprint = value; } }
    public bool Crawl { get { return isCrawl; }  set { isCrawl = value; } }

    // �ִϸ��̼� �Ŵ���
    SurviverAnimation surviverAnimation;
    private string waitAnimState;

    // ET
    public CharacterController controller;
    Camera mainCamera;

    public Animator playerAnimator;

    void Start()
    {
        mainCamera = Camera.main;
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        surviverAnimation = SurviverAnimation.instance;
    }

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();
        Move();
    }

    public float rotationSpeed;
    public float currentSpeed;

    public bool banMove = false;
    private void Move()
    {
        if(banMove) 
        {
            return;
        }

        // ���� �ӷ��� �޸��� ��ư�� ���������� �ȴ������� ������.
        isSprint = Input.GetKey(KeyCode.LeftShift) ? true : false;
        isCrouching = Input.GetKey(KeyCode.LeftControl) ? true : false;

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
            isRotating = rotation == new Vector3(0, transform.eulerAngles.y + deltaAngle, 0) ? false : true;
            playerAnimator.SetBool("IsRotation", isRotating);

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

    bool WaitAnim()
    {
        if(waitAnimState == null)
        {
            return true;
        }
        else if(waitAnimState != null)
        {
            if (surviverAnimation.IsAnimEnd(waitAnimState))
            {
                waitAnimState = null;
                return true;
            }
        }
        return false;
    }
}
