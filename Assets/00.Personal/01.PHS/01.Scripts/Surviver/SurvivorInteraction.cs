using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

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
        HookEscape,
        HealCamper,
        EscapeCamper
    }

    [SerializeField]
    InteractiveType interactiveType;

    public InteractiveType Type { get { return interactiveType; } set {
            interactiveType = value;
        } 
    }

    SurviverHealing surviverHealing;
    SurviverAnimation surviverAnimation;
    SurviverController surviverController;
    SurviverAutoMove surviverAutoMove;
    SurviverLookAt surviverLookAt;
    SurviverUI ui;
    SurviverHealth health;

    public Window window;
    public Pallet pallet;
    public Generator generator;
    public Exit exit;
    public SurviverHealing camperHealing;
    public SurvivorHookEscape camperEscape;

    bool activate = false;
    public bool Activate { get { return activate; } set {
            activate = value;
            if(value == true)
            {
                surviverLookAt.LookAt = false;
            }
            else
            {
                surviverLookAt.LookAt = true;
            }
        } 
    }

    public void NullInteractScript()
    {
        if(window != null) window = null;
        if(pallet != null) pallet = null;
        if(generator != null) generator = null;
        if(camperHealing != null) camperHealing = null;
    }

    #region 유니티
    private void Start()
    {
        health = GetComponent<SurviverHealth>();
        surviverAnimation = GetComponent<SurviverAnimation>();
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
    #endregion

    public bool offAutoUI = false;
    #region UI업데이트
    public void UpdateSurvivorUI()
    {
        if (photonView.IsMine == false) return;


        ui.UnFocusSpaceBarUI();
        ui.OffFocusProgressUI();

        if (offAutoUI) return;

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
                if (surviverHealing.SelfHeal) ui.ChangePrograssUI(SurviverUI.PrograssUI.On, "자가 치료");
                else ui.ChangePrograssUI(SurviverUI.PrograssUI.Focus, "자가 치료");
                ui.prograssBar.fillAmount = surviverHealing.Prograss / surviverHealing.maxPrograssTime;
                break;
            case InteractiveType.HookEscape:
                if (health.Escape) ui.ChangePrograssUI(SurviverUI.PrograssUI.On, "탈출");
                else ui.ChangePrograssUI(SurviverUI.PrograssUI.Focus, "탈출");
                break;
            case InteractiveType.HealCamper:
                if (camperHealing.OtherHealing) ui.ChangePrograssUI(SurviverUI.PrograssUI.On, "치료");
                else ui.ChangePrograssUI(SurviverUI.PrograssUI.Focus, "치료");
                ui.prograssBar.fillAmount = camperHealing.Prograss / camperHealing.maxPrograssTime;
                break;
            case InteractiveType.EscapeCamper:
                if(camperEscape.Escape) ui.ChangePrograssUI(SurviverUI.PrograssUI.On, "구출");
                else ui.ChangePrograssUI(SurviverUI.PrograssUI.Focus, "구출");
                ui.prograssBar.fillAmount = camperEscape.Prograss / camperEscape.maxPrograssTime;
                break;
        }
    }
    #endregion

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

    #region 인풋
    void UpdateInteractionInput()
    {
        if (photonView.IsMine == false) return;

        if (Activate == true) return;

        switch (interactiveType)
        {
            case InteractiveType.None:       
                break;
            case InteractiveType.Window:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartJumpWindow();
                }
                break;
            case InteractiveType.Pallet:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    InteractivePallet(); 
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
                    surviverHealing.OnSelfHeal();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    surviverHealing.OffSelfHeal();
                }
                break;
            case InteractiveType.HookEscape:
                if (Input.GetMouseButtonDown(0))
                {
                    health.Escape = true;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    health.Escape = false;
                }
                break;
            case InteractiveType.HealCamper:
                if (Input.GetMouseButtonDown(0))
                {
                    FriendHealing();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    OffFriendHealing();
                }
                break;
            case InteractiveType.EscapeCamper:
                if (Input.GetMouseButtonDown(0))
                {
                    CamperEscape();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    OffCamperEscape();
                }
                break;
        }
    }
    #endregion

    Coroutine escapeCor;
    public void CamperEscape()
    {
        if (escapeCor != null) return;
        print("온");
        escapeCor = StartCoroutine(surviverAutoMove.FriendHealingAutoMoveCor(
            camperEscape.OnEscaping,
            () => { surviverAnimation.Play("RescueCamperIn"); escapeCor = null; },
            this,
            camperEscape.transform)
            );
    }

    public void OffCamperEscape(bool escaped = false)
    {
        if (escaped == true) { StartCoroutine(CamperEscapingCor()); return; }

        print("오프");
        if (escapeCor != null)
        {
            StopCoroutine(escapeCor);
            escapeCor = null;
        }
        surviverController.BanMove = false;
        camperEscape.OffEscaping();
        surviverAnimation.AnimationChange();
    }

    IEnumerator CamperEscapingCor()
    {
        surviverAnimation.Play("RescueCamperEnd");
        while (true)
        {
            if(surviverAnimation.IsAnimEnd("RescueCamperEnd")) break;
            yield return null;
        }
        if (escapeCor != null)
        {
            StopCoroutine(escapeCor);
            escapeCor = null;
        }
        surviverController.BanMove = false;
        camperEscape.OffEscaping();
        surviverAnimation.AnimationChange();
    }


    Coroutine friendHealCor;
    void FriendHealing()
    {
        if (friendHealCor != null) return;
        friendHealCor = StartCoroutine(surviverAutoMove.FriendHealingAutoMoveCor(
            camperHealing.OnFriendHeal, 
            () => { surviverAnimation.Play("Heal_Camper"); friendHealCor = null; },
            this,
            camperHealing.transform)
            );
    }

    public void OffFriendHealing()
    {
        if (friendHealCor != null)
        {
            StopCoroutine(friendHealCor);
            friendHealCor = null;
        }
        surviverController.BanMove = false;
        camperHealing?.OffFriendHeal();
        surviverAnimation.AnimationChange();
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
        Activate = true;
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
        Activate = true;
        Transform targetTrans = pallet.GetAnimPosition(transform.position);
        switch (pallet.state)
        {
            case Pallet.PalletState.Stand:
                surviverAutoMove.OnAutoMove(targetTrans, 
                    () => {
                    surviverAnimation.Play("PullDownPalletRT");
                    pallet.State = Pallet.PalletState.Ground;
                    StartCoroutine(WaitAnimEnd("PullDownPalletRT"));
                }
                );
                break;
            case Pallet.PalletState.Ground:
                if (targetTrans.localEulerAngles.y == 90) surviverAutoMove.OnAutoMove(targetTrans, 
                    () => {
                        if (surviverController.Sprint == false)
                        {
                            surviverAnimation.Play("JumpPalletLT");
                            StartCoroutine(WaitAnimEnd("JumpPalletLT"));
                            return;
                        }

                    surviverAnimation.Play("JumpPalletFastLT");
                    StartCoroutine(WaitAnimEnd("JumpPalletFastLT"));
                }, true);
                else surviverAutoMove.OnAutoMove(targetTrans, 
                    () => {
                        if (surviverController.Sprint == false)
                        {
                            surviverAnimation.Play("JumpPalletRT");
                            StartCoroutine(WaitAnimEnd("JumpPalletRT"));
                            return;
                        }

                    surviverAnimation.Play("JumpPalletFastRT");
                    StartCoroutine(WaitAnimEnd("JumpPalletFastRT"));
                }, true);
                break;
        }
    }

    [Header("창문 점프 각도")]
    public float fastJumpAngle = 50f;
    public float midJumpAngle = 90f;

    void JumpWindow(float targetAngle)
    {
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
    }

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
        surviverAnimation.AnimationChange();
        Activate = false;
    }


    // 진행바 

    public void OnRepairGen()
    {
        Transform targetTrans = generator.GetAnimationPos(transform.position);
        surviverAutoMove.OnAutoMove(targetTrans, () => {
            surviverController.BanMove = true;
            generator.Repair = true;
            generator.interaction = this;
            surviverAnimation.Play("Generator_Idle_FT");
        });
    }

    public void OffRepairGen()
    {
        surviverAutoMove.StopCoroutine();
        surviverController.BanMove = false;
        generator.Repair = false;
        generator.interaction = null;
    }

    public void ActivateExit()
    {
        Transform targetTrans = exit.GetAnimationPos(transform.position);
        exit.interaction = this;
        surviverAutoMove.OnAutoMove(targetTrans, () => {
            surviverController.BanMove = true;
            surviverAnimation.Play("UnlockExit");
            exit.OnSwitch();
        });
    }

    public void DeActivateExit()
    {
        surviverAutoMove.StopCoroutine();
        surviverController.BanMove = false;
        exit.OffSwitch();
    }

    public void GeneratorFail()
    {
         StartCoroutine(GeneratorFailCor()); 
    }

    IEnumerator GeneratorFailCor()
    {
        generator.Fail = true;
        surviverAnimation.Play("Generator_Fail_FT", 0.1f, 0, true);
        yield return new WaitForSeconds(3.25f);
        generator.Fail = false;
        if(generator.Repair)
        {
            surviverAnimation.Play("Generator_Idle_FT");
        }
    }
}
