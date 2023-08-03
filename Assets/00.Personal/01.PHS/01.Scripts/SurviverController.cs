using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; //import
using UnityEditor.SceneTemplate;

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

    // 애니메이션 상태
    public enum State
    {
        StandIdle,
        StandWalk,
        StandRun,
    }

    public State state = State.StandIdle;

    // 카메라 저장 변수들
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // 플레이어
    private float speed;
    private float targetSpeed;
    private float targetRotation = 0.0f;
    private float verticalVelocity = 0;
    private float moveVelocity = 0;

    // 플레이어 마우스 감도
    public float mouseSensitivity = 1f;
    public bool isSprint = false;
    public bool isRotating = false;
    public bool isCrouch = false;
    //private float terminalVelocity = 53.0f;

    // 애니메이션 스피드
    private int animIDSpeed;

    // 애니메이션 매니저
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
        // 최종 속력을 달리기 버튼을 눌렀을때와 안눌렀을때 나눈다.
        isSprint = Input.GetKey(KeyCode.LeftShift) ? true : false;
        isCrouch = Input.GetKey(KeyCode.LeftControl) ? true : false;
        targetSpeed = isSprint ? sprintSpeed : walkSpeed;
        targetSpeed = isRotating ? walkSpeed : targetSpeed;

        playerAnimator.SetBool("IsCrouch", isCrouch);

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
        speed = Mathf.MoveTowards(speed, targetSpeed, Time.deltaTime * speedChangeRate);
        //speed = Mathf.SmoothDamp(speed, targetSpeed, ref moveVelocity, speedChangeRate);


        ////애니메이션에 속도에 맞는 부드러움 주기
        //animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        //if (animationBlend < 0.01f) animationBlend = 0f;

        // 입력된 방향을 정규화 한다.
        Vector3 inputDirection = new Vector3(inputHorizontal, 0.0f, inputVertical).normalized;

        // 움직일때 정면을 부드럽게 넘겨준다.
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

            // 얼굴을 카메라 포지션에 맞게 회전한다. (에임 카메라 아닐때만)
            transform.rotation = Quaternion.Euler(rotation);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        verticalVelocity = Grounded ? 0f : -2f;

        // 최종적으로 플레이어를 움직인다.
        controller.Move(targetDirection * (speed * Time.deltaTime) + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        playerAnimator.SetFloat(animIDSpeed, speed);
    }

    void MoveAnimation()
    {
        switch (state)
        {
            // 기본 상태
            case State.StandIdle:
                surviverAnimationMgr.Play("StandIdle");
                break;

            // 걷기 상태
            case State.StandWalk:
                surviverAnimationMgr.Play("StandingToStandWalk");
                break;

            // 달리기
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
