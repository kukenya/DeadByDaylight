using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
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
            if (repairing == false) skillCheck.EndRandomSkillCheck();
            else skillCheck.StartRandomSkillCheck(GetSkillCheckValue);
        } 
    }

    SurviverUI ui;
    public SurvivorInteraction interaction;
    SkillCheck skillCheck;
    Animator anim;

    [Header("스파크 파티클")]
    public ParticleSystem spark;
    public Transform[] sparkParticlePos;

    public AudioSource failAudio;

    private void Start()
    {
        anim = gameObject.GetComponentInParent<Animator>();
        ui = SurviverUI.instance;
        skillCheck = SkillCheck.Instance;
    }

    private void Update()
    {
        GenRepair();
        UpdateAnim();
    }

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
        Prograss += Time.deltaTime;
    }

    void UpdateAnim()
    {
        if (repairing == false) return;
        anim.SetFloat("Prograss", Prograss / maxPrograssTime);
    }

    [Header("스킬 체크 상승 하락 수치")]
    public float hardValue = 2;
    public float normalValue = 1;
    public float failValue = -1;

    void GetSkillCheckValue(int value)
    {
        switch (value)
        {
            case 0:
                SkillCheckFail();
                break;
            case 1:
                Prograss += normalValue;
                break;
            case 2:
                Prograss += hardValue;
                break;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (repaierd) return;

    //    interaction = other.GetComponent<SurvivorInteraction>();
    //    interaction.ChangeInteract(SurvivorInteraction.InteractiveType.Generator, this, this.transform);

    //    compareTrans = other.transform;
    //    ui.FocusProgressUI("수리");
    //    ui.prograssBar.fillAmount = Prograss / maxPrograssTime;
    //}

    //private void OnTriggerStay(Collider other)
    //{
    //    if (repaierd) return;

    //    System.Array.Sort(animPos, TransformListSortComparer);
    //    interaction.Position = animPos[0];
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (repaierd) return;

    //    ui.UnFocusProgressUI();
    //    interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
    //}


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
