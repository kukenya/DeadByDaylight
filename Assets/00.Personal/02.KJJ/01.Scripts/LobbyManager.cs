using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject[] player;

    public GameObject[] playerObject;

    int i = 0;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // ������ �÷��� ������ �Ѿ�� �ٸ� �÷��̾ �÷��� ������ �Ѿ��
    }
    // Start is called before the first frame update
    void Start()
    {
        i = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void JoinCreateRoom()
    {
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

        if (!PhotonNetwork.IsMasterClient) PlayerSpawn(); // �÷��̾� ����
       
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
        PhotonNetwork.Destroy(playerObject[i].gameObject);
        i--;
        PhotonNetwork.LeaveRoom();
        print("���� �������ϴ�.");
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
        playerObject[i] = PhotonNetwork.Instantiate("Player", player[i].transform.position, Quaternion.Euler(0, 150, 0)); // ("���������̸�",������ġ,��������)
        i++;
    }
}
