using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{
    public GameObject lobby;
    public GameObject killerLobby;
    public GameObject playerLobby;

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

    }

    public void OnclickPlayer()
    {
        killer.SetActive(false);
        player.SetActive(true);
        lobby.SetActive(false);
        playerLobby.SetActive(true);
        killerLobby.SetActive(false);
    }

    public void OnclickBack()
    {
        killer.SetActive(false);
        player.SetActive(false);
        killerLobby.SetActive(false);
        playerLobby.SetActive(false);
        lobby.SetActive(true);
    }
}
