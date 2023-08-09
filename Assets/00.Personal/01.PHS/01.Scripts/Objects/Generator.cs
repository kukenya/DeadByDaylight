using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public Transform[] animPos;
    Transform compareTrans;
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
    SurvivorInteraction interaction;
    SkillCheck skillCheck;
    Animator anim;

    float animationChangeTime;

    private void Start()
    {
        anim = gameObject.GetComponentInParent<Animator>();
        animationChangeTime = maxPrograssTime / 4;
        ui = SurviverUI.instance;
        skillCheck = SkillCheck.Instance;
    }

    private void Update()
    {
        GenRepair();
        UpdateAnim();
    }

    void GenRepair()
    {
        if (repairing == false) return;

        Prograss += Time.deltaTime;
        ui.prograssBar.fillAmount = Prograss / maxPrograssTime;
        ui.OnProgressUI();
    }

    void UpdateAnim()
    {
        if (repairing == false) return;

        anim.SetLayerWeight(1, Mathf.Clamp(Prograss / animationChangeTime, 0, 1));
        anim.SetLayerWeight(2, Mathf.Clamp((Prograss - animationChangeTime) / animationChangeTime, 0, 1));
        anim.SetLayerWeight(3, Mathf.Clamp((Prograss - animationChangeTime * 2) / animationChangeTime, 0, 1));
        anim.SetLayerWeight(4, Mathf.Clamp((Prograss - animationChangeTime * 3) / animationChangeTime, 0, 1));
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
                Prograss += failValue;
                break;
            case 1:
                Prograss += normalValue;
                break;
            case 2:
                Prograss += hardValue;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        interaction = other.GetComponent<SurvivorInteraction>();
        interaction.ChangeInteract(SurvivorInteraction.InteractiveType.Generator, this, this.transform);

        compareTrans = other.transform;
        ui.FocusProgressUI();
        ui.prograssBar.fillAmount = Prograss / maxPrograssTime;
    }

    private void OnTriggerStay(Collider other)
    {
        System.Array.Sort(animPos, TransformListSortComparer);
        interaction.Position = animPos[0];
    }

    private void OnTriggerExit(Collider other)
    {
        ui.UnFocusProgressUI();
        interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
    }




    int TransformListSortComparer(Transform A, Transform B)
    {
        return Vector3.Distance(new Vector3(compareTrans.position.x, 0, compareTrans.position.z), new Vector3(A.transform.position.x, 0, A.transform.position.z))
                .CompareTo(Vector3.Distance(new Vector3(compareTrans.position.x, 0, compareTrans.position.z), new Vector3(B.transform.position.x, 0, B.transform.position.z)));
    }
}
