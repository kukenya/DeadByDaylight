using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    // �� ������ư
    public Button[] btnCreateRoom;
    // �� ������ư
    public Button btnJoninRoom;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateRoom()
    {
        // �� �ɼ��� ���� (�ִ� �ο�)
        RoomOptions option = new RoomOptions();
        option.MaxPlayers = 5;

        // �濡 ������ �� �ִ°�
        option.IsOpen = true;

        // �⺻ �κ� �� ���� ��û (���̸�, �� �ɼ�, )
        PhotonNetwork.CreateRoom("GameRoom", option);
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

    public void JoinRoom()
    {
        // �� ���� ��û
        PhotonNetwork.JoinRoom("GameRoom");
    }

    // �� ���� �Ϸ�� ȣ��Ǵ� �Լ�
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("�� ���� �Ϸ�");
    }

    // �� ���� ���н� ȣ��Ǵ� �Լ�
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("�� ���� ���� : " + message);
    }
}
