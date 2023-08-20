using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    // NickName InputField
    public InputField inputNickName;

    // Connect Button
    public Button btnConnect;

    // Start is called before the first frame update
    void Start()
    {
        // inputNickName의 내용이 변경될 때 호출되는 함수 등록
        inputNickName.onValueChanged.AddListener(OnValueChanged);

        // inputNickName에서 엔터 쳤을 때 호출되는 함수 등록
        inputNickName.onSubmit.AddListener(
            (string s) =>
            {
                // 버튼이 활성화 되어있다면
                if (btnConnect.interactable)
                {
                    // OnclickConnect 호출
                    OnClickConnect();
                }
            }
        );

        // 버튼 비활성화
        btnConnect.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValueChanged(string s)
    {
        // 접속버튼 활성화 비활성화
        btnConnect.interactable = s.Length > 0;
    }

    public void OnClickConnect()
    {
        // 서버 접속 요청
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        // 닉네임 설정
        PhotonNetwork.NickName = inputNickName.text;

        // 로비 진입 요청
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        // 로비씬으로 이동
        PhotonNetwork.LoadLevel("KJJ_Lobby");
    }
}
