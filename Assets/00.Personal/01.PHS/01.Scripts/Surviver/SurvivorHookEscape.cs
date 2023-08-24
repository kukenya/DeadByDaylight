using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using static SurviverHealth;

public class SurvivorHookEscape : MonoBehaviourPun, IPunObservable
{
    Animator anim;
    SurviverHealth health;
    SurviverAnimation surviverAnimation;
    SurviverController controller;
    SurvivorShader shader;
    SurvivorInteraction ownerInteraction;
    float prograss = 0;
    public float maxPrograssTime = 2;
    public float Prograss
    {
        get { return prograss; }
        set
        {
            prograss = Mathf.Clamp(value, 0, maxPrograssTime);
        }
    }

    public bool escaping = false;
    public bool Escape
    {
        get { return escaping; }
        set
        {
            escaping = value;
        }
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<SurviverHealth>();
        surviverAnimation = GetComponent<SurviverAnimation>();
        controller = GetComponent<SurviverController>();
        shader = GetComponent<SurvivorShader>();
        ownerInteraction = GetComponent<SurvivorInteraction>();
    }

    private void Update()
    {
        OtherHookEscape();
    }

    public bool escaped = false;

    Coroutine hookCor;
    void OtherHookEscape()
    {
        if (escaping == false) return;

        if (escaped == true) return;

        if (Prograss >= maxPrograssTime)
        {
            if (hookCor == null)
            {
                hookCor = StartCoroutine(WaitHook());
                escaped = true;
            }
            return;
        }

        if(photonView.IsMine == false) return;

        if (escaping) 
        { 
            Prograss += Time.deltaTime;
        }
        else 
        {
            Prograss -= Time.deltaTime;
        }
    }

    SurvivorInteraction interaction;
    public void OnEscaping(SurvivorInteraction interaction)
    {
        this.interaction = interaction;
        photonView.RPC(nameof(OnEscapingRPC), RpcTarget.All);
    }

    public void OffEscaping()
    {
        photonView.RPC(nameof(OffEscapingRPC), RpcTarget.All);
    }

    Coroutine a;
    [PunRPC]
    public void OnEscapingRPC()
    {
        if(photonView.IsMine == false) return;
        a = StartCoroutine(UpdateUI());
        ownerInteraction.offAutoUI = true;
        Escape = true;
        surviverAnimation.Play("BeingRescueIn");
    }

    IEnumerator UpdateUI()
    {
        while (true)
        {
            SurviverUI.instance.ChangePrograssUI(SurviverUI.PrograssUI.On, "±∏√‚");
            SurviverUI.instance.prograssBar.fillAmount = Prograss / maxPrograssTime;
            yield return null;
        }
    }

    [PunRPC]
    public void OffEscapingRPC()
    {
        if(photonView.IsMine == false) return;
        StopCoroutine(a);
        ownerInteraction.offAutoUI = false;
        Escape = false;
    }

    IEnumerator WaitHook()
    {
        health.rootCameraPosition.position -= new Vector3(0, health.yOffset, 0);
        surviverAnimation.Play("BeingRescueEnd");
        while (true)
        {
            if (surviverAnimation.IsAnimEnd("BeingRescueEnd")) break;
            yield return null;
        }
        controller.BanMove = false;
        WorldShaderManager.Instance.SurvivorShader = WorldShaderManager.Survivor.None;
        health.State = HealthState.Injured;
        controller.Crawl = false;
        photonView.RPC(nameof(ChangePose2), RpcTarget.All);
        hookCor = null;
        shader.RedXray = false;
    }

    [PunRPC]
    void ChangePose2()
    {
        surviverAnimation.Pose = SurviverAnimation.PoseState.Standing;
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
