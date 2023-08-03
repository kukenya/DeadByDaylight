using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnaAttack : MonoBehaviour
{
    Animator anim;                          // 애니메이터


    void Start()
    {
        anim = GetComponent<Animator>();    // 안나 Animator 컴포넌트
    }

    void Update()
    {

    }

    private void OneHandAttack()
    {
        if (Input.GetButton("Fire2"))
        {
            print("Charging");
            anim.SetBool("Throwing", true);
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            anim.SetTrigger("Throw");
        }
        else if (Input.GetButtonDown("Fire1"))
        {
            anim.SetBool("Throwing", false);
        }
    }


}
