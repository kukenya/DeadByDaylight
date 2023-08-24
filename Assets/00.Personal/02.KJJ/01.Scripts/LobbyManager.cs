using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager instance = null;

    public GameObject[] player;

    public List<PhotonView> playerObject;

    public PhotonView myPhotonView;

    public int readyCount;
    int maxPlayerReady;

    public GameObject startWaitButton;
    public GameObject startButton;

    int num = 0;

    // 살인마 체크
    bool isMurderer = false;

    private void Awake()
    {
        // 만약에 instance 값이 null 이라면
        if (instance == null)
        {
            // instance에 나 자신을 셋팅
            instance = this;
        }
        // 그렇지 않으면
        else
        {
            // 나를 파괴한다.
            Destroy(gameObject);
        }

        PhotonNetwork.AutomaticallySyncScene = true; // 방장이 플레이 씬으로 넘어가면 다른 플레이어도 플레이 씬으로 넘어가게
    }
    // Start is called before the first frame update
    void Start()
    {
        playerObject = new List<PhotonView>();
        readyCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        maxPlayerReady = playerObject.Count;
        if (PhotonNetwork.IsMasterClient)
        {
            if (readyCount == maxPlayerReady && maxPlayerReady > 0)
            {
                startWaitButton.SetActive(false);
                startButton.SetActive(true);
            }
            else
            {
                startButton.SetActive(false);
                startWaitButton.SetActive(true);
            }
        }
    }

    public void JoinCreateRoom()
    {
        // 살인마가 맞는지 체크
        isMurderer = true;

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

        // 살인마가 아니면 캐릭터를 생성
        if (isMurderer == false)
        {
            PlayerSpawn();
        }
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
        PlayerDestory();
        PhotonNetwork.LeaveRoom();
    }

    public void GameStart()
    {
        print("게임씬으로 이동");
        if (PhotonNetwork.IsMasterClient)
        {
            // GameScene으로 이동
            PhotonNetwork.LoadLevel("PHS");
        }
    }

    public void PlayerSpawn()
    {
        num = PhotonNetwork.CountOfPlayersInRooms;
        PhotonNetwork.Instantiate("Player", player[num].transform.position, Quaternion.Euler(0, 95, 0)); // ("생성파일이름",생성위치,생성방향)
    }


    public void PlayerDestory()
    {
        PhotonNetwork.Destroy(playerObject[num].gameObject);
    }

    // 플레이어 입장시
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        print(newPlayer.NickName + "님이 들어왔습니다!");
    }

    // 플레이어 나갈시
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        print(otherPlayer.NickName + "님이 나갔습니다!");
    }

    public void AddPlayer(PhotonView go)
    {
        playerObject.Add(go);
    }

    public void SetReady(bool isReady)
    {
        myPhotonView.GetComponent<LobbyNickName>().SetReady(isReady);
    }
}
