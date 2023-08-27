using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SurviverHealing : MonoBehaviourPun, IPunObservable
{
    SurviverController surviverController;
    SurviverAnimation surviverAnimation;
    SurvivorInteraction interaction;
    SurviverHealth health;
    SurviverUI ui;
    SkillCheck skillCheck;
    SurvivorShader shader;

    float prograss;


    public float Prograss { get { return prograss; } set { prograss = Mathf.Clamp(value, 0, maxPrograssTime); } }
    public float maxPrograssTime;
    public bool selfHeal = false;
    public bool SelfHeal
    {
        get { return selfHeal; }
        set
        {
            if (value == false && selfHeal != value)
            {
                skillCheck.enabled = false;
                selfHeal = value;
            }
            else if(value == true && selfHeal != value)
            {
                skillCheck.enabled = true;
                skillCheck.InputAction(GetSkillCheckValue);
                selfHeal = value;
            }
        }
    }

    public bool otherHealing = false;

    public bool OtherHealing
    {
        get
        {
            return otherHealing;
        }

        set
        {
            if (value == true && otherHealing != value)
            {
                if(photonView.IsMine == false)
                {
                    skillCheck.enabled = true;
                    skillCheck.InputAction(GetSkillCheckValue);
                }
                HealingSurvivor++;
                otherHealing = value;
            }
            else if (value == false && otherHealing != value)
            {
                if (photonView.IsMine == false)
                {
                    skillCheck.enabled = false;
                }
                HealingSurvivor--;
                otherHealing = value;
            }
        }
    }



    public bool healed = false;

    public float normalValue = 2f;
    public float hardValue = 5f;

    [Header("플레이어 수")]
    int intSurvivor = 0;
    public int HealingSurvivor { get { return intSurvivor; } set {
            photonView.RPC(nameof(SetIntSurvivor), RpcTarget.All, value); 
        } 
    }

    [PunRPC]
    void SetIntSurvivor(int value)
    {
        intSurvivor = value;
        SetMultiplayIncrease();
        SurviverUI.instance.ChangePrograssBarSprite(intSurvivor);
    }

    float multiplyIncrease = 0;
    float selfHealingIncrease = 0.8f;

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
        shader = GetComponent<SurvivorShader>();
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
                photonView.RPC(nameof(SkillCheckFail), RpcTarget.All);
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
        if (healed) return;

        if (Prograss >= maxPrograssTime)
        {
            if (health.State == SurviverHealth.HealthState.Down && OtherHealing == false) return;


            if (photonView.IsMine)
            {
                if (health.State == SurviverHealth.HealthState.Injured) { health.State = SurviverHealth.HealthState.Healthy; healed = true; }
                else { 
                    surviverController.Crawl = false; 
                    photonView.RPC("ChangePose", RpcTarget.All); 
                    shader.RedXray = false;
                    health.State = SurviverHealth.HealthState.Injured;
                    WorldShaderManager.Instance.SurvivorShader = WorldShaderManager.Survivor.None;
                    healed = false;
                }
            }
            
            print("치료 완료");
            if (selfHeal)
            {
                OffSelfHeal();
            }
            else if (otherHealing)
            {
                if (photonView.IsMine)
                {
                    photonView.RPC(nameof(Off), RpcTarget.Others);
                }
            }
            return;
        }

        if (photonView.IsMine)
        {
            if (OtherHealing) Prograss += Time.deltaTime * multiplyIncrease;
            else if (SelfHeal) Prograss += Time.deltaTime * selfHealingIncrease;
        }
    }

    [PunRPC]
    void Off()
    {
        interaction.OffFriendHealing();
    }

    public void OnSelfHeal()
    {
        if (OtherHealing) return;
        surviverController.BanMove = true;
        SelfHeal = true;
        if (health.State == SurviverHealth.HealthState.Down) return;
        surviverAnimation.Play("Healing_Self");
    }

    public void OffSelfHeal()
    {
        if (OtherHealing) return;
        surviverController.BanMove = false;
        SelfHeal = false;
    }

    public void OnFriendHeal(SurvivorInteraction interaction)
    {
        print("OnFriendHeal : " + gameObject.name);
        this.interaction = interaction;
        photonView.RPC(nameof(OnFriendHealRPC), RpcTarget.All);
    }

    [PunRPC]
    public void OnFriendHealRPC()
    {
        surviverController.BanMove = true;
        OtherHealing = true;
        if (health.State == SurviverHealth.HealthState.Down) return;
        surviverAnimation.Play("Being_Heal");
    }


    public void OffFriendHeal()
    {
        print("OffFriendHeal : " + gameObject.name);
        photonView.RPC(nameof(OffFriendHealRPC), RpcTarget.All);
    }

    [PunRPC]
    public void OffFriendHealRPC()
    {
        surviverController.BanMove = false;
        OtherHealing = false;
    }

    public float failValue = 5f;


    [PunRPC]
    void SkillCheckFail()
    {
        if (photonView.IsMine == false) return;
        Prograss += -failValue;
        //failAudio.Play();
        if (SelfHeal)
        {
            surviverAnimation.Play("Healing_Self_Fail");
        }
        else if(OtherHealing)
        {
            surviverAnimation.Play("Being_Heal_Fail");
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
