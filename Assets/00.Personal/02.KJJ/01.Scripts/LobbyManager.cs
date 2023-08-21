using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    // ���ӽ��� Ȱ��ȭ
    //public Button[] btnCreateRoom;

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
        // �� �ɼ��� ���� (�ִ� �ο�)
        RoomOptions option = new RoomOptions();
        option.MaxPlayers = 5;

        // �濡 ������ �� �ִ°�
        option.IsOpen = true;

        // �⺻ �κ� �� ���� or ���� ��û (���̸�, �� �ɼ�, )
        PhotonNetwork.JoinOrCreateRoom("GameRoom", option, null);
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
        PhotonNetwork.LeaveRoom();
        print("���� �������ϴ�.");
    }
}
