using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class Generator : MonoBehaviourPun, IPunObservable
{
    public bool repaierd = false;
    public Transform[] animPos;
    float prograss = 0;

    public float Prograss { get { return  prograss; } set {
            prograss = Mathf.Clamp(value, 0, maxPrograssTime);
        }
    }

    public float maxPrograssTime = 80;
    bool repairing = false;

    public bool Repair { get { return repairing; } set {
            
            if (value == false && repairing != value)
            {
                skillCheck.enabled = false;
                RepairingSurvivor--;
                repairing = value;
            }
            else if(value == true && repairing != value)
            {
                skillCheck.enabled = true;
                skillCheck.InputAction(GetSkillCheckValue);
                RepairingSurvivor++;
                repairing = value;
            }
        } 
    }

    public SurvivorInteraction interaction;
    SkillCheck skillCheck;
    Animator anim;

    [Header("����ũ ��ƼŬ")]
    public ParticleSystem spark;
    public Transform[] sparkParticlePos;

    public AudioSource failAudio;

    [Header("�÷��̾� ��")]
    int intSurvivor = 0;
    public int RepairingSurvivor { get { return intSurvivor; } set {
             photonView.RPC(nameof(SetIntSurvivor), RpcTarget.All, value); } }

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

    bool fail = false;
    public bool Fail { get { return fail; } set { photonView.RPC(nameof(SetFail), RpcTarget.All, value); } }
    [PunRPC]
    void SetFail(bool value)
    {
        fail = value;
    }

    private void Start()
    {
        anim = gameObject.GetComponentInParent<Animator>();
        skillCheck = SkillCheck.Instance;
    }

    private void Update()
    {
        GenRepair();
        UpdateAnim();
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            photonView.RPC(nameof(GenerateBlackHole), RpcTarget.All);
        }
    }
    public GameObject blackHoleGO;
    public Ease blackHoleEase;

    public float blackHoleGenerateDist = 10f;

    public GameObject generatorMesh1;
    public GameObject generatorMesh2;

    [PunRPC]
    void GenerateBlackHole()
    {
        if(Vector3.Distance(Camera.main.transform.position, transform.position) >= blackHoleGenerateDist)
        {
            generatorMesh1.layer = 11;
            generatorMesh2.layer = 11;
            GameObject go = Instantiate(blackHoleGO, transform.position, transform.rotation);
            go.GetComponent<BlackHoleEffect>().action = () => { generatorMesh1.layer = 0; generatorMesh2.layer = 0; };
            //go.transform.DOScale(0, 10).SetDelay(4).SetEase(blackHoleEase).SetAutoKill();
        }
    }

    

    [PunRPC]
    void SkillCheckFail()
    {
        Prograss += failValue;
        anim.CrossFadeInFixedTime("Fail", 0.25f);

        Transform sparkTrans = animPos[0].GetChild(0).transform;
        Instantiate(spark, sparkTrans.position, sparkTrans.rotation);
        failAudio.Play();
    }


    void GenRepair()
    {
        if(repaierd) { return; }

        if(Prograss >= maxPrograssTime)
        {
            Repair = false;
            repaierd = true;
            GenerateBlackHole();
            gameObject.layer = 0;
            GameManager.Instance.Generator--;
            WorldSound.Instacne.PlayWorldSound(0);
            if(interaction != null) interaction.EndInteract(SurvivorInteraction.InteractiveType.Generator);
        }

        if(photonView.IsMine && fail == false)
        {
            Prograss += Time.deltaTime * multiplyIncrease;
        }
    }

    void UpdateAnim()
    {
        if (repairing == false) return;
        anim.SetFloat("Prograss", Prograss / maxPrograssTime);
    }

    [Header("��ų üũ ��� �϶� ��ġ")]
    public float hardValue = 2;
    public float normalValue = 1;
    public float failValue = -1;

    void GetSkillCheckValue(int value)
    {
        switch (value)
        {
            case 0:
                photonView.RPC(nameof(SkillCheckFail), RpcTarget.All);
                if (interaction != null) interaction.GeneratorFail();
                break;
            case 1:
                photonView.RPC(nameof(SKillCheckIncrease), RpcTarget.All, normalValue);
                break;
            case 2:
                photonView.RPC(nameof(SKillCheckIncrease), RpcTarget.All, hardValue);
                break;
        }
    }
    Vector3 playerPos;

    [PunRPC]
    void SKillCheckIncrease(float value)
    {
        Prograss += value;
    }


    int TransformListSortComparer(Transform A, Transform B)
    {
        return Vector3.Distance(new Vector3(playerPos.x, 0, playerPos.z), new Vector3(A.transform.position.x, 0, A.transform.position.z))
                .CompareTo(Vector3.Distance(new Vector3(playerPos.x, 0, playerPos.z), new Vector3(B.transform.position.x, 0, B.transform.position.z)));
    }

    public Transform GetAnimationPos(Vector3 position)
    {
        playerPos = position;
        System.Array.Sort(animPos, TransformListSortComparer);
        return animPos[0];
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
}
