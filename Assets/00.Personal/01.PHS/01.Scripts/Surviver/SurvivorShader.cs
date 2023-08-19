using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorShader : MonoBehaviourPun
{
    public GameObject go;

    bool red = false;
    public bool RedXray { get { return red; } set { photonView.RPC(nameof(SetRedXray), RpcTarget.All, value); } }

    [PunRPC]
    void SetRedXray(bool value)
    {
        if(value == true)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = 9;
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = 6;
            }
        }
        red = value;
    }
}
