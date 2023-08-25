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
        if (photonView.IsMine) return;
        if (SelecterManager.Instance.IsSurvivor == false) return;
        if(value == true)
        {
            foreach (Transform child in go.transform)
            {
                child.gameObject.layer = 9;
            }
        }
        else
        {
            foreach (Transform child in go.transform)
            {
                child.gameObject.layer = 6;
            }
        }
        red = value;
    }

    bool yellow = false;

    public bool YellowXray { get { return yellow; } set { photonView.RPC(nameof(SetYellowXray), RpcTarget.All, value); } }

    [PunRPC]
    void SetYellowXray(bool value)
    {
        if (photonView.IsMine) return;
        if (SelecterManager.Instance.IsSurvivor == false) return;
        if (value == true)
        {
            foreach (Transform child in go.transform)
            {
                child.gameObject.layer = 8;
            }
        }
        else
        {
            foreach (Transform child in go.transform)
            {
                child.gameObject.layer = 6;
            }
        }
        yellow = value;
    }
}
