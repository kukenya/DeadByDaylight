using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Generator : MonoBehaviourPun
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
            repairing = value;
            if (repairing == false)
            {
                skillCheck.EndRandomSkillCheck();
                RepairingSurvivor--;
            }
            else
            {
                skillCheck.StartRandomSkillCheck(GetSkillCheckValue);
                RepairingSurvivor++;
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
    public int RepairingSurvivor { get { return intSurvivor; } set { photonView.RPC(nameof(SetIntSurvivor), RpcTarget.All, value); } }
    [PunRPC]
    void SetIntSurvivor(int value)
    {
        intSurvivor = value; SetMultiplayIncrease();
    }
    public float multiplyIncrease = 1;

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
        anim = gameObject.GetComponentInParent<Animator>();
        skillCheck = SkillCheck.Instance;
    }

    private void Update()
    {
        GenRepair();
        UpdateAnim();
    }

    [PunRPC]
    void SkillCheckFail()
    {
        Prograss += failValue;
        anim.CrossFadeInFixedTime("Fail", 0.25f);

        Transform sparkTrans = animPos[0].GetChild(0).transform;
        Instantiate(spark, sparkTrans.position, sparkTrans.rotation);
        failAudio.Play();
        interaction.GeneratorFail();
    }

    void GenRepair()
    {
        if (repairing == false) return;
        if(Prograss >= maxPrograssTime)
        {
            repaierd = true;
            WorldSound.Instacne.PlayGeneratorClear();
            interaction.EndInteract(SurvivorInteraction.InteractiveType.Generator);
            gameObject.layer = 0;
        }
        Prograss += Time.deltaTime * multiplyIncrease;
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
                break;
            case 1:
                Prograss += normalValue;
                break;
            case 2:
                Prograss += hardValue;
                break;
        }
    }
    Vector3 playerPos;

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
}
