using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverHealing : MonoBehaviourPun, IPunObservable
{
    public enum HealingTarget
    {
        Self,
        Being
    }

    public HealingTarget target = HealingTarget.Self;

    SurviverController surviverController;
    SurviverAnimation surviverAnimation;
    SurvivorInteraction interaction;
    SurviverHealth health;
    SurviverUI ui;
    SkillCheck skillCheck;

    float prograss;


    public float Prograss { get { return prograss; } set { prograss = value; } }
    public float maxPrograssTime;
    public bool healing = false;
    public bool Heal
    {
        get { return healing; }
        set
        {
            if (value == false && healing != value)
            {
                skillCheck.enabled = false;
                HealingSurvivor--;
                healing = value;
            }
            else if(value == true && healing != value)
            {
                if(photonView.IsMine && target == HealingTarget.Self) 
                {
                    skillCheck.enabled = true;
                    skillCheck.InputAction(GetSkillCheckValue);
                }
                else if(!photonView.IsMine && target == HealingTarget.Being)
                {
                    skillCheck.enabled = true;
                    skillCheck.InputAction(GetSkillCheckValue);
                }
                HealingSurvivor++;
                healing = value;
            }
        }
    }

    public bool healed = false;

    public float normalValue = 2f;
    public float hardValue = 5f;

    [Header("플레이어 수")]
    int intSurvivor = 0;
    public int HealingSurvivor { get { return intSurvivor; } set { photonView.RPC(nameof(SetIntSurvivor), RpcTarget.All, value); } }

    [PunRPC]
    void SetIntSurvivor(int value)
    {
        intSurvivor = value;
        SetMultiplayIncrease();
        SurviverUI.instance.ChangePrograssBarSprite(intSurvivor);
    }

    float multiplyIncrease = 0;

    void SetMultiplayIncrease()
    {
        switch (intSurvivor)
        {
            case 0:
                multiplyIncrease = 0;
                break;
            case 1:
                multiplyIncrease = 1;
                break;
            case 2:
                multiplyIncrease = 1.5f;
                break;
            case 3:
                multiplyIncrease = 2;
                break;
        }
    }


    private void Start()
    {
        surviverController = GetComponent<SurviverController>();
        surviverAnimation = GetComponent<SurviverAnimation>();
        health = GetComponent<SurviverHealth>();
        ui = SurviverUI.instance;
        skillCheck = SkillCheck.Instance;
    }

    private void Update()
    {
        Healing();
    }

    void GetSkillCheckValue(int value)
    {
        switch (value)
        {
            case 0:
                SkillCheckFail();
                break;
            case 1:
                photonView.RPC(nameof(SkillCheckSuccess), RpcTarget.All, normalValue);
                break;
            case 2:
                photonView.RPC(nameof(SkillCheckSuccess), RpcTarget.All, hardValue);
                break;
        }
    }

    [PunRPC]
    void SkillCheckSuccess(float value)
    {
        Prograss += value;
    }

    void Healing()
    {
        if (Prograss >= maxPrograssTime)
        {
            health.State = SurviverHealth.HealthState.Healthy;
            if (multiplyIncrease == 0) return;
            switch (target)
            {
                case HealingTarget.Self:
                    OffSelfHeal();
                    break;
                case HealingTarget.Being:
                    if (interaction != null)
                    {
                        interaction.OffFriendHealing();  
                        interaction = null;
                    }
                    break;
            }
            return;
        }

        if (photonView.IsMine)
        {
            Prograss += Time.deltaTime * multiplyIncrease;
        }
    }

    public void OnSelfHeal()
    {
        if(target == HealingTarget.Being) { return; }
        surviverController.BanMove = true;
        surviverAnimation.Play("Healing_Self");
        Heal = true;
    }

    public void OffSelfHeal()
    {
        if (target == HealingTarget.Being) { return; }
        surviverController.BanMove = false;
        Heal = false;
    }

    public void OnFriendHeal(SurvivorInteraction interaction)
    {
        this.interaction = interaction;
        photonView.RPC(nameof(OnFriendHealRPC), RpcTarget.All);
    }

    [PunRPC]
    public void OnFriendHealRPC()
    {
        target = HealingTarget.Being;
        surviverController.BanMove = true;
        surviverAnimation.Play("Being_Heal");
        Heal = true;
    }


    public void OffFriendHeal()
    {
        photonView.RPC(nameof(OffFriendHealRPC), RpcTarget.All);
    }

    [PunRPC]
    public void OffFriendHealRPC()
    {
        target = HealingTarget.Self;
        surviverController.BanMove = false;
        Heal = false;
    }

    public float failValue = 5f;

    void SkillCheckFail()
    {
        photonView.RPC(nameof(SkillCheckSuccess), RpcTarget.All, -failValue);
        //failAudio.Play();
        switch (target)
        {
            case HealingTarget.Self:
                surviverAnimation.Play("Healing_Self_Fail");
                break;
            case HealingTarget.Being:
                surviverAnimation.Play("Being_Heal_Fail");
                break;
        }  
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Prograss);
        }
        else
        {
            Prograss = (float)stream.ReceiveNext();
        }
    }
}
