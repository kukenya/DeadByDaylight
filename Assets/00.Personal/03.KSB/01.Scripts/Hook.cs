using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviourPun
{

    public Transform hookPos;
    public Transform anna;

    public float blackHoleGenerateDist = 10f;
    public GameObject hook;
    public GameObject blackHoleGO;

    [PunRPC]
    public void GenerateHookBlackHoleEffect()
    {
        if (Vector3.Distance(Camera.main.transform.position, transform.position) >= blackHoleGenerateDist)
        {
            hook.layer = 11;
            GameObject go = Instantiate(blackHoleGO, transform.position, transform.rotation);
            go.GetComponent<BlackHoleEffect>().action = () => { hook.layer = 0; };
        }
    }

    
}
