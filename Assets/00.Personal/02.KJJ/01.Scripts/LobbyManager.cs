using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Transform player;

    public GameObject playerObject;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // 방장이 플레이 씬으로 넘어가면 다른 플레이어도 플레이 씬으로 넘어가게
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void JoinCreateRoom()
    {
        // 방 옵션을 설정 (최대 인원)
        RoomOptions option = new RoomOptions();
        option.MaxPlayers = 5;

        // 방에 참여할 수 있는가
        option.IsOpen = true;

        // 기본 로비에 방 참가 or 생성 요청 (방이름, 방 옵션, )
        PhotonNetwork.JoinOrCreateRoom("GameRoom", option, null);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom("GameRoom");

        // 플레이어 스폰
        PlayerSpawn();
    }


    // 방 생성 완료시 호출되는 함수
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        print("방 생성 완료");
    }

    // 방 생성 실패시 호출되는 함수
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        print("방 생성 실패 : " + message);
    }


    // 방 입장 완료시 호출되는 함수
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("방 입장 완료");
    }

    // 방 입장 실패시 호출되는 함수
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("방 입장 실패 : " + message);
    }

    // 방 나가기
    public void LeaveRoom()
    {
        PhotonNetwork.Destroy(playerObject.gameObject);
        PhotonNetwork.LeaveRoom();
        print("방을 떠났습니다.");
    }

    public void GameStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // GameScene으로 이동
            PhotonNetwork.LoadLevel("Murderer");
        }
    }

    //public override void OnLeftRoom()
    //{
    //    base.OnLeftRoom();
    //    print("OnLeftRoom");
    //}

    //public override void OnConnectedToMaster()
    //{
    //    base.OnConnectedToMaster();
    //    print("OnConnectedToMaster");
    //}

    public void PlayerSpawn()
    {
        playerObject = PhotonNetwork.Instantiate("Player", player.position, Quaternion.Euler(0, 150, 0)); // ("생성파일이름",생성위치,생성방향)
    }
}
