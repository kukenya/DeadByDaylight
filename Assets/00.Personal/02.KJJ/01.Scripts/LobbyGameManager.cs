using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyGameManager : MonoBehaviourPunCallbacks
{
    public Transform[] trSpawnPosGroup;

    // Start is called before the first frame update
    void Start()
    {
        // �÷��̾� ���� ��ġ
        int idx = PhotonNetwork.CurrentRoom.PlayerCount;
        PhotonNetwork.Instantiate("Survivor", trSpawnPosGroup[idx].position, Quaternion.identity); // ("���������̸�",������ġ,��������)
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
