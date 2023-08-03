using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverHit : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SlahserHitPlayer();
        }
    }
    void SlahserHitPlayer()
    {
        SurviverAnimationMgr.instance.Play("HitBack");
    }
}
