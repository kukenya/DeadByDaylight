using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviourPun
{
    public GameObject lobby;
    public GameObject killerLobby;
    public GameObject playerLobby;
    public GameObject playerReady;
    public GameObject playerCancel;
    public GameObject readycheck;
    public GameObject readyStartImage;

    public GameObject killer;
    public GameObject player;

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnclickBack();
            LobbyManager.instance.LeaveRoom();
        }
    }

    public void OnclickKiller()
    {
        player.SetActive(false);
        killer.SetActive(true);
        lobby.SetActive(false);
        killerLobby.SetActive(true);
        playerLobby.SetActive(false);
        readyStartImage.SetActive(true);
    }

    public void OnclickPlayer()
    {
        killer.SetActive(false);
        player.SetActive(true);
        lobby.SetActive(false);
        playerLobby.SetActive(true);
        killerLobby.SetActive(false);
        readyStartImage.SetActive(true);
    }

    public void OnclickBack()
    {
        killer.SetActive(false);
        player.SetActive(false);
        killerLobby.SetActive(false);
        playerLobby.SetActive(false);
        lobby.SetActive(true);
        readyStartImage.SetActive(false);
    }

    public void PlayerReady()
    {
        playerCancel.SetActive(true);
        playerReady.SetActive(false);
        readycheck.SetActive(true);
        LobbyManager.instance.SetReady(true);
        //SetReadyImage(true);

        //LobbyManager.instance.myPhotonView.RPC("RpcSetReady", Photon.Pun.RpcTarget.All, true);
    }
    

    public void PlayerCancel()
    {
        playerReady.SetActive(true);
        playerCancel.SetActive(false);
        readycheck.SetActive(false);
        LobbyManager.instance.SetReady(false);
        //SetReadyImage(false);
    }

    //public void SetReadyImage(bool onReady)
    //{
    //    LobbyManager.instance.readyImage.GetComponent<PhotonView>().RPC(nameof(RpcReadyImage), RpcTarget.AllBuffered, onReady);
    //}

    //[PunRPC]
    //void RpcReadyImage(bool onReady)
    //{
    //    if (onReady == true) LobbyManager.instance.readyImage.GetComponent<Image>().color = Color.red;
    //    else LobbyManager.instance.readyImage.GetComponent<Image>().color = Color.white;
    //}
}
