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

    // ���θ� üũ
    bool isMurderer = false;

    private void Awake()
    {
        // ���࿡ instance ���� null �̶��
        if (instance == null)
        {
            // instance�� �� �ڽ��� ����
            instance = this;
        }
        // �׷��� ������
        else
        {
            // ���� �ı��Ѵ�.
            Destroy(gameObject);
        }

        PhotonNetwork.AutomaticallySyncScene = true; // ������ �÷��� ������ �Ѿ�� �ٸ� �÷��̾ �÷��� ������ �Ѿ��
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
        // ���θ��� �´��� üũ
        isMurderer = true;

        // �� �ɼ��� ���� (�ִ� �ο�)
        RoomOptions option = new RoomOptions();
        option.MaxPlayers = 5;

        // �濡 ������ �� �ִ°�
        option.IsOpen = true;

        // �⺻ �κ� �� ���� or ���� ��û (���̸�, �� �ɼ�, )
        PhotonNetwork.JoinOrCreateRoom("GameRoom", option, null);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom("GameRoom");
    }


    // �� ���� �Ϸ�� ȣ��Ǵ� �Լ�
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        print("�� ���� �Ϸ�");
    }

    // �� ���� ���н� ȣ��Ǵ� �Լ�
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        print("�� ���� ���� : " + message);
    }


    // �� ���� �Ϸ�� ȣ��Ǵ� �Լ�
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("�� ���� �Ϸ�");

        // ���θ��� �ƴϸ� ĳ���͸� ����
        if (isMurderer == false)
        {
            PlayerSpawn();
        }
    }

    // �� ���� ���н� ȣ��Ǵ� �Լ�
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("�� ���� ���� : " + message);
    }

    // �� ������
    public void LeaveRoom()
    {
        PlayerDestory();
        PhotonNetwork.LeaveRoom();
    }

    public void GameStart()
    {
        print("���Ӿ����� �̵�");
        if (PhotonNetwork.IsMasterClient)
        {
            // GameScene���� �̵�
            PhotonNetwork.LoadLevel("PHS");
        }
    }

    public void PlayerSpawn()
    {
        num = PhotonNetwork.CountOfPlayersInRooms;
        PhotonNetwork.Instantiate("Player", player[num].transform.position, Quaternion.Euler(0, 95, 0)); // ("���������̸�",������ġ,��������)
    }


    public void PlayerDestory()
    {
        PhotonNetwork.Destroy(playerObject[num].gameObject);
    }

    // �÷��̾� �����
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        print(newPlayer.NickName + "���� ���Խ��ϴ�!");
    }

    // �÷��̾� ������
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        print(otherPlayer.NickName + "���� �������ϴ�!");
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
