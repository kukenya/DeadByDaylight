using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNickName : MonoBehaviourPun
{

    public Text nickName;
    public GameObject ready;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            nickName.text = PhotonNetwork.NickName;
            nickName.color = new Color(0, 0, 0, 0);
            LobbyManager.instance.myPhotonView = photonView;
            ready.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
        else
        {
            nickName.text = photonView.Owner.NickName;
        }
        LobbyManager.instance.AddPlayer(photonView);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetReady(bool isReady)
    {
        photonView.RPC(nameof(RpcSetReady), RpcTarget.All, isReady);
    }

    [PunRPC]
    void RpcSetReady(bool isReady)
    {
        ready.SetActive(isReady);
        if (isReady == true) LobbyManager.instance.readyCount += 1;
        else LobbyManager.instance.readyCount -= 1;
    }


}
