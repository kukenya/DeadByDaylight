using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SurvivorInteraction : MonoBehaviourPun
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
            switch (interactiveType)
            {
                case InteractiveType.None:
                    window = null;
                    pallet = null;
                    generator = null;
                    exit = null;
                    break;
                case InteractiveType.Window:
                    pallet = null;
                    generator = null;
                    exit = null;
                    break;
                case InteractiveType.Pallet:
                    window = null;
                    generator = null;
                    exit = null;
                    break;
                case InteractiveType.Generator:
                    window = null;
                    pallet = null;
                    exit = null;
                    break;
                case InteractiveType.ExitLever:
                    window = null;
                    generator = null;
                    pallet = null;
                    break;
            }
        } 
    }

    SurviverHealing surviverHealing;
    SurviverAnimation surviverAnimation;
    CharacterController controller;
    public bool Controller { get { return controller.enabled; } set { photonView.RPC(nameof(SetController), RpcTarget.All, value); } }

    [PunRPC]
    void SetController(bool value)
    {
        controller.enabled = value; surviverController.anim = !value;
    }

    SurviverController surviverController;
    SurviverAutoMove surviverAutoMove;
    SurviverLookAt surviverLookAt;
    SurviverUI ui;

    public Pallet pallet;
    public Window window;
    public Generator generator;
    public Exit exit;

    public Pallet Pallet { get { return pallet; } set { if (pallet == value) return; pallet = value; Type = InteractiveType.Pallet; } }
    public Window Window { get { return window; } set { if (window == value) return; window = value; Type = InteractiveType.Window; } }
    public Generator Generator { get { return generator; } set { if (generator == value) return; generator = value; Type = InteractiveType.Generator; } }
    public Exit Exit { get { return exit; } set { if (exit == value) return; exit = value; Type = InteractiveType.ExitLever; } }

    public bool activate = false;

    private void Start()
    {
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
        UpdateSurvivorUI();
    }
    
    public void UpdateSurvivorUI()
    {
        if (photonView.IsMine == false) return;

        ui.UnFocusSpaceBarUI();
        ui.OffFocusProgressUI();

        switch (Type)
        {
            case InteractiveType.None:
                break;
            case InteractiveType.Window:
                ui.FocusSpaceBarUI();
                break;

            case InteractiveType.Pallet:
                ui.FocusSpaceBarUI();
                break;

            case InteractiveType.Generator:
                if (generator.Repair) ui.ChangePrograssUI(SurviverUI.PrograssUI.On, "수리");
                else ui.ChangePrograssUI(SurviverUI.PrograssUI.Focus, "수리");
                ui.prograssBar.fillAmount = generator.Prograss / generator.maxPrograssTime;
                break;

            case InteractiveType.ExitLever:
                if (exit.activate) ui.ChangePrograssUI(SurviverUI.PrograssUI.On, "출구");
                else ui.ChangePrograssUI(SurviverUI.PrograssUI.Focus, "출구");
                ui.prograssBar.fillAmount = exit.Prograss / exit.maxPrograssTime;
                break;

            case InteractiveType.SelfHeal:
                if (surviverHealing.healing) ui.ChangePrograssUI(SurviverUI.PrograssUI.On, "자가 치료");
                else ui.ChangePrograssUI(SurviverUI.PrograssUI.Focus, "자가 치료");
                ui.prograssBar.fillAmount = surviverHealing.Prograss / surviverHealing.maxPrograssTime;
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

    void UpdateInteractionInput()
    {
        if (photonView.IsMine == false) return;

        if (activate == true) return;

        switch (interactiveType)
        {
            case InteractiveType.None:       
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
                if (Input.GetMouseButtonDown(0))
                {
                    surviverHealing.OnSelfHeal(this);
                }
                else if (Input.GetMouseButtonUp(0))
                {
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

    public void StartJumpWindow()
    {
        surviverLookAt.LookAt = false;
        Transform targetTrans = window.GetAnimPosition(transform);
        Vector3 targetDir = targetTrans.position - transform.position;
        float targetAngle = Vector3.Angle(transform.forward, new Vector3(targetDir.x, 0, targetDir.z));
        if (targetAngle < midJumpAngle)
        {
            surviverAutoMove.OnAutoMove(targetTrans, JumpWindow, targetAngle);
        }
        else
        {
            surviverAutoMove.OnAutoMove(targetTrans, JumpWindow, 80f);
        }
    }

    public void InteractivePallet()
    {
        surviverLookAt.LookAt = false;
        Transform targetTrans = pallet.GetAnimPosition(transform.position);
        switch (pallet.state)
        {
            case Pallet.PalletState.Stand:
                surviverAutoMove.OnAutoMove(targetTrans, PullDownPallet);
                break;
            case Pallet.PalletState.Ground:
                if (targetTrans.localEulerAngles.y == 90) surviverAutoMove.OnAutoMove(targetTrans, JumpPalletLT, true);
                else surviverAutoMove.OnAutoMove(targetTrans, JumpPalletRT, true);
                break;
        }
    }

    [Header("창문 점프 각도")]
    public float fastJumpAngle = 50f;
    public float midJumpAngle = 90f;

    void JumpWindow(float targetAngle)
    {
        Controller = false;
        print(surviverController.anim);
        if (photonView.IsMine == false) 

        if (surviverController.sprintTime == 0)
        {
            surviverAnimation.Play("WindowIn");
            StartCoroutine(WaitAnimEnd("WindowJump"));
            return;
        }

        if (targetAngle < fastJumpAngle && surviverController.sprintTime >= surviverController.maxSprintTime)
        {
            surviverAnimation.Play("WindowFast");
            StartCoroutine(WaitAnimEnd("WindowFast"));
            return;
        }

        surviverAnimation.Play("WindowMid");
        StartCoroutine(WaitAnimEnd("WindowMid"));

        
        //else if (targetAngle < midJumpAngle)
        //{
        //    surviverAnimation.Play("WindowMid");
        //    StartCoroutine(WaitAnimEnd("WindowMid"));
        //}
    }

    void JumpPalletRT()
    {
        Controller = false;
        if (surviverController.Sprint == false)
        {
            surviverAnimation.Play("JumpPalletRT");
            StartCoroutine(WaitAnimEnd("JumpPalletRT"));
            return;
        }

        surviverAnimation.Play("JumpPalletFastRT");
        StartCoroutine(WaitAnimEnd("JumpPalletFastRT"));
    }

    void JumpPalletLT()
    {
        Controller = false;
        if (surviverController.Sprint == false)
        {
            surviverAnimation.Play("JumpPalletLT");
            StartCoroutine(WaitAnimEnd("JumpPalletLT"));
            return;
        }

        surviverAnimation.Play("JumpPalletFastLT");
        StartCoroutine(WaitAnimEnd("JumpPalletFastLT"));
    }

    void PullDownPallet()
    {
        surviverAnimation.Play("PullDownPalletRT");
        pallet.State = Pallet.PalletState.Ground;
        StartCoroutine(WaitAnimEnd("PullDownPalletRT"));
    }

    public float a;

    Coroutine cor;

    public void StartFowardMoveSprint(string animName)
    {
        if(photonView.IsMine == false) { return; }
        cor = StartCoroutine(FowardMoveCor(animName, 4));
    }

    public void StartFowardMoveWalk(string animName)
    {
        if (photonView.IsMine == false) { return; }
        cor = StartCoroutine(FowardMoveCor(animName, 2.26f));
    }

    public void StopFowardMove()
    {
        if (photonView.IsMine == false) { return; }
        StopCoroutine(cor);
    }

    IEnumerator FowardMoveCor(string animName, float speed)
    {
        while (true)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            if (surviverAnimation.IsAnimEnd(animName)) break;
            yield return null;
        }
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
        Controller = true;
        surviverAnimation.AnimationChange();
        activate = false;
        surviverLookAt.LookAt = true;
    }


    // 진행바 

    public void OnRepairGen()
    {
        Transform targetTrans = generator.GetAnimationPos(transform.position);
        surviverAutoMove.OnAutoMove(targetTrans, OnRepairAnim);
        surviverLookAt.LookAt = false;
    }

    public void OnRepairAnim()
    {
        surviverController.BanMove = true;
        generator.Repair = true;
        generator.interaction = this;
        surviverAnimation.Play("Generator_Idle_FT");
    }

    public void OffRepairGen()
    {
        surviverAutoMove.StopCoroutine();
        surviverController.BanMove = false;
        generator.Repair = false;
        Controller = true;
        surviverLookAt.LookAt = true;
    }

    public void ActivateExit()
    {
        Transform targetTrans = exit.GetAnimationPos(transform.position);
        exit.interaction = this;
        surviverAutoMove.OnAutoMove(targetTrans, ActivateExitAnim);
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
        Controller = true;
        surviverLookAt.LookAt = true;
        exit.OffSwitch();
    }

    public void GeneratorFail()
    {
         StartCoroutine(GeneratorFailCor()); 
    }

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
