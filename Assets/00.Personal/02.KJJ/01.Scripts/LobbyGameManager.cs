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
        // 플레이어 스폰 위치
        int idx = PhotonNetwork.CurrentRoom.PlayerCount;
        PhotonNetwork.Instantiate("Survivor", trSpawnPosGroup[idx].position, Quaternion.identity); // ("생성파일이름",생성위치,생성방향)
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
