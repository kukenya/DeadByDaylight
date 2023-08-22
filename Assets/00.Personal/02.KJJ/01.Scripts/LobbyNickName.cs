using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNickName : MonoBehaviourPun
{
    public Text nickName;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            nickName.text = PhotonNetwork.NickName;
            nickName.color = new Color(0,0,0,0);
        }
        else
        {
            nickName.text = photonView.Owner.NickName;
        }
        LobbyManager.instance.AddPlayer(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
