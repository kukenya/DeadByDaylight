using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Exit : MonoBehaviourPun, IPunObservable
{
    public enum State
    {
        Closed,
        Open
    }

    public State state = State.Closed;
    public Transform animPos;
    public Animator anim;

    float prograss;
    public float Prograss { get { return prograss; } set { prograss = Mathf.Clamp(value, 0, maxPrograssTime); } }
    public float maxPrograssTime = 10;

    SurviverUI ui;
    public SurvivorInteraction interaction;


    public void OnSwitch()
    {
        activate = true;
        Play("SwitchActivate");
    }

    public void OffSwitch()
    {
        activate = false;
        if (state == State.Closed) Play("Closed");
    }

    string currentState;

    public void Play(string state, float time = 0.1f)
    {
        if (state == currentState) return;


        anim.enabled = true;
        anim.CrossFadeInFixedTime(state, time, 0);

        currentState = state;
    }

    private void Start()
    {
        ui = SurviverUI.instance;
        gameObject.layer = 0;
    }

    private void Update()
    {
        ExitActivate();
    }
    
    public GameObject doorMesh;
    public GameObject blackHoleGO;
    public Transform blackHoleTrans;
    public float blackHoleGenerateDist = 10f;

    [PunRPC]
    public void GenerateDoorBlackHole()
    {
        if (Vector3.Distance(Camera.main.transform.position, blackHoleTrans.position) >= blackHoleGenerateDist)
        {
            doorMesh.layer = 11;
            GameObject go = Instantiate(blackHoleGO, blackHoleTrans.position, blackHoleTrans.rotation);
            go.GetComponent<BlackHoleEffect>().action = () => { doorMesh.layer = 0; };
        }
    }

    public bool activate = false;
    public bool open = false;

    public void ExitActivate()
    {
        if (open == true) return;

        ChangeSound();

        if (activate == false) return;
        if (prograss >= maxPrograssTime)
        {
            photonView.RPC(nameof(OpenDoor), RpcTarget.All);
        }
        Prograss += Time.deltaTime;
        ui.prograssBar.fillAmount = Prograss / maxPrograssTime;
    }

    void ChangeSound()
    {
        if (maxPrograssTime / 2 > Prograss && Prograss > maxPrograssTime / 3)
        {
            if (soundState == SoundState.Sound2) return;
            soundState = SoundState.Sound2;
            audioSo.PlayOneShot(exitSounds[0]);
        }
        else if (maxPrograssTime > Prograss && Prograss > maxPrograssTime / 2)
        {
            if (soundState == SoundState.Sound3) return;
            soundState = SoundState.Sound3;
            audioSo.PlayOneShot(exitSounds[1]);
        }
        else if (Prograss >= maxPrograssTime)
        {
            if (soundState == SoundState.Sound4) return;
            soundState = SoundState.Sound4;
            audioSo.PlayOneShot(exitSounds[2]);
        }
    }

    [PunRPC]
    void OpenDoor()
    {
        Play("Opening");
        open = true;
        state = State.Open;
        interaction?.EndInteract(SurvivorInteraction.InteractiveType.ExitLever);
        gameObject.layer = 0;
        EndingLine.instance.enabled = true;
        
    }

    public Transform GetAnimationPos(Vector3 position)
    {
        return animPos;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(Prograss);
        }
        else
        {
            Prograss = (float)stream.ReceiveNext();
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (state == State.Open) return;

    //    ui.FocusProgressUI("√‚±∏");
    //    ui.prograssBar.fillAmount = Prograss / maxPrograssTime;
    //    interaction = other.GetComponent<SurvivorInteraction>();
    //    interaction.ChangeInteract(SurvivorInteraction.InteractiveType.ExitLever, this, animPos);
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (state == State.Open) return;

    //    ui.UnFocusProgressUI();
    //    interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
    //}

    public enum SoundState
    {
        Sound1,
        Sound2,
        Sound3,
        Sound4,
    }

    public SoundState soundState = SoundState.Sound1;
    public AudioSource audioSo;
    public AudioClip[] exitSounds;
}
