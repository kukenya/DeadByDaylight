using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; //import

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

    // ī�޶� ���� ������
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // �÷��̾�
    private float speed;
    private float targetSpeed;
    private float targetRotation = 0.0f;
    private float rotationVelocity = 0;
    private float startCameraVelocity = 0;
    private float verticalVelocity = 0;

    // �÷��̾� ���콺 ����
    public float mouseSensitivity = 1f;
    public bool isSprint = false;
    //private float terminalVelocity = 53.0f;

    // �ִϸ��̼� ���ǵ�
    private int animIDSpeed;
    private float animationBlend;

    // �ִϸ��̼� �Ŵ���
    SurviverAnimationMgr surviverAnimationMgr;

    // ET
    CharacterController controller;
    Camera mainCamera;

    private float currentSpeed;

    void Start()
    {
        startCameraVelocity = 0;
        mainCamera = Camera.main;
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        controller = GetComponent<CharacterController>();
        surviverAnimationMgr = GetComponent<SurviverAnimationMgr>();
        animIDSpeed = Animator.StringToHash("Speed");
    }

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();
        Move();
    }

    float rotation;
    public float rotationSpeed;
    private void Move()
    {
        // ���� �ӷ��� �޸��� ��ư�� ���������� �ȴ������� ������.
        isSprint = Input.GetKey(KeyCode.LeftShift) ? true : false;
        targetSpeed = isSprint ? sprintSpeed : walkSpeed;

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


        //���� �ӵ��� �޾ƿ´�
        // �̰� �� �ȴ�?
        //float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

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

        //�ִϸ��̼ǿ� �ӵ��� �´� �ε巯�� �ֱ�
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (animationBlend < 0.01f) animationBlend = 0f;

        // �Էµ� ������ ����ȭ �Ѵ�.
        Vector3 inputDirection = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;

        // �����϶� ������ �ε巴�� �Ѱ��ش�.
        if (inputDirection != Vector3.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                mainCamera.transform.eulerAngles.y;
            float deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.y, targetRotation);

            if(deltaAngle > 1)
            {
                rotation += rotationSpeed * Time.deltaTime;
            }
            else if (deltaAngle < -1)
            {
                rotation -= rotationSpeed * Time.deltaTime;
            }
            else
            {
                rotation = Mathf.Lerp(rotation, deltaAngle, Time.deltaTime);
            }
            //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

            // ���� ī�޶� �����ǿ� �°� ȸ���Ѵ�. (���� ī�޶� �ƴҶ���)
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        currentSpeed = speed;

        verticalVelocity = Grounded ? 0f : -2f;

        // ���������� �÷��̾ �����δ�.
        controller.Move(targetDirection * (speed * Time.deltaTime) + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        //playerAnimator.SetFloat(animIDSpeed, animationBlend);
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
}
