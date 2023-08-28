using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public Quaternion originRotation;
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
        if (escaped == true) return;

        if (Prograss >= maxPrograssTime)
        {
            if (hookCor == null)
            {
                escaped = true;
                hookCor = StartCoroutine(WaitHook());
                if (photonView.IsMine == false)
                {
                    interaction.OffCamperEscape(escaped);
                }
            }
            return;
        }

        if (photonView.IsMine == false) return;

        if (escaping) 
        { 
            Prograss += Time.deltaTime;
        }
        else 
        {
            Prograss = 0;
        }
    }

    SurvivorInteraction interaction;
    public void OnEscaping(SurvivorInteraction interaction)
    {
        this.interaction = interaction;
        photonView.RPC(nameof(OnEscapingRPC), RpcTarget.All, interaction.transform.position);
    }

    public void OffEscaping()
    {
        photonView.RPC(nameof(OffEscapingRPC), RpcTarget.All);
    }

    Coroutine a;
    [PunRPC]
    public void OnEscapingRPC(Vector3 camperPos)
    {
        Escape = true;
        originRotation = transform.rotation;
        transform.rotation = Quaternion.LookRotation(camperPos - transform.position);
        if (photonView.IsMine == false) return;
        a = StartCoroutine(UpdateUI());
        ownerInteraction.offAutoUI = true;

        if (escaped) return;
        surviverAnimation.Play("BeingRescueIn");
    }

    IEnumerator UpdateUI()
    {
        while (true)
        {
            if (escaped) break;
            SurviverUI.instance.ChangePrograssUI(SurviverUI.PrograssUI.On, "±∏√‚");
            SurviverUI.instance.prograssBar.fillAmount = Prograss / maxPrograssTime;
            yield return null;
        }
    }

    [PunRPC]
    public void OffEscapingRPC()
    {
        Escape = false;
        transform.rotation = originRotation;
        if (photonView.IsMine == false) return;
        StopCoroutine(a);
        ownerInteraction.offAutoUI = false;

        if (escaped) return;
        surviverAnimation.Play("Hook_Idle");
    }

    IEnumerator WaitHook()
    {
        health.rootCameraPosition.localPosition = health.rootCameraOriginPos;
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
        anim.speed = 1;
        Prograss = 0;
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

    public bool onPhoton = false;
    public float targetValue;

    IEnumerator PrograssCustomLerp()
    {
        float currentTime = 0;
        while (true)
        {
            if (onPhoton)
            {
                StartCoroutine(DelayPrograss(currentTime));
                onPhoton = false;
                currentTime = 0;
            }
            currentTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator DelayPrograss(float time)
    {
        while (true)
        {
            if(Prograss == targetValue) break;
            Prograss += time * Time.unscaledDeltaTime * (targetValue - Prograss);
            yield return null;
        }
    }
}
