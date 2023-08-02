using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; //import

public class SurviverController : MonoBehaviour
{
    // 플레이어 속도
    [Header("플레이어 속도")]
    public float walkSpeed = 2.26f;
    public float hopeWalkSpeed = 2.42f;
    public float sprintSpeed = 4f;
    public float hopeSprintSpeed = 4.28f;
    public float crouchingSpeed = 1.13f;

    // 카메라 로테이션 속도
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;
    public float speedChangeRate = 10.0f;

    // 플레이어 지면 처리 관련 변수들
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

    // 카메라 저장 변수들
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // 플레이어
    private float speed;
    private float targetSpeed;
    private float targetRotation = 0.0f;
    private float rotationVelocity = 0;
    private float startCameraVelocity = 0;
    private float verticalVelocity = 0;

    // 플레이어 마우스 감도
    public float mouseSensitivity = 1f;
    public bool isSprint = false;
    //private float terminalVelocity = 53.0f;

    // 애니메이션 스피드
    private int animIDSpeed;
    private float animationBlend;

    // 애니메이션 매니저
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
        // 최종 속력을 달리기 버튼을 눌렀을때와 안눌렀을때 나눈다.
        isSprint = Input.GetKey(KeyCode.LeftShift) ? true : false;
        targetSpeed = isSprint ? sprintSpeed : walkSpeed;

        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");

        // 애니메이션에 값을 입력한다
        //playerAnimator.SetFloat("VelocityX", inputVertical);
        //playerAnimator.SetFloat("VelocityZ", inputHorizontal);

        // 인풋이 없을땐 최종 속력을 0으로 만든다.
        if (inputHorizontal == 0 && inputVertical == 0)
        {
            targetSpeed = 0.0f;
        }


        //현재 속도를 받아온다
        // 이건 왜 안댐?
        //float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        // 움직일때 가속과 가속헤제
        if (currentSpeed < targetSpeed - speedOffset ||
           currentSpeed > targetSpeed + speedOffset)
        {
            speed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);

            // 소숫점 3자리수까지 자르기
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        //애니메이션에 속도에 맞는 부드러움 주기
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (animationBlend < 0.01f) animationBlend = 0f;

        // 입력된 방향을 정규화 한다.
        Vector3 inputDirection = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;

        // 움직일때 정면을 부드럽게 넘겨준다.
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

            // 얼굴을 카메라 포지션에 맞게 회전한다. (에임 카메라 아닐때만)
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        currentSpeed = speed;

        verticalVelocity = Grounded ? 0f : -2f;

        // 최종적으로 플레이어를 움직인다.
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
        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        cinemachineTargetYaw += mouseX * mouseSensitivity;
        cinemachineTargetPitch += -mouseY * mouseSensitivity;

        // 360도로 제한한다.
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // 타겟을 따라간다.
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride,
            cinemachineTargetYaw, 0.0f);
    }

    // 지면 체크
    private void GroundedCheck()
    {
        // 체크할 구체 위치 정하기
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
            transform.position.z);

        // 지면 체크 QueryTriggerInteraction.Ignore는 trigger 충돌을 무시하게 해준다.
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
