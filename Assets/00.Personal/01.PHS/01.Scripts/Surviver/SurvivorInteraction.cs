using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorInteraction : MonoBehaviour
{
    public enum InteractiveType
    {
        None,
        Window,
        Pallet,
        Generator,
        ExitLever,
        SelfHeal,
    }

    [SerializeField]
    InteractiveType interactiveType;

    public InteractiveType Type { get { return interactiveType; } set {
            interactiveType = value; 
        } 
    }
    Transform animationPos;

    public Transform Position { get { return animationPos; } set {  animationPos = value; } }

    SurviverHealing surviverHealing;
    SurviverHealth health;
    SurviverAnimation surviverAnimation;
    CharacterController controller;
    SurviverController surviverController;
    SurviverAutoMove surviverAutoMove;
    SurviverLookAt surviverLookAt;
    SurviverUI ui;

    public bool activate = false;

    private void Start()
    {
        health = GetComponent<SurviverHealth>();
        surviverAnimation = GetComponent<SurviverAnimation>();
        controller = GetComponent<CharacterController>();
        surviverController = GetComponent<SurviverController>();
        surviverAutoMove = GetComponent<SurviverAutoMove>();
        surviverLookAt = GetComponent<SurviverLookAt>();
        surviverHealing = GetComponent<SurviverHealing>();
        ui = SurviverUI.instance;
    }

    public void Update()
    {
        UpdateInteractionInput();
        CancelInteract();
    }

    public void CancelInteract()
    {
        switch (interactiveType)
        {
            case InteractiveType.None:
                break;
            case InteractiveType.Window:
                if(Vector3.Distance(transform.position, window.transform.position) > window.GetComponent<SphereCollider>().radius + 0.2f)
                {
                    interactiveType = InteractiveType.None;
                    window.OnTriggerExitMethod();
                    window = null;
                }
                break;
        }
    }

    public void EndInteract(InteractiveType type)
    {
        switch (type)
        {
            case InteractiveType.Generator:
                OffRepairGen();
                break;
            case InteractiveType.ExitLever:
                DeActivateExit();
                break;
        }
        Type = InteractiveType.None;
    }

    public void ChangeInteract(InteractiveType type, MonoBehaviour interact = null, Transform animationPos = null)
    {
        switch (type)
        {
            case InteractiveType.None:
                Type = InteractiveType.None;
                break;
            case InteractiveType.Window:
                Type = InteractiveType.Window;
                window = (Window)interact;
                this.animationPos = animationPos;
                break;
            case InteractiveType.Pallet:
                Type = InteractiveType.Pallet;
                pallet = (Pallet)interact;
                this.animationPos = animationPos;
                break;
            case InteractiveType.Generator:
                Type = InteractiveType.Generator;
                generator = (Generator)interact;
                this.animationPos = animationPos;
                break;
            case InteractiveType.ExitLever:
                Type = InteractiveType.ExitLever;
                exit = (Exit)interact;
                this.animationPos = animationPos;
                break;
            case InteractiveType.SelfHeal:
                Type = InteractiveType.SelfHeal;
                break;
        }
    }

    void UpdateInteractionInput()
    {
        if (activate == true) return;

        switch (interactiveType)
        {
            case InteractiveType.None:
                if (health.state == SurviverHealth.HealthState.Injured)
                {
                    print(surviverController.Moving);
                    
                    if(surviverController.Moving == false)
                    {
                        interactiveType = InteractiveType.SelfHeal;
                        ui.FocusProgressUI("치료");
                    }
                }
                
                break;
            case InteractiveType.Window:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartJumpWindow();
                    activate = true;
                }
                break;
            case InteractiveType.Pallet:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    InteractivePallet();
                    activate = true;
                }
                break;
            case InteractiveType.Generator:
                if (Input.GetMouseButtonDown(0))
                {
                    OnRepairGen();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    OffRepairGen();
                }
                break;
            case InteractiveType.ExitLever:
                if (Input.GetMouseButtonDown(0))
                {
                    ActivateExit();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    DeActivateExit();
                }
                break;
            case InteractiveType.SelfHeal:
                if (surviverController.Moving)
                {
                    interactiveType = InteractiveType.None;
                    ui.UnFocusProgressUI();
                }
                if (Input.GetMouseButtonDown(0))
                {
                    ui.OnProgressUI();
                    surviverHealing.OnSelfHeal();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    ui.FocusProgressUI("치료");
                    surviverHealing.OffSelfHeal();
                }
                break;
        }
    }

    // 장애물

    [Header("장애물 관련")]
    public float possibleAngle = 120f;
    public float autoMoverStopDistance = 0.2f;
    public float autoMoverSpeed = 4.0f;
    public Transform currentObstacleTrans;

    public Transform Obstacle { get { return currentObstacleTrans; } set { currentObstacleTrans = value; } }

    public Pallet pallet;
    public Window window;

    public void StartJumpWindow()
    {
        surviverLookAt.LookAt = false;
        Vector3 targetDir = animationPos.position - transform.position;
        float targetAngle = Vector3.Angle(transform.forward, new Vector3(targetDir.x, 0, targetDir.z));
        if (targetAngle < midJumpAngle)
        {
            surviverAutoMove.OnAutoMove(animationPos, JumpWindow, targetAngle);
        }
        else
        {
            surviverAutoMove.OnAutoMove(animationPos, JumpWindow, 80f);
        }
    }

    public void InteractivePallet()
    {
        surviverLookAt.LookAt = false;
        switch (pallet.state)
        {
            case Pallet.State.Stand:
                surviverAutoMove.OnAutoMove(animationPos, PullDownPallet);
                pallet.state = Pallet.State.Ground;
                break;
            case Pallet.State.Ground:
                surviverAutoMove.OnAutoMove(animationPos, JumpPallet, true);
                break;
        }

    }

    [Header("창문 점프 각도")]
    public float fastJumpAngle = 50f;
    public float midJumpAngle = 90f;

    void JumpWindow(float targetAngle)
    {
        controller.enabled = false;

        if (surviverController.Sprint == false)
        {
            surviverAnimation.Play("WindowIn");
            StartCoroutine(WaitAnimEnd("WindowJump"));
            return;
        }

        if (targetAngle < fastJumpAngle)
        {
            surviverAnimation.Play("WindowFast");
            StartCoroutine(WaitAnimFast());
        }
        else if (targetAngle < midJumpAngle)
        {
            surviverAnimation.Play("WindowMid");
            StartCoroutine(WaitAnimEnd("WindowMid"));
        }

    }

    void JumpPallet()
    {
        controller.enabled = false;
        if (surviverController.Sprint == false)
        {
            surviverAnimation.Play("JumpPallet");
            StartCoroutine(WaitAnimEnd("JumpPallet"));
            return;
        }

        surviverAnimation.Play("JumpPalletFast");
        StartCoroutine(WaitAnimEnd("JumpPalletFast"));
    }

    void PullDownPallet()
    {
        surviverAnimation.Play("PullDownPalletRT");
        pallet.Play("FallOnGround");
        StartCoroutine(WaitAnimEnd("PullDownPalletRT"));
    }

    public float a;

    IEnumerator WaitAnimFast()
    {
        surviverController.BanMove = true;
        float currentTime = 0;
        while (true)
        {
            currentTime += Time.deltaTime;
            if (currentTime > a)
            {
                transform.position += transform.forward * 4.0f * Time.deltaTime;
            }
            if (surviverAnimation.IsAnimEnd("WindowFast")) break;
            yield return null;
        }
        surviverController.BanMove = false;
        controller.enabled = true;
        surviverAnimation.AnimationChange();
        activate = false;
        surviverLookAt.LookAt = true;
    }

    IEnumerator WaitAnimEnd(string animName)
    {
        surviverController.BanMove = true;
        while (true)
        {
            if (surviverAnimation.IsAnimEnd(animName)) break;
            yield return null;
        }
        surviverController.BanMove = false;
        controller.enabled = true;
        surviverAnimation.AnimationChange();
        activate = false;
        surviverLookAt.LookAt = true;
    }


    // 진행바 
    Generator generator;
    Exit exit;

    public void OnRepairGen()
    {
        surviverAutoMove.OnAutoMove(animationPos, OnRepairAnim);
        surviverLookAt.LookAt = false;
    }

    public void OnRepairAnim()
    {
        surviverController.BanMove = true;
        generator.Repair = true;
        surviverAnimation.Play("Generator_Idle_FT");
    }

    public void OffRepairGen()
    {
        surviverAutoMove.StopCoroutine();
        surviverController.BanMove = false;
        generator.Repair = false;
        controller.enabled = true;
        surviverLookAt.LookAt = true;
    }

    public void ActivateExit()
    {
        surviverAutoMove.OnAutoMove(animationPos, ActivateExitAnim);
        surviverLookAt.LookAt = false;
    }

    public void ActivateExitAnim()
    {
        surviverController.BanMove = true;
        surviverAnimation.Play("UnlockExit");
        exit.OnSwitch();
    }

    public void DeActivateExit()
    {
        surviverAutoMove.StopCoroutine();
        surviverController.BanMove = false;
        controller.enabled = true;
        surviverLookAt.LookAt = true;
        exit.OffSwitch();
    }

    public void GeneratorFail()
    {
         StartCoroutine(GeneratorFailCor());    }

    IEnumerator GeneratorFailCor()
    {
        surviverAnimation.Play("Generator_Fail_FT", 0.1f, true);
        generator.Repair = false;
        while (true)
        {
            if (surviverAnimation.IsAnimEnd("Generator_Fail_FT")) break;
            yield return null;
        }
        generator.Repair = true;
        surviverAnimation.Play("Generator_Idle_FT");
    }
}
