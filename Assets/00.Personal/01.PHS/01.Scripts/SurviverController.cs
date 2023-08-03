using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; //import
using UnityEditor.SceneTemplate;

public class SurviverController : MonoBehaviour
{
    // �÷��̾� �ӵ�
    [Header("�÷��̾� �ӵ�")]
    public float walkSpeed = 2.26f;
    public float hopeWalkSpeed = 2.42f;
    public float sprintSpeed = 4f;
    public float hopeSprintSpeed = 4.28f;
    public float crouchingSpeed = 1.13f;

    // ī�޶� �����̼� �ӵ�
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;
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

    // �ִϸ��̼� ����
    public enum State
    {
        StandIdle,
        StandWalk,
        StandRun,
    }

    public State state = State.StandIdle;

    // ī�޶� ���� ������
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // �÷��̾�
    private float speed;
    private float targetSpeed;
    private float targetRotation = 0.0f;
    private float verticalVelocity = 0;
    private float moveVelocity = 0;

    // �÷��̾� ���콺 ����
    public float mouseSensitivity = 1f;
    public bool isSprint = false;
    public bool isRotating = false;
    public bool isCrouch = false;
    //private float terminalVelocity = 53.0f;

    // �ִϸ��̼� ���ǵ�
    private int animIDSpeed;

    // �ִϸ��̼� �Ŵ���
    public SurviverAnimationMgr surviverAnimationMgr;
    private string waitAnimState;

    // ET
    CharacterController controller;
    Camera mainCamera;

    public Animator playerAnimator;

    void Start()
    {
        mainCamera = Camera.main;
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        controller = GetComponent<CharacterController>();
        animIDSpeed = Animator.StringToHash("Speed");
    }

    // Update is called once per frame
    void Update()
    {
        //MoveAnimation();
        GroundedCheck();
        Move();
    }

    public float rotationSpeed;
    private void Move()
    {
        // ���� �ӷ��� �޸��� ��ư�� ���������� �ȴ������� ������.
        isSprint = Input.GetKey(KeyCode.LeftShift) ? true : false;
        isCrouch = Input.GetKey(KeyCode.LeftControl) ? true : false;
        targetSpeed = isSprint ? sprintSpeed : walkSpeed;
        targetSpeed = isRotating ? walkSpeed : targetSpeed;

        playerAnimator.SetBool("IsCrouch", isCrouch);

        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");

        // �ִϸ��̼ǿ� ���� �Է��Ѵ�
        //playerAnimator.SetFloat("VelocityX", inputVertical);
        //playerAnimator.SetFloat("VelocityZ", inputHorizontal);

        // ��ǲ�� ������ ���� �ӷ��� 0���� �����.
        if (inputHorizontal == 0 && inputVertical == 0)
        {
            targetSpeed = 0.0f;
        }
        speed = Mathf.MoveTowards(speed, targetSpeed, Time.deltaTime * speedChangeRate);
        //speed = Mathf.SmoothDamp(speed, targetSpeed, ref moveVelocity, speedChangeRate);


        ////�ִϸ��̼ǿ� �ӵ��� �´� �ε巯�� �ֱ�
        //animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        //if (animationBlend < 0.01f) animationBlend = 0f;

        // �Էµ� ������ ����ȭ �Ѵ�.
        Vector3 inputDirection = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;

        // �����϶� ������ �ε巴�� �Ѱ��ش�.
        if (inputDirection != Vector3.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                mainCamera.transform.eulerAngles.y;
            float deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.y, targetRotation);
            //print("1");
            //if(deltaAngle > 1)
            //{
            //    print("2");
            //    rotation += rotationSpeed * Time.deltaTime;
            //}
            //else if (deltaAngle < -1)
            //{
            //    rotation -= rotationSpeed * Time.deltaTime;
            //}
            //else
            //{
            //    rotation = Mathf.Lerp(rotation, deltaAngle, Time.deltaTime);
            //}
            Vector3 rotation = Vector3.MoveTowards(new Vector3(0, transform.eulerAngles.y, 0), new Vector3(0, transform.eulerAngles.y + deltaAngle, 0), rotationSpeed * Time.deltaTime);
            isRotating = rotation == new Vector3(0, transform.eulerAngles.y + deltaAngle, 0) ? false : true;
            playerAnimator.SetBool("IsRotation", isRotating);

            // ���� ī�޶� �����ǿ� �°� ȸ���Ѵ�. (���� ī�޶� �ƴҶ���)
            transform.rotation = Quaternion.Euler(rotation);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        verticalVelocity = Grounded ? 0f : -2f;

        // ���������� �÷��̾ �����δ�.
        controller.Move(targetDirection * (speed * Time.deltaTime) + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        playerAnimator.SetFloat(animIDSpeed, speed);
    }

    void MoveAnimation()
    {
        switch (state)
        {
            // �⺻ ����
            case State.StandIdle:
                surviverAnimationMgr.Play("StandIdle");
                break;

            // �ȱ� ����
            case State.StandWalk:
                surviverAnimationMgr.Play("StandingToStandWalk");
                break;

            // �޸���
            case State.StandRun:
                surviverAnimationMgr.Play("StandRun");
                break;
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
            if (surviverAnimationMgr.IsAnimEnd(waitAnimState))
            {
                waitAnimState = null;
                return true;
            }
        }
        return false;
    }
}
